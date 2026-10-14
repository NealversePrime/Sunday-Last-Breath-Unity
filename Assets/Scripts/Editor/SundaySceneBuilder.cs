#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SundayLastBreath.Core;
using SundayLastBreath.UI;
using SundayLastBreath.Data;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SundayLastBreath.Editor
{
    public static class SundaySceneBuilder
    {
        [InitializeOnLoadMethod]
        private static void AutoInit()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;

                string flagPath = "Temp/scene_v3_auto.flag";
                if (!File.Exists(flagPath))
                {
                    Directory.CreateDirectory("Temp");
                    File.WriteAllText(flagPath, "done");
                    BuildScene();
                }
            };
        }

        [MenuItem("Tools/Sunday's Last Breath/Build Main Menu Scene", false, 0)]
        public static void BuildScene()
        {
            // 1. Create or open scene
            string scenePath = "Assets/Scenes/MainGame.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Main Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();

            // 3. EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObj.AddComponent<StandaloneInputModule>();
#endif

            // 4. Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 5. AudioManager
            GameObject audioMgrObj = new GameObject("AudioManager");
            AudioManager audioMgr = audioMgrObj.AddComponent<AudioManager>();

            AudioClip menuMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/main_menu.mp3");
            AudioClip clickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/terminal_beep.mp3");

            SerializedObject audioSo = new SerializedObject(audioMgr);
            if (menuMusic != null) audioSo.FindProperty("menuMusicClip").objectReferenceValue = menuMusic;
            if (clickSfx != null)
            {
                audioSo.FindProperty("buttonClickClip").objectReferenceValue = clickSfx;
                audioSo.FindProperty("buttonHoverClip").objectReferenceValue = clickSfx;
            }
            audioSo.ApplyModifiedProperties();

            // 6. Screens Root
            GameObject screensObj = new GameObject("Screens");
            screensObj.transform.SetParent(canvasObj.transform, false);
            RectTransform screensRect = screensObj.AddComponent<RectTransform>();
            StretchFull(screensRect);

            // Build Screens
            StudioSplashScreen splashScreen = BuildSplashScreen(screensObj.transform);
            DisclaimerScreen disclaimerScreen = BuildDisclaimerScreen(screensObj.transform);
            TitleScreen titleScreen = BuildTitleScreen(screensObj.transform);
            CharacterCreationScreen creationScreen = BuildCharacterCreationScreen(screensObj.transform);

            // 7. GameManager
            GameObject gameMgrObj = new GameObject("GameManager");
            GameManager gameMgr = gameMgrObj.AddComponent<GameManager>();

            SerializedObject gameSo = new SerializedObject(gameMgr);
            gameSo.FindProperty("studioSplashScreen").objectReferenceValue = splashScreen;
            gameSo.FindProperty("disclaimerScreen").objectReferenceValue = disclaimerScreen;
            gameSo.FindProperty("titleScreen").objectReferenceValue = titleScreen;
            gameSo.FindProperty("characterCreationScreen").objectReferenceValue = creationScreen;
            gameSo.ApplyModifiedProperties();

            // Save Scene
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);

            // Register in Build Settings
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[Sunday's Last Breath]</b> Main Menu Scene built successfully at: " + scenePath + "</color>");
        }

        private static StudioSplashScreen BuildSplashScreen(Transform parent)
        {
            GameObject obj = new GameObject("StudioSplashScreen");
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            StretchFull(rect);

            // Background Image
            Image bg = obj.AddComponent<Image>();
            bg.color = Color.black;
            bg.raycastTarget = true; // Click to skip

            StudioSplashScreen screen = obj.AddComponent<StudioSplashScreen>();

            // Logo & Present Container
            GameObject container = new GameObject("SplashContainer");
            container.transform.SetParent(obj.transform, false);
            RectTransform cRect = container.AddComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(500, 500);
            cRect.anchoredPosition = Vector2.zero;

            // Logo Image
            GameObject logoObj = new GameObject("StudioLogo");
            logoObj.transform.SetParent(container.transform, false);
            RectTransform logoRect = logoObj.AddComponent<RectTransform>();
            logoRect.sizeDelta = new Vector2(380, 380);
            logoRect.anchoredPosition = new Vector2(0, 40);

            Image logoImg = logoObj.AddComponent<Image>();
            logoImg.color = Color.white;
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;

            FrameAnimationPlayer animPlayer = logoObj.AddComponent<FrameAnimationPlayer>();
            List<Sprite> studioFrames = LoadSpritesFromFolder("Assets/Sprites/StudioLogo");
            if (studioFrames.Count > 0)
            {
                AssignFramesToPlayer(animPlayer, studioFrames, 8f, true);
                logoImg.sprite = studioFrames[0];
            }

            // Subtitle text: P R E S E N T S
            GameObject textObj = new GameObject("PresentsText");
            textObj.transform.SetParent(container.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            tRect.sizeDelta = new Vector2(500, 60);
            tRect.anchoredPosition = new Vector2(0, -180);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "P  R  E  S  E  N  T  S";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            tmp.characterSpacing = 20;

            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("logoAnimationPlayer").objectReferenceValue = animPlayer;
            so.FindProperty("logoTransform").objectReferenceValue = cRect;
            so.FindProperty("displayDuration").floatValue = 4.2f;
            so.ApplyModifiedProperties();

            return screen;
        }

        private static DisclaimerScreen BuildDisclaimerScreen(Transform parent)
        {
            GameObject obj = new GameObject("DisclaimerScreen");
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            StretchFull(rect);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.02f, 0.02f, 1f);
            bg.raycastTarget = true; // Click anywhere to continue

            DisclaimerScreen screen = obj.AddComponent<DisclaimerScreen>();

            // Content Panel
            GameObject contentObj = new GameObject("DisclaimerContent");
            contentObj.transform.SetParent(obj.transform, false);
            RectTransform cRect = contentObj.AddComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(850, 600);
            cRect.anchoredPosition = Vector2.zero;

            // Header: CONTENT WARNING
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(contentObj.transform, false);
            RectTransform hRect = headerObj.AddComponent<RectTransform>();
            hRect.sizeDelta = new Vector2(850, 60);
            hRect.anchoredPosition = new Vector2(0, 160);

            TextMeshProUGUI headerTmp = headerObj.AddComponent<TextMeshProUGUI>();
            headerTmp.text = "CONTENT WARNING";
            headerTmp.fontSize = 36;
            headerTmp.fontStyle = FontStyles.Bold;
            headerTmp.alignment = TextAlignmentOptions.Center;
            headerTmp.color = new Color(0.83f, 0.33f, 0f, 1f); // Accent Gold/Orange
            headerTmp.characterSpacing = 12;

            // Warning Text
            GameObject bodyObj = new GameObject("BodyText");
            bodyObj.transform.SetParent(contentObj.transform, false);
            RectTransform bRect = bodyObj.AddComponent<RectTransform>();
            bRect.sizeDelta = new Vector2(850, 240);
            bRect.anchoredPosition = new Vector2(0, 10);

            TextMeshProUGUI bodyTmp = bodyObj.AddComponent<TextMeshProUGUI>();
            bodyTmp.text = "This story contains graphic violence, blood and gore, death, strong language, and psychological horror.\n\n" +
                           "This is <color=#B32D2E><b>PART I : DAY ZERO</b></color> of an episodic survival series.\n" +
                           "The choices you make, the people you save, and the humanity you lose will carry over into future installments.\n\n" +
                           "Names, characters, businesses, and incidents are either the products of the author's imagination or used in a fictitious manner. Any resemblance to actual events or locales is entirely coincidental.";
            bodyTmp.fontSize = 18;
            bodyTmp.lineSpacing = 20;
            bodyTmp.alignment = TextAlignmentOptions.Center;
            bodyTmp.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            // Prompt: [ CLICK ANYWHERE TO CONTINUE ]
            GameObject promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(contentObj.transform, false);
            RectTransform pRect = promptObj.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(850, 50);
            pRect.anchoredPosition = new Vector2(0, -180);

            TextMeshProUGUI promptTmp = promptObj.AddComponent<TextMeshProUGUI>();
            promptTmp.text = "[ CLICK ANYWHERE TO CONTINUE ]";
            promptTmp.fontSize = 20;
            promptTmp.alignment = TextAlignmentOptions.Center;
            promptTmp.color = new Color(0.83f, 0.33f, 0f, 0.8f);
            promptTmp.characterSpacing = 8;
            promptObj.AddComponent<PulsingText>();

            return screen;
        }

        private static TitleScreen BuildTitleScreen(Transform parent)
        {
            GameObject obj = new GameObject("TitleScreen");
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            StretchFull(rect);

            TitleScreen screen = obj.AddComponent<TitleScreen>();

            // 1. Background Animation (main_menu_bg)
            GameObject bgObj = new GameObject("AnimatedBackground");
            bgObj.transform.SetParent(obj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            StretchFull(bgRect);

            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            FrameAnimationPlayer bgAnim = bgObj.AddComponent<FrameAnimationPlayer>();
            List<Sprite> bgFrames = LoadSpritesFromFolder("Assets/Sprites/MainMenuBg");
            if (bgFrames.Count > 0)
            {
                AssignFramesToPlayer(bgAnim, bgFrames, 8f, true);
                bgImg.sprite = bgFrames[0];
            }

            // Dark Vignette / Overlay
            GameObject overlayObj = new GameObject("DarkOverlay");
            overlayObj.transform.SetParent(obj.transform, false);
            RectTransform oRect = overlayObj.AddComponent<RectTransform>();
            StretchFull(oRect);
            Image overlayImg = overlayObj.AddComponent<Image>();
            overlayImg.color = new Color(0.02f, 0.02f, 0.02f, 0.75f);
            overlayImg.raycastTarget = false;

            // 2. Menu Container (Left-aligned)
            GameObject menuContainer = new GameObject("MainMenuContent");
            menuContainer.transform.SetParent(obj.transform, false);
            RectTransform mRect = menuContainer.AddComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.1f, 0.5f);
            mRect.anchorMax = new Vector2(0.1f, 0.5f);
            mRect.pivot = new Vector2(0f, 0.5f);
            mRect.sizeDelta = new Vector2(800, 650);
            mRect.anchoredPosition = new Vector2(0, 30);

            // Title Logo
            GameObject logoObj = new GameObject("TitleLogo");
            logoObj.transform.SetParent(menuContainer.transform, false);
            RectTransform logoRect = logoObj.AddComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0f, 1f);
            logoRect.anchorMax = new Vector2(0f, 1f);
            logoRect.pivot = new Vector2(0f, 1f);
            logoRect.sizeDelta = new Vector2(680, 260);
            logoRect.anchoredPosition = new Vector2(0, 0);

            Image logoImg = logoObj.AddComponent<Image>();
            logoImg.preserveAspect = true;
            logoImg.color = Color.white;
            FrameAnimationPlayer logoAnim = logoObj.AddComponent<FrameAnimationPlayer>();
            List<Sprite> titleFrames = LoadSpritesFromFolder("Assets/Sprites/TitleLogo");
            if (titleFrames.Count > 0)
            {
                AssignFramesToPlayer(logoAnim, titleFrames, 28f, true);
                logoImg.sprite = titleFrames[0];
            }

            // Subtitle
            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(menuContainer.transform, false);
            RectTransform sRect = subObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 1f);
            sRect.anchorMax = new Vector2(0f, 1f);
            sRect.pivot = new Vector2(0f, 1f);
            sRect.sizeDelta = new Vector2(700, 70);
            sRect.anchoredPosition = new Vector2(10, -265);

            TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
            subTmp.text = "<color=#D35400><b>PART I : DAY ZERO</b></color>\n\"The world ends today. Survival starts tomorrow.\"";
            subTmp.fontSize = 20;
            subTmp.lineSpacing = 10;
            subTmp.color = new Color(0.75f, 0.75f, 0.75f, 1f);

            // 3. Menu Buttons
            GameObject buttonsObj = new GameObject("MenuButtons");
            buttonsObj.transform.SetParent(menuContainer.transform, false);
            RectTransform bRect = buttonsObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 1f);
            bRect.anchorMax = new Vector2(0f, 1f);
            bRect.pivot = new Vector2(0f, 1f);
            bRect.sizeDelta = new Vector2(400, 240);
            bRect.anchoredPosition = new Vector2(10, -360);

            Button btnNewGame = CreateMenuButton(buttonsObj.transform, "NEW GAME", 0);
            Button btnLoadGame = CreateMenuButton(buttonsObj.transform, "LOAD GAME", -55);
            Button btnSettings = CreateMenuButton(buttonsObj.transform, "SETTINGS", -110);
            Button btnQuit = CreateMenuButton(buttonsObj.transform, "QUIT GAME", -165);

            // 4. Version Watermark (Bottom-left)
            GameObject verObj = new GameObject("VersionInfo");
            verObj.transform.SetParent(obj.transform, false);
            RectTransform vRect = verObj.AddComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0f, 0f);
            vRect.anchorMax = new Vector2(0f, 0f);
            vRect.pivot = new Vector2(0f, 0f);
            vRect.sizeDelta = new Vector2(500, 60);
            vRect.anchoredPosition = new Vector2(30, 25);

            TextMeshProUGUI verTmp = verObj.AddComponent<TextMeshProUGUI>();
            verTmp.text = "EARLY ACCESS v1.0.0 | BUILD 2026\nCOPYRIGHT © 2026 NEALVERSE STUDIO";
            verTmp.fontSize = 14;
            verTmp.lineSpacing = 8;
            verTmp.color = new Color(0.45f, 0.45f, 0.45f, 0.8f);

            // 5. Settings Modal
            GameObject modalObj = BuildSettingsModal(obj.transform);
            modalObj.SetActive(false);

            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("backgroundAnimationPlayer").objectReferenceValue = bgAnim;
            so.FindProperty("titleLogoAnimationPlayer").objectReferenceValue = logoAnim;
            so.FindProperty("newGameButton").objectReferenceValue = btnNewGame;
            so.FindProperty("loadGameButton").objectReferenceValue = btnLoadGame;
            so.FindProperty("settingsButton").objectReferenceValue = btnSettings;
            so.FindProperty("quitButton").objectReferenceValue = btnQuit;
            so.FindProperty("settingsModal").objectReferenceValue = modalObj;
            so.ApplyModifiedProperties();

            return screen;
        }

        private static Button CreateMenuButton(Transform parent, string text, float yPos)
        {
            GameObject btnObj = new GameObject("Btn_" + text.Replace(" ", ""));
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(350, 48);
            rect.anchoredPosition = new Vector2(0, yPos);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.01f); // Transparent raycast target

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Left Accent Bar (revealed on hover)
            GameObject barObj = new GameObject("AccentBar");
            barObj.transform.SetParent(btnObj.transform, false);
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0f, 0.5f);
            barRect.sizeDelta = new Vector2(4, 0);
            barRect.anchoredPosition = Vector2.zero;
            Image barImg = barObj.AddComponent<Image>();
            barImg.color = new Color(0.83f, 0.33f, 0f, 1f); // Gold Accent

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            StretchFull(textRect);
            textRect.offsetMin = new Vector2(15, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.88f, 0.88f, 0.88f, 1f);
            tmp.characterSpacing = 8;

            MenuButtonHover hover = btnObj.AddComponent<MenuButtonHover>();
            SerializedObject so = new SerializedObject(hover);
            so.FindProperty("buttonText").objectReferenceValue = tmp;
            so.FindProperty("leftAccentBar").objectReferenceValue = barImg;
            so.ApplyModifiedProperties();

            return btn;
        }

        private static GameObject BuildSettingsModal(Transform parent)
        {
            GameObject modalObj = new GameObject("SettingsModal");
            modalObj.transform.SetParent(parent, false);
            RectTransform rect = modalObj.AddComponent<RectTransform>();
            StretchFull(rect);

            // Dark Backdrop
            Image backdrop = modalObj.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.85f);

            // Dialog Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(modalObj.transform, false);
            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(600, 480);
            pRect.anchoredPosition = Vector2.zero;

            Image pBg = panel.AddComponent<Image>();
            pBg.color = new Color(0.08f, 0.08f, 0.08f, 0.98f);

            // Panel Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.sizeDelta = new Vector2(0, 60);
            tRect.anchoredPosition = new Vector2(0, -30);

            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "SYSTEM SETTINGS";
            titleTmp.fontSize = 26;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.83f, 0.33f, 0f, 1f);
            titleTmp.characterSpacing = 10;

            SettingsModal modal = modalObj.AddComponent<SettingsModal>();

            // Close Button
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(panel.transform, false);
            RectTransform cbRect = closeBtnObj.AddComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.5f, 0f);
            cbRect.anchorMax = new Vector2(0.5f, 0f);
            cbRect.sizeDelta = new Vector2(200, 45);
            cbRect.anchoredPosition = new Vector2(0, 45);

            Image cbBg = closeBtnObj.AddComponent<Image>();
            cbBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = cbBg;

            GameObject cbTextObj = new GameObject("Text");
            cbTextObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform cbtRect = cbTextObj.AddComponent<RectTransform>();
            StretchFull(cbtRect);

            TextMeshProUGUI cbTmp = cbTextObj.AddComponent<TextMeshProUGUI>();
            cbTmp.text = "CLOSE";
            cbTmp.fontSize = 20;
            cbTmp.alignment = TextAlignmentOptions.Center;
            cbTmp.color = Color.white;

            SerializedObject so = new SerializedObject(modal);
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.ApplyModifiedProperties();

            return modalObj;
        }

        private static List<Sprite> LoadSpritesFromFolder(string folderPath)
        {
            List<Sprite> sprites = new List<Sprite>();
            if (!Directory.Exists(folderPath)) return sprites;

            string[] files = Directory.GetFiles(folderPath, "*.png");
            System.Array.Sort(files);

            foreach (string file in files)
            {
                string relPath = file.Replace("\\", "/");
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(relPath);
                if (s != null) sprites.Add(s);
            }

            return sprites;
        }

        private static void AssignFramesToPlayer(FrameAnimationPlayer player, List<Sprite> sprites, float fps, bool loop)
        {
            if (player == null || sprites == null) return;
            SerializedObject so = new SerializedObject(player);
            SerializedProperty framesProp = so.FindProperty("frames");
            framesProp.ClearArray();
            for (int i = 0; i < sprites.Count; i++)
            {
                framesProp.InsertArrayElementAtIndex(i);
                framesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
            so.FindProperty("framesPerSecond").floatValue = fps;
            so.FindProperty("loop").boolValue = loop;
            so.ApplyModifiedProperties();
        }

        private static CharacterCreationScreen BuildCharacterCreationScreen(Transform parent)
        {
            GameObject obj = new GameObject("CharacterCreationScreen");
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            StretchFull(rect);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.03f, 0.03f, 1f);

            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();

            CharacterCreationScreen screen = obj.AddComponent<CharacterCreationScreen>();
            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("canvasGroup").objectReferenceValue = cg;

            AudioClip creationMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/creation_theme.mp3");
            AudioClip tickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/tick.mp3");
            if (creationMusic != null) so.FindProperty("creationMusicClip").objectReferenceValue = creationMusic;
            if (tickSfx != null) so.FindProperty("clockTickClip").objectReferenceValue = tickSfx;

            GameObject panelsRoot = new GameObject("Panels");
            panelsRoot.transform.SetParent(obj.transform, false);
            RectTransform prRect = panelsRoot.AddComponent<RectTransform>();
            StretchFull(prRect);

            // Sub-panels
            GameObject genderPanel = BuildGenderPanel(panelsRoot.transform, screen, so);
            so.FindProperty("genderPanel").objectReferenceValue = genderPanel;

            GameObject firstNamePanel = BuildFirstNamePanel(panelsRoot.transform, screen, so);
            so.FindProperty("firstNamePanel").objectReferenceValue = firstNamePanel;

            GameObject lastNamePanel = BuildLastNamePanel(panelsRoot.transform, screen, so);
            so.FindProperty("lastNamePanel").objectReferenceValue = lastNamePanel;

            GameObject jobPanel = BuildJobSelectionPanel(panelsRoot.transform, screen);
            so.FindProperty("jobSelectionPanel").objectReferenceValue = jobPanel;

            GameObject trainingPanel = BuildStudentTrainingPanel(panelsRoot.transform, screen, so);
            so.FindProperty("studentTrainingPanel").objectReferenceValue = trainingPanel;

            GameObject majorPanel = BuildStudentMajorPanel(panelsRoot.transform, screen);
            so.FindProperty("studentMajorPanel").objectReferenceValue = majorPanel;

            GameObject minorPanel = BuildStudentMinorPanel(panelsRoot.transform, screen);
            so.FindProperty("studentMinorPanel").objectReferenceValue = minorPanel;

            GameObject hobbyPanel = BuildHobbySelectionPanel(panelsRoot.transform, screen, so);
            so.FindProperty("hobbySelectionPanel").objectReferenceValue = hobbyPanel;

            GameObject summaryPanel = BuildSummaryDossierPanel(panelsRoot.transform, screen, so);
            so.FindProperty("summaryDossierPanel").objectReferenceValue = summaryPanel;

            so.ApplyModifiedProperties();

            obj.SetActive(false);
            return screen;
        }

        private static GameObject CreatePanelContainer(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            StretchFull(rect);
            panel.SetActive(false);
            return panel;
        }

        private static void CreateHeader(Transform parent, string title, string subtitle)
        {
            GameObject container = new GameObject("Header");
            container.transform.SetParent(parent, false);
            RectTransform cRect = container.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(0.5f, 1f);
            cRect.sizeDelta = new Vector2(0, 120);
            cRect.anchoredPosition = new Vector2(0, -30);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(container.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.sizeDelta = new Vector2(0, 50);
            tRect.anchoredPosition = new Vector2(0, 0);

            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = 32;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.83f, 0.33f, 0f);
            titleTmp.characterSpacing = 8;

            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(container.transform, false);
            RectTransform sRect = subObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 1f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.sizeDelta = new Vector2(0, 40);
            sRect.anchoredPosition = new Vector2(0, -50);

            TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
            subTmp.text = subtitle;
            subTmp.fontSize = 18;
            subTmp.fontStyle = FontStyles.Italic;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.6f, 0.6f, 0.6f);
        }

        private static GameObject BuildGenderPanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "GenderPanel");
            CreateHeader(panel.transform, "IDENTITY: GENDER", "\"The face staring back at you in the darkened glass...\"");

            GameObject btnsContainer = new GameObject("GenderButtons");
            btnsContainer.transform.SetParent(panel.transform, false);
            RectTransform bRect = btnsContainer.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.sizeDelta = new Vector2(700, 60);
            bRect.anchoredPosition = new Vector2(0, 30);
            HorizontalLayoutGroup hlg = btnsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            CreateStandardButton(btnsContainer.transform, "MALE", () => screen.SelectGender("Male"), new Vector2(200, 50));
            CreateStandardButton(btnsContainer.transform, "FEMALE", () => screen.SelectGender("Female"), new Vector2(200, 50));
            CreateStandardButton(btnsContainer.transform, "NON-BINARY", () => screen.SelectGender("Non-Binary"), new Vector2(200, 50));

            CreateCustomInputRow(panel.transform, "CustomGenderRow", "Or define your own:", "Survivor", 
                () => screen.ConfirmCustomGender(), out TMP_InputField inputField);
            so.FindProperty("customGenderInput").objectReferenceValue = inputField;

            return panel;
        }

        private static GameObject BuildFirstNamePanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "FirstNamePanel");
            CreateHeader(panel.transform, "IDENTITY: FIRST NAME", "\"What was your given name before the world fell silent?\"");

            // Male Names
            GameObject maleBox = CreateButtonsGrid(panel.transform, "MaleNames", 3);
            string[] maleNames = { "James", "Michael", "Robert", "William", "David", "Neal" };
            foreach (var n in maleNames) CreateStandardButton(maleBox.transform, n, () => screen.SelectFirstName(n), new Vector2(160, 42));
            so.FindProperty("maleNamesContainer").objectReferenceValue = maleBox.transform;

            // Female Names
            GameObject femaleBox = CreateButtonsGrid(panel.transform, "FemaleNames", 3);
            string[] femaleNames = { "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Nadia" };
            foreach (var n in femaleNames) CreateStandardButton(femaleBox.transform, n, () => screen.SelectFirstName(n), new Vector2(160, 42));
            so.FindProperty("femaleNamesContainer").objectReferenceValue = femaleBox.transform;

            // Non-Binary Names
            GameObject nbBox = CreateButtonsGrid(panel.transform, "NonBinaryNames", 3);
            string[] nbNames = { "Alex", "Jordan", "Taylor", "Quinn", "Casey", "Skyler" };
            foreach (var n in nbNames) CreateStandardButton(nbBox.transform, n, () => screen.SelectFirstName(n), new Vector2(160, 42));
            so.FindProperty("nonBinaryNamesContainer").objectReferenceValue = nbBox.transform;

            CreateCustomInputRow(panel.transform, "CustomFirstNameRow", "Or enter your own:", "Enter first name...", 
                () => screen.ConfirmCustomFirstName(), out TMP_InputField inputField);
            so.FindProperty("customFirstNameInput").objectReferenceValue = inputField;

            return panel;
        }

        private static GameObject BuildLastNamePanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "LastNamePanel");
            CreateHeader(panel.transform, "IDENTITY: LAST NAME", "\"A name to carry through the chaos, or to be forgotten in a shallow grave.\"");

            GameObject box = CreateButtonsGrid(panel.transform, "LastNameGrid", 3);
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Sage" };
            foreach (var n in lastNames) CreateStandardButton(box.transform, n, () => screen.SelectLastName(n), new Vector2(160, 42));

            CreateCustomInputRow(panel.transform, "CustomLastNameRow", "Or enter your own:", "Enter last name...", 
                () => screen.ConfirmCustomLastName(), out TMP_InputField inputField);
            so.FindProperty("customLastNameInput").objectReferenceValue = inputField;

            return panel;
        }

        private static GameObject BuildJobSelectionPanel(Transform parent, CharacterCreationScreen screen)
        {
            GameObject panel = CreatePanelContainer(parent, "JobSelectionPanel");
            CreateHeader(panel.transform, "SURVIVOR IDENTITY", "\"What was your life before the screaming started?\"");

            // Vertical ScrollView with 3-column Grid (matching Twine layout)
            GameObject scrollObj = CreateScrollView(panel.transform, "JobScrollView", new Vector2(1540, 830), true, out Transform content);
            RectTransform sRect = scrollObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0, -40);

            GridLayoutGroup glg = content.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(470, 500);
            glg.spacing = new Vector2(24, 24);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.padding = new RectOffset(10, 10, 10, 30);
            glg.childAlignment = TextAnchor.UpperCenter;

            foreach (var job in CharacterCreationDefinitions.Jobs)
            {
                CreateJobCard(content, job, () => screen.SelectJob(job.id));
            }

            return panel;
        }

        private static GameObject BuildStudentTrainingPanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "StudentTrainingPanel");
            CreateHeader(panel.transform, "PRE-APOCALYPSE TRAINING", "\"While others worked, you were preparing. How did you spend your free time?\"");

            GameObject pointsObj = new GameObject("PointsText");
            pointsObj.transform.SetParent(panel.transform, false);
            RectTransform ptRect = pointsObj.AddComponent<RectTransform>();
            ptRect.anchorMin = new Vector2(0.5f, 0.5f);
            ptRect.anchorMax = new Vector2(0.5f, 0.5f);
            ptRect.sizeDelta = new Vector2(600, 40);
            ptRect.anchoredPosition = new Vector2(0, 140);
            TextMeshProUGUI pointsTmp = pointsObj.AddComponent<TextMeshProUGUI>();
            pointsTmp.fontSize = 24;
            pointsTmp.fontStyle = FontStyles.Bold;
            pointsTmp.alignment = TextAlignmentOptions.Center;
            pointsTmp.color = new Color(0.83f, 0.33f, 0f);
            so.FindProperty("studentPointsText").objectReferenceValue = pointsTmp;

            GameObject rowsBox = new GameObject("AttributesRows");
            rowsBox.transform.SetParent(panel.transform, false);
            RectTransform rRect = rowsBox.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.sizeDelta = new Vector2(500, 240);
            rRect.anchoredPosition = new Vector2(0, -10);
            VerticalLayoutGroup vlg = rowsBox.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            TextMeshProUGUI strTmp = CreateTrainingRow(rowsBox.transform, "STRENGTH", () => screen.AddStudentPoint("str"));
            TextMeshProUGUI intTmp = CreateTrainingRow(rowsBox.transform, "INTELLIGENCE", () => screen.AddStudentPoint("int"));
            TextMeshProUGUI agiTmp = CreateTrainingRow(rowsBox.transform, "AGILITY", () => screen.AddStudentPoint("agi"));
            TextMeshProUGUI chaTmp = CreateTrainingRow(rowsBox.transform, "CHARISMA", () => screen.AddStudentPoint("cha"));
            TextMeshProUGUI perTmp = CreateTrainingRow(rowsBox.transform, "PERCEPTION", () => screen.AddStudentPoint("per"));

            so.FindProperty("studentStrText").objectReferenceValue = strTmp;
            so.FindProperty("studentIntText").objectReferenceValue = intTmp;
            so.FindProperty("studentAgiText").objectReferenceValue = agiTmp;
            so.FindProperty("studentChaText").objectReferenceValue = chaTmp;
            so.FindProperty("studentPerText").objectReferenceValue = perTmp;

            GameObject warnObj = new GameObject("WarningText");
            warnObj.transform.SetParent(panel.transform, false);
            RectTransform wRect = warnObj.AddComponent<RectTransform>();
            wRect.anchorMin = new Vector2(0.5f, 0.5f);
            wRect.anchorMax = new Vector2(0.5f, 0.5f);
            wRect.sizeDelta = new Vector2(500, 30);
            wRect.anchoredPosition = new Vector2(0, -145);
            TextMeshProUGUI warnTmp = warnObj.AddComponent<TextMeshProUGUI>();
            warnTmp.text = "[!] SPEND ALL POINTS TO CONTINUE";
            warnTmp.fontSize = 18;
            warnTmp.fontStyle = FontStyles.Bold;
            warnTmp.alignment = TextAlignmentOptions.Center;
            warnTmp.color = new Color(0.8f, 0.2f, 0.2f);
            warnTmp.raycastTarget = false;
            so.FindProperty("studentWarningText").objectReferenceValue = warnTmp;

            Button continueBtn = CreateStandardButton(panel.transform, "CONTINUE TO UNIVERSITY MAJOR", () => screen.ConfirmStudentTraining(), new Vector2(380, 50));
            RectTransform cRect = continueBtn.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.anchoredPosition = new Vector2(0, -200);
            so.FindProperty("studentContinueButton").objectReferenceValue = continueBtn;

            return panel;
        }

        private static GameObject BuildStudentMajorPanel(Transform parent, CharacterCreationScreen screen)
        {
            GameObject panel = CreatePanelContainer(parent, "StudentMajorPanel");
            CreateHeader(panel.transform, "UNIVERSITY TRANSCRIPT: MAJOR", "\"Before the sirens, you were preparing for a career. What was your focus?\"");

            GameObject grid = CreateButtonsGrid(panel.transform, "MajorGrid", 3);
            CreateStandardButton(grid.transform, "ROTC\n<size=14><color=#aaa>Military Science (+3 Shoot)</color></size>", () => screen.SelectStudentMajor("shooting"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "SPORTS\n<size=14><color=#aaa>Combat Sports (+3 Melee)</color></size>", () => screen.SelectStudentMajor("melee"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "TRACK\n<size=14><color=#aaa>Outdoor Survival (+3 Stealth)</color></size>", () => screen.SelectStudentMajor("stealth"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "CS & IT\n<size=14><color=#aaa>Hardware Engineering (+3 Tech)</color></size>", () => screen.SelectStudentMajor("tech"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "BIOLOGY\n<size=14><color=#aaa>Pre-med Sciences (+3 Medicine)</color></size>", () => screen.SelectStudentMajor("medicine"), new Vector2(240, 70));

            return panel;
        }

        private static GameObject BuildStudentMinorPanel(Transform parent, CharacterCreationScreen screen)
        {
            GameObject panel = CreatePanelContainer(parent, "StudentMinorPanel");
            CreateHeader(panel.transform, "UNIVERSITY TRANSCRIPT: MINOR", "\"And what did you study on the side?\"");

            GameObject grid = CreateButtonsGrid(panel.transform, "MinorGrid", 3);
            CreateStandardButton(grid.transform, "SHOOTING\n<size=14><color=#aaa>Target Practice (+2 Shoot)</color></size>", () => screen.SelectStudentMinor("shooting"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "MELEE\n<size=14><color=#aaa>Self-defense Combat (+2 Melee)</color></size>", () => screen.SelectStudentMinor("melee"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "STEALTH\n<size=14><color=#aaa>Moving Unseen (+2 Stealth)</color></size>", () => screen.SelectStudentMinor("stealth"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "TECH\n<size=14><color=#aaa>Hardware Repair (+2 Tech)</color></size>", () => screen.SelectStudentMinor("tech"), new Vector2(240, 70));
            CreateStandardButton(grid.transform, "MEDICINE\n<size=14><color=#aaa>First Aid Protocols (+2 Medicine)</color></size>", () => screen.SelectStudentMinor("medicine"), new Vector2(240, 70));

            return panel;
        }

        private static GameObject BuildHobbySelectionPanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "HobbySelectionPanel");
            
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panel.transform, false);
            RectTransform hRect = headerObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(0, 90);
            hRect.anchoredPosition = new Vector2(0, -20);

            TextMeshProUGUI hTmp = headerObj.AddComponent<TextMeshProUGUI>();
            hTmp.text = "PRIMARY HOBBY";
            hTmp.fontSize = 28;
            hTmp.fontStyle = FontStyles.Bold;
            hTmp.alignment = TextAlignmentOptions.Center;
            hTmp.color = Color.white;
            hTmp.characterSpacing = 6;
            so.FindProperty("hobbyHeaderTitle").objectReferenceValue = hTmp;

            // Scroll View with Grid
            GameObject scrollObj = CreateScrollView(panel.transform, "HobbyScrollView", new Vector2(1200, 600), true, out Transform content);
            GridLayoutGroup glg = content.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(260, 240);
            glg.spacing = new Vector2(20, 20);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            glg.padding = new RectOffset(20, 20, 20, 20);
            so.FindProperty("hobbyCardsContainer").objectReferenceValue = content;

            // Card Template (Prefab)
            GameObject template = BuildHobbyCardTemplate(panel.transform);
            template.SetActive(false);
            so.FindProperty("hobbyCardPrefab").objectReferenceValue = template;

            return panel;
        }

        private static GameObject BuildSummaryDossierPanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "SummaryDossierPanel");
            CreateHeader(panel.transform, "SURVIVOR DOSSIER: DAY ZERO", "\"The choices you made have defined you. The nightmare begins tomorrow.\"");

            GameObject box = new GameObject("DossierBox");
            box.transform.SetParent(panel.transform, false);
            RectTransform bRect = box.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.sizeDelta = new Vector2(900, 480);
            bRect.anchoredPosition = new Vector2(0, 20);

            Image bBg = box.AddComponent<Image>();
            bBg.color = new Color(0.07f, 0.07f, 0.07f, 0.95f);

            VerticalLayoutGroup vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 25, 25);
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            so.FindProperty("summaryNameText").objectReferenceValue = CreateDossierRow(box.transform, 30, FontStyles.Bold, new Color(0.83f, 0.33f, 0f));
            so.FindProperty("summaryIdentityText").objectReferenceValue = CreateDossierRow(box.transform, 18, FontStyles.Normal, Color.white);
            so.FindProperty("summaryVitalsText").objectReferenceValue = CreateDossierRow(box.transform, 18, FontStyles.Normal, new Color(0.36f, 0.55f, 0.71f));
            so.FindProperty("summaryAttributesText").objectReferenceValue = CreateDossierRow(box.transform, 18, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f));
            so.FindProperty("summarySkillsText").objectReferenceValue = CreateDossierRow(box.transform, 16, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));
            so.FindProperty("summaryTraitsText").objectReferenceValue = CreateDossierRow(box.transform, 15, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f));
            so.FindProperty("summaryInventoryText").objectReferenceValue = CreateDossierRow(box.transform, 15, FontStyles.Normal, new Color(0.6f, 0.6f, 0.6f));

            Button beginBtn = CreateStandardButton(panel.transform, "BEGIN SURVIVAL >>", () => screen.SendMessage("OnBeginSurvivalClicked", UnityEngine.SendMessageOptions.DontRequireReceiver), new Vector2(350, 55));
            RectTransform btnRect = beginBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0, -260);
            so.FindProperty("beginSurvivalButton").objectReferenceValue = beginBtn;

            return panel;
        }

        private static Button CreateStandardButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, Vector2 size, Color? textColor = null)
        {
            GameObject btnObj = new GameObject("Btn_" + label.Replace(" ", "").Replace("\n", ""));
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            StretchFull(tRect);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor ?? Color.white;
            tmp.raycastTarget = false;

            return btn;
        }

        private static GameObject CreateButtonsGrid(Transform parent, string name, int columns)
        {
            GameObject gridObj = new GameObject(name);
            gridObj.transform.SetParent(parent, false);
            RectTransform rect = gridObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600, 180);
            rect.anchoredPosition = new Vector2(0, 40);

            GridLayoutGroup glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(180, 48);
            glg.spacing = new Vector2(15, 15);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = columns;
            glg.childAlignment = TextAnchor.MiddleCenter;

            return gridObj;
        }

        private static GameObject CreateCustomInputRow(Transform parent, string name, string label, string placeholder, UnityEngine.Events.UnityAction onConfirm, out TMP_InputField inputField)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            RectTransform rRect = row.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.sizeDelta = new Vector2(600, 100);
            rRect.anchoredPosition = new Vector2(0, -140);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            RectTransform lRect = labelObj.AddComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0f, 1f);
            lRect.anchorMax = new Vector2(1f, 1f);
            lRect.sizeDelta = new Vector2(0, 30);
            lRect.anchoredPosition = Vector2.zero;
            TextMeshProUGUI lTmp = labelObj.AddComponent<TextMeshProUGUI>();
            lTmp.text = label;
            lTmp.fontSize = 16;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.color = new Color(0.6f, 0.6f, 0.6f);

            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            RectTransform iRect = inputObj.AddComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0.5f, 0f);
            iRect.anchorMax = new Vector2(0.5f, 0f);
            iRect.sizeDelta = new Vector2(300, 44);
            iRect.anchoredPosition = new Vector2(-80, 20);

            Image iBg = inputObj.AddComponent<Image>();
            iBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            inputField = inputObj.AddComponent<TMP_InputField>();

            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRect = textArea.AddComponent<RectTransform>();
            StretchFull(taRect);
            taRect.offsetMin = new Vector2(10, 0);
            taRect.offsetMax = new Vector2(-10, 0);
            textArea.AddComponent<RectMask2D>();

            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            RectTransform phRect = placeholderObj.AddComponent<RectTransform>();
            StretchFull(phRect);
            TextMeshProUGUI phText = placeholderObj.AddComponent<TextMeshProUGUI>();
            phText.text = placeholder;
            phText.fontSize = 18;
            phText.fontStyle = FontStyles.Italic;
            phText.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            StretchFull(tRect);
            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 18;
            textComp.color = Color.white;

            inputField.textViewport = taRect;
            inputField.textComponent = textComp;
            inputField.placeholder = phText;

            GameObject confirmBtnObj = new GameObject("ConfirmBtn");
            confirmBtnObj.transform.SetParent(row.transform, false);
            RectTransform cbRect = confirmBtnObj.AddComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.5f, 0f);
            cbRect.anchorMax = new Vector2(0.5f, 0f);
            cbRect.sizeDelta = new Vector2(140, 44);
            cbRect.anchoredPosition = new Vector2(150, 20);

            Image cbBg = confirmBtnObj.AddComponent<Image>();
            cbBg.color = new Color(0.83f, 0.33f, 0f);
            Button cBtn = confirmBtnObj.AddComponent<Button>();
            cBtn.targetGraphic = cbBg;
            if (onConfirm != null) cBtn.onClick.AddListener(onConfirm);

            GameObject cbTextObj = new GameObject("Text");
            cbTextObj.transform.SetParent(confirmBtnObj.transform, false);
            RectTransform cbtRect = cbTextObj.AddComponent<RectTransform>();
            StretchFull(cbtRect);
            TextMeshProUGUI cbTmp = cbTextObj.AddComponent<TextMeshProUGUI>();
            cbTmp.text = "CONFIRM";
            cbTmp.fontSize = 18;
            cbTmp.fontStyle = FontStyles.Bold;
            cbTmp.alignment = TextAlignmentOptions.Center;
            cbTmp.color = Color.white;
            cbTmp.raycastTarget = false;

            return row;
        }

        private static GameObject CreateScrollView(Transform parent, string name, Vector2 size, bool isVertical, out Transform content)
        {
            GameObject scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);
            RectTransform sRect = scrollObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);
            sRect.sizeDelta = size;
            sRect.anchoredPosition = new Vector2(0, -20);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = !isVertical;
            sr.vertical = isVertical;
            sr.scrollSensitivity = 30f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            RectTransform vRect = viewport.AddComponent<RectTransform>();
            StretchFull(vRect);
            viewport.AddComponent<RectMask2D>();

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewport.transform, false);
            RectTransform cRect = contentObj.AddComponent<RectTransform>();

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            if (isVertical)
            {
                cRect.anchorMin = new Vector2(0f, 1f);
                cRect.anchorMax = new Vector2(1f, 1f);
                cRect.pivot = new Vector2(0.5f, 1f);
                cRect.sizeDelta = new Vector2(0, 0);
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                cRect.anchorMin = new Vector2(0f, 0f);
                cRect.anchorMax = new Vector2(0f, 1f);
                cRect.pivot = new Vector2(0f, 0.5f);
                cRect.sizeDelta = new Vector2(0, 0);
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            sr.viewport = vRect;
            sr.content = cRect;
            content = contentObj.transform;

            return scrollObj;
        }

        private static Sprite _cachedGradientSprite;
        private static Sprite GetBottomFadeGradient()
        {
            if (_cachedGradientSprite != null) return _cachedGradientSprite;

            Texture2D tex = new Texture2D(1, 32, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++)
            {
                float a = (float)y / 31f; // 0 at top to 1 at bottom
                tex.SetPixel(0, 31 - y, new Color(0.031f, 0.031f, 0.031f, a));
            }
            tex.Apply();
            _cachedGradientSprite = Sprite.Create(tex, new Rect(0, 0, 1, 32), new Vector2(0.5f, 0.5f));
            return _cachedGradientSprite;
        }

        private static string GetJobPerkSummary(JobData job)
        {
            switch (job.id.ToLower())
            {
                case "police": return "Steady Hands & Authority";
                case "worker": return "Breacher & Iron Grip";
                case "thief": return "Locksmith & Ghost";
                case "doctor": return "Field Medic & Anatomist";
                case "manager": return "Silver Tongue & Inspiration";
                case "student": return "Fast Learner & Adrenaline";
                default:
                    if (job.perks != null && job.perks.Length >= 2) return $"{job.perks[0].name} & {job.perks[1].name}";
                    if (job.perks != null && job.perks.Length == 1) return job.perks[0].name;
                    return "None";
            }
        }

        private static string GetJobStartSummary(JobData job)
        {
            switch (job.id.ToLower())
            {
                case "police": return "Glock 19, Ammo x24";
                case "worker": return "Sledgehammer, Duct Tape";
                case "thief": return "Lockpicks, Knife, Light";
                case "doctor": return "Medkit, Antibiotics";
                case "manager": return "Luxury Watch, Golf Club";
                case "student": return "Baseball Bat, Textbook";
                default:
                    if (job.startingItems != null && job.startingItems.Length > 0)
                    {
                        List<string> items = new List<string>();
                        foreach (var itm in job.startingItems) items.Add(itm.item);
                        return string.Join(", ", items);
                    }
                    return "None";
            }
        }

        private static string GetSelectButtonText(JobData job)
        {
            switch (job.id.ToLower())
            {
                case "police": return "SELECT POLICE OFFICER";
                case "worker": return "SELECT WORKER";
                case "thief": return "SELECT BURGLAR";
                case "doctor": return "SELECT DOCTOR";
                case "manager": return "SELECT MANAGER";
                case "student": return "SELECT STUDENT";
                default: return "SELECT " + job.title;
            }
        }

        private static void CreateJobCard(Transform parent, JobData job, UnityEngine.Events.UnityAction onSelect)
        {
            GameObject cardObj = new GameObject("JobCard_" + job.id);
            cardObj.transform.SetParent(parent, false);
            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(470, 500);

            // Card border outline (1px dark border)
            Image cardBorder = cardObj.AddComponent<Image>();
            cardBorder.color = new Color(0.14f, 0.14f, 0.14f, 1f);

            // Inner dark panel (#080808)
            GameObject innerCard = new GameObject("InnerCard");
            innerCard.transform.SetParent(cardObj.transform, false);
            RectTransform innerRect = innerCard.AddComponent<RectTransform>();
            StretchFull(innerRect);
            innerRect.offsetMin = new Vector2(1, 1);
            innerRect.offsetMax = new Vector2(-1, -1);
            Image innerBg = innerCard.AddComponent<Image>();
            innerBg.color = new Color(0.031f, 0.031f, 0.031f, 1f);

            // Top accent bar (3px line with job.accentColor)
            GameObject topBar = new GameObject("TopBorder");
            topBar.transform.SetParent(innerCard.transform, false);
            RectTransform tbRect = topBar.AddComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(0, 3);
            tbRect.anchoredPosition = Vector2.zero;
            topBar.AddComponent<Image>().color = job.accentColor;

            // Banner Image Container (Masked)
            GameObject bannerObj = new GameObject("Banner");
            bannerObj.transform.SetParent(innerCard.transform, false);
            RectTransform bRect = bannerObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 1f);
            bRect.anchorMax = new Vector2(1f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.sizeDelta = new Vector2(0, 225);
            bRect.anchoredPosition = new Vector2(0, -3);
            bannerObj.AddComponent<RectMask2D>();

            // Illustration image
            GameObject imgObj = new GameObject("JobImage");
            imgObj.transform.SetParent(bannerObj.transform, false);
            RectTransform imgRect = imgObj.AddComponent<RectTransform>();
            StretchFull(imgRect);
            Image pImg = imgObj.AddComponent<Image>();
            pImg.raycastTarget = false;

            Sprite jobSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Jobs/job_{job.id}.png");
            if (jobSprite == null && job.id == "manager")
            {
                jobSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Jobs/job_sales.png");
            }
            if (jobSprite != null)
            {
                pImg.sprite = jobSprite;
                pImg.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            }
            else
            {
                pImg.color = Color.clear;
            }

            // Dark bottom gradient overlay
            GameObject gradObj = new GameObject("FadeOverlay");
            gradObj.transform.SetParent(bannerObj.transform, false);
            RectTransform gRect = gradObj.AddComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0f);
            gRect.anchorMax = new Vector2(1f, 0f);
            gRect.pivot = new Vector2(0.5f, 0f);
            gRect.sizeDelta = new Vector2(0, 110);
            gRect.anchoredPosition = Vector2.zero;
            Image gradImg = gradObj.AddComponent<Image>();
            gradImg.sprite = GetBottomFadeGradient();
            gradImg.raycastTarget = false;

            // Title
            string displayTitle = job.id.Equals("worker", System.StringComparison.OrdinalIgnoreCase) ? "CONSTRUCTION" : job.title;
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(innerCard.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.sizeDelta = new Vector2(430, 36);
            tRect.anchoredPosition = new Vector2(0, -235);
            TextMeshProUGUI tTmp = titleObj.AddComponent<TextMeshProUGUI>();
            tTmp.text = displayTitle;
            tTmp.fontSize = 24;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.characterSpacing = 2.5f;
            tTmp.alignment = TextAlignmentOptions.Left;
            tTmp.color = job.accentColor;
            tTmp.raycastTarget = false;

            // Description & Quote
            string cleanQuote = job.quote.Trim('\"');
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(innerCard.transform, false);
            RectTransform dRect = descObj.AddComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 1f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.pivot = new Vector2(0.5f, 1f);
            dRect.sizeDelta = new Vector2(430, 68);
            dRect.anchoredPosition = new Vector2(0, -274);
            TextMeshProUGUI dTmp = descObj.AddComponent<TextMeshProUGUI>();
            dTmp.text = $"{job.description}\n<color=#888888><i>\"{cleanQuote}\"</i></color>";
            dTmp.fontSize = 12.5f;
            dTmp.lineSpacing = 2.5f;
            dTmp.alignment = TextAlignmentOptions.Left;
            dTmp.color = new Color(0.75f, 0.73f, 0.73f);
            dTmp.textWrappingMode = TextWrappingModes.Normal;
            dTmp.overflowMode = TextOverflowModes.Ellipsis;
            dTmp.raycastTarget = false;

            // Stats / Perks Box (Semi-transparent with colored left stripe)
            GameObject statsObj = new GameObject("StatsBox");
            statsObj.transform.SetParent(innerCard.transform, false);
            RectTransform stRect = statsObj.AddComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0f, 1f);
            stRect.anchorMax = new Vector2(1f, 1f);
            stRect.pivot = new Vector2(0.5f, 1f);
            stRect.sizeDelta = new Vector2(430, 60);
            stRect.anchoredPosition = new Vector2(0, -352);

            Image stBg = statsObj.AddComponent<Image>();
            stBg.color = new Color(0f, 0f, 0f, 0.45f);

            // Left vertical accent stripe
            GameObject stripeObj = new GameObject("LeftStripe");
            stripeObj.transform.SetParent(statsObj.transform, false);
            RectTransform sRect = stripeObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0f);
            sRect.anchorMax = new Vector2(0f, 1f);
            sRect.pivot = new Vector2(0f, 0.5f);
            sRect.sizeDelta = new Vector2(3, 0);
            sRect.anchoredPosition = Vector2.zero;
            stripeObj.AddComponent<Image>().color = job.accentColor;

            // Stats Text inside
            GameObject sTextObj = new GameObject("Text");
            sTextObj.transform.SetParent(statsObj.transform, false);
            RectTransform stTextRect = sTextObj.AddComponent<RectTransform>();
            StretchFull(stTextRect);
            stTextRect.offsetMin = new Vector2(14, 4);
            stTextRect.offsetMax = new Vector2(-10, -4);
            TextMeshProUGUI stTmp = sTextObj.AddComponent<TextMeshProUGUI>();
            stTmp.text = $"<color=#FFFFFF><b>PERKS:</b></color> {GetJobPerkSummary(job)}\n<color=#FFFFFF><b>START:</b></color> {GetJobStartSummary(job)}";
            stTmp.fontSize = 12.5f;
            stTmp.lineSpacing = 3f;
            stTmp.alignment = TextAlignmentOptions.Left;
            stTmp.color = new Color(0.68f, 0.68f, 0.68f);
            stTmp.raycastTarget = false;

            // Select Button
            GameObject btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(innerCard.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.sizeDelta = new Vector2(430, 46);
            btnRect.anchoredPosition = new Vector2(0, 16);

            Image bBg = btnObj.AddComponent<Image>();
            bBg.color = new Color(0.09f, 0.09f, 0.09f, 0.98f);
            Button bBtn = btnObj.AddComponent<Button>();
            bBtn.targetGraphic = bBg;

            ColorBlock cb = bBtn.colors;
            cb.normalColor = new Color(0.09f, 0.09f, 0.09f, 0.98f);
            cb.highlightedColor = Color.Lerp(new Color(0.18f, 0.18f, 0.18f, 1f), job.accentColor, 0.40f);
            cb.pressedColor = job.accentColor * 0.75f;
            cb.selectedColor = new Color(0.09f, 0.09f, 0.09f, 0.98f);
            bBtn.colors = cb;

            if (onSelect != null) bBtn.onClick.AddListener(onSelect);

            GameObject btObj = new GameObject("Text");
            btObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btObj.AddComponent<RectTransform>();
            StretchFull(btRect);
            TextMeshProUGUI btTmp = btObj.AddComponent<TextMeshProUGUI>();
            btTmp.text = GetSelectButtonText(job);
            btTmp.fontSize = 14f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.characterSpacing = 2.5f;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;
            btTmp.raycastTarget = false;
        }

        private static GameObject BuildHobbyCardTemplate(Transform parent)
        {
            GameObject cardObj = new GameObject("HobbyCardTemplate");
            cardObj.transform.SetParent(parent, false);
            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260, 240);

            Image borderImg = cardObj.AddComponent<Image>();
            borderImg.color = new Color(0.83f, 0.33f, 0f);

            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(cardObj.transform, false);
            RectTransform inRect = innerObj.AddComponent<RectTransform>();
            StretchFull(inRect);
            inRect.offsetMin = new Vector2(2, 2);
            inRect.offsetMax = new Vector2(-2, -2);
            innerObj.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.98f);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(innerObj.transform, false);
            RectTransform icRect = iconObj.AddComponent<RectTransform>();
            icRect.anchorMin = new Vector2(0.5f, 1f);
            icRect.anchorMax = new Vector2(0.5f, 1f);
            icRect.pivot = new Vector2(0.5f, 1f);
            icRect.sizeDelta = new Vector2(50, 50);
            icRect.anchoredPosition = new Vector2(0, -10);
            Image icImg = iconObj.AddComponent<Image>();
            icImg.preserveAspect = true;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(innerObj.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.sizeDelta = new Vector2(0, 26);
            tRect.anchoredPosition = new Vector2(0, -65);
            TextMeshProUGUI tTmp = titleObj.AddComponent<TextMeshProUGUI>();
            tTmp.fontSize = 16;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.raycastTarget = false;

            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(innerObj.transform, false);
            RectTransform dRect = descObj.AddComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 1f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.sizeDelta = new Vector2(230, 50);
            dRect.anchoredPosition = new Vector2(0, -95);
            TextMeshProUGUI dTmp = descObj.AddComponent<TextMeshProUGUI>();
            dTmp.fontSize = 12;
            dTmp.alignment = TextAlignmentOptions.Center;
            dTmp.color = new Color(0.7f, 0.7f, 0.7f);
            dTmp.raycastTarget = false;

            GameObject stObj = new GameObject("StatsPreview");
            stObj.transform.SetParent(innerObj.transform, false);
            RectTransform stRect = stObj.AddComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0f, 1f);
            stRect.anchorMax = new Vector2(1f, 1f);
            stRect.sizeDelta = new Vector2(0, 24);
            stRect.anchoredPosition = new Vector2(0, -145);
            TextMeshProUGUI stTmp = stObj.AddComponent<TextMeshProUGUI>();
            stTmp.fontSize = 13;
            stTmp.fontStyle = FontStyles.Bold;
            stTmp.alignment = TextAlignmentOptions.Center;
            stTmp.raycastTarget = false;

            GameObject btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(innerObj.transform, false);
            RectTransform bRect = btnObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0f);
            bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.sizeDelta = new Vector2(180, 36);
            bRect.anchoredPosition = new Vector2(0, 15);
            Image bBg = btnObj.AddComponent<Image>();
            bBg.color = new Color(0.18f, 0.18f, 0.18f);
            Button bBtn = btnObj.AddComponent<Button>();
            bBtn.targetGraphic = bBg;

            GameObject btObj = new GameObject("Text");
            btObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btObj.AddComponent<RectTransform>();
            StretchFull(btRect);
            TextMeshProUGUI btTmp = btObj.AddComponent<TextMeshProUGUI>();
            btTmp.text = "SELECT";
            btTmp.fontSize = 16;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;
            btTmp.raycastTarget = false;

            HobbyCardView cardView = cardObj.AddComponent<HobbyCardView>();
            SerializedObject cso = new SerializedObject(cardView);
            cso.FindProperty("cardBorder").objectReferenceValue = borderImg;
            cso.FindProperty("iconImage").objectReferenceValue = icImg;
            cso.FindProperty("titleText").objectReferenceValue = tTmp;
            cso.FindProperty("descriptionText").objectReferenceValue = dTmp;
            cso.FindProperty("statsPreviewText").objectReferenceValue = stTmp;
            cso.FindProperty("selectButton").objectReferenceValue = bBtn;
            cso.ApplyModifiedProperties();

            return cardObj;
        }

        private static TextMeshProUGUI CreateTrainingRow(Transform parent, string label, UnityEngine.Events.UnityAction onAdd)
        {
            GameObject row = new GameObject("Row_" + label);
            row.transform.SetParent(parent, false);
            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(480, 40);

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(row.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(0.7f, 0.5f);
            tRect.anchoredPosition = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{label}: 2";
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.36f, 0.55f, 0.71f);

            GameObject btnObj = new GameObject("Btn_Plus");
            btnObj.transform.SetParent(row.transform, false);
            RectTransform bRect = btnObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.85f, 0.5f);
            bRect.anchorMax = new Vector2(1f, 0.5f);
            bRect.sizeDelta = new Vector2(60, 32);
            bRect.anchoredPosition = Vector2.zero;
            Image bBg = btnObj.AddComponent<Image>();
            bBg.color = new Color(0.18f, 0.18f, 0.18f);
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bBg;
            if (onAdd != null) btn.onClick.AddListener(onAdd);

            GameObject btObj = new GameObject("Text");
            btObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btObj.AddComponent<RectTransform>();
            StretchFull(btRect);
            TextMeshProUGUI btTmp = btObj.AddComponent<TextMeshProUGUI>();
            btTmp.text = "+1";
            btTmp.fontSize = 16;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;

            return tmp;
        }

        private static TextMeshProUGUI CreateDossierRow(Transform parent, int fontSize, FontStyles style, Color color)
        {
            GameObject obj = new GameObject("Row");
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            return tmp;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
