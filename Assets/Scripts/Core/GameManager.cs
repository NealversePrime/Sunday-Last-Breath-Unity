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
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.StudioSplash;

        [Header("Screen References")]
        [SerializeField] private StudioSplashScreen studioSplashScreen;
        [SerializeField] private DisclaimerScreen disclaimerScreen;
        [SerializeField] private TitleScreen titleScreen;
        [SerializeField] private PrologueWakeUpScreen prologueWakeUpScreen;
        [SerializeField] private CharacterCreationScreen characterCreationScreen;
        [SerializeField] private ApartmentHubScreen apartmentHubScreen;

        public GameState CurrentState => currentState;
        public SundayLastBreath.Data.CharacterData CurrentPlayer { get; private set; }
        public ApartmentHubScreen ApartmentHub => apartmentHubScreen;

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
            if (apartmentHubScreen == null) apartmentHubScreen = FindAnyObjectByType<ApartmentHubScreen>(FindObjectsInactive.Include);
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

            GoToApartmentHub();
        }

        public void GoToApartmentHub()
        {
            currentState = GameState.ApartmentHub;
            HideAllScreensInstant();

            if (apartmentHubScreen != null)
            {
                apartmentHubScreen.InitializeWithCharacter(CurrentPlayer);
                apartmentHubScreen.Show(0.8f);
            }
            else
            {
                Debug.LogWarning("[GameManager] ApartmentHubScreen not assigned!");
            }
        }

        public void TransitionFromApartmentToHallway()
        {
            currentState = GameState.Hallway;
            Debug.Log("<color=yellow>[GameManager] Transitioning from Apartment Unit 407 to 4th Floor Hallway...</color>");
            SetPlayerLocation("hallway_1", "CORRIDOR N // NORTH SPINE");
        }

        public void SetPlayerLocation(string roomId, string sectorName = null)
        {
            if (CurrentPlayer != null)
            {
                CurrentPlayer.currentLocationId = roomId;
            }

            if (apartmentHubScreen != null)
            {
                apartmentHubScreen.SetLocation(roomId, sectorName);
            }
        }

        private void HideAllScreensInstant()
        {
            if (studioSplashScreen != null) studioSplashScreen.HideInstant();
            if (disclaimerScreen != null) disclaimerScreen.HideInstant();
            if (titleScreen != null) titleScreen.HideInstant();
            if (prologueWakeUpScreen != null) prologueWakeUpScreen.HideInstant();
            if (characterCreationScreen != null) characterCreationScreen.HideInstant();
            if (apartmentHubScreen != null) apartmentHubScreen.HideInstant();
        }
    }
}

