using System;
using UnityEngine;
using SundayLastBreath.UI;

namespace SundayLastBreath.Core
{
    public enum GameState
    {
        StudioSplash,
        Disclaimer,
        Title,
        PrologueWakeUp,
        CharacterCreation,
        ApartmentHub,
        Hallway,
        InGame
    }

    /// <summary>
    /// Master Game Manager controlling flow between game states and passage screens.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.StudioSplash;

        [Header("Screen References")]
        [SerializeField] private StudioSplashScreen studioSplashScreen;
        [SerializeField] private DisclaimerScreen disclaimerScreen;
        [SerializeField] private TitleScreen titleScreen;
        [SerializeField] private PrologueWakeUpScreen prologueWakeUpScreen;
        [SerializeField] private CharacterCreationScreen characterCreationScreen;
        [UnityEngine.Serialization.FormerlySerializedAs("apartmentHubScreen")]
        [SerializeField] private GameStageScreen gameStageScreen;

        public GameState CurrentState => currentState;
        public SundayLastBreath.Data.CharacterData CurrentPlayer { get; private set; }
        public GameStageScreen GameStage => gameStageScreen;
        public GameStageScreen MainGameScreen => gameStageScreen;
        public GameStageScreen ApartmentHub => gameStageScreen;

        public void SetCurrentPlayer(SundayLastBreath.Data.CharacterData player)
        {
            CurrentPlayer = player;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Fallback screen resolution if inspector references were lost
            if (studioSplashScreen == null) studioSplashScreen = FindAnyObjectByType<StudioSplashScreen>(FindObjectsInactive.Include);
            if (disclaimerScreen == null) disclaimerScreen = FindAnyObjectByType<DisclaimerScreen>(FindObjectsInactive.Include);
            if (titleScreen == null) titleScreen = FindAnyObjectByType<TitleScreen>(FindObjectsInactive.Include);
            if (prologueWakeUpScreen == null) prologueWakeUpScreen = FindAnyObjectByType<PrologueWakeUpScreen>(FindObjectsInactive.Include);
            if (characterCreationScreen == null) characterCreationScreen = FindAnyObjectByType<CharacterCreationScreen>(FindObjectsInactive.Include);
            if (gameStageScreen == null) gameStageScreen = FindAnyObjectByType<GameStageScreen>(FindObjectsInactive.Include);
        }

        private void Start()
        {
            // Initial boot starts at StudioSplash
            GoToStudioSplash();
        }

        public void GoToStudioSplash()
        {
            currentState = GameState.StudioSplash;
            HideAllScreensInstant();

            if (studioSplashScreen != null)
            {
                studioSplashScreen.Show(0.8f);
            }
            else
            {
                // Fallback to Disclaimer if splash not assigned
                GoToDisclaimer();
            }
        }

        public void GoToDisclaimer()
        {
            currentState = GameState.Disclaimer;
            HideAllScreensInstant();

            if (disclaimerScreen != null)
            {
                disclaimerScreen.Show(0.8f);
            }
            else
            {
                // Fallback to Title
                GoToTitle();
            }
        }

        public void GoToTitle()
        {
            currentState = GameState.Title;
            HideAllScreensInstant();

            if (titleScreen != null)
            {
                titleScreen.Show(1.0f);
            }
        }

        public void StartNewGame()
        {
            currentState = GameState.PrologueWakeUp;
            Debug.Log("[GameManager] Starting New Game! Transitioning to Prologue Awakening...");

            HideAllScreensInstant();

            if (prologueWakeUpScreen != null)
            {
                prologueWakeUpScreen.Show(0.8f);
            }
            else if (characterCreationScreen != null)
            {
                Debug.LogWarning("[GameManager] PrologueWakeUpScreen not found, falling back to CharacterCreationScreen.");
                characterCreationScreen.Show(0.8f);
            }
            else
            {
                Debug.LogWarning("[GameManager] Neither PrologueWakeUpScreen nor CharacterCreationScreen assigned!");
            }
        }

        public void TransitionFromPrologueToCharacterCreation()
        {
            currentState = GameState.CharacterCreation;
            Debug.Log("[GameManager] Focusing on reflection... Transitioning to Citizen Intake Console.");

            HideAllScreensInstant();

            if (characterCreationScreen != null)
            {
                characterCreationScreen.Show(0.8f);
            }
            else
            {
                Debug.LogWarning("[GameManager] CharacterCreationScreen not assigned!");
            }
        }

        public void CompleteCharacterCreation(SundayLastBreath.Data.CharacterData character)
        {
            CurrentPlayer = character;
            currentState = GameState.ApartmentHub;
            Debug.Log($"<color=green><b>[GameManager] Character Creation Complete!</b></color>\n" +
                      $"Name: {character.FullName} | Gender: {character.gender} | Job: {character.job}\n" +
                      $"Vitals: HP {character.hp}/{character.hpMax}, Stamina {character.stamina}/{character.staminaMax}\n" +
                      $"Traits Acquired: {character.traits.Count} | Starting Items: {character.inventory.Count}");

            GoToGameStage();
        }

        public void GoToGameStage()
        {
            currentState = GameState.ApartmentHub;
            HideAllScreensInstant();

            if (gameStageScreen != null)
            {
                gameStageScreen.gameObject.SetActive(true);
                gameStageScreen.Show(0.8f);
                gameStageScreen.InitializeWithCharacter(CurrentPlayer);
            }
            else
            {
                Debug.LogWarning("[GameManager] GameStageScreen not assigned!");
            }
        }

        public void GoToApartmentHub() => GoToGameStage();

        public void ResumeFromSave(SundayLastBreath.Data.CharacterData loadedChar, string roomId = null)
        {
            CurrentPlayer = loadedChar;
            currentState = GameState.ApartmentHub;
            HideAllScreensInstant();

            if (gameStageScreen != null)
            {
                gameStageScreen.gameObject.SetActive(true);
                gameStageScreen.Show(0.8f);
                gameStageScreen.LoadCharacterState(CurrentPlayer, roomId);
            }
            else
            {
                Debug.LogWarning("[GameManager] GameStageScreen not assigned!");
            }
        }

        public void TransitionFromApartmentToHallway()
        {
            currentState = GameState.Hallway;
            Debug.Log("<color=yellow>[GameManager] Transitioning from Apartment Unit 407 to 4th Floor Hallway...</color>");
            SetPlayerLocation("hallway_1", "UPPER HALLWAY (406 / 407)");
        }

        public void SetPlayerLocation(string roomId, string sectorName = null)
        {
            if (CurrentPlayer != null)
            {
                CurrentPlayer.currentLocationId = roomId;
            }

            if (gameStageScreen != null)
            {
                gameStageScreen.SetLocation(roomId, sectorName);
            }
        }

        private void HideAllScreensInstant()
        {
            if (studioSplashScreen != null) studioSplashScreen.HideInstant();
            if (disclaimerScreen != null) disclaimerScreen.HideInstant();
            if (titleScreen != null) titleScreen.HideInstant();
            if (prologueWakeUpScreen != null) prologueWakeUpScreen.HideInstant();
            if (characterCreationScreen != null) characterCreationScreen.HideInstant();
            if (gameStageScreen != null) gameStageScreen.HideInstant();
        }
    }
}

