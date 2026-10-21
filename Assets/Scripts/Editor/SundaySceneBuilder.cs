#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
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

                string flagPath = "Temp/scene_v17_auto.flag";
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
            PrologueWakeUpScreen prologueScreen = BuildPrologueWakeUpScreen(screensObj.transform);
            CharacterCreationScreen creationScreen = BuildCharacterCreationScreen(screensObj.transform);
            SundayTypography.ApplyHierarchy(screensObj.transform);

            // 7. GameManager
            GameObject gameMgrObj = new GameObject("GameManager");
            GameManager gameMgr = gameMgrObj.AddComponent<GameManager>();

            SerializedObject gameSo = new SerializedObject(gameMgr);
            gameSo.FindProperty("studioSplashScreen").objectReferenceValue = splashScreen;
            gameSo.FindProperty("disclaimerScreen").objectReferenceValue = disclaimerScreen;
            gameSo.FindProperty("titleScreen").objectReferenceValue = titleScreen;
            gameSo.FindProperty("prologueWakeUpScreen").objectReferenceValue = prologueScreen;
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

                private static PrologueWakeUpScreen BuildPrologueWakeUpScreen(Transform parent)
        {
            GameObject obj = new GameObject("PrologueWakeUpScreen");
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            StretchFull(rect);

            PrologueWakeUpScreen screen = obj.AddComponent<PrologueWakeUpScreen>();

            // Fullscreen Dark Ambience Backdrop (Base floor)
            GameObject bgObj = new GameObject("DarkBackdrop");
            bgObj.transform.SetParent(obj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            StretchFull(bgRect);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.07f, 1f);
            bgImg.raycastTarget = false;

            // =========================================================================
            // TOP SCREEN (3DS TOP VISUAL STAGE: 60% HEIGHT - Anchors Y: 0.40f to 1.0f)
            // Stretches 100% edge-to-edge across the 1920x1080 display.
            // =========================================================================
            GameObject topScreenObj = new GameObject("TopVisualStage");
            topScreenObj.transform.SetParent(obj.transform, false);
            RectTransform topRect = topScreenObj.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 0.40f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.offsetMin = Vector2.zero;
            topRect.offsetMax = Vector2.zero;

            Image topBg = topScreenObj.AddComponent<Image>();
            topBg.color = new Color(0.03f, 0.04f, 0.06f, 1f);
            topBg.raycastTarget = false;

            // Top Header Bar (Tactical Status Rail)
            GameObject topHeader = new GameObject("TopStatusRail");
            topHeader.transform.SetParent(topScreenObj.transform, false);
            RectTransform thRect = topHeader.AddComponent<RectTransform>();
            thRect.anchorMin = new Vector2(0f, 1f);
            thRect.anchorMax = new Vector2(1f, 1f);
            thRect.pivot = new Vector2(0.5f, 1f);
            thRect.sizeDelta = new Vector2(0f, 36f);
            thRect.anchoredPosition = Vector2.zero;
            Image thBg = topHeader.AddComponent<Image>();
            thBg.color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
            thBg.raycastTarget = false;

            GameObject thTitle = new GameObject("Title");
            thTitle.transform.SetParent(topHeader.transform, false);
            RectTransform thtRect = thTitle.AddComponent<RectTransform>();
            thtRect.anchorMin = new Vector2(0f, 0f);
            thtRect.anchorMax = new Vector2(1f, 1f);
            thtRect.offsetMin = new Vector2(24f, 0f);
            thtRect.offsetMax = new Vector2(-24f, 0f);
            TextMeshProUGUI thTmp = thTitle.AddComponent<TextMeshProUGUI>();
            thTmp.fontSize = 12f;
            thTmp.alignment = TextAlignmentOptions.MidlineLeft;
            thTmp.text = "<b>■ UNIT 404 // LIVING ROOM INTERIOR</b>";
            thTmp.color = new Color(0.65f, 0.75f, 0.88f);
            thTmp.raycastTarget = false;

            GameObject thStatus = new GameObject("Status");
            thStatus.transform.SetParent(topHeader.transform, false);
            RectTransform thsRect = thStatus.AddComponent<RectTransform>();
            thsRect.anchorMin = new Vector2(0f, 0f);
            thsRect.anchorMax = new Vector2(1f, 1f);
            thsRect.offsetMin = new Vector2(24f, 0f);
            thsRect.offsetMax = new Vector2(-24f, 0f);
            TextMeshProUGUI thsTmp = thStatus.AddComponent<TextMeshProUGUI>();
            thsTmp.fontSize = 12f;
            thsTmp.alignment = TextAlignmentOptions.MidlineRight;
            thsTmp.text = "<color=#FFA726>SATURDAY NIGHT</color>  |  23:45";
            thsTmp.color = new Color(0.80f, 0.88f, 0.95f);
            thsTmp.raycastTarget = false;

            // Visual Stage Viewport (Inset below status rail)
            GameObject vpObj = new GameObject("StageViewport");
            vpObj.transform.SetParent(topScreenObj.transform, false);
            RectTransform vpRect = vpObj.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = new Vector2(0f, 0f);
            vpRect.offsetMax = new Vector2(0f, -36f);

            // Layer 1: Living Room Background (Animated GIF frames / Artwork)
            GameObject roomObj = new GameObject("RoomBackdrop");
            roomObj.transform.SetParent(vpObj.transform, false);
            RectTransform roomRect = roomObj.AddComponent<RectTransform>();
            StretchFull(roomRect);
            Image roomImg = roomObj.AddComponent<Image>();
            roomImg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            roomImg.preserveAspect = true;
            roomImg.raycastTarget = false;
            FrameAnimationPlayer roomAnim = roomObj.AddComponent<FrameAnimationPlayer>();
            List<Sprite> normalFrames = LoadSpritesFromFolder("Assets/Sprites/Prologue/LivingRoomNormalFrames");
            if (normalFrames.Count == 0) normalFrames = LoadSpritesFromFolder("Assets/Sprites/Prologue/LivingRoomFrames");
            List<Sprite> redFrames = LoadSpritesFromFolder("Assets/Sprites/Prologue/LivingRoomRedFrames");
            List<Sprite> roomFrames = normalFrames;
            if (roomFrames.Count > 0)
            {
                AssignFramesToPlayer(roomAnim, roomFrames, 8f, true);
                roomImg.sprite = roomFrames[0];
            }
            else
            {
                Sprite roomSprite = LoadSpriteSafe("Assets/Sprites/Prologue/bg_living_room_red.png");
                if (roomSprite != null) roomImg.sprite = roomSprite;
            }

            // Layer 2: TV Static (Animated GIF frames)
            GameObject staticObj = new GameObject("TVStatic");
            staticObj.transform.SetParent(vpObj.transform, false);
            RectTransform staticRect = staticObj.AddComponent<RectTransform>();
            StretchFull(staticRect);
            Image staticImg = staticObj.AddComponent<Image>();
            staticImg.color = Color.white;
            staticImg.preserveAspect = false;
            staticImg.raycastTarget = false;
            FrameAnimationPlayer staticAnim = staticObj.AddComponent<FrameAnimationPlayer>();
            List<Sprite> staticFrames = LoadSpritesFromFolder("Assets/Sprites/Prologue/StaticFrames");
            if (staticFrames.Count > 0)
            {
                AssignFramesToPlayer(staticAnim, staticFrames, 24f, true);
                staticImg.sprite = staticFrames[0];
            }
            else
            {
                Sprite staticSprite = LoadSpriteSafe("Assets/Sprites/Prologue/tv_static.png");
                if (staticSprite != null) staticImg.sprite = staticSprite;
            }
            staticObj.SetActive(false);

            // Layer 3: CRT Scanlines / Vignette Overlay
            GameObject crtObj = new GameObject("CRTScanlineOverlay");
            crtObj.transform.SetParent(vpObj.transform, false);
            RectTransform crtRect = crtObj.AddComponent<RectTransform>();
            StretchFull(crtRect);
            Image crtImg = crtObj.AddComponent<Image>();
            crtImg.color = new Color(0f, 0f, 0f, 0.18f);
            crtImg.raycastTarget = false;

            // =========================================================================
            // PHYSICAL 3DS BEZEL / SEAM DIVIDER (Border at Y: 40%)
            // =========================================================================
            GameObject dividerObj = new GameObject("SeamDividerRail");
            dividerObj.transform.SetParent(obj.transform, false);
            RectTransform divRect = dividerObj.AddComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0f, 0.40f);
            divRect.anchorMax = new Vector2(1f, 0.40f);
            divRect.pivot = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(0f, 4f);
            divRect.anchoredPosition = Vector2.zero;
            Image divImg = dividerObj.AddComponent<Image>();
            divImg.color = new Color(0.85f, 0.55f, 0.15f, 0.95f); // Amber CDC tactical seam
            divImg.raycastTarget = false;
            Outline divOl = dividerObj.AddComponent<Outline>();
            divOl.effectColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            divOl.effectDistance = new Vector2(0f, -2f);

            // =========================================================================
            // BOTTOM SCREEN (3DS BOTTOM NARRATIVE & TACTICAL DOCK: 40% HEIGHT)
            // Docked flush to the bottom and side rails of the screen (0 to 1920 x 432px).
            // =========================================================================
            GameObject botScreenObj = new GameObject("BottomNarrativeDock");
            botScreenObj.transform.SetParent(obj.transform, false);
            RectTransform botRect = botScreenObj.AddComponent<RectTransform>();
            botRect.anchorMin = Vector2.zero;
            botRect.anchorMax = new Vector2(1f, 0.40f);
            botRect.offsetMin = Vector2.zero;
            botRect.offsetMax = Vector2.zero;

            Image botBg = botScreenObj.AddComponent<Image>();
            botBg.color = new Color(0.06f, 0.08f, 0.11f, 0.98f);
            botBg.raycastTarget = false;

            // Header Bar on Bottom Dock (Speaker & Channel status)
            GameObject botHeader = new GameObject("HeaderBar");
            botHeader.transform.SetParent(botScreenObj.transform, false);
            RectTransform bhRect = botHeader.AddComponent<RectTransform>();
            bhRect.anchorMin = new Vector2(0f, 1f);
            bhRect.anchorMax = new Vector2(1f, 1f);
            bhRect.pivot = new Vector2(0.5f, 1f);
            bhRect.sizeDelta = new Vector2(0f, 38f);
            bhRect.anchoredPosition = Vector2.zero;
            Image bhBg = botHeader.AddComponent<Image>();
            bhBg.color = new Color(0.09f, 0.12f, 0.16f, 0.98f);
            bhBg.raycastTarget = false;

            // Speaker Label (Left)
            GameObject spkObj = new GameObject("SpeakerLabel");
            spkObj.transform.SetParent(botHeader.transform, false);
            RectTransform spkRect = spkObj.AddComponent<RectTransform>();
            spkRect.anchorMin = new Vector2(0f, 0f);
            spkRect.anchorMax = new Vector2(0.6f, 1f);
            spkRect.offsetMin = new Vector2(32f, 0f);
            spkRect.offsetMax = Vector2.zero;
            TextMeshProUGUI spkTmp = spkObj.AddComponent<TextMeshProUGUI>();
            spkTmp.fontSize = 13.5f;
            spkTmp.alignment = TextAlignmentOptions.MidlineLeft;
            spkTmp.fontStyle = FontStyles.Bold;
            spkTmp.text = "<color=#94A3B8>[ SATURDAY NIGHT 23:45 ]</color>";
            spkTmp.color = new Color(0.95f, 0.65f, 0.15f, 1f);
            spkTmp.raycastTarget = false;

            // Right Channel Indicator
            GameObject chObj = new GameObject("SubChannel");
            chObj.transform.SetParent(botHeader.transform, false);
            RectTransform chRect = chObj.AddComponent<RectTransform>();
            chRect.anchorMin = new Vector2(0.6f, 0f);
            chRect.anchorMax = new Vector2(1f, 1f);
            chRect.offsetMin = Vector2.zero;
            chRect.offsetMax = new Vector2(-32f, 0f);
            TextMeshProUGUI chTmp = chObj.AddComponent<TextMeshProUGUI>();
            chTmp.fontSize = 12f;
            chTmp.alignment = TextAlignmentOptions.MidlineRight;
            chTmp.text = "";
            chTmp.color = new Color(0.60f, 0.70f, 0.85f);
            chTmp.raycastTarget = false;

            // Body Narrative Text Container
            GameObject bodyObj = new GameObject("NarrativeBody");
            bodyObj.transform.SetParent(botScreenObj.transform, false);
            RectTransform bodyRect = bodyObj.AddComponent<RectTransform>();
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(40f, 50f);
            bodyRect.offsetMax = new Vector2(-40f, -50f);
            TextMeshProUGUI bodyTmp = bodyObj.AddComponent<TextMeshProUGUI>();
            bodyTmp.fontSize = 20f;
            bodyTmp.lineSpacing = 12f;
            bodyTmp.alignment = TextAlignmentOptions.TopLeft;
            bodyTmp.color = new Color(0.92f, 0.95f, 0.98f);
            bodyTmp.text = "";
            bodyTmp.raycastTarget = false;

            // Continue Prompt Indicator (Bottom Right)
            GameObject promptObj = new GameObject("ContinuePrompt");
            promptObj.transform.SetParent(botScreenObj.transform, false);
            RectTransform prRect = promptObj.AddComponent<RectTransform>();
            prRect.anchorMin = new Vector2(1f, 0f);
            prRect.anchorMax = new Vector2(1f, 0f);
            prRect.pivot = new Vector2(1f, 0f);
            prRect.sizeDelta = new Vector2(360f, 32f);
            prRect.anchoredPosition = new Vector2(-32f, 12f);
            TextMeshProUGUI prTmp = promptObj.AddComponent<TextMeshProUGUI>();
            prTmp.fontSize = 12f;
            prTmp.fontStyle = FontStyles.Bold;
            prTmp.alignment = TextAlignmentOptions.MidlineRight;
            prTmp.text = "<color=#FFA726>CLICK ANYWHERE OR SPACE TO ADVANCE</color> <color=#FFFFFF>▶</color>";
            prTmp.color = new Color(0.95f, 0.65f, 0.15f, 0.9f);
            prTmp.raycastTarget = false;
            promptObj.SetActive(false);

            // ==========================================
            // FULLSCREEN INVISIBLE CLICK CATCHER BUTTON
            // Covers entire screen: allows clicking ANYWHERE without obstruction!
            // ==========================================
            GameObject clickCatcher = new GameObject("FullScreenClickCatcher");
            clickCatcher.transform.SetParent(obj.transform, false);
            RectTransform ccRect = clickCatcher.AddComponent<RectTransform>();
            StretchFull(ccRect);
            Image ccImg = clickCatcher.AddComponent<Image>();
            ccImg.color = Color.clear;
            ccImg.raycastTarget = true;
            Button ccBtn = clickCatcher.AddComponent<Button>();
            ccBtn.targetGraphic = ccImg;
            UnityEventTools.AddPersistentListener(ccBtn.onClick, screen.OnDialogueAreaClicked);

            // Action Button: FOCUS ON YOUR REFLECTION... (Above click catcher)
            GameObject btnObj = new GameObject("Btn_FocusReflection");
            btnObj.transform.SetParent(obj.transform, false);
            btnObj.transform.SetAsLastSibling();
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.sizeDelta = new Vector2(460f, 52f);
            btnRect.anchoredPosition = new Vector2(0f, 20f);

            CanvasGroup btnCg = btnObj.AddComponent<CanvasGroup>();
            btnCg.alpha = 1f;
            btnCg.interactable = true;
            btnCg.blocksRaycasts = true;
            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.85f, 0.35f, 0.08f, 0.98f);
            btnBg.raycastTarget = true;
            Outline btnOl = btnObj.AddComponent<Outline>();
            btnOl.effectColor = new Color(1f, 0.70f, 0.25f, 0.95f);
            btnOl.effectDistance = new Vector2(1.5f, -1.5f);
            Button focusBtn = btnObj.AddComponent<Button>();
            focusBtn.targetGraphic = btnBg;
            UnityEventTools.AddPersistentListener(focusBtn.onClick, screen.OnFocusReflectionClicked);

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btnTextObj.AddComponent<RectTransform>();
            StretchFull(btRect);
            TextMeshProUGUI btTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            btTmp.fontSize = 15.5f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.text = "<b>FOCUS ON YOUR REFLECTION... >></b>";
            btTmp.color = Color.white;
            btTmp.raycastTarget = false;
            btnObj.SetActive(false);

            // Audio Clips
            AudioClip staticClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/static.mp3");
            AudioClip sirensClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sirens.mp3");
            AudioClip screamClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/scream.mp3");

            // Wire Serialized Properties
            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("roomBackdropImage").objectReferenceValue = roomImg;
            so.FindProperty("tvStaticImage").objectReferenceValue = staticImg;
            so.FindProperty("livingRoomAnimPlayer").objectReferenceValue = roomAnim;
            so.FindProperty("tvStaticAnimPlayer").objectReferenceValue = staticAnim;
            so.FindProperty("speakerLabel").objectReferenceValue = spkTmp;
            so.FindProperty("narrativeBodyText").objectReferenceValue = bodyTmp;
            so.FindProperty("continuePrompt").objectReferenceValue = promptObj;
            so.FindProperty("focusReflectionButton").objectReferenceValue = focusBtn;
            so.FindProperty("fullScreenClickCatcher").objectReferenceValue = ccBtn;
            so.FindProperty("staticClip").objectReferenceValue = staticClip;
            so.FindProperty("sirensClip").objectReferenceValue = sirensClip;
            so.FindProperty("screamClip").objectReferenceValue = screamClip;
            AssignSpriteListToProperty(so, "normalLivingRoomFrames", normalFrames);
            AssignSpriteListToProperty(so, "redLivingRoomFrames", redFrames);
            so.ApplyModifiedProperties();

            return screen;
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

        private static void AssignSpriteListToProperty(SerializedObject so, string propName, List<Sprite> sprites)
        {
            if (so == null || sprites == null) return;
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null) return;
            prop.ClearArray();
            for (int i = 0; i < sprites.Count; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
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
            screen.EnsureIdentityConsoleBuilt();
            screen.EnsureBargainPanelBuilt();

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
            cRect.sizeDelta = new Vector2(0, 95);
            cRect.anchoredPosition = new Vector2(0, -42);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(container.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.sizeDelta = new Vector2(0, 42);
            tRect.anchoredPosition = new Vector2(0, 0);

            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = 28;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.83f, 0.33f, 0f);
            titleTmp.characterSpacing = 8;

            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(container.transform, false);
            RectTransform sRect = subObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 1f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.sizeDelta = new Vector2(0, 32);
            sRect.anchoredPosition = new Vector2(0, -44);

            TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
            subTmp.text = subtitle;
            subTmp.fontSize = 16;
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
            GameObject scrollObj = CreateScrollView(panel.transform, "JobScrollView", new Vector2(1540, 890), true, out Transform content);
            RectTransform sRect = scrollObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0, -56);

            GridLayoutGroup glg = content.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(470, 420);
            glg.spacing = new Vector2(24, 16);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.padding = new RectOffset(15, 15, 10, 10);
            glg.childAlignment = TextAnchor.UpperCenter;

            // Auto-detect screen & fit all 6 jobs without scroll if requirements met
            scrollObj.AddComponent<JobSelectionScreenAdapter>();

            foreach (var job in CharacterCreationDefinitions.Jobs)
            {
                CreateJobCard(content, job, () => screen.SelectJob(job.id));
            }

            return panel;
        }

        private static GameObject BuildStudentTrainingPanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "StudentTrainingPanel");
            CreateHeader(panel.transform, "PRE-APOCALYPSE TRAINING", "\"While others worked, you were preparing. Where did your natural talents lie?\"");

            // Main Dossier Box (1160 x 580)
            GameObject dossierBox = new GameObject("DossierBox");
            dossierBox.transform.SetParent(panel.transform, false);
            RectTransform dbRect = dossierBox.AddComponent<RectTransform>();
            dbRect.anchorMin = new Vector2(0.5f, 0.5f);
            dbRect.anchorMax = new Vector2(0.5f, 0.5f);
            dbRect.sizeDelta = new Vector2(1160, 580);
            dbRect.anchoredPosition = new Vector2(0, -35);

            // Outer border
            Image dbBorder = dossierBox.AddComponent<Image>();
            dbBorder.color = new Color(0.18f, 0.18f, 0.18f, 1f);

            // Inner dark panel
            GameObject innerBox = new GameObject("Inner");
            innerBox.transform.SetParent(dossierBox.transform, false);
            RectTransform inRect = innerBox.AddComponent<RectTransform>();
            StretchFull(inRect);
            inRect.offsetMin = new Vector2(1, 1);
            inRect.offsetMax = new Vector2(-1, -1);
            Image inBg = innerBox.AddComponent<Image>();
            inBg.color = new Color(0.035f, 0.035f, 0.035f, 0.98f);

            // Top accent stripe (3px orange across the entire top)
            GameObject topStripe = new GameObject("TopStripe");
            topStripe.transform.SetParent(innerBox.transform, false);
            RectTransform tsRect = topStripe.AddComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0f, 1f);
            tsRect.anchorMax = new Vector2(1f, 1f);
            tsRect.pivot = new Vector2(0.5f, 1f);
            tsRect.sizeDelta = new Vector2(0, 3);
            tsRect.anchoredPosition = Vector2.zero;
            topStripe.AddComponent<Image>().color = new Color(0.83f, 0.33f, 0f);

            // ==========================================
            // LEFT COLUMN: ATTRIBUTE ALLOCATOR CARD (535 x 542)
            // ==========================================
            GameObject leftCol = new GameObject("LeftColumn");
            leftCol.transform.SetParent(innerBox.transform, false);
            RectTransform lcRect = leftCol.AddComponent<RectTransform>();
            lcRect.anchorMin = new Vector2(0f, 0f);
            lcRect.anchorMax = new Vector2(0f, 1f);
            lcRect.pivot = new Vector2(0f, 0.5f);
            lcRect.sizeDelta = new Vector2(535, 0);
            lcRect.anchoredPosition = new Vector2(25, 0);

            // Left Card panel container
            GameObject allocBox = new GameObject("AllocatorPanel");
            allocBox.transform.SetParent(leftCol.transform, false);
            RectTransform apRect = allocBox.AddComponent<RectTransform>();
            StretchFull(apRect);
            apRect.offsetMin = new Vector2(0, 18);
            apRect.offsetMax = new Vector2(0, -18);
            Image apBg = allocBox.AddComponent<Image>();
            apBg.color = new Color(0.022f, 0.022f, 0.022f, 0.95f);

            // Card Title (Aptitude Allocation)
            GameObject allocTitleObj = new GameObject("AllocTitle");
            allocTitleObj.transform.SetParent(allocBox.transform, false);
            RectTransform atRect = allocTitleObj.AddComponent<RectTransform>();
            atRect.anchorMin = new Vector2(0f, 1f);
            atRect.anchorMax = new Vector2(1f, 1f);
            atRect.pivot = new Vector2(0.5f, 1f);
            atRect.sizeDelta = new Vector2(-30, 26);
            atRect.anchoredPosition = new Vector2(0, -16);
            TextMeshProUGUI atTmp = allocTitleObj.AddComponent<TextMeshProUGUI>();
            atTmp.text = "S.P.A.I.C. APTITUDE ALLOCATION";
            atTmp.fontSize = 18;
            atTmp.fontStyle = FontStyles.Bold;
            atTmp.characterSpacing = 2f;
            atTmp.alignment = TextAlignmentOptions.Center;
            atTmp.color = new Color(0.90f, 0.49f, 0.13f);

            // Card Subtitle
            GameObject allocSubObj = new GameObject("AllocSubtitle");
            allocSubObj.transform.SetParent(allocBox.transform, false);
            RectTransform asRect = allocSubObj.AddComponent<RectTransform>();
            asRect.anchorMin = new Vector2(0f, 1f);
            asRect.anchorMax = new Vector2(1f, 1f);
            asRect.pivot = new Vector2(0.5f, 1f);
            asRect.sizeDelta = new Vector2(-30, 20);
            asRect.anchoredPosition = new Vector2(0, -44);
            TextMeshProUGUI asTmp = allocSubObj.AddComponent<TextMeshProUGUI>();
            asTmp.text = "\"Distribute 5 AP points across your S.P.A.I.C. profile.\"";
            asTmp.fontSize = 12.5f;
            asTmp.fontStyle = FontStyles.Italic;
            asTmp.alignment = TextAlignmentOptions.Center;
            asTmp.color = new Color(0.60f, 0.60f, 0.60f);

            // Divider Line
            GameObject leftDivObj = new GameObject("LeftDivider");
            leftDivObj.transform.SetParent(allocBox.transform, false);
            RectTransform ldRect = leftDivObj.AddComponent<RectTransform>();
            ldRect.anchorMin = new Vector2(0f, 1f);
            ldRect.anchorMax = new Vector2(1f, 1f);
            ldRect.pivot = new Vector2(0.5f, 1f);
            ldRect.sizeDelta = new Vector2(-30, 1);
            ldRect.anchoredPosition = new Vector2(0, -68);
            leftDivObj.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f, 1f);

            // Points Remaining Pill Badge
            GameObject pointsBadge = new GameObject("PointsBadge");
            pointsBadge.transform.SetParent(allocBox.transform, false);
            RectTransform pbRect = pointsBadge.AddComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.5f, 1f);
            pbRect.anchorMax = new Vector2(0.5f, 1f);
            pbRect.pivot = new Vector2(0.5f, 1f);
            pbRect.sizeDelta = new Vector2(495, 36);
            pbRect.anchoredPosition = new Vector2(0, -78);
            Image pbBg = pointsBadge.AddComponent<Image>();
            pbBg.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

            GameObject ptTextObj = new GameObject("Text");
            ptTextObj.transform.SetParent(pointsBadge.transform, false);
            RectTransform pttRect = ptTextObj.AddComponent<RectTransform>();
            StretchFull(pttRect);
            TextMeshProUGUI pointsTmp = ptTextObj.AddComponent<TextMeshProUGUI>();
            pointsTmp.fontSize = 14;
            pointsTmp.fontStyle = FontStyles.Bold;
            pointsTmp.alignment = TextAlignmentOptions.Center;
            pointsTmp.color = new Color(0.90f, 0.49f, 0.13f);
            pointsTmp.text = "[ 5 AP AVAILABLE FOR ALLOCATION ]";
            so.FindProperty("studentPointsText").objectReferenceValue = pointsTmp;

            // Attributes Rows Container (5 rows x 46px + 6px spacing = 254px)
            GameObject rowsBox = new GameObject("AttributesRows");
            rowsBox.transform.SetParent(allocBox.transform, false);
            RectTransform rRect = rowsBox.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 1f);
            rRect.anchorMax = new Vector2(0.5f, 1f);
            rRect.pivot = new Vector2(0.5f, 1f);
            rRect.sizeDelta = new Vector2(495, 260);
            rRect.anchoredPosition = new Vector2(0, -124);
            VerticalLayoutGroup vlg = rowsBox.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;

            TextMeshProUGUI strTmp = CreateTrainingRow(rowsBox.transform, "STRENGTH", new Color(0.91f, 0.30f, 0.24f), 
                () => screen.AddStudentPoint("str"), () => screen.RemoveStudentPoint("str"), () => screen.InspectStudentStat("str", true));
            TextMeshProUGUI perTmp = CreateTrainingRow(rowsBox.transform, "PERCEPTION", new Color(0.95f, 0.77f, 0.06f), 
                () => screen.AddStudentPoint("per"), () => screen.RemoveStudentPoint("per"), () => screen.InspectStudentStat("per", true));
            TextMeshProUGUI agiTmp = CreateTrainingRow(rowsBox.transform, "AGILITY", new Color(0.18f, 0.80f, 0.44f), 
                () => screen.AddStudentPoint("agi"), () => screen.RemoveStudentPoint("agi"), () => screen.InspectStudentStat("agi", true));
            TextMeshProUGUI intTmp = CreateTrainingRow(rowsBox.transform, "INTELLIGENCE", new Color(0.20f, 0.60f, 0.86f), 
                () => screen.AddStudentPoint("int"), () => screen.RemoveStudentPoint("int"), () => screen.InspectStudentStat("int", true));
            TextMeshProUGUI chaTmp = CreateTrainingRow(rowsBox.transform, "CHARISMA", new Color(0.61f, 0.35f, 0.71f), 
                () => screen.AddStudentPoint("cha"), () => screen.RemoveStudentPoint("cha"), () => screen.InspectStudentStat("cha", true));

            so.FindProperty("studentStrText").objectReferenceValue = strTmp;
            so.FindProperty("studentPerText").objectReferenceValue = perTmp;
            so.FindProperty("studentAgiText").objectReferenceValue = agiTmp;
            so.FindProperty("studentIntText").objectReferenceValue = intTmp;
            so.FindProperty("studentChaText").objectReferenceValue = chaTmp;

            // Warning Text
            GameObject warnObj = new GameObject("WarningText");
            warnObj.transform.SetParent(allocBox.transform, false);
            RectTransform wRect = warnObj.AddComponent<RectTransform>();
            wRect.anchorMin = new Vector2(0.5f, 0f);
            wRect.anchorMax = new Vector2(0.5f, 0f);
            wRect.pivot = new Vector2(0.5f, 0f);
            wRect.sizeDelta = new Vector2(495, 36);
            wRect.anchoredPosition = new Vector2(0, 18);
            TextMeshProUGUI warnTmp = warnObj.AddComponent<TextMeshProUGUI>();
            warnTmp.text = "[!] ALLOCATE ALL 5 POINTS TO FINALIZE RECORD";
            warnTmp.fontSize = 13f;
            warnTmp.fontStyle = FontStyles.Bold;
            warnTmp.alignment = TextAlignmentOptions.Center;
            warnTmp.color = new Color(0.92f, 0.30f, 0.30f);
            warnTmp.raycastTarget = false;
            so.FindProperty("studentWarningText").objectReferenceValue = warnTmp;

            // Continue Button (Placed in exact same bottom slot)
            Button continueBtn = CreateStandardButton(allocBox.transform, "CONFIRM APTITUDE & PROCEED ->", () => screen.ConfirmStudentTraining(), new Vector2(495, 44));
            RectTransform cRect = continueBtn.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0f);
            cRect.anchorMax = new Vector2(0.5f, 0f);
            cRect.pivot = new Vector2(0.5f, 0f);
            cRect.anchoredPosition = new Vector2(0, 18);
            so.FindProperty("studentContinueButton").objectReferenceValue = continueBtn;

            // ==========================================
            // RIGHT COLUMN: STAT INSPECTOR CARD (535 x 542)
            // ==========================================
            GameObject rightCol = new GameObject("RightColumn");
            rightCol.transform.SetParent(innerBox.transform, false);
            RectTransform rcRect = rightCol.AddComponent<RectTransform>();
            rcRect.anchorMin = new Vector2(1f, 0f);
            rcRect.anchorMax = new Vector2(1f, 1f);
            rcRect.pivot = new Vector2(1f, 0.5f);
            rcRect.sizeDelta = new Vector2(535, 0);
            rcRect.anchoredPosition = new Vector2(-25, 0);

            // Card panel container
            GameObject inspBox = new GameObject("InspectorPanel");
            inspBox.transform.SetParent(rightCol.transform, false);
            RectTransform ipRect = inspBox.AddComponent<RectTransform>();
            StretchFull(ipRect);
            ipRect.offsetMin = new Vector2(0, 18);
            ipRect.offsetMax = new Vector2(0, -18);
            Image ipBg = inspBox.AddComponent<Image>();
            ipBg.color = new Color(0.022f, 0.022f, 0.022f, 0.95f);

            // Inspector Title
            GameObject inspTitleObj = new GameObject("InspTitle");
            inspTitleObj.transform.SetParent(inspBox.transform, false);
            RectTransform itRect = inspTitleObj.AddComponent<RectTransform>();
            itRect.anchorMin = new Vector2(0f, 1f);
            itRect.anchorMax = new Vector2(1f, 1f);
            itRect.pivot = new Vector2(0.5f, 1f);
            itRect.sizeDelta = new Vector2(-30, 26);
            itRect.anchoredPosition = new Vector2(0, -16);
            TextMeshProUGUI itTmp = inspTitleObj.AddComponent<TextMeshProUGUI>();
            itTmp.text = "STRENGTH (PHYSICAL POWER)";
            itTmp.fontSize = 18;
            itTmp.fontStyle = FontStyles.Bold;
            itTmp.characterSpacing = 2f;
            itTmp.alignment = TextAlignmentOptions.Center;
            itTmp.color = new Color(0.91f, 0.30f, 0.24f);
            so.FindProperty("inspectorStatTitle").objectReferenceValue = itTmp;

            // Inspector Quote
            GameObject inspQuoteObj = new GameObject("InspQuote");
            inspQuoteObj.transform.SetParent(inspBox.transform, false);
            RectTransform iqRect = inspQuoteObj.AddComponent<RectTransform>();
            iqRect.anchorMin = new Vector2(0f, 1f);
            iqRect.anchorMax = new Vector2(1f, 1f);
            iqRect.pivot = new Vector2(0.5f, 1f);
            iqRect.sizeDelta = new Vector2(-30, 20);
            iqRect.anchoredPosition = new Vector2(0, -44);
            TextMeshProUGUI iqTmp = inspQuoteObj.AddComponent<TextMeshProUGUI>();
            iqTmp.text = "\"Muscles conditioned from athletics, manual labor, and gym drills.\"";
            iqTmp.fontSize = 12.5f;
            iqTmp.fontStyle = FontStyles.Italic;
            iqTmp.alignment = TextAlignmentOptions.Center;
            iqTmp.color = new Color(0.60f, 0.60f, 0.60f);
            so.FindProperty("inspectorStatQuote").objectReferenceValue = iqTmp;

            // Divider Line
            GameObject divObj = new GameObject("Divider");
            divObj.transform.SetParent(inspBox.transform, false);
            RectTransform dRect = divObj.AddComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 1f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.pivot = new Vector2(0.5f, 1f);
            dRect.sizeDelta = new Vector2(-30, 1);
            dRect.anchoredPosition = new Vector2(0, -68);
            divObj.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f, 1f);

            // Banner illustration
            GameObject iBannerObj = new GameObject("VignetteBanner");
            iBannerObj.transform.SetParent(inspBox.transform, false);
            RectTransform ibRect = iBannerObj.AddComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0f, 1f);
            ibRect.anchorMax = new Vector2(1f, 1f);
            ibRect.pivot = new Vector2(0.5f, 1f);
            ibRect.sizeDelta = new Vector2(-30, 135);
            ibRect.anchoredPosition = new Vector2(0, -78);
            iBannerObj.AddComponent<RectMask2D>();
            Image ibBg = iBannerObj.AddComponent<Image>();
            ibBg.color = new Color(0.06f, 0.06f, 0.06f, 0.8f);

            GameObject iImgObj = new GameObject("Image");
            iImgObj.transform.SetParent(iBannerObj.transform, false);
            RectTransform iiRect = iImgObj.AddComponent<RectTransform>();
            StretchFull(iiRect);
            Image iiImg = iImgObj.AddComponent<Image>();
            iiImg.raycastTarget = false;
            Sprite studSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Jobs/job_student.png");
            if (studSprite != null)
            {
                iiImg.sprite = studSprite;
                iiImg.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                AspectRatioFitter arf = iImgObj.AddComponent<AspectRatioFitter>();
                arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                arf.aspectRatio = studSprite.rect.width / studSprite.rect.height;
            }

            // Dark bottom fade on vignette
            GameObject iGradObj = new GameObject("FadeOverlay");
            iGradObj.transform.SetParent(iBannerObj.transform, false);
            RectTransform igRect = iGradObj.AddComponent<RectTransform>();
            igRect.anchorMin = new Vector2(0f, 0f);
            igRect.anchorMax = new Vector2(1f, 0f);
            igRect.pivot = new Vector2(0.5f, 0f);
            igRect.sizeDelta = new Vector2(0, 50);
            igRect.anchoredPosition = Vector2.zero;
            Image igImg = iGradObj.AddComponent<Image>();
            igImg.sprite = GetBottomFadeGradient();
            igImg.raycastTarget = false;

            // Inspector Description (Explicit Word Wrapping Enabled!)
            GameObject inspDescObj = new GameObject("InspDesc");
            inspDescObj.transform.SetParent(inspBox.transform, false);
            RectTransform idRect = inspDescObj.AddComponent<RectTransform>();
            idRect.anchorMin = new Vector2(0f, 1f);
            idRect.anchorMax = new Vector2(1f, 1f);
            idRect.pivot = new Vector2(0.5f, 1f);
            idRect.sizeDelta = new Vector2(-36, 48);
            idRect.anchoredPosition = new Vector2(0, -222);
            TextMeshProUGUI idTmp = inspDescObj.AddComponent<TextMeshProUGUI>();
            idTmp.text = "Determines raw physical power, encumbrance limit, and brute force survival actions in combat and exploration.";
            idTmp.fontSize = 12.5f;
            idTmp.lineSpacing = 3f;
            idTmp.alignment = TextAlignmentOptions.TopLeft;
            idTmp.color = new Color(0.75f, 0.75f, 0.75f);
            idTmp.textWrappingMode = TextWrappingModes.Normal;
            idTmp.overflowMode = TextOverflowModes.Truncate;
            so.FindProperty("inspectorStatDesc").objectReferenceValue = idTmp;

            // Inspector Impact Card Box (Framed Survival Impact)
            GameObject impactCard = new GameObject("ImpactCard");
            impactCard.transform.SetParent(inspBox.transform, false);
            RectTransform icRect = impactCard.AddComponent<RectTransform>();
            icRect.anchorMin = new Vector2(0f, 1f);
            icRect.anchorMax = new Vector2(1f, 1f);
            icRect.pivot = new Vector2(0.5f, 1f);
            icRect.sizeDelta = new Vector2(-30, 245);
            icRect.anchoredPosition = new Vector2(0, -278);
            Image icBg = impactCard.AddComponent<Image>();
            icBg.color = new Color(0.04f, 0.04f, 0.04f, 0.95f);

            // Subtle top line on ImpactCard
            GameObject icTopLine = new GameObject("TopLine");
            icTopLine.transform.SetParent(impactCard.transform, false);
            RectTransform ictlRect = icTopLine.AddComponent<RectTransform>();
            ictlRect.anchorMin = new Vector2(0f, 1f);
            ictlRect.anchorMax = new Vector2(1f, 1f);
            ictlRect.pivot = new Vector2(0.5f, 1f);
            ictlRect.sizeDelta = new Vector2(0, 2);
            ictlRect.anchoredPosition = Vector2.zero;
            icTopLine.AddComponent<Image>().color = new Color(0.83f, 0.33f, 0f, 0.7f);

            GameObject inspImpactObj = new GameObject("InspImpact");
            inspImpactObj.transform.SetParent(impactCard.transform, false);
            RectTransform imRect = inspImpactObj.AddComponent<RectTransform>();
            StretchFull(imRect);
            imRect.offsetMin = new Vector2(14, 10);
            imRect.offsetMax = new Vector2(-14, -10);
            TextMeshProUGUI imTmp = inspImpactObj.AddComponent<TextMeshProUGUI>();
            imTmp.text = "<color=#E67E22><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                "• <color=#FFFFFF><b>Melee Combat:</b></color> +15% damage with blunt and edged weapons\n" +
                "• <color=#FFFFFF><b>Carrying Capacity:</b></color> +8 kg maximum inventory encumbrance\n" +
                "• <color=#FFFFFF><b>Breaching:</b></color> Force open jammed doors and barricades without tools";
            imTmp.fontSize = 12.5f;
            imTmp.lineSpacing = 5f;
            imTmp.alignment = TextAlignmentOptions.TopLeft;
            imTmp.color = new Color(0.85f, 0.85f, 0.85f);
            imTmp.textWrappingMode = TextWrappingModes.Normal;
            imTmp.overflowMode = TextOverflowModes.Truncate;
            so.FindProperty("inspectorStatImpact").objectReferenceValue = imTmp;

            return panel;
        }

        private struct AcademicCardDef
        {
            public string id;
            public string majorTitle;
            public string minorTitle;
            public string majorDesc;
            public string minorDesc;
            public string majorStat;
            public string minorStat;
            public Color accentColor;
            public string spriteName;
        }

        private static readonly AcademicCardDef[] AcademicOptions = new AcademicCardDef[]
        {
            new AcademicCardDef
            {
                id = "shooting",
                majorTitle = "ROTC",
                minorTitle = "SHOOTING",
                majorDesc = "Military science. You spent weekends at the range.",
                minorDesc = "Target practice and firearms handling.",
                majorStat = "+3 SHOOTING",
                minorStat = "+2 SHOOTING",
                accentColor = new Color(0.91f, 0.30f, 0.24f), // #E74C3C Red
                spriteName = "shooting"
            },
            new AcademicCardDef
            {
                id = "melee",
                majorTitle = "SPORTS",
                minorTitle = "MELEE",
                majorDesc = "Varsity combat sports. How to use your body as a weapon.",
                minorDesc = "Self-defense combat and hand-to-hand martial arts.",
                majorStat = "+3 MELEE",
                minorStat = "+2 MELEE",
                accentColor = new Color(0.90f, 0.49f, 0.13f), // #E67E22 Orange/Gold
                spriteName = "melee"
            },
            new AcademicCardDef
            {
                id = "stealth",
                majorTitle = "TRACK",
                minorTitle = "STEALTH",
                majorDesc = "Outdoor survival. Move through terrain without being noticed.",
                minorDesc = "Moving unseen and field evasion techniques.",
                majorStat = "+3 STEALTH",
                minorStat = "+2 STEALTH",
                accentColor = new Color(0.18f, 0.80f, 0.44f), // #2ECC71 Green
                spriteName = "stealth"
            },
            new AcademicCardDef
            {
                id = "tech",
                majorTitle = "CS & IT",
                minorTitle = "TECH",
                majorDesc = "Hardware engineering. Manipulate electronics and security.",
                minorDesc = "Hardware repair and electronics fundamentals.",
                majorStat = "+3 TECH",
                minorStat = "+2 TECH",
                accentColor = new Color(0.20f, 0.60f, 0.86f), // #3498DB Blue
                spriteName = "tech"
            },
            new AcademicCardDef
            {
                id = "medicine",
                majorTitle = "BIOLOGY",
                minorTitle = "MEDICINE",
                majorDesc = "Pre-med sciences. You understand anatomy and how to patch wounds.",
                minorDesc = "First aid protocols and emergency response.",
                majorStat = "+3 MEDICINE",
                minorStat = "+2 MEDICINE",
                accentColor = new Color(0.61f, 0.35f, 0.71f), // #9B59B6 Purple
                spriteName = "medicine"
            }
        };

        private static GameObject CreateAcademicCard(Transform parent, AcademicCardDef opt, bool isMajor, UnityEngine.Events.UnityAction onSelect)
        {
            GameObject cardObj = new GameObject($"AcademicCard_{opt.id}");
            cardObj.transform.SetParent(parent, false);
            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 580);

            // Card border outline (1px dark border)
            Image cardBorder = cardObj.AddComponent<Image>();
            cardBorder.color = new Color(0.18f, 0.18f, 0.18f, 1f);

            // Inner dark panel (#080808)
            GameObject innerCard = new GameObject("InnerCard");
            innerCard.transform.SetParent(cardObj.transform, false);
            RectTransform innerRect = innerCard.AddComponent<RectTransform>();
            StretchFull(innerRect);
            innerRect.offsetMin = new Vector2(1, 1);
            innerRect.offsetMax = new Vector2(-1, -1);
            Image innerBg = innerCard.AddComponent<Image>();
            innerBg.color = new Color(0.031f, 0.031f, 0.031f, 1f);
            Button cardBtn = innerCard.AddComponent<Button>();
            cardBtn.targetGraphic = innerBg;
            ColorBlock ccb = cardBtn.colors;
            ccb.normalColor = new Color(0.031f, 0.031f, 0.031f, 1f);
            ccb.highlightedColor = new Color(0.08f, 0.08f, 0.08f, 1f);
            ccb.pressedColor = new Color(0.12f, 0.12f, 0.12f, 1f);
            ccb.selectedColor = ccb.normalColor;
            cardBtn.colors = ccb;
            if (onSelect != null) cardBtn.onClick.AddListener(onSelect);

            // Top accent bar (3px line with opt.accentColor)
            GameObject topBar = new GameObject("TopBorder");
            topBar.transform.SetParent(innerCard.transform, false);
            RectTransform tbRect = topBar.AddComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(0, 3);
            tbRect.anchoredPosition = Vector2.zero;
            topBar.AddComponent<Image>().color = opt.accentColor;

            // Banner Image Container (Masked)
            GameObject bannerObj = new GameObject("Banner");
            bannerObj.transform.SetParent(innerCard.transform, false);
            RectTransform bRect = bannerObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 1f);
            bRect.anchorMax = new Vector2(1f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.sizeDelta = new Vector2(0, 240);
            bRect.anchoredPosition = new Vector2(0, -3);
            bannerObj.AddComponent<RectMask2D>();
            Image bBg = bannerObj.AddComponent<Image>();
            bBg.color = new Color(0.06f, 0.06f, 0.06f, 0.8f);

            // Illustration image
            GameObject imgObj = new GameObject("Icon");
            imgObj.transform.SetParent(bannerObj.transform, false);
            RectTransform imgRect = imgObj.AddComponent<RectTransform>();
            Image pImg = imgObj.AddComponent<Image>();
            pImg.raycastTarget = false;

            Sprite hobbySprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Hobbies/{opt.spriteName}.png");
            if (hobbySprite != null)
            {
                pImg.sprite = hobbySprite;
                pImg.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            }

            float spriteRatio = (hobbySprite != null && hobbySprite.rect.height > 0)
                ? (hobbySprite.rect.width / hobbySprite.rect.height)
                : 1.0f;

            if (spriteRatio > 1.2f)
            {
                // Cover banner mode (for 3:2 landscape illustrations)
                StretchFull(imgRect);
                AspectRatioFitter arf = imgObj.AddComponent<AspectRatioFitter>();
                arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                arf.aspectRatio = spriteRatio;
                pImg.preserveAspect = false;

                // Pre-calculate exact sizeDelta for instant editor layout without waiting for layout pass
                float containerW = 298f;
                float containerH = 240f;
                float neededW = containerH * spriteRatio;
                if (neededW > containerW)
                {
                    imgRect.sizeDelta = new Vector2(neededW - containerW, 0f);
                }
                else
                {
                    float neededH = containerW / spriteRatio;
                    imgRect.sizeDelta = new Vector2(0f, Mathf.Max(0f, neededH - containerH));
                }
                imgRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                // Centered icon mode (for square clipart icons)
                imgRect.anchorMin = new Vector2(0.5f, 0.5f);
                imgRect.anchorMax = new Vector2(0.5f, 0.5f);
                imgRect.pivot = new Vector2(0.5f, 0.5f);
                imgRect.sizeDelta = new Vector2(170, 170);
                imgRect.anchoredPosition = new Vector2(0, -5);
                pImg.preserveAspect = true;
            }

            // Dark bottom gradient overlay
            GameObject gradObj = new GameObject("FadeOverlay");
            gradObj.transform.SetParent(bannerObj.transform, false);
            RectTransform gRect = gradObj.AddComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0f);
            gRect.anchorMax = new Vector2(1f, 0f);
            gRect.pivot = new Vector2(0.5f, 0f);
            gRect.sizeDelta = new Vector2(0, 65);
            gRect.anchoredPosition = Vector2.zero;
            Image gradImg = gradObj.AddComponent<Image>();
            gradImg.sprite = GetBottomFadeGradient();
            gradImg.raycastTarget = false;

            // Title
            string displayTitle = isMajor ? opt.majorTitle : opt.minorTitle;
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(innerCard.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.sizeDelta = new Vector2(-20, 32);
            tRect.anchoredPosition = new Vector2(0, -258);
            TextMeshProUGUI tTmp = titleObj.AddComponent<TextMeshProUGUI>();
            tTmp.text = displayTitle;
            tTmp.fontSize = 22;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.characterSpacing = 2f;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = opt.accentColor;
            tTmp.raycastTarget = false;

            // Stat Badge Pill
            string displayStat = isMajor ? opt.majorStat : opt.minorStat;
            GameObject statObj = new GameObject("StatBadge");
            statObj.transform.SetParent(innerCard.transform, false);
            RectTransform stRect = statObj.AddComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0f, 1f);
            stRect.anchorMax = new Vector2(1f, 1f);
            stRect.pivot = new Vector2(0.5f, 1f);
            stRect.sizeDelta = new Vector2(-40, 28);
            stRect.anchoredPosition = new Vector2(0, -298);
            Image stBg = statObj.AddComponent<Image>();
            stBg.color = new Color(0f, 0f, 0f, 0.55f);

            GameObject stTextObj = new GameObject("Text");
            stTextObj.transform.SetParent(statObj.transform, false);
            RectTransform sttRect = stTextObj.AddComponent<RectTransform>();
            StretchFull(sttRect);
            TextMeshProUGUI stTmp = stTextObj.AddComponent<TextMeshProUGUI>();
            stTmp.text = displayStat;
            stTmp.fontSize = 13;
            stTmp.fontStyle = FontStyles.Bold;
            stTmp.alignment = TextAlignmentOptions.Center;
            stTmp.color = opt.accentColor;
            stTmp.raycastTarget = false;

            // Description
            string displayDesc = isMajor ? opt.majorDesc : opt.minorDesc;
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(innerCard.transform, false);
            RectTransform dRect = descObj.AddComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 1f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.pivot = new Vector2(0.5f, 1f);
            dRect.sizeDelta = new Vector2(-24, 80);
            dRect.anchoredPosition = new Vector2(0, -338);
            TextMeshProUGUI dTmp = descObj.AddComponent<TextMeshProUGUI>();
            dTmp.text = displayDesc;
            dTmp.fontSize = 12.5f;
            dTmp.lineSpacing = 2.5f;
            dTmp.alignment = TextAlignmentOptions.Center;
            dTmp.color = new Color(0.72f, 0.72f, 0.72f);
            dTmp.textWrappingMode = TextWrappingModes.Normal;
            dTmp.overflowMode = TextOverflowModes.Ellipsis;
            dTmp.raycastTarget = false;

            // Action Button
            GameObject btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(innerCard.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.sizeDelta = new Vector2(250, 48);
            btnRect.anchoredPosition = new Vector2(0, 20);

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.09f, 0.09f, 0.09f, 0.98f);
            Button bBtn = btnObj.AddComponent<Button>();
            bBtn.targetGraphic = btnBg;

            ColorBlock cb = bBtn.colors;
            cb.normalColor = new Color(0.09f, 0.09f, 0.09f, 0.98f);
            cb.highlightedColor = Color.Lerp(new Color(0.18f, 0.18f, 0.18f, 1f), opt.accentColor, 0.45f);
            cb.pressedColor = opt.accentColor * 0.75f;
            cb.selectedColor = cb.normalColor;
            bBtn.colors = cb;

            if (onSelect != null) bBtn.onClick.AddListener(onSelect);

            GameObject btObj = new GameObject("Text");
            btObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btObj.AddComponent<RectTransform>();
            StretchFull(btRect);
            TextMeshProUGUI btTmp = btObj.AddComponent<TextMeshProUGUI>();
            btTmp.text = isMajor ? "SELECT MAJOR" : "SELECT MINOR";
            btTmp.fontSize = 13.5f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.characterSpacing = 2f;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;
            btTmp.raycastTarget = false;

            AcademicCardView cardView = cardObj.AddComponent<AcademicCardView>();
            cardView.skillId = opt.id;

            return cardObj;
        }

        private static GameObject BuildStudentMajorPanel(Transform parent, CharacterCreationScreen screen)
        {
            GameObject panel = CreatePanelContainer(parent, "StudentMajorPanel");
            CreateHeader(panel.transform, "UNIVERSITY TRANSCRIPT: MAJOR", "\"Before the sirens, you were preparing for a career. What was your focus?\"");

            GameObject container = new GameObject("AcademicContainer");
            container.transform.SetParent(panel.transform, false);
            RectTransform cRect = container.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(1650, 600);
            cRect.anchoredPosition = new Vector2(0, -35);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 28;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            foreach (var opt in AcademicOptions)
            {
                CreateAcademicCard(container.transform, opt, isMajor: true, () => screen.SelectStudentMajor(opt.id));
            }

            return panel;
        }

        private static GameObject BuildStudentMinorPanel(Transform parent, CharacterCreationScreen screen)
        {
            GameObject panel = CreatePanelContainer(parent, "StudentMinorPanel");
            CreateHeader(panel.transform, "UNIVERSITY TRANSCRIPT: MINOR", "\"And what did you study on the side?\"");

            GameObject container = new GameObject("AcademicContainer");
            container.transform.SetParent(panel.transform, false);
            RectTransform cRect = container.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(1650, 600);
            cRect.anchoredPosition = new Vector2(0, -35);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 28;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            foreach (var opt in AcademicOptions)
            {
                CreateAcademicCard(container.transform, opt, isMajor: false, () => screen.SelectStudentMinor(opt.id));
            }

            return panel;
        }

        private static GameObject BuildHobbySelectionPanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "HobbySelectionPanel");
            
            // Header with atmospheric title & subtitle
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panel.transform, false);
            RectTransform hRect = headerObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(0, 95);
            hRect.anchoredPosition = new Vector2(0, -42);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.sizeDelta = new Vector2(0, 42);
            tRect.anchoredPosition = new Vector2(0, 0);

            TextMeshProUGUI hTmp = titleObj.AddComponent<TextMeshProUGUI>();
            hTmp.text = "PRIMARY HOBBY";
            hTmp.fontSize = 28;
            hTmp.fontStyle = FontStyles.Bold;
            hTmp.alignment = TextAlignmentOptions.Center;
            hTmp.color = new Color(0.83f, 0.33f, 0f);
            hTmp.characterSpacing = 8;
            so.FindProperty("hobbyHeaderTitle").objectReferenceValue = hTmp;

            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(headerObj.transform, false);
            RectTransform sRect = subObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 1f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.sizeDelta = new Vector2(0, 32);
            sRect.anchoredPosition = new Vector2(0, -44);

            TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
            subTmp.text = "\"How did you spend your weekends before everything fell apart?\"";
            subTmp.fontSize = 16;
            subTmp.fontStyle = FontStyles.Italic;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.6f, 0.6f, 0.6f);
            so.FindProperty("hobbyHeaderSubtitle").objectReferenceValue = subTmp;

            // Thematic Tab Bar (Combat/Field vs Mind/Tech for Positive Hobbies, Physical vs Mental for Weaknesses)
            GameObject tabBarObj = new GameObject("WeaknessTabBar");
            tabBarObj.transform.SetParent(panel.transform, false);
            RectTransform tbRect = tabBarObj.AddComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0.5f, 1f);
            tbRect.anchorMax = new Vector2(0.5f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(780, 38);
            tbRect.anchoredPosition = new Vector2(0, -122);

            HorizontalLayoutGroup hlg = tabBarObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Physical / Combat Tab Button
            GameObject physTabObj = new GameObject("Btn_PhysicalTab");
            physTabObj.transform.SetParent(tabBarObj.transform, false);
            Image physBg = physTabObj.AddComponent<Image>();
            physBg.color = new Color(0.65f, 0.28f, 0.05f, 0.95f);
            Button physBtn = physTabObj.AddComponent<Button>();
            physBtn.targetGraphic = physBg;
            Outline physOutline = physTabObj.AddComponent<Outline>();
            physOutline.effectColor = new Color(0.95f, 0.50f, 0.12f, 0.90f);
            physOutline.effectDistance = new Vector2(1, -1);

            GameObject physTxtObj = new GameObject("Text");
            physTxtObj.transform.SetParent(physTabObj.transform, false);
            RectTransform pttRect = physTxtObj.AddComponent<RectTransform>();
            StretchFull(pttRect);
            TextMeshProUGUI physTmp = physTxtObj.AddComponent<TextMeshProUGUI>();
            physTmp.text = "<b>● COMBAT, FIELD & STEALTH (8)</b>";
            physTmp.fontSize = 13f;
            physTmp.fontStyle = FontStyles.Bold;
            physTmp.characterSpacing = 2f;
            physTmp.alignment = TextAlignmentOptions.Center;
            physTmp.color = Color.white;
            physTmp.raycastTarget = false;

            // Mental / Tech Tab Button
            GameObject mentalTabObj = new GameObject("Btn_MentalTab");
            mentalTabObj.transform.SetParent(tabBarObj.transform, false);
            Image mentalBg = mentalTabObj.AddComponent<Image>();
            mentalBg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
            Button mentalBtn = mentalTabObj.AddComponent<Button>();
            mentalBtn.targetGraphic = mentalBg;
            Outline mentalOutline = mentalTabObj.AddComponent<Outline>();
            mentalOutline.effectColor = new Color(0.20f, 0.20f, 0.20f, 0.5f);
            mentalOutline.effectDistance = new Vector2(1, -1);

            GameObject mentalTxtObj = new GameObject("Text");
            mentalTxtObj.transform.SetParent(mentalTabObj.transform, false);
            RectTransform mttRect = mentalTxtObj.AddComponent<RectTransform>();
            StretchFull(mttRect);
            TextMeshProUGUI mentalTmp = mentalTxtObj.AddComponent<TextMeshProUGUI>();
            mentalTmp.text = "<b>○ MIND, TECH & SUPPORT (7)</b>";
            mentalTmp.fontSize = 13f;
            mentalTmp.fontStyle = FontStyles.Bold;
            mentalTmp.characterSpacing = 2f;
            mentalTmp.alignment = TextAlignmentOptions.Center;
            mentalTmp.color = new Color(0.6f, 0.6f, 0.6f);
            mentalTmp.raycastTarget = false;

            tabBarObj.SetActive(true);

            so.FindProperty("weaknessTabBar").objectReferenceValue = tabBarObj;
            so.FindProperty("physicalTabButton").objectReferenceValue = physBtn;
            so.FindProperty("mentalTabButton").objectReferenceValue = mentalBtn;
            so.FindProperty("physicalTabBg").objectReferenceValue = physBg;
            so.FindProperty("mentalTabBg").objectReferenceValue = mentalBg;
            so.FindProperty("physicalTabText").objectReferenceValue = physTmp;
            so.FindProperty("mentalTabText").objectReferenceValue = mentalTmp;
            so.FindProperty("physicalTabOutline").objectReferenceValue = physOutline;
            so.FindProperty("mentalTabOutline").objectReferenceValue = mentalOutline;

            // Responsive ScrollView with 4/5-column Grid (2 rows per tab)
            GameObject scrollObj = CreateScrollView(panel.transform, "HobbyScrollView", new Vector2(1540, 840), true, out Transform content);
            RectTransform scrollRectTransform = scrollObj.GetComponent<RectTransform>();
            scrollRectTransform.anchoredPosition = new Vector2(0, -82);

            GridLayoutGroup glg = content.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(350, 385);
            glg.spacing = new Vector2(22, 22);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            glg.padding = new RectOffset(20, 20, 10, 15);
            glg.childAlignment = TextAnchor.UpperCenter;
            so.FindProperty("hobbyCardsContainer").objectReferenceValue = content;

            // Auto-detect screen & fit 2 rows per tab cleanly without scroll
            scrollObj.AddComponent<HobbySelectionScreenAdapter>();

            // Card Template (Prefab)
            GameObject template = BuildHobbyCardTemplate(panel.transform);
            template.SetActive(false);
            so.FindProperty("hobbyCardPrefab").objectReferenceValue = template;

            return panel;
        }

        private static GameObject BuildSummaryDossierPanel(Transform parent, CharacterCreationScreen screen, SerializedObject so)
        {
            GameObject panel = CreatePanelContainer(parent, "SummaryDossierPanel");

            // Main Dossier Frame
            GameObject frame = new GameObject("FederalDossierFrame");
            frame.transform.SetParent(panel.transform, false);
            RectTransform frameRect = frame.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(1420, 760);
            frameRect.anchoredPosition = new Vector2(0, 0);

            Image frameBg = frame.AddComponent<Image>();
            frameBg.color = new Color(0.05f, 0.07f, 0.10f, 0.98f); // Deep federal navy/black

            Outline outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.28f, 0.40f, 0.85f);
            outline.effectDistance = new Vector2(2, -2);

            frame.AddComponent<RectMask2D>();

            // ==========================================
            // 1. TOP HEADER BAR (Height: 88px)
            // ==========================================
            GameObject headerBar = new GameObject("HeaderBar");
            headerBar.transform.SetParent(frame.transform, false);
            RectTransform hRect = headerBar.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 1);
            hRect.anchorMax = new Vector2(1, 1);
            hRect.pivot = new Vector2(0.5f, 1);
            hRect.sizeDelta = new Vector2(0, 88);
            hRect.anchoredPosition = Vector2.zero;

            Image hBg = headerBar.AddComponent<Image>();
            hBg.color = new Color(0.08f, 0.12f, 0.18f, 1f); // Official navy

            // Gold underline
            GameObject goldLine = new GameObject("GoldDivider");
            goldLine.transform.SetParent(headerBar.transform, false);
            RectTransform glRect = goldLine.AddComponent<RectTransform>();
            glRect.anchorMin = new Vector2(0, 0);
            glRect.anchorMax = new Vector2(1, 0);
            glRect.pivot = new Vector2(0.5f, 0);
            glRect.sizeDelta = new Vector2(0, 2);
            glRect.anchoredPosition = Vector2.zero;
            Image glImg = goldLine.AddComponent<Image>();
            glImg.color = new Color(0.85f, 0.68f, 0.25f, 1f); // Metallic Gold

            // Left: Official Federal Seal
            GameObject sealObj = new GameObject("FederalSeal");
            sealObj.transform.SetParent(headerBar.transform, false);
            RectTransform sRect = sealObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0, 0.5f);
            sRect.anchorMax = new Vector2(0, 0.5f);
            sRect.pivot = new Vector2(0, 0.5f);
            sRect.sizeDelta = new Vector2(68, 68);
            sRect.anchoredPosition = new Vector2(20, 0);

            Image sealImg = sealObj.AddComponent<Image>();
            Sprite sealSprite = LoadSpriteSafe("Assets/Sprites/FederalBioEmergencySeal.png");
            if (sealSprite != null)
            {
                sealImg.sprite = sealSprite;
                sealImg.preserveAspect = true;
            }

            // Middle: Title Block
            GameObject titleBlock = new GameObject("TitleBlock");
            titleBlock.transform.SetParent(headerBar.transform, false);
            RectTransform tbRect = titleBlock.AddComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0, 0.5f);
            tbRect.anchorMax = new Vector2(0, 0.5f);
            tbRect.pivot = new Vector2(0, 0.5f);
            tbRect.sizeDelta = new Vector2(750, 56);
            tbRect.anchoredPosition = new Vector2(104, 0);

            VerticalLayoutGroup tbVlg = titleBlock.AddComponent<VerticalLayoutGroup>();
            tbVlg.childAlignment = TextAnchor.MiddleLeft;
            tbVlg.spacing = 2;
            tbVlg.childControlWidth = true;
            tbVlg.childForceExpandWidth = true;
            tbVlg.childControlHeight = true;
            tbVlg.childForceExpandHeight = false;

            GameObject t1Obj = new GameObject("Title");
            t1Obj.transform.SetParent(titleBlock.transform, false);
            LayoutElement t1Le = t1Obj.AddComponent<LayoutElement>();
            t1Le.preferredHeight = 28;
            TextMeshProUGUI t1 = t1Obj.AddComponent<TextMeshProUGUI>();
            t1.text = "<cspace=0.15em>FEDERAL BIO-EMERGENCY AGENCY</cspace>";
            t1.fontSize = 20;
            t1.fontStyle = FontStyles.Bold;
            t1.color = new Color(0.96f, 0.75f, 0.26f); // Warm Gold

            GameObject t2Obj = new GameObject("Subtitle");
            t2Obj.transform.SetParent(titleBlock.transform, false);
            LayoutElement t2Le = t2Obj.AddComponent<LayoutElement>();
            t2Le.preferredHeight = 18;
            TextMeshProUGUI t2 = t2Obj.AddComponent<TextMeshProUGUI>();
            t2.text = "CDC • FEMA JOINT OUTBREAK RESPONSE TASKFORCE   |   EMERGENCY REGISTER PROTOCOL 2026";
            t2.fontSize = 11.5f;
            t2.color = new Color(0.55f, 0.68f, 0.82f); // Slate cyan

            // Right: Case Metadata
            GameObject metaBlock = new GameObject("MetaBlock");
            metaBlock.transform.SetParent(headerBar.transform, false);
            RectTransform mbRect = metaBlock.AddComponent<RectTransform>();
            mbRect.anchorMin = new Vector2(1, 0.5f);
            mbRect.anchorMax = new Vector2(1, 0.5f);
            mbRect.pivot = new Vector2(1, 0.5f);
            mbRect.sizeDelta = new Vector2(440, 56);
            mbRect.anchoredPosition = new Vector2(-22, 0);

            VerticalLayoutGroup mbVlg = metaBlock.AddComponent<VerticalLayoutGroup>();
            mbVlg.childAlignment = TextAnchor.MiddleRight;
            mbVlg.spacing = 2;
            mbVlg.childControlWidth = true;
            mbVlg.childForceExpandWidth = true;
            mbVlg.childControlHeight = true;
            mbVlg.childForceExpandHeight = false;

            GameObject m1Obj = new GameObject("CaseId");
            m1Obj.transform.SetParent(metaBlock.transform, false);
            LayoutElement m1Le = m1Obj.AddComponent<LayoutElement>();
            m1Le.preferredHeight = 22;
            TextMeshProUGUI m1 = m1Obj.AddComponent<TextMeshProUGUI>();
            m1.text = "<mspace=0.6em>CASE FILE: #US-88204-EAS</mspace>   |   DATE: DAY ZERO 23:45";
            m1.fontSize = 12;
            m1.alignment = TextAlignmentOptions.Right;
            m1.color = new Color(0.68f, 0.78f, 0.88f);

            GameObject m2Obj = new GameObject("ThreatLevel");
            m2Obj.transform.SetParent(metaBlock.transform, false);
            LayoutElement m2Le = m2Obj.AddComponent<LayoutElement>();
            m2Le.preferredHeight = 22;
            TextMeshProUGUI m2 = m2Obj.AddComponent<TextMeshProUGUI>();
            m2.text = "<color=#EF5350><b>[ STATUS: CIVIL DANGER WARNING // LEVEL 4 ]</b></color>";
            m2.fontSize = 12.5f;
            m2.alignment = TextAlignmentOptions.Right;

            // ==========================================
            // 2. COLUMNS CONTAINER (Height: 588px)
            // ==========================================
            GameObject colsObj = new GameObject("ColumnsContainer");
            colsObj.transform.SetParent(frame.transform, false);
            RectTransform cRect = colsObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 0);
            cRect.anchorMax = new Vector2(1, 1);
            cRect.offsetMin = new Vector2(18, 76);
            cRect.offsetMax = new Vector2(-18, -96);
            colsObj.AddComponent<RectMask2D>();

            HorizontalLayoutGroup hlg = colsObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;

            // --- COLUMN 1: CITIZEN IDENTITY & VITALS ---
            GameObject col1 = CreateDossierCard(colsObj.transform, "Col1_Identity", 420);
            CreateSectionHeader(col1.transform, "01 // CITIZEN IDENTITY & VITALS");
            CreateSubLabel(col1.transform, "REGISTRANT LEGAL IDENTITY:");
            so.FindProperty("summaryNameText").objectReferenceValue = CreateDossierRow(col1.transform, 22, FontStyles.Bold, new Color(1f, 0.78f, 0.25f));
            so.FindProperty("summaryIdentityText").objectReferenceValue = CreateDossierRow(col1.transform, 12.5f, FontStyles.Normal, Color.white);
            CreateDivider(col1.transform);
            CreateSubLabel(col1.transform, "PHYSIOLOGICAL VITALS & CONDITION:");
            so.FindProperty("summaryVitalsText").objectReferenceValue = CreateDossierRow(col1.transform, 12.5f, FontStyles.Normal, new Color(0.40f, 0.75f, 0.95f));
            CreateDivider(col1.transform);
            CreateSubLabel(col1.transform, "BIOMETRIC & INFECTION SCREENING PROTOCOL:");
            TextMeshProUGUI screeningText = CreateDossierRow(col1.transform, 11f, FontStyles.Normal, new Color(0.70f, 0.78f, 0.85f));
            screeningText.lineSpacing = 2f;
            screeningText.text = 
                "• CHECKPOINT: <color=#FFE082>SECTOR-04 // METRO DECONTAM GATE</color>\n" +
                "• BODY TEMP: <color=#66BB6A>36.8°C [NORMAL / AFEBRILE]</color>\n" +
                "• VIRAL ANTIGEN SWAB: <color=#66BB6A>NEGATIVE (NON-REACTIVE)</color>\n" +
                "• RETINAL OCULAR SCAN: <color=#66BB6A>NORMAL (NO CAPILLARY RUPTURE)</color>\n" +
                "• EPIDERMAL CHECK: <color=#66BB6A>NO INFECTED LACERATIONS / BITES</color>\n" +
                "• STATUS: <color=#29B6F6><b>TIER-1 CONDITIONAL CLEARANCE</b></color>\n\n" +
                "<mspace=0.45em><color=#556677>||| | |||| | ||| |||| | | |||| |||</color></mspace>  <color=#8899AA>#EAS-2026-US88204</color>";

            // Rubber stamp in Column 1 (Overlay inside Card 1)
            GameObject stampHolder = new GameObject("StampHolder");
            stampHolder.transform.SetParent(col1.transform, false);

            LayoutElement stLe = stampHolder.AddComponent<LayoutElement>();
            stLe.ignoreLayout = true; // Sits as an overlay, does not push content down!

            RectTransform stRect = stampHolder.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0.5f, 0f);
            stRect.anchorMax = new Vector2(0.5f, 0f);
            stRect.pivot = new Vector2(0.5f, 0f);
            stRect.sizeDelta = new Vector2(185, 185);
            stRect.anchoredPosition = new Vector2(25, 25); // Neatly anchored within Card 1 bottom area

            Image stampImg = stampHolder.AddComponent<Image>();
            Sprite stampSprite = LoadSpriteSafe("Assets/Sprites/BiohazardApprovedStampAngled.png");
            if (stampSprite == null) stampSprite = LoadSpriteSafe("Assets/Sprites/BiohazardApprovedStamp.png");
            if (stampSprite != null)
            {
                stampImg.sprite = stampSprite;
                stampImg.preserveAspect = true;
                stampImg.color = new Color(1f, 1f, 1f, 0.95f);
            }

            // --- COLUMN 2: ATTRIBUTES & PROFICIENCIES ---
            GameObject col2 = CreateDossierCard(colsObj.transform, "Col2_Aptitudes", 440);
            CreateSectionHeader(col2.transform, "02 // M.O.R.T.A.L. ATTRIBUTES & PROFICIENCIES");
            CreateSubLabel(col2.transform, "M.O.R.T.A.L. BIOLOGICAL APTITUDE INDEX:");
            so.FindProperty("summaryAttributesText").objectReferenceValue = CreateDossierRow(col2.transform, 13.5f, FontStyles.Normal, Color.white);
            CreateDivider(col2.transform);
            CreateSubLabel(col2.transform, "SURVIVAL & WEAPON PROFICIENCIES:");
            so.FindProperty("summarySkillsText").objectReferenceValue = CreateDossierRow(col2.transform, 12.5f, FontStyles.Normal, Color.white);
            CreateDivider(col2.transform);
            CreateSubLabel(col2.transform, "CDC OUTBREAK RESPONSE ADVISORY // PROTOCOL 104:");

            // Framed Tactical Advisory Box (Tightened to 115px)
            GameObject advBox = new GameObject("AdvisoryBox");
            advBox.transform.SetParent(col2.transform, false);
            RectTransform abRect = advBox.AddComponent<RectTransform>();
            abRect.sizeDelta = new Vector2(0, 115);

            Image abBg = advBox.AddComponent<Image>();
            abBg.color = new Color(0.04f, 0.06f, 0.09f, 0.85f);

            Outline abOutline = advBox.AddComponent<Outline>();
            abOutline.effectColor = new Color(0.18f, 0.26f, 0.38f, 0.6f);
            abOutline.effectDistance = new Vector2(1, -1);

            GameObject abTextObj = new GameObject("Text");
            abTextObj.transform.SetParent(advBox.transform, false);
            RectTransform abtRect = abTextObj.AddComponent<RectTransform>();
            StretchFull(abtRect);
            abtRect.offsetMin = new Vector2(10, 8);
            abtRect.offsetMax = new Vector2(-10, -8);

            TextMeshProUGUI abTmp = abTextObj.AddComponent<TextMeshProUGUI>();
            abTmp.fontSize = 10f;
            abTmp.lineSpacing = 1.5f;
            abTmp.alignment = TextAlignmentOptions.TopLeft;
            abTmp.color = new Color(0.72f, 0.78f, 0.84f);
            abTmp.textWrappingMode = TextWrappingModes.Normal;
            abTmp.text = 
                "<color=#FFA726><b>\"Infected display hyper-kinesis, sensory frenzy, and loss of pain response due to rapid frontal necrosis.\"</b></color>\n" +
                "<color=#45B7FF><b>• COMBAT DIRECTIVE:</b></color> <color=#CFD8DC>Cranial destruction is sole neutralization vector. Prioritize retreat.</color>\n" +
                "<color=#45B7FF><b>• HYDRATION SAFETY:</b></color> <color=#CFD8DC>Boil tap water min. 10 minutes. Dehydration accelerates susceptibility.</color>";

            // --- COLUMN 3: TRAITS & FIELD MANIFEST ---
            GameObject col3 = CreateDossierCard(colsObj.transform, "Col3_Manifest", 460);
            CreateSectionHeader(col3.transform, "03 // TRAITS & FIELD MANIFEST");
            CreateSubLabel(col3.transform, "RECORDED TRAITS & CONDITIONAL FLAWS:");
            so.FindProperty("summaryTraitsText").objectReferenceValue = CreateDossierRow(col3.transform, 11.5f, FontStyles.Normal, Color.white);
            CreateDivider(col3.transform);
            CreateSubLabel(col3.transform, "DECLARED SURVIVAL SUPPLIES:");
            so.FindProperty("summaryInventoryText").objectReferenceValue = CreateDossierRow(col3.transform, 11.5f, FontStyles.Normal, Color.white);
            CreateDivider(col3.transform);
            CreateSubLabel(col3.transform, "FEMA CIVIL DEFENSE EDICT // SEC-44B COMPLIANCE:");

            // Framed Edict Box (Tightened to 105px)
            GameObject edictBox = new GameObject("EdictBox");
            edictBox.transform.SetParent(col3.transform, false);
            RectTransform ebRect = edictBox.AddComponent<RectTransform>();
            ebRect.sizeDelta = new Vector2(0, 105);

            Image ebBg = edictBox.AddComponent<Image>();
            ebBg.color = new Color(0.04f, 0.06f, 0.09f, 0.85f);

            Outline ebOutline = edictBox.AddComponent<Outline>();
            ebOutline.effectColor = new Color(0.18f, 0.26f, 0.38f, 0.6f);
            ebOutline.effectDistance = new Vector2(1, -1);

            GameObject ebTextObj = new GameObject("Text");
            ebTextObj.transform.SetParent(edictBox.transform, false);
            RectTransform ebtRect = ebTextObj.AddComponent<RectTransform>();
            StretchFull(ebtRect);
            ebtRect.offsetMin = new Vector2(10, 8);
            ebtRect.offsetMax = new Vector2(-10, -8);

            TextMeshProUGUI ebTmp = ebTextObj.AddComponent<TextMeshProUGUI>();
            ebTmp.fontSize = 10f;
            ebTmp.lineSpacing = 1.5f;
            ebTmp.alignment = TextAlignmentOptions.TopLeft;
            ebTmp.color = new Color(0.72f, 0.78f, 0.84f);
            ebTmp.textWrappingMode = TextWrappingModes.Normal;
            ebTmp.text = 
                "<color=#EF5350><b>• DISARMAMENT EXEMPTION:</b></color> <color=#CFD8DC>Declared defensive sidearms licensed under Martial Edict 2026-09.</color>\n" +
                "<color=#EF5350><b>• EVACUATION CORRIDOR:</b></color> <color=#CFD8DC>Proceed along I-80 North checkpoints. Mandatory curfew 19:00 - 06:00.</color>\n" +
                "<color=#EF5350><b>• CONCEALMENT PENALTY:</b></color> <color=#CFD8DC>Concealing bite wounds is high treason punishable by internment.</color>\n" +
                "<color=#78909C>FIELD EXAMINER: [Agent M. Vance // #8841-B]</color>    <color=#66BB6A><b>STAMP: VERIFIED</b></color>";

            // ==========================================
            // 3. BOTTOM FOOTER DOCK (Height: 72px)
            // ==========================================
            GameObject footerBar = new GameObject("FooterBar");
            footerBar.transform.SetParent(frame.transform, false);
            RectTransform footRect = footerBar.AddComponent<RectTransform>();
            footRect.anchorMin = new Vector2(0, 0);
            footRect.anchorMax = new Vector2(1, 0);
            footRect.pivot = new Vector2(0.5f, 0);
            footRect.sizeDelta = new Vector2(0, 72);
            footRect.anchoredPosition = Vector2.zero;

            Image footBg = footerBar.AddComponent<Image>();
            footBg.color = new Color(0.06f, 0.08f, 0.12f, 1f);

            GameObject footLine = new GameObject("TopDivider");
            footLine.transform.SetParent(footerBar.transform, false);
            RectTransform flRect = footLine.AddComponent<RectTransform>();
            flRect.anchorMin = new Vector2(0, 1);
            flRect.anchorMax = new Vector2(1, 1);
            flRect.pivot = new Vector2(0.5f, 1);
            flRect.sizeDelta = new Vector2(0, 1.5f);
            flRect.anchoredPosition = Vector2.zero;
            Image flImg = footLine.AddComponent<Image>();
            flImg.color = new Color(0.18f, 0.28f, 0.40f, 0.8f);

            // Warning Notice Left
            GameObject warnObj = new GameObject("WarningNotice");
            warnObj.transform.SetParent(footerBar.transform, false);
            RectTransform wRect = warnObj.AddComponent<RectTransform>();
            wRect.anchorMin = new Vector2(0, 0.5f);
            wRect.anchorMax = new Vector2(0, 0.5f);
            wRect.pivot = new Vector2(0, 0.5f);
            wRect.sizeDelta = new Vector2(680, 50);
            wRect.anchoredPosition = new Vector2(24, 0);

            TextMeshProUGUI wTmp = warnObj.AddComponent<TextMeshProUGUI>();
            wTmp.text = "<color=#EF5350><b>[!] OFFICIAL NOTICE:</b></color> <color=#90A4AE>FALSE DECLARATIONS PUNISHABLE UNDER FEMA DIRECTIVE 44-B. PROCEED TO DESIGNATED EVACUATION SECTOR IMMEDIATELY.</color>";
            wTmp.fontSize = 11.5f;
            wTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Start Over Button
            Button restartBtn = CreateStandardButton(footerBar.transform, "<< START OVER", () => screen.SendMessage("RestartCharacterCreation", UnityEngine.SendMessageOptions.DontRequireReceiver), new Vector2(210, 46), new Color(0.85f, 0.90f, 0.95f));
            restartBtn.gameObject.name = "Btn_StartOver";
            RectTransform rRect = restartBtn.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(1, 0.5f);
            rRect.anchorMax = new Vector2(1, 0.5f);
            rRect.pivot = new Vector2(1, 0.5f);
            rRect.anchoredPosition = new Vector2(-386, 0);

            Image rBg = restartBtn.GetComponent<Image>();
            rBg.color = new Color(0.12f, 0.16f, 0.22f, 0.95f);

            Outline rOutline = restartBtn.gameObject.AddComponent<Outline>();
            rOutline.effectColor = new Color(0.35f, 0.45f, 0.58f, 0.75f);
            rOutline.effectDistance = new Vector2(1, -1);

            TextMeshProUGUI rTmp = restartBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (rTmp != null)
            {
                rTmp.fontSize = 13.5f;
            }

            so.FindProperty("restartCreationButton").objectReferenceValue = restartBtn;

            // Action Button Right
            Button beginBtn = CreateStandardButton(footerBar.transform, "CONFIRM & COMMENCE SURVIVAL >>", () => screen.SendMessage("OnBeginSurvivalClicked", UnityEngine.SendMessageOptions.DontRequireReceiver), new Vector2(350, 46));
            RectTransform btnRect = beginBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 0.5f);
            btnRect.anchorMax = new Vector2(1, 0.5f);
            btnRect.pivot = new Vector2(1, 0.5f);
            btnRect.anchoredPosition = new Vector2(-24, 0);

            Image btnImg = beginBtn.GetComponent<Image>();
            btnImg.color = new Color(0.78f, 0.16f, 0.16f, 1f); // Emergency Hazard Red

            TextMeshProUGUI btnText = beginBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.fontSize = 15f;
            }

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
            rect.sizeDelta = new Vector2(470, 440);

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
            bRect.sizeDelta = new Vector2(0, 180);
            bRect.anchoredPosition = new Vector2(0, -3);
            bannerObj.AddComponent<RectMask2D>();

            // Illustration image (Cover fit with AspectRatioFitter - never stretch)
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

            AspectRatioFitter arf = imgObj.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            float spriteRatio = (jobSprite != null && jobSprite.rect.height > 0)
                ? (jobSprite.rect.width / jobSprite.rect.height)
                : 1.5f;
            arf.aspectRatio = spriteRatio;

            // Pre-calculate exact sizeDelta for instant editor layout without waiting for layout pass
            float containerW = 468f;
            float neededH = containerW / spriteRatio;
            imgRect.sizeDelta = new Vector2(0f, Mathf.Max(0f, neededH - 180f));
            imgRect.anchoredPosition = Vector2.zero;

            // Dark bottom gradient overlay
            GameObject gradObj = new GameObject("FadeOverlay");
            gradObj.transform.SetParent(bannerObj.transform, false);
            RectTransform gRect = gradObj.AddComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0f);
            gRect.anchorMax = new Vector2(1f, 0f);
            gRect.pivot = new Vector2(0.5f, 0f);
            gRect.sizeDelta = new Vector2(0, 75);
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
            tRect.sizeDelta = new Vector2(-40, 32);
            tRect.anchoredPosition = new Vector2(0, -190);
            TextMeshProUGUI tTmp = titleObj.AddComponent<TextMeshProUGUI>();
            tTmp.text = displayTitle;
            tTmp.fontSize = 22;
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
            dRect.sizeDelta = new Vector2(-40, 54);
            dRect.anchoredPosition = new Vector2(0, -226);
            TextMeshProUGUI dTmp = descObj.AddComponent<TextMeshProUGUI>();
            dTmp.text = $"{job.description}\n<color=#888888><i>\"{cleanQuote}\"</i></color>";
            dTmp.fontSize = 11.5f;
            dTmp.lineSpacing = 2f;
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
            stRect.sizeDelta = new Vector2(-40, 52);
            stRect.anchoredPosition = new Vector2(0, -286);

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
            stTextRect.offsetMax = new Vector2(-8, -4);
            TextMeshProUGUI stTmp = sTextObj.AddComponent<TextMeshProUGUI>();
            stTmp.text = $"<color=#FFFFFF><b>PERKS:</b></color> {GetJobPerkSummary(job)}\n<color=#FFFFFF><b>START:</b></color> {GetJobStartSummary(job)}";
            stTmp.fontSize = 11.5f;
            stTmp.lineSpacing = 2.5f;
            stTmp.alignment = TextAlignmentOptions.Left;
            stTmp.color = new Color(0.68f, 0.68f, 0.68f);
            stTmp.raycastTarget = false;

            // Select Button
            GameObject btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(innerCard.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0f, 0f);
            btnRect.anchorMax = new Vector2(1f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.sizeDelta = new Vector2(-40, 42);
            btnRect.anchoredPosition = new Vector2(0, 14);

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
            btTmp.fontSize = 14.5f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.characterSpacing = 2.5f;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;
            btTmp.raycastTarget = false;

            JobCardView jobCardView = cardObj.AddComponent<JobCardView>();
            jobCardView.Setup(job, () => onSelect?.Invoke());
        }

        private static GameObject BuildHobbyCardTemplate(Transform parent)
        {
            GameObject cardObj = new GameObject("HobbyCardTemplate");
            cardObj.transform.SetParent(parent, false);
            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(345, 310);

            // Card border outline (1px dark border)
            Image borderImg = cardObj.AddComponent<Image>();
            borderImg.color = new Color(0.20f, 0.22f, 0.26f, 1f);

            // Inner dark panel (#080A0E)
            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(cardObj.transform, false);
            RectTransform inRect = innerObj.AddComponent<RectTransform>();
            StretchFull(inRect);
            inRect.offsetMin = new Vector2(1, 1);
            inRect.offsetMax = new Vector2(-1, -1);
            Image innerBg = innerObj.AddComponent<Image>();
            innerBg.color = new Color(0.035f, 0.045f, 0.065f, 1f);

            // Top accent bar (3px line)
            GameObject topBar = new GameObject("TopBorder");
            topBar.transform.SetParent(innerObj.transform, false);
            RectTransform tbRect = topBar.AddComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(0, 3);
            tbRect.anchoredPosition = Vector2.zero;
            Image topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(0.83f, 0.33f, 0f);

            // Banner Container (Masked, 114px high)
            GameObject bannerObj = new GameObject("Banner");
            bannerObj.transform.SetParent(innerObj.transform, false);
            RectTransform bRect = bannerObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 1f);
            bRect.anchorMax = new Vector2(1f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.sizeDelta = new Vector2(0, 114);
            bRect.anchoredPosition = new Vector2(0, -3);
            bannerObj.AddComponent<RectMask2D>();
            Image bBg = bannerObj.AddComponent<Image>();
            bBg.color = new Color(0.05f, 0.07f, 0.10f, 0.85f);

            // Larger Trait Illustration Icon (112x112)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(bannerObj.transform, false);
            RectTransform icRect = iconObj.AddComponent<RectTransform>();
            icRect.anchorMin = new Vector2(0.5f, 0.5f);
            icRect.anchorMax = new Vector2(0.5f, 0.5f);
            icRect.pivot = new Vector2(0.5f, 0.5f);
            icRect.sizeDelta = new Vector2(112, 112);
            icRect.anchoredPosition = new Vector2(0, -1);
            Image icImg = iconObj.AddComponent<Image>();
            icImg.preserveAspect = true;
            icImg.raycastTarget = false;

            // Subtle dark bottom fade overlay
            GameObject gradObj = new GameObject("FadeOverlay");
            gradObj.transform.SetParent(bannerObj.transform, false);
            RectTransform gRect = gradObj.AddComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0f);
            gRect.anchorMax = new Vector2(1f, 0f);
            gRect.pivot = new Vector2(0.5f, 0f);
            gRect.sizeDelta = new Vector2(0, 34);
            gRect.anchoredPosition = Vector2.zero;
            Image gradImg = gradObj.AddComponent<Image>();
            gradImg.sprite = GetBottomFadeGradient();
            gradImg.raycastTarget = false;

            // Archetype Category Badge (Top-Right of Banner)
            GameObject badgeObj = new GameObject("CategoryBadge");
            badgeObj.transform.SetParent(bannerObj.transform, false);
            RectTransform catRect = badgeObj.AddComponent<RectTransform>();
            catRect.anchorMin = new Vector2(1f, 1f);
            catRect.anchorMax = new Vector2(1f, 1f);
            catRect.pivot = new Vector2(1f, 1f);
            catRect.sizeDelta = new Vector2(104f, 22f);
            catRect.anchoredPosition = new Vector2(-6f, -6f);
            Image catBgImg = badgeObj.AddComponent<Image>();
            catBgImg.color = new Color(0.05f, 0.08f, 0.12f, 0.92f);
            catBgImg.raycastTarget = false;
            Outline catOl = badgeObj.AddComponent<Outline>();
            catOl.effectColor = new Color(1f, 1f, 1f, 0.15f);
            catOl.effectDistance = new Vector2(1f, -1f);

            GameObject catTxtObj = new GameObject("BadgeText");
            catTxtObj.transform.SetParent(badgeObj.transform, false);
            RectTransform ctRect = catTxtObj.AddComponent<RectTransform>();
            StretchFull(ctRect);
            TextMeshProUGUI catTmp = catTxtObj.AddComponent<TextMeshProUGUI>();
            catTmp.text = "TRAIT";
            catTmp.fontSize = 11f;
            catTmp.fontStyle = FontStyles.Bold;
            catTmp.alignment = TextAlignmentOptions.Center;
            catTmp.color = Color.white;
            catTmp.raycastTarget = false;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(innerObj.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.sizeDelta = new Vector2(-20, 28);
            tRect.anchoredPosition = new Vector2(0, -124);
            TextMeshProUGUI tTmp = titleObj.AddComponent<TextMeshProUGUI>();
            tTmp.fontSize = 19f;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.characterSpacing = 1.5f;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = Color.white;
            tTmp.raycastTarget = false;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(innerObj.transform, false);
            RectTransform dRect = descObj.AddComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 1f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.pivot = new Vector2(0.5f, 1f);
            dRect.sizeDelta = new Vector2(-24, 44);
            dRect.anchoredPosition = new Vector2(0, -154);
            TextMeshProUGUI dTmp = descObj.AddComponent<TextMeshProUGUI>();
            dTmp.fontSize = 13f;
            dTmp.lineSpacing = 2.5f;
            dTmp.alignment = TextAlignmentOptions.Center;
            dTmp.color = new Color(0.84f, 0.88f, 0.92f);
            dTmp.textWrappingMode = TextWrappingModes.Normal;
            dTmp.overflowMode = TextOverflowModes.Ellipsis;
            dTmp.raycastTarget = false;

            // Stats Preview Box (Dark pill background)
            GameObject statsObj = new GameObject("StatsBox");
            statsObj.transform.SetParent(innerObj.transform, false);
            RectTransform stRect = statsObj.AddComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0f, 1f);
            stRect.anchorMax = new Vector2(1f, 1f);
            stRect.pivot = new Vector2(0.5f, 1f);
            stRect.sizeDelta = new Vector2(-24, 28);
            stRect.anchoredPosition = new Vector2(0, -206);
            Image statsBgImg = statsObj.AddComponent<Image>();
            statsBgImg.color = new Color(0.04f, 0.09f, 0.16f, 0.75f);

            GameObject stTextObj = new GameObject("Text");
            stTextObj.transform.SetParent(statsObj.transform, false);
            RectTransform sttRect = stTextObj.AddComponent<RectTransform>();
            StretchFull(sttRect);
            TextMeshProUGUI stTmp = stTextObj.AddComponent<TextMeshProUGUI>();
            stTmp.fontSize = 12.5f;
            stTmp.fontStyle = FontStyles.Bold;
            stTmp.alignment = TextAlignmentOptions.Center;
            stTmp.color = new Color(0.42f, 0.78f, 1.0f);
            stTmp.raycastTarget = false;

            // Bottom Interactive Action Strip
            GameObject btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(innerObj.transform, false);
            RectTransform bBtnRect = btnObj.AddComponent<RectTransform>();
            bBtnRect.anchorMin = new Vector2(0f, 0f);
            bBtnRect.anchorMax = new Vector2(1f, 0f);
            bBtnRect.pivot = new Vector2(0.5f, 0f);
            bBtnRect.sizeDelta = new Vector2(-24, 38);
            bBtnRect.anchoredPosition = new Vector2(0, 12);

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.08f, 0.10f, 0.14f, 0.98f);
            Button bBtn = btnObj.AddComponent<Button>();
            bBtn.targetGraphic = btnBg;

            ColorBlock cb = bBtn.colors;
            cb.normalColor = new Color(0.08f, 0.10f, 0.14f, 0.98f);
            cb.highlightedColor = new Color(0.75f, 0.34f, 0.06f, 1f);
            cb.pressedColor = new Color(0.50f, 0.20f, 0.02f, 1f);
            cb.selectedColor = cb.normalColor;
            bBtn.colors = cb;

            GameObject btObj = new GameObject("Text");
            btObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btObj.AddComponent<RectTransform>();
            StretchFull(btRect);
            TextMeshProUGUI btTmp = btObj.AddComponent<TextMeshProUGUI>();
            btTmp.text = "SELECT TRAIT";
            btTmp.fontSize = 13.5f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.characterSpacing = 2f;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;
            btTmp.raycastTarget = false;

            HobbyCardView cardView = cardObj.AddComponent<HobbyCardView>();
            SerializedObject cso = new SerializedObject(cardView);
            cso.FindProperty("cardBorder").objectReferenceValue = borderImg;
            cso.FindProperty("topAccentBar").objectReferenceValue = topBarImg;
            cso.FindProperty("iconImage").objectReferenceValue = icImg;
            cso.FindProperty("titleText").objectReferenceValue = tTmp;
            cso.FindProperty("descriptionText").objectReferenceValue = dTmp;
            cso.FindProperty("statsPreviewText").objectReferenceValue = stTmp;
            cso.FindProperty("statsBg").objectReferenceValue = statsBgImg;
            cso.FindProperty("selectButton").objectReferenceValue = bBtn;
            cso.FindProperty("buttonText").objectReferenceValue = btTmp;
            cso.FindProperty("categoryBadgeBg").objectReferenceValue = catBgImg;
            cso.FindProperty("categoryBadgeText").objectReferenceValue = catTmp;
            cso.ApplyModifiedProperties();

            return cardObj;
        }

        private static TextMeshProUGUI CreateTrainingRow(Transform parent, string label, Color statColor, UnityEngine.Events.UnityAction onAdd, UnityEngine.Events.UnityAction onSub, UnityEngine.Events.UnityAction onInspect)
        {
            GameObject row = new GameObject("Row_" + label);
            row.transform.SetParent(parent, false);
            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(495, 46);

            // Row background (interactive button to inspect stat)
            Image rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0.06f, 0.06f, 0.06f, 0.85f);
            Button rowBtn = row.AddComponent<Button>();
            rowBtn.targetGraphic = rowBg;
            ColorBlock rcb = rowBtn.colors;
            rcb.normalColor = new Color(0.06f, 0.06f, 0.06f, 0.85f);
            rcb.highlightedColor = new Color(0.12f, 0.12f, 0.12f, 0.98f);
            rcb.pressedColor = new Color(0.16f, 0.16f, 0.16f, 1f);
            rcb.selectedColor = rcb.normalColor;
            rowBtn.colors = rcb;
            if (onInspect != null) rowBtn.onClick.AddListener(onInspect);
            row.AddComponent<EventTrigger>();

            // Left colored accent indicator (3px vertical stripe)
            GameObject indObj = new GameObject("StatPip");
            indObj.transform.SetParent(row.transform, false);
            RectTransform indRect = indObj.AddComponent<RectTransform>();
            indRect.anchorMin = new Vector2(0f, 0f);
            indRect.anchorMax = new Vector2(0f, 1f);
            indRect.pivot = new Vector2(0f, 0.5f);
            indRect.sizeDelta = new Vector2(3, 0);
            indRect.anchoredPosition = Vector2.zero;
            indObj.AddComponent<Image>().color = statColor;

            // Stat Label (Left side)
            GameObject lblObj = new GameObject("NameLabel");
            lblObj.transform.SetParent(row.transform, false);
            RectTransform lblRect = lblObj.AddComponent<RectTransform>();
            lblRect.anchorMin = new Vector2(0f, 0f);
            lblRect.anchorMax = new Vector2(0f, 1f);
            lblRect.pivot = new Vector2(0f, 0.5f);
            lblRect.sizeDelta = new Vector2(125, 0);
            lblRect.anchoredPosition = new Vector2(14, 0);
            TextMeshProUGUI lblTmp = lblObj.AddComponent<TextMeshProUGUI>();
            lblTmp.text = label;
            lblTmp.fontSize = 14;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.color = statColor;
            lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
            lblTmp.raycastTarget = false;

            // Btn_Minus [-]
            GameObject mBtnObj = new GameObject("Btn_Minus");
            mBtnObj.transform.SetParent(row.transform, false);
            RectTransform mbRect = mBtnObj.AddComponent<RectTransform>();
            mbRect.anchorMin = new Vector2(0f, 0.5f);
            mbRect.anchorMax = new Vector2(0f, 0.5f);
            mbRect.pivot = new Vector2(0.5f, 0.5f);
            mbRect.sizeDelta = new Vector2(34, 32);
            mbRect.anchoredPosition = new Vector2(158, 0);
            Image mbBg = mBtnObj.AddComponent<Image>();
            mbBg.color = new Color(0.16f, 0.16f, 0.16f, 1f);
            Button mBtn = mBtnObj.AddComponent<Button>();
            mBtn.targetGraphic = mbBg;
            ColorBlock mcb = mBtn.colors;
            mcb.normalColor = new Color(0.16f, 0.16f, 0.16f, 1f);
            mcb.highlightedColor = new Color(0.70f, 0.25f, 0.15f, 1f);
            mcb.pressedColor = new Color(0.50f, 0.15f, 0.10f, 1f);
            mBtn.colors = mcb;
            if (onSub != null) mBtn.onClick.AddListener(onSub);

            GameObject mTextObj = new GameObject("Text");
            mTextObj.transform.SetParent(mBtnObj.transform, false);
            RectTransform mtRect = mTextObj.AddComponent<RectTransform>();
            StretchFull(mtRect);
            TextMeshProUGUI mtTmp = mTextObj.AddComponent<TextMeshProUGUI>();
            mtTmp.text = "-";
            mtTmp.fontSize = 20;
            mtTmp.fontStyle = FontStyles.Bold;
            mtTmp.alignment = TextAlignmentOptions.Center;
            mtTmp.color = Color.white;
            mtTmp.raycastTarget = false;

            // Pip & Value Text (Center area)
            GameObject textObj = new GameObject("ValueDisplay");
            textObj.transform.SetParent(row.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.offsetMin = new Vector2(180, 0);
            tRect.offsetMax = new Vector2(-48, 0);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "<color=#2A2A2A>■ ■ ■ ■ ■ ■ ■ ■ ■ ■</color>   <b>2</b>";
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;

            // Btn_Plus [+]
            GameObject pBtnObj = new GameObject("Btn_Plus");
            pBtnObj.transform.SetParent(row.transform, false);
            RectTransform pbRect = pBtnObj.AddComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(1f, 0.5f);
            pbRect.anchorMax = new Vector2(1f, 0.5f);
            pbRect.pivot = new Vector2(0.5f, 0.5f);
            pbRect.sizeDelta = new Vector2(34, 32);
            pbRect.anchoredPosition = new Vector2(-22, 0);
            Image pbBg = pBtnObj.AddComponent<Image>();
            pbBg.color = new Color(0.16f, 0.16f, 0.16f, 1f);
            Button pBtn = pBtnObj.AddComponent<Button>();
            pBtn.targetGraphic = pbBg;
            ColorBlock pcb = pBtn.colors;
            pcb.normalColor = new Color(0.16f, 0.16f, 0.16f, 1f);
            pcb.highlightedColor = Color.Lerp(new Color(0.22f, 0.22f, 0.22f, 1f), statColor, 0.65f);
            pcb.pressedColor = statColor * 0.85f;
            pBtn.colors = pcb;
            if (onAdd != null) pBtn.onClick.AddListener(onAdd);

            GameObject ptObj = new GameObject("Text");
            ptObj.transform.SetParent(pBtnObj.transform, false);
            RectTransform ptRect = ptObj.AddComponent<RectTransform>();
            StretchFull(ptRect);
            TextMeshProUGUI ptTmp = ptObj.AddComponent<TextMeshProUGUI>();
            ptTmp.text = "+";
            ptTmp.fontSize = 20;
            ptTmp.fontStyle = FontStyles.Bold;
            ptTmp.alignment = TextAlignmentOptions.Center;
            ptTmp.color = Color.white;
            ptTmp.raycastTarget = false;

            return tmp;
        }

        private static TextMeshProUGUI CreateDossierRow(Transform parent, float fontSize, FontStyles style, Color color)
        {
            GameObject obj = new GameObject("Row");
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            ContentSizeFitter csf = obj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            return tmp;
        }

        private static GameObject CreateDossierCard(Transform parent, string name, float preferredWidth)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = preferredWidth;
            le.flexibleWidth = 1;
            le.preferredHeight = 580;
            le.flexibleHeight = 0;

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.10f, 0.15f, 0.95f); // Deep tactical slate card

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.16f, 0.24f, 0.35f, 0.8f);
            outline.effectDistance = new Vector2(1, -1);

            card.AddComponent<RectMask2D>();

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 5;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;

            return card;
        }

        private static void CreateSectionHeader(Transform parent, string title)
        {
            GameObject barObj = new GameObject("SectionHeader");
            barObj.transform.SetParent(parent, false);
            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 28);

            Image bg = barObj.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.16f, 0.25f, 0.95f); // Deep navy badge

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(barObj.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            StretchFull(tRect);
            tRect.offsetMin = new Vector2(10, 0);
            tRect.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = title;
            tmp.fontSize = 12.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.45f, 0.72f, 0.98f); // Light cyan blue
        }

        private static void CreateSubLabel(Transform parent, string text)
        {
            GameObject obj = new GameObject("SubLabel");
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 10.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.48f, 0.58f, 0.68f); // Muted slate

            ContentSizeFitter csf = obj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private static void CreateDivider(Transform parent)
        {
            GameObject line = new GameObject("Divider");
            line.transform.SetParent(parent, false);
            RectTransform rect = line.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 1);
            Image img = line.AddComponent<Image>();
            img.color = new Color(0.14f, 0.20f, 0.28f, 0.8f);
        }

        private static Sprite LoadSpriteSafe(string path)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && (ti.textureType != TextureImporterType.Sprite || !ti.alphaIsTransparency))
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
