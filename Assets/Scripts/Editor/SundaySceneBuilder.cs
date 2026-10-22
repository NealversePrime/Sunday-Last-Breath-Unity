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
            EditorApplication.update += CheckAutoBuild;
        }

        private static void CheckAutoBuild()
        {
            string flagPath = "Temp/scene_v50_auto.flag";
            if (File.Exists(flagPath))
            {
                EditorApplication.update -= CheckAutoBuild;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            Directory.CreateDirectory("Temp");
            File.WriteAllText(flagPath, "done");
            EditorApplication.update -= CheckAutoBuild;
            BuildScene();
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
            // Note: tick.mp3 is the 8-second ambient clock ticking track from Twine, NOT a short UI hover tick.
            // Leaving buttonHoverClip unassigned (null) avoids firing an 8-second clock loop on hover.
            AudioClip hoverSfx = null;

            SerializedObject audioSo = new SerializedObject(audioMgr);
            if (menuMusic != null) audioSo.FindProperty("menuMusicClip").objectReferenceValue = menuMusic;
            if (clickSfx != null) audioSo.FindProperty("buttonClickClip").objectReferenceValue = clickSfx;
            if (hoverSfx != null) audioSo.FindProperty("buttonHoverClip").objectReferenceValue = hoverSfx;
            audioSo.FindProperty("bgmVolume").floatValue = 0.75f;
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
            ApartmentHubScreen apartmentHubScreen = BuildApartmentHubScreen(screensObj.transform);
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
            gameSo.FindProperty("apartmentHubScreen").objectReferenceValue = apartmentHubScreen;
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
            prTmp.text = "<color=#FFA726>CLICK ANYWHERE OR SPACE TO ADVANCE</color> <color=#FFFFFF>>></color>";
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
            so.FindProperty("continuePrompt").objectReferenceValue = prTmp.gameObject;
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

                private static ApartmentHubScreen BuildApartmentHubScreen(Transform parent)
        {
            GameObject obj = new GameObject("ApartmentHubScreen", typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rootRect = obj.GetComponent<RectTransform>();
            StretchFull(rootRect);

            Image rootBg = obj.AddComponent<Image>();
            rootBg.color = new Color(0.04f, 0.045f, 0.06f, 1f);
            rootBg.raycastTarget = false;

            ApartmentHubScreen screen = obj.AddComponent<ApartmentHubScreen>();

            // =========================================================================
            // 1. TOP HUD BAR (46px Height - Authentic Twine #top-hud replica)
            // =========================================================================
            GameObject topHeader = new GameObject("TopHUD", typeof(RectTransform));
            topHeader.transform.SetParent(obj.transform, false);
            RectTransform thRect = topHeader.GetComponent<RectTransform>();
            thRect.anchorMin = new Vector2(0f, 1f);
            thRect.anchorMax = new Vector2(1f, 1f);
            thRect.pivot = new Vector2(0.5f, 1f);
            thRect.sizeDelta = new Vector2(0f, 46f);
            thRect.anchoredPosition = Vector2.zero;
            Image thBg = topHeader.AddComponent<Image>();
            thBg.color = new Color(0.05f, 0.055f, 0.07f, 0.98f);
            thBg.raycastTarget = false;

            // 1px bottom border on Top HUD
            GameObject thBorder = new GameObject("BottomBorder", typeof(RectTransform));
            thBorder.transform.SetParent(topHeader.transform, false);
            RectTransform thbRect = thBorder.GetComponent<RectTransform>();
            thbRect.anchorMin = new Vector2(0f, 0f);
            thbRect.anchorMax = new Vector2(1f, 0f);
            thbRect.pivot = new Vector2(0.5f, 0f);
            thbRect.sizeDelta = new Vector2(0f, 1f);
            thbRect.anchoredPosition = Vector2.zero;
            Image thbImg = thBorder.AddComponent<Image>();
            thbImg.color = new Color(0.20f, 0.24f, 0.30f, 0.85f);
            thbImg.raycastTarget = false;

            // LEFT: Tactical Location Badge
            TextMeshProUGUI locBadgeTmp = CreateTMPText(topHeader.transform, "LocationBadge", "■ UNIT 404 // THE ASCOTT RESIDENCY [4TH FLOOR]", 12f, new Color(0.72f, 0.82f, 0.94f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform lbr = locBadgeTmp.rectTransform;
            lbr.anchorMin = new Vector2(0f, 0.5f);
            lbr.anchorMax = new Vector2(0f, 0.5f);
            lbr.pivot = new Vector2(0f, 0.5f);
            lbr.anchoredPosition = new Vector2(20f, 0f);
            lbr.sizeDelta = new Vector2(380f, 32f);

            // CENTER: 4 VITAL BARS (Horizontal Row: HP, Stamina, Hunger, Thirst)
            GameObject vitalsRowObj = new GameObject("VitalsRow", typeof(RectTransform));
            vitalsRowObj.transform.SetParent(topHeader.transform, false);
            RectTransform vrRect = vitalsRowObj.GetComponent<RectTransform>();
            vrRect.anchorMin = new Vector2(0.5f, 0.5f);
            vrRect.anchorMax = new Vector2(0.5f, 0.5f);
            vrRect.pivot = new Vector2(0.5f, 0.5f);
            vrRect.sizeDelta = new Vector2(580f, 22f);
            vrRect.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup vHlg = vitalsRowObj.AddComponent<HorizontalLayoutGroup>();
            vHlg.spacing = 10f;
            vHlg.childAlignment = TextAnchor.MiddleCenter;
            vHlg.childControlWidth = true;
            vHlg.childControlHeight = true;
            vHlg.childForceExpandWidth = true;
            vHlg.childForceExpandHeight = true;

            Sprite hpIconSprite = LoadSpriteSafe("Assets/Sprites/UI/hp_icon.png");
            Sprite stmIconSprite = LoadSpriteSafe("Assets/Sprites/UI/stamina_icon.png");
            Sprite hngIconSprite = LoadSpriteSafe("Assets/Sprites/UI/hunger_icon.png");
            Sprite thrIconSprite = LoadSpriteSafe("Assets/Sprites/UI/thirst_icon.png");
            Sprite hpHazardSprite = LoadSpriteSafe("Assets/Sprites/UI/hp_hazard_stripe.png");

            CreateVitalBar(vitalsRowObj.transform, "Vital_HP", hpIconSprite, hpHazardSprite, new Color(0.85f, 0.15f, 0.15f),
                out Image hpFill, out TextMeshProUGUI hpValTmp, "100");
            CreateVitalBar(vitalsRowObj.transform, "Vital_Stamina", stmIconSprite, null, new Color(0.95f, 0.65f, 0.15f),
                out Image stmFill, out TextMeshProUGUI stmValTmp, "80");
            CreateVitalBar(vitalsRowObj.transform, "Vital_Hunger", hngIconSprite, null, new Color(0.63f, 0.32f, 0.18f),
                out Image hngFill, out TextMeshProUGUI hngValTmp, "100");
            CreateVitalBar(vitalsRowObj.transform, "Vital_Thirst", thrIconSprite, null, new Color(0.29f, 0.56f, 0.89f),
                out Image thrFill, out TextMeshProUGUI thrValTmp, "100");

            // RIGHT: DIGITAL CLOCK & DAY
            GameObject clockBlock = new GameObject("ClockBlock", typeof(RectTransform));
            clockBlock.transform.SetParent(topHeader.transform, false);
            RectTransform cbRect = clockBlock.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(1f, 0.5f);
            cbRect.anchorMax = new Vector2(1f, 0.5f);
            cbRect.pivot = new Vector2(1f, 0.5f);
            cbRect.anchoredPosition = new Vector2(-24f, 0f);
            cbRect.sizeDelta = new Vector2(130f, 44f);

            TextMeshProUGUI clockTimeTmp = CreateTMPText(clockBlock.transform, "ClockTime", "23:45", 20f, new Color(0.72f, 0.86f, 0.98f), TextAlignmentOptions.Right, FontStyles.Bold);
            RectTransform ctRect = clockTimeTmp.rectTransform;
            ctRect.anchorMin = new Vector2(0f, 0.4f);
            ctRect.anchorMax = new Vector2(1f, 1f);
            ctRect.offsetMin = Vector2.zero;
            ctRect.offsetMax = Vector2.zero;

            TextMeshProUGUI clockDayTmp = CreateTMPText(clockBlock.transform, "ClockDay", "DAY 0", 10f, new Color(0.55f, 0.55f, 0.55f), TextAlignmentOptions.Right, FontStyles.Bold);
            RectTransform cdRect = clockDayTmp.rectTransform;
            cdRect.anchorMin = new Vector2(0f, 0f);
            cdRect.anchorMax = new Vector2(1f, 0.4f);
            cdRect.offsetMin = Vector2.zero;
            cdRect.offsetMax = Vector2.zero;

            // =========================================================================
            // 2. TOP SCREEN: VISUAL STAGE & 2D VIEWPORT (Upper ~55% of Screen)
            // =========================================================================
            GameObject topScreen = new GameObject("TopScreen_VisualStage", typeof(RectTransform));
            topScreen.transform.SetParent(obj.transform, false);
            RectTransform tsRect = topScreen.GetComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0f, 0.44f);
            tsRect.anchorMax = new Vector2(1f, 1f);
            tsRect.offsetMin = Vector2.zero;
            tsRect.offsetMax = new Vector2(0f, -46f);

            // Framed Viewport Container (Centered, 16:9 widescreen ratio, expansive room panorama)
            GameObject framedStage = new GameObject("FramedStage", typeof(RectTransform));
            framedStage.transform.SetParent(topScreen.transform, false);
            RectTransform fsRect = framedStage.GetComponent<RectTransform>();
            fsRect.anchorMin = new Vector2(0.5f, 0.5f);
            fsRect.anchorMax = new Vector2(0.5f, 0.5f);
            fsRect.pivot = new Vector2(0.5f, 0.5f);
            fsRect.sizeDelta = new Vector2(1360f, 530f);
            fsRect.anchoredPosition = Vector2.zero;

            Image fsBg = framedStage.AddComponent<Image>();
            fsBg.color = Color.black;

            Outline fsOl = framedStage.AddComponent<Outline>();
            fsOl.effectColor = new Color(0.24f, 0.28f, 0.35f, 0.95f);
            fsOl.effectDistance = new Vector2(2f, -2f);

            // Layer 1: Living Room Backdrop
            GameObject roomObj = new GameObject("RoomBackdrop", typeof(RectTransform));
            roomObj.transform.SetParent(framedStage.transform, false);
            RectTransform roomRect = roomObj.GetComponent<RectTransform>();
            StretchFull(roomRect);
            Image roomImg = roomObj.AddComponent<Image>();
            roomImg.color = Color.white;
            roomImg.preserveAspect = false;
            roomImg.raycastTarget = false;
            FrameAnimationPlayer roomAnim = roomObj.AddComponent<FrameAnimationPlayer>();
            List<Sprite> redFrames = LoadSpritesFromFolder("Assets/Sprites/Prologue/LivingRoomRedFrames");
            if (redFrames.Count > 0)
            {
                AssignFramesToPlayer(roomAnim, redFrames, 8f, true);
                roomImg.sprite = redFrames[0];
            }
            else
            {
                Sprite roomSprite = LoadSpriteSafe("Assets/Sprites/Prologue/bg_living_room_red.png");
                if (roomSprite != null) roomImg.sprite = roomSprite;
            }

            // Layer 2: CRT TV Static
            GameObject staticObj = new GameObject("TVStatic", typeof(RectTransform));
            staticObj.transform.SetParent(framedStage.transform, false);
            RectTransform staticRect = staticObj.GetComponent<RectTransform>();
            StretchFull(staticRect);
            Image staticImg = staticObj.AddComponent<Image>();
            staticImg.color = Color.white;
            staticImg.raycastTarget = false;
            staticObj.SetActive(false);

            // Layer 3: CRT Scanlines & Vignette
            GameObject crtObj = new GameObject("CRTScanlineOverlay", typeof(RectTransform));
            crtObj.transform.SetParent(framedStage.transform, false);
            RectTransform crtRect = crtObj.GetComponent<RectTransform>();
            StretchFull(crtRect);
            Image crtImg = crtObj.AddComponent<Image>();
            crtImg.color = new Color(0f, 0f, 0f, 0.15f);
            crtImg.raycastTarget = false;

            // =========================================================================
            // 3. CENTER SEAM BEZEL: 3DS / FALLOUT TACTICAL CONSOLE SWITCHER
            // =========================================================================
            GameObject seamBezel = new GameObject("CenterSeam_ConsoleSwitcher", typeof(RectTransform));
            seamBezel.transform.SetParent(obj.transform, false);
            RectTransform sbRect = seamBezel.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0f, 0.40f);
            sbRect.anchorMax = new Vector2(1f, 0.44f);
            sbRect.offsetMin = Vector2.zero;
            sbRect.offsetMax = Vector2.zero;

            Image sbBg = seamBezel.AddComponent<Image>();
            sbBg.color = new Color(0.08f, 0.09f, 0.12f, 1f);
            sbBg.raycastTarget = true;

            // Top Seam Line (2px Crimson #e74c3c)
            GameObject topSeam = new GameObject("TopCrimsonSeam", typeof(RectTransform));
            topSeam.transform.SetParent(seamBezel.transform, false);
            RectTransform tsLineRect = topSeam.GetComponent<RectTransform>();
            tsLineRect.anchorMin = new Vector2(0f, 1f); tsLineRect.anchorMax = new Vector2(1f, 1f);
            tsLineRect.pivot = new Vector2(0.5f, 1f);
            tsLineRect.sizeDelta = new Vector2(0f, 2f);
            Image tsLineImg = topSeam.AddComponent<Image>();
            tsLineImg.color = new Color(0.91f, 0.30f, 0.24f, 0.95f);

            // Bottom Seam Line (1px Steel)
            GameObject botSeam = new GameObject("BottomSteelSeam", typeof(RectTransform));
            botSeam.transform.SetParent(seamBezel.transform, false);
            RectTransform bsLineRect = botSeam.GetComponent<RectTransform>();
            bsLineRect.anchorMin = new Vector2(0f, 0f); bsLineRect.anchorMax = new Vector2(1f, 0f);
            bsLineRect.pivot = new Vector2(0.5f, 0f);
            bsLineRect.sizeDelta = new Vector2(0f, 1f);
            Image bsLineImg = botSeam.AddComponent<Image>();
            bsLineImg.color = new Color(0.20f, 0.24f, 0.30f, 0.8f);

            // Centered Mode Buttons Row
            GameObject btnRow = new GameObject("ButtonRow", typeof(RectTransform));
            btnRow.transform.SetParent(seamBezel.transform, false);
            RectTransform brRect = btnRow.GetComponent<RectTransform>();
            brRect.anchorMin = new Vector2(0.5f, 0.5f); brRect.anchorMax = new Vector2(0.5f, 0.5f);
            brRect.pivot = new Vector2(0.5f, 0.5f);
            brRect.sizeDelta = new Vector2(1100f, 34f);
            brRect.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup brHlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            brHlg.spacing = 16f;
            brHlg.childAlignment = TextAnchor.MiddleCenter;
            brHlg.childControlWidth = true; brHlg.childControlHeight = true;
            brHlg.childForceExpandWidth = true; brHlg.childForceExpandHeight = true;

            CreateConsoleModeButton(btnRow.transform, "Btn_ModeNarrative", "[1] NARRATIVE & LOG", out Button btnNarrative, out Image indNarrative, out TextMeshProUGUI _, true);
            CreateConsoleModeButton(btnRow.transform, "Btn_ModePDA", "[2] SURVIVOR PDA [TAB]", out Button btnPDA, out Image indPDA, out TextMeshProUGUI _, false);
            CreateConsoleModeButton(btnRow.transform, "Btn_ModeGear", "[3] TACTICAL GEAR [I]", out Button btnGear, out Image indGear, out TextMeshProUGUI gearLabelTmp, false);

            // Centered Content container for Tactical Gear button (Text + Notification Badge side-by-side)
            GameObject gearContent = new GameObject("Content", typeof(RectTransform));
            gearContent.transform.SetParent(btnGear.transform, false);
            RectTransform gcr = gearContent.GetComponent<RectTransform>();
            gcr.anchorMin = new Vector2(0.5f, 0.5f);
            gcr.anchorMax = new Vector2(0.5f, 0.5f);
            gcr.pivot = new Vector2(0.5f, 0.5f);
            gcr.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup gHlg = gearContent.AddComponent<HorizontalLayoutGroup>();
            gHlg.childAlignment = TextAnchor.MiddleCenter;
            gHlg.childControlWidth = false;
            gHlg.childControlHeight = false;
            gHlg.childForceExpandWidth = false;
            gHlg.childForceExpandHeight = false;
            gHlg.spacing = 8f;

            ContentSizeFitter gCsf = gearContent.AddComponent<ContentSizeFitter>();
            gCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            gCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Reparent and configure gearLabelTmp inside centered gearContent
            gearLabelTmp.transform.SetParent(gearContent.transform, false);
            RectTransform glTr = gearLabelTmp.rectTransform;
            glTr.anchorMin = new Vector2(0.5f, 0.5f);
            glTr.anchorMax = new Vector2(0.5f, 0.5f);
            glTr.pivot = new Vector2(0.5f, 0.5f);
            glTr.anchoredPosition = Vector2.zero;
            gearLabelTmp.textWrappingMode = TextWrappingModes.NoWrap;
            gearLabelTmp.alignment = TextAlignmentOptions.Center;
            ContentSizeFitter glCsf = gearLabelTmp.gameObject.AddComponent<ContentSizeFitter>();
            glCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            glCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Notification Badge directly beside text (Centered pivot, Canva sprite, Pulsing)
            GameObject gearBadgeObj = new GameObject("LootNotificationBadge", typeof(RectTransform));
            gearBadgeObj.transform.SetParent(gearContent.transform, false);
            RectTransform gbr = gearBadgeObj.GetComponent<RectTransform>();
            gbr.anchorMin = new Vector2(0.5f, 0.5f);
            gbr.anchorMax = new Vector2(0.5f, 0.5f);
            gbr.pivot = new Vector2(0.5f, 0.5f);
            gbr.anchoredPosition = Vector2.zero;
            gbr.sizeDelta = new Vector2(22f, 22f);

            LayoutElement gbLe = gearBadgeObj.AddComponent<LayoutElement>();
            gbLe.preferredWidth = 22f;
            gbLe.preferredHeight = 22f;
            gbLe.minWidth = 22f;
            gbLe.minHeight = 22f;

            Image gbImg = gearBadgeObj.AddComponent<Image>();
            gbImg.preserveAspect = true;
            gbImg.raycastTarget = false;
            Sprite notifSprite = LoadSpriteSafe("Assets/Sprites/UI/Notification.png");
            if (notifSprite != null)
            {
                gbImg.sprite = notifSprite;
                gbImg.color = Color.white;
            }
            else
            {
                gbImg.color = new Color(0.92f, 0.18f, 0.14f, 0.98f);
            }

            gearBadgeObj.AddComponent<PulsingBadge>();
            gearBadgeObj.SetActive(false);

            // =========================================================================
            // 4. BOTTOM SCREEN: INTERACTIVE TOUCH CONSOLE (Lower 40% of Screen)
            // =========================================================================
            GameObject botScreen = new GameObject("BottomScreen_TouchConsole", typeof(RectTransform));
            botScreen.transform.SetParent(obj.transform, false);
            RectTransform btsRect = botScreen.GetComponent<RectTransform>();
            btsRect.anchorMin = new Vector2(0f, 0f);
            btsRect.anchorMax = new Vector2(1f, 0.40f);
            btsRect.offsetMin = Vector2.zero;
            btsRect.offsetMax = Vector2.zero;

            Image btsBg = botScreen.AddComponent<Image>();
            btsBg.color = new Color(0.04f, 0.045f, 0.06f, 1f);
            btsBg.raycastTarget = false;

            // -------------------------------------------------------------------------
            // 4A. DOCK CONTAINER 1: NARRATIVE & LOG (MODE A - Active by Default)
            // -------------------------------------------------------------------------
            GameObject dockNarrative = new GameObject("Dock_Narrative", typeof(RectTransform));
            dockNarrative.transform.SetParent(botScreen.transform, false);
            RectTransform dnRect = dockNarrative.GetComponent<RectTransform>();
            StretchFull(dnRect);

            // Centered VN Column (Fixed width: 1140px, safe from text clipping)
            GameObject vnColObj = new GameObject("VNDialogueColumn", typeof(RectTransform));
            vnColObj.transform.SetParent(dockNarrative.transform, false);
            RectTransform vnColRect = vnColObj.GetComponent<RectTransform>();
            vnColRect.anchorMin = new Vector2(0.5f, 0f);
            vnColRect.anchorMax = new Vector2(0.5f, 1f);
            vnColRect.pivot = new Vector2(0.5f, 0.5f);
            vnColRect.offsetMin = new Vector2(-570f, 10f);
            vnColRect.offsetMax = new Vector2(570f, -10f);

            // MODE A: AWAKENING CONTAINER (Beats 1 to 3)
            GameObject awakeObj = new GameObject("AwakeningContainer", typeof(RectTransform));
            awakeObj.transform.SetParent(vnColObj.transform, false);
            RectTransform awakeRect = awakeObj.GetComponent<RectTransform>();
            StretchFull(awakeRect);

            // Click catcher strictly inside narrative container
            GameObject ccObj = new GameObject("DialogueClickCatcher", typeof(RectTransform));
            ccObj.transform.SetParent(awakeObj.transform, false);
            RectTransform ccRect = ccObj.GetComponent<RectTransform>();
            StretchFull(ccRect);
            Image ccImg = ccObj.AddComponent<Image>();
            ccImg.color = new Color(0f, 0f, 0f, 0.005f);
            ccImg.raycastTarget = true;
            Button ccBtn = ccObj.AddComponent<Button>();
            ccBtn.targetGraphic = ccImg;

            // --- BEAT 1: EAS WARNING ---
            GameObject b1Obj = new GameObject("Beat1Container", typeof(RectTransform));
            b1Obj.transform.SetParent(awakeObj.transform, false);
            RectTransform b1Rect = b1Obj.GetComponent<RectTransform>();
            StretchFull(b1Rect);

            TextMeshProUGUI b1Tmp = CreateTMPText(b1Obj.transform, "Beat1IntroText",
                "You turn away from the glass. The silence in your apartment breaks—not with a scream, but with an electronic tone pulsing from the television.",
                16f, new Color(0.92f, 0.92f, 0.95f));
            b1Tmp.lineSpacing = 8f;
            RectTransform b1tr = b1Tmp.rectTransform;
            b1tr.anchorMin = new Vector2(0f, 1f); b1tr.anchorMax = new Vector2(1f, 1f);
            b1tr.pivot = new Vector2(0.5f, 1f);
            b1tr.sizeDelta = new Vector2(0f, 54f);
            b1tr.anchoredPosition = Vector2.zero;

            GameObject easBox = new GameObject("EASAlertBox", typeof(RectTransform));
            easBox.transform.SetParent(b1Obj.transform, false);
            RectTransform easRect = easBox.GetComponent<RectTransform>();
            easRect.anchorMin = new Vector2(0f, 0f); easRect.anchorMax = new Vector2(1f, 0f);
            easRect.pivot = new Vector2(0.5f, 0f);
            easRect.sizeDelta = new Vector2(0f, 290f);
            easRect.anchoredPosition = new Vector2(0f, 20f);
            Image easImg = easBox.AddComponent<Image>();
            easImg.color = new Color(0.18f, 0.02f, 0.02f, 0.88f);
            Outline easOl = easBox.AddComponent<Outline>();
            easOl.effectColor = new Color(0.68f, 0.14f, 0.14f, 0.95f); // #ae2424
            easOl.effectDistance = new Vector2(2f, -2f);

            TextMeshProUGUI ehTmp = CreateTMPText(easBox.transform, "EASHeader",
                "EMERGENCY ALERT SYSTEM\n<size=82%><color=#ffffff>CIVIL DANGER WARNING</color></size>",
                15.5f, new Color(0.94f, 0.32f, 0.26f), TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform ehRect = ehTmp.rectTransform;
            ehRect.anchorMin = new Vector2(0f, 1f); ehRect.anchorMax = new Vector2(1f, 1f);
            ehRect.pivot = new Vector2(0.5f, 1f);
            ehRect.sizeDelta = new Vector2(0f, 48f);
            ehRect.anchoredPosition = new Vector2(0f, -8f);

            GameObject easDivObj = new GameObject("HeaderDiv", typeof(RectTransform));
            easDivObj.transform.SetParent(easBox.transform, false);
            RectTransform edRect = easDivObj.GetComponent<RectTransform>();
            edRect.anchorMin = new Vector2(0.06f, 1f); edRect.anchorMax = new Vector2(0.94f, 1f);
            edRect.pivot = new Vector2(0.5f, 1f);
            edRect.sizeDelta = new Vector2(0f, 1.5f);
            edRect.anchoredPosition = new Vector2(0f, -60f);
            Image edImg = easDivObj.AddComponent<Image>();
            edImg.color = new Color(0.64f, 0.15f, 0.15f, 0.85f);

            TextMeshProUGUI easTmp = CreateTMPText(easBox.transform, "EASText",
                "\"The following message is transmitted at the request of the King County Sheriff's Office. This is not a test.\n\n" +
                "A Civil Danger Warning is in effect for the Seattle metropolitan area. Authorities reported a rapidly spreading biological hazard. Affected individuals exhibit extreme, unprovoked violence. The pathogen is highly contagious through fluid contact.\n\n" +
                "Shelter in place immediately. Major interstates are closed. Lock all doors, turn off lights, and remain completely silent. The National Guard has been deployed. Lethal force is authorized.\"",
                14f, new Color(1f, 0.86f, 0.86f));
            easTmp.lineSpacing = 7.5f;
            RectTransform easTextRect = easTmp.rectTransform;
            StretchFull(easTextRect);
            easTextRect.offsetMin = new Vector2(24f, 14f);
            easTextRect.offsetMax = new Vector2(-24f, -68f);

            // --- BEAT 2: REACTION MONOLOGUE (Twine .chat-block replica) ---
            GameObject b2Obj = new GameObject("Beat2Container", typeof(RectTransform));
            b2Obj.transform.SetParent(awakeObj.transform, false);
            RectTransform b2Rect = b2Obj.GetComponent<RectTransform>();
            StretchFull(b2Rect);
            b2Obj.SetActive(false);

            TextMeshProUGUI b2Tmp = CreateTMPText(b2Obj.transform, "Beat2IntroText",
                "The robotic voice cuts out. Cold static returns to the TV.",
                16f, new Color(0.92f, 0.92f, 0.95f));
            RectTransform b2tr = b2Tmp.rectTransform;
            b2tr.anchorMin = new Vector2(0f, 1f); b2tr.anchorMax = new Vector2(1f, 1f);
            b2tr.pivot = new Vector2(0.5f, 1f);
            b2tr.sizeDelta = new Vector2(0f, 32f);
            b2tr.anchoredPosition = new Vector2(0f, -44f);

            // Chat Block Card
            GameObject monoBox = new GameObject("MonologueBox", typeof(RectTransform));
            monoBox.transform.SetParent(b2Obj.transform, false);
            RectTransform monoRect = monoBox.GetComponent<RectTransform>();
            monoRect.anchorMin = new Vector2(0f, 1f); monoRect.anchorMax = new Vector2(1f, 1f);
            monoRect.pivot = new Vector2(0.5f, 1f);
            monoRect.sizeDelta = new Vector2(0f, 120f);
            monoRect.anchoredPosition = new Vector2(0f, -96f);
            Image monoBg = monoBox.AddComponent<Image>();
            monoBg.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);
            Outline monoOl = monoBox.AddComponent<Outline>();
            monoOl.effectColor = new Color(0.20f, 0.25f, 0.32f, 0.85f);
            monoOl.effectDistance = new Vector2(1f, -1f);

            GameObject monoAccent = new GameObject("GoldBar", typeof(RectTransform));
            monoAccent.transform.SetParent(monoBox.transform, false);
            RectTransform maRect = monoAccent.GetComponent<RectTransform>();
            maRect.anchorMin = new Vector2(0f, 0f); maRect.anchorMax = new Vector2(0f, 1f);
            maRect.pivot = new Vector2(0f, 0.5f);
            maRect.sizeDelta = new Vector2(4f, 0f);
            maRect.anchoredPosition = Vector2.zero;
            Image maImg = monoAccent.AddComponent<Image>();
            maImg.color = new Color(0.95f, 0.75f, 0.35f);

            GameObject avatarContainer = new GameObject("AvatarContainer", typeof(RectTransform));
            avatarContainer.transform.SetParent(monoBox.transform, false);
            RectTransform acRect = avatarContainer.GetComponent<RectTransform>();
            acRect.anchorMin = new Vector2(0f, 0.5f); acRect.anchorMax = new Vector2(0f, 0.5f);
            acRect.pivot = new Vector2(0f, 0.5f);
            acRect.anchoredPosition = new Vector2(18f, 0f);
            acRect.sizeDelta = new Vector2(76f, 76f);
            Image acBg = avatarContainer.AddComponent<Image>();
            acBg.color = new Color(0.05f, 0.07f, 0.10f, 1f);
            Outline acOl = avatarContainer.AddComponent<Outline>();
            acOl.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.95f);
            acOl.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject mAvatarObj = new GameObject("AvatarImage", typeof(RectTransform));
            mAvatarObj.transform.SetParent(avatarContainer.transform, false);
            RectTransform mAvRect = mAvatarObj.GetComponent<RectTransform>();
            StretchFull(mAvRect);
            Image mAvatarImg = mAvatarObj.AddComponent<Image>();
            mAvatarImg.preserveAspect = true;
            Sprite defaultPlayerSprite = LoadSpriteSafe("Assets/Sprites/Avatars/player.png");
            if (defaultPlayerSprite != null) mAvatarImg.sprite = defaultPlayerSprite;

            GameObject speechContainer = new GameObject("SpeechContainer", typeof(RectTransform));
            speechContainer.transform.SetParent(monoBox.transform, false);
            RectTransform scRect = speechContainer.GetComponent<RectTransform>();
            StretchFull(scRect);
            scRect.offsetMin = new Vector2(110f, 12f);
            scRect.offsetMax = new Vector2(-18f, -12f);

            TextMeshProUGUI mSpkTmp = CreateTMPText(speechContainer.transform, "Speaker", "YOU", 12.5f, new Color(0.95f, 0.75f, 0.35f), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            RectTransform spkRect = mSpkTmp.rectTransform;
            spkRect.anchorMin = new Vector2(0f, 1f); spkRect.anchorMax = new Vector2(1f, 1f);
            spkRect.pivot = new Vector2(0f, 1f);
            spkRect.sizeDelta = new Vector2(0f, 22f);
            spkRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI mTextTmp = CreateTMPText(speechContainer.transform, "Speech",
                "\"Lethal force authorized... They don't lock down the whole city for mass hysteria. And this front door isn't heavy enough to keep a panicked mob out.\"",
                15f, new Color(0.92f, 0.94f, 0.98f), TextAlignmentOptions.TopLeft, FontStyles.Italic);
            mTextTmp.lineSpacing = 6f;
            RectTransform stRect = mTextTmp.rectTransform;
            StretchFull(stRect);
            stRect.offsetMin = Vector2.zero;
            stRect.offsetMax = new Vector2(0f, -24f);

            TextMeshProUGUI b2fTmp = CreateTMPText(b2Obj.transform, "Beat2FootnoteText",
                "Through the thin plaster of your apartment walls, you can hear it: the distant, overlapping chorus of emergency sirens. And closer... the muffled sound of heavy, frantic footsteps pounding down the hallway outside your door.",
                15f, new Color(0.74f, 0.78f, 0.85f), TextAlignmentOptions.TopLeft, FontStyles.Italic);
            b2fTmp.lineSpacing = 6f;
            RectTransform b2fRect = b2fTmp.rectTransform;
            b2fRect.anchorMin = new Vector2(0f, 1f); b2fRect.anchorMax = new Vector2(1f, 1f);
            b2fRect.pivot = new Vector2(0.5f, 1f);
            b2fRect.sizeDelta = new Vector2(0f, 75f);
            b2fRect.anchoredPosition = new Vector2(0f, -238f);

            // --- BEAT 3: TABLE STASH ---
            GameObject b3Obj = new GameObject("Beat3Container", typeof(RectTransform));
            b3Obj.transform.SetParent(awakeObj.transform, false);
            RectTransform b3Rect = b3Obj.GetComponent<RectTransform>();
            StretchFull(b3Rect);
            b3Obj.SetActive(false);

            GameObject tagBadge = new GameObject("RoomBadge", typeof(RectTransform));
            tagBadge.transform.SetParent(b3Obj.transform, false);
            RectTransform tbRect = tagBadge.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f); tbRect.anchorMax = new Vector2(0f, 1f);
            tbRect.pivot = new Vector2(0f, 1f);
            tbRect.anchoredPosition = Vector2.zero;
            tbRect.sizeDelta = new Vector2(120f, 24f);
            Image tbBg = tagBadge.AddComponent<Image>();
            tbBg.color = new Color(0.91f, 0.30f, 0.24f, 0.15f);

            TextMeshProUGUI tbTmp = CreateTMPText(tagBadge.transform, "Text", "LIVING ROOM", 12f, new Color(0.91f, 0.30f, 0.24f), TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform tbtRect = tbTmp.rectTransform;
            StretchFull(tbtRect);

            TextMeshProUGUI b3Tmp = CreateTMPText(b3Obj.transform, "Beat3NarrativeText",
                "You are standing in the middle of your living room in your pajamas. You feel naked. Vulnerable.\n\n" +
                "You glance around the dim room, illuminated only by the harsh red glow of the TV. Your everyday carry sits on the cramped desk. But your heavier equipment rests by the front door, exactly where you dropped it after yesterday's shift.",
                15.5f, new Color(0.90f, 0.90f, 0.94f));
            b3Tmp.lineSpacing = 7f;
            RectTransform b3tRect = b3Tmp.rectTransform;
            b3tRect.anchorMin = new Vector2(0f, 1f); b3tRect.anchorMax = new Vector2(1f, 1f);
            b3tRect.pivot = new Vector2(0.5f, 1f);
            b3tRect.sizeDelta = new Vector2(0f, 140f);
            b3tRect.anchoredPosition = new Vector2(0f, -32f);

            GameObject stashObj = new GameObject("TableStashDisplay", typeof(RectTransform));
            stashObj.transform.SetParent(b3Obj.transform, false);
            RectTransform stashRect = stashObj.GetComponent<RectTransform>();
            stashRect.anchorMin = new Vector2(0f, 0f); stashRect.anchorMax = new Vector2(1f, 0f);
            stashRect.pivot = new Vector2(0.5f, 0f);
            stashRect.sizeDelta = new Vector2(0f, 155f);
            stashRect.anchoredPosition = new Vector2(0f, 16f);
            Image stashBg = stashObj.AddComponent<Image>();
            stashBg.color = new Color(0.05f, 0.07f, 0.10f, 0.90f);
            Outline stashOl = stashObj.AddComponent<Outline>();
            stashOl.effectColor = new Color(0.20f, 0.25f, 0.32f, 0.85f);
            stashOl.effectDistance = new Vector2(1f, -1f);

            GameObject stashAccent = new GameObject("GoldBar", typeof(RectTransform));
            stashAccent.transform.SetParent(stashObj.transform, false);
            RectTransform saRect = stashAccent.GetComponent<RectTransform>();
            saRect.anchorMin = new Vector2(0f, 0f); saRect.anchorMax = new Vector2(0f, 1f);
            saRect.pivot = new Vector2(0f, 0.5f);
            saRect.sizeDelta = new Vector2(4f, 0f);
            saRect.anchoredPosition = Vector2.zero;
            Image saImg = stashAccent.AddComponent<Image>();
            saImg.color = new Color(0.95f, 0.75f, 0.35f);
            LayoutElement saIgnore = stashAccent.AddComponent<LayoutElement>();
            saIgnore.ignoreLayout = true;

            VerticalLayoutGroup sVlg = stashObj.AddComponent<VerticalLayoutGroup>();
            sVlg.padding = new RectOffset(18, 18, 12, 12);
            sVlg.spacing = 8f;
            sVlg.childControlWidth = true; sVlg.childControlHeight = false;

            TextMeshProUGUI shTmp = CreateTMPText(stashObj.transform, "Header", "TABLE STASH // CURRENT WORK GEAR & WEAPONS", 12.5f, new Color(0.95f, 0.75f, 0.35f), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            RectTransform shRect = shTmp.rectTransform;
            shRect.sizeDelta = new Vector2(0f, 22f);

            GameObject sGridObj = new GameObject("Grid", typeof(RectTransform));
            sGridObj.transform.SetParent(stashObj.transform, false);
            RectTransform sgRect = sGridObj.GetComponent<RectTransform>();
            sgRect.sizeDelta = new Vector2(0f, 106f);
            HorizontalLayoutGroup sgHlg = sGridObj.AddComponent<HorizontalLayoutGroup>();
            sgHlg.spacing = 14f;
            sgHlg.childAlignment = TextAnchor.MiddleLeft;
            sgHlg.childControlWidth = false; sgHlg.childControlHeight = false;

            TextMeshProUGUI seTmp = CreateTMPText(stashObj.transform, "EmptyNotice", "(Table is empty)", 12.5f, new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.TopLeft, FontStyles.Italic);
            RectTransform seRect = seTmp.rectTransform;
            seRect.sizeDelta = new Vector2(0f, 22f);



            // Continue Prompt
            TextMeshProUGUI prTmp = CreateTMPText(awakeObj.transform, "ContinuePrompt", "CLICK TO CONTINUE <color=#FFA726>>></color>", 13.5f, new Color(0.85f, 0.85f, 0.85f), TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            RectTransform prRect = prTmp.rectTransform;
            prRect.anchorMin = new Vector2(1f, 0f); prRect.anchorMax = new Vector2(1f, 0f);
            prRect.pivot = new Vector2(1f, 0f);
            prRect.anchoredPosition = new Vector2(0f, -4f);
            prRect.sizeDelta = new Vector2(220f, 26f);
            prTmp.gameObject.AddComponent<PulsingText>();

            // MODE B: INTERACTIVE HUB MENU CONTAINER
            GameObject menuObj = new GameObject("HubMenuContainer", typeof(RectTransform));
            menuObj.transform.SetParent(vnColObj.transform, false);
            RectTransform menuRect = menuObj.GetComponent<RectTransform>();
            StretchFull(menuRect);
            menuObj.SetActive(false);

            VerticalLayoutGroup mVlg2 = menuObj.AddComponent<VerticalLayoutGroup>();
            mVlg2.spacing = 6f;
            mVlg2.childControlWidth = true;
            mVlg2.childControlHeight = false;

            // Current Situation & Story Box (Option A)
            GameObject sitBox = new GameObject("SituationBox", typeof(RectTransform));
            sitBox.transform.SetParent(menuObj.transform, false);
            LayoutElement sitLe = sitBox.AddComponent<LayoutElement>();
            sitLe.preferredHeight = 84f;
            sitLe.flexibleWidth = 1f;

            Image sitBg = sitBox.AddComponent<Image>();
            sitBg.color = new Color(0.06f, 0.08f, 0.11f, 0.95f);
            Outline sitOl = sitBox.AddComponent<Outline>();
            sitOl.effectColor = new Color(0.20f, 0.28f, 0.38f, 0.8f);
            sitOl.effectDistance = new Vector2(1f, -1f);

            GameObject sitAccent = new GameObject("GoldBar", typeof(RectTransform));
            sitAccent.transform.SetParent(sitBox.transform, false);
            RectTransform saRect2 = sitAccent.GetComponent<RectTransform>();
            saRect2.anchorMin = new Vector2(0f, 0f); saRect2.anchorMax = new Vector2(0f, 1f);
            saRect2.pivot = new Vector2(0f, 0.5f);
            saRect2.sizeDelta = new Vector2(4f, 0f);
            saRect2.anchoredPosition = Vector2.zero;
            Image saImg2 = sitAccent.AddComponent<Image>();
            saImg2.color = new Color(0.95f, 0.75f, 0.35f);
            LayoutElement saIgnore2 = sitAccent.AddComponent<LayoutElement>();
            saIgnore2.ignoreLayout = true;

            // Header Row: Title & View Log Button
            GameObject sitTopRow = new GameObject("TopRow", typeof(RectTransform));
            sitTopRow.transform.SetParent(sitBox.transform, false);
            RectTransform strRect = sitTopRow.GetComponent<RectTransform>();
            strRect.anchorMin = new Vector2(0f, 1f); strRect.anchorMax = new Vector2(1f, 1f);
            strRect.pivot = new Vector2(0.5f, 1f);
            strRect.sizeDelta = new Vector2(0f, 28f);
            strRect.anchoredPosition = new Vector2(0f, -6f);

            TextMeshProUGUI sitTitleTmp = CreateTMPText(sitTopRow.transform, "Title",
                "<b><color=#F5A623>● CURRENT SITUATION</color></b>  <color=#78909C>// LIVING ROOM (23:45 DAY 0)</color>",
                12.5f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform sttRect = sitTitleTmp.rectTransform;
            sttRect.anchorMin = new Vector2(0f, 0f); sttRect.anchorMax = new Vector2(0.75f, 1f);
            sttRect.offsetMin = new Vector2(16f, 0f); sttRect.offsetMax = Vector2.zero;

            // View Log Button
            GameObject btnLogObj = new GameObject("Btn_OpenStoryLog", typeof(RectTransform));
            btnLogObj.transform.SetParent(sitTopRow.transform, false);
            RectTransform btnLogRect = btnLogObj.GetComponent<RectTransform>();
            btnLogRect.anchorMin = new Vector2(1f, 0.5f); btnLogRect.anchorMax = new Vector2(1f, 0.5f);
            btnLogRect.pivot = new Vector2(1f, 0.5f);
            btnLogRect.sizeDelta = new Vector2(148f, 24f);
            btnLogRect.anchoredPosition = new Vector2(-12f, 0f);

            Image btnLogBg = btnLogObj.AddComponent<Image>();
            btnLogBg.color = new Color(0.10f, 0.16f, 0.22f, 0.95f);
            Outline btnLogOl = btnLogObj.AddComponent<Outline>();
            btnLogOl.effectColor = new Color(0.30f, 0.65f, 0.85f, 0.8f);
            btnLogOl.effectDistance = new Vector2(1f, -1f);

            Button btnOpenLog = btnLogObj.AddComponent<Button>();
            btnOpenLog.targetGraphic = btnLogBg;
            ColorBlock lcb = btnOpenLog.colors;
            lcb.normalColor = btnLogBg.color;
            lcb.highlightedColor = new Color(0.18f, 0.28f, 0.38f, 1f);
            btnOpenLog.colors = lcb;

            TextMeshProUGUI btnLogTmp = CreateTMPText(btnLogObj.transform, "Label",
                "<b><color=#4FC3F7>[ VIEW LOG ]</color></b>",
                11f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform bltRect = btnLogTmp.rectTransform;
            StretchFull(bltRect);

            // Situation Body Text
            TextMeshProUGUI sitBodyTmp = CreateTMPText(sitBox.transform, "SituationBody",
                "\"The television drones with cold emergency static. Sirens wail across the Seattle skyline. You stand in your living room in the dark, weighing your immediate options before the power grid or the front door fails.\"",
                13f, new Color(0.82f, 0.86f, 0.92f), TextAlignmentOptions.TopLeft, FontStyles.Italic);
            sitBodyTmp.lineSpacing = 4f;
            sitBodyTmp.textWrappingMode = TextWrappingModes.Normal;
            RectTransform sbtRect = sitBodyTmp.rectTransform;
            sbtRect.anchorMin = new Vector2(0f, 0f); sbtRect.anchorMax = new Vector2(1f, 1f);
            sbtRect.offsetMin = new Vector2(16f, 6f);
            sbtRect.offsetMax = new Vector2(-16f, -32f);

            // Packed Success Banner
            GameObject pBanner = new GameObject("PackedSuccessBanner", typeof(RectTransform));
            pBanner.transform.SetParent(menuObj.transform, false);
            LayoutElement pBle = pBanner.AddComponent<LayoutElement>();
            pBle.preferredHeight = 44f;
            Image pbImg = pBanner.AddComponent<Image>();
            pbImg.color = new Color(0.04f, 0.16f, 0.06f, 0.90f);

            GameObject pBannerGreenBar = new GameObject("GreenBar", typeof(RectTransform));
            pBannerGreenBar.transform.SetParent(pBanner.transform, false);
            RectTransform pbgRect = pBannerGreenBar.GetComponent<RectTransform>();
            pbgRect.anchorMin = new Vector2(0f, 0f); pbgRect.anchorMax = new Vector2(0f, 1f);
            pbgRect.pivot = new Vector2(0f, 0.5f);
            pbgRect.sizeDelta = new Vector2(3f, 0f);
            pbgRect.anchoredPosition = Vector2.zero;
            Image pbgImg = pBannerGreenBar.AddComponent<Image>();
            pbgImg.color = new Color(0.18f, 0.80f, 0.44f);

            TextMeshProUGUI pbTmp = CreateTMPText(pBanner.transform, "Text",
                "[+] You geared up. You strapped on your heavy equipment, shoved the smaller items into your pockets, and pulled a Kitchen Knife from the block.",
                11.5f, new Color(0.35f, 0.95f, 0.55f), TextAlignmentOptions.MidlineLeft);
            RectTransform pbTextRect = pbTmp.rectTransform;
            StretchFull(pbTextRect);
            pbTextRect.offsetMin = new Vector2(14f, 0f);
            pbTextRect.offsetMax = new Vector2(-14f, 0f);
            pBanner.SetActive(false);

            // Menu Action Buttons
            CreateApartmentMenuButton(menuObj.transform, "Btn_PackGear", "GATHER YOUR GEAR & SUPPLIES",
                "4 Minutes | Gear up. Grab your work equipment by the door and raid the kitchen for anything useful.",
                new Color(0.95f, 0.75f, 0.35f), out Button btnPack, out TextMeshProUGUI _, out TextMeshProUGUI _);

            CreateApartmentMenuButton(menuObj.transform, "Btn_TakeFloorItems", "COLLECT ROOM SUPPLIES [FLOOR ITEMS]",
                "Gather extra supplies discovered in Unit 404 into your backpack.",
                new Color(0.95f, 0.65f, 0.25f), out Button btnTakeFloor, out TextMeshProUGUI takeFloorLbl, out TextMeshProUGUI takeFloorSub);

            CreateApartmentMenuButton(menuObj.transform, "Btn_CheckPhone", "CHECK NEWS FEED",
                "1 Minute | See what the internet knows before the grid goes down.",
                new Color(0.35f, 0.75f, 0.95f), out Button btnPhone, out TextMeshProUGUI _, out TextMeshProUGUI _);

            CreateApartmentMenuButton(menuObj.transform, "Btn_ListenRadio", "LISTEN TO EMERGENCY BAND [POLICE]",
                "3 Minutes | Monitor encrypted dispatch channels. Find out where lines are breaking.",
                new Color(0.95f, 0.75f, 0.35f), out Button btnRadio, out TextMeshProUGUI radioLbl, out TextMeshProUGUI radioSub);

            CreateApartmentMenuButton(menuObj.transform, "Btn_HackTerminal", "BYPASS GOV FIREWALL [TECH 3]",
                "8 Minutes | Breach military-grade encryption to extract classified directives.",
                new Color(0.35f, 0.95f, 0.45f), out Button btnHacking, out TextMeshProUGUI hackLbl, out TextMeshProUGUI hackSub);

            CreateApartmentMenuButton(menuObj.transform, "Btn_LeaveApartment", "LEAVE APARTMENT",
                "Exit to the 4th Floor Hallway. Ready your weapon. There is no turning back.",
                new Color(0.95f, 0.3f, 0.3f), out Button btnLeave, out TextMeshProUGUI _, out TextMeshProUGUI _,
                new Color(0.20f, 0.05f, 0.05f, 0.95f));

            // -------------------------------------------------------------------------
            // 4B. DOCK CONTAINER 2: SURVIVOR PDA (MODE B - Widescreen Console Layout)
            // -------------------------------------------------------------------------
            GameObject dockPDA = new GameObject("Dock_SurvivorPDA", typeof(RectTransform));
            dockPDA.transform.SetParent(botScreen.transform, false);
            RectTransform dpRect = dockPDA.GetComponent<RectTransform>();
            dpRect.anchorMin = new Vector2(0.5f, 0.5f); dpRect.anchorMax = new Vector2(0.5f, 0.5f);
            dpRect.pivot = new Vector2(0.5f, 0.5f);
            dpRect.sizeDelta = new Vector2(1620f, 400f);
            dpRect.anchoredPosition = Vector2.zero;
            dockPDA.SetActive(false);

            // Left Sub-Panel (380px): Identity, Tabs & System Controls
            GameObject pdaLeftCol = new GameObject("PDALeftColumn", typeof(RectTransform));
            pdaLeftCol.transform.SetParent(dockPDA.transform, false);
            RectTransform plcRect = pdaLeftCol.GetComponent<RectTransform>();
            plcRect.anchorMin = new Vector2(0f, 0f); plcRect.anchorMax = new Vector2(0f, 1f);
            plcRect.pivot = new Vector2(0f, 0.5f);
            plcRect.offsetMin = Vector2.zero;
            plcRect.offsetMax = new Vector2(380f, 0f);
            Image plcBg = pdaLeftCol.AddComponent<Image>();
            plcBg.color = new Color(0.05f, 0.06f, 0.08f, 0.98f);
            Outline plcOl = pdaLeftCol.AddComponent<Outline>();
            plcOl.effectColor = new Color(0.20f, 0.24f, 0.30f, 0.85f);
            plcOl.effectDistance = new Vector2(1f, -1f);

            // Identity Header
            GameObject cHeadObj = new GameObject("CharHeader", typeof(RectTransform));
            cHeadObj.transform.SetParent(pdaLeftCol.transform, false);
            RectTransform chr = cHeadObj.GetComponent<RectTransform>();
            chr.anchorMin = new Vector2(0f, 1f); chr.anchorMax = new Vector2(1f, 1f);
            chr.pivot = new Vector2(0.5f, 1f);
            chr.sizeDelta = new Vector2(0f, 60f);
            chr.anchoredPosition = new Vector2(0f, -8f);

            TextMeshProUGUI pdaCharNameTmp = CreateTMPText(cHeadObj.transform, "Name", "James Smith", 17f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform cnr = pdaCharNameTmp.rectTransform;
            cnr.anchorMin = new Vector2(0f, 0.45f); cnr.anchorMax = new Vector2(1f, 1f);
            cnr.offsetMin = Vector2.zero; cnr.offsetMax = Vector2.zero;

            TextMeshProUGUI pdaCharSubTmp = CreateTMPText(cHeadObj.transform, "Sub", "Male | Police Officer", 11.5f, new Color(0.65f, 0.68f, 0.72f), TextAlignmentOptions.Center);
            RectTransform csr = pdaCharSubTmp.rectTransform;
            csr.anchorMin = new Vector2(0f, 0f); csr.anchorMax = new Vector2(1f, 0.45f);
            csr.offsetMin = Vector2.zero; csr.offsetMax = Vector2.zero;

            // 5 Tabs in 2 Rows
            GameObject tabGridObj = new GameObject("TabsContainer", typeof(RectTransform));
            tabGridObj.transform.SetParent(pdaLeftCol.transform, false);
            RectTransform tgRect = tabGridObj.GetComponent<RectTransform>();
            tgRect.anchorMin = new Vector2(0f, 1f); tgRect.anchorMax = new Vector2(1f, 1f);
            tgRect.pivot = new Vector2(0.5f, 1f);
            tgRect.sizeDelta = new Vector2(0f, 64f);
            tgRect.anchoredPosition = new Vector2(0f, -74f);

            GameObject tabRow1 = new GameObject("Row1", typeof(RectTransform));
            tabRow1.transform.SetParent(tabGridObj.transform, false);
            RectTransform tr1Rect = tabRow1.GetComponent<RectTransform>();
            tr1Rect.anchorMin = new Vector2(0f, 0.5f); tr1Rect.anchorMax = new Vector2(1f, 1f);
            tr1Rect.offsetMin = new Vector2(8f, 1f); tr1Rect.offsetMax = new Vector2(-8f, 0f);
            HorizontalLayoutGroup r1Hlg = tabRow1.AddComponent<HorizontalLayoutGroup>();
            r1Hlg.spacing = 4f;
            r1Hlg.childControlWidth = true; r1Hlg.childControlHeight = true;
            r1Hlg.childForceExpandWidth = true; r1Hlg.childForceExpandHeight = true;

            GameObject tabRow2 = new GameObject("Row2", typeof(RectTransform));
            tabRow2.transform.SetParent(tabGridObj.transform, false);
            RectTransform tr2Rect = tabRow2.GetComponent<RectTransform>();
            tr2Rect.anchorMin = new Vector2(0f, 0f); tr2Rect.anchorMax = new Vector2(1f, 0.5f);
            tr2Rect.offsetMin = new Vector2(8f, 0f); tr2Rect.offsetMax = new Vector2(-8f, -1f);
            HorizontalLayoutGroup r2Hlg = tabRow2.AddComponent<HorizontalLayoutGroup>();
            r2Hlg.spacing = 4f;
            r2Hlg.childControlWidth = true; r2Hlg.childControlHeight = true;
            r2Hlg.childForceExpandWidth = true; r2Hlg.childForceExpandHeight = true;

            CreatePDATabButton(tabRow1.transform, "Tab_Status", "STATUS", out Button btnTabStatus);
            CreatePDATabButton(tabRow1.transform, "Tab_Skills", "SKILLS", out Button btnTabSkills);
            CreatePDATabButton(tabRow1.transform, "Tab_Traits", "TRAITS", out Button btnTabTraits);
            CreatePDATabButton(tabRow2.transform, "Tab_Map", "MAP", out Button btnTabMap);
            CreatePDATabButton(tabRow2.transform, "Tab_Lore", "LORE", out Button btnTabLore);

            // PDA Volume & Settings Footer
            GameObject pdaFooter = new GameObject("PDAFooter", typeof(RectTransform));
            pdaFooter.transform.SetParent(pdaLeftCol.transform, false);
            RectTransform pfRect = pdaFooter.GetComponent<RectTransform>();
            pfRect.anchorMin = new Vector2(0f, 0f); pfRect.anchorMax = new Vector2(1f, 0f);
            pfRect.pivot = new Vector2(0.5f, 0f);
            pfRect.sizeDelta = new Vector2(-16f, 74f);
            pfRect.anchoredPosition = new Vector2(0f, 8f);

            GameObject volRow = new GameObject("VolRow", typeof(RectTransform));
            volRow.transform.SetParent(pdaFooter.transform, false);
            RectTransform vrRect2 = volRow.GetComponent<RectTransform>();
            vrRect2.anchorMin = new Vector2(0f, 0.5f); vrRect2.anchorMax = new Vector2(1f, 1f);
            vrRect2.offsetMin = Vector2.zero; vrRect2.offsetMax = Vector2.zero;
            Image vrBg = volRow.AddComponent<Image>();
            vrBg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);

            TextMeshProUGUI volTmp = CreateTMPText(volRow.transform, "Label", "VOL", 11f, new Color(0.75f, 0.78f, 0.82f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform vlr = volTmp.rectTransform;
            vlr.anchorMin = new Vector2(0f, 0f); vlr.anchorMax = new Vector2(0f, 1f);
            vlr.pivot = new Vector2(0f, 0.5f);
            vlr.anchoredPosition = new Vector2(10f, 0f);
            vlr.sizeDelta = new Vector2(36f, 0f);

            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform));
            sliderObj.transform.SetParent(volRow.transform, false);
            RectTransform sldRect = sliderObj.GetComponent<RectTransform>();
            sldRect.anchorMin = new Vector2(0f, 0.5f); sldRect.anchorMax = new Vector2(1f, 0.5f);
            sldRect.pivot = new Vector2(0.5f, 0.5f);
            sldRect.offsetMin = new Vector2(50f, -8f);
            sldRect.offsetMax = new Vector2(-10f, 8f);

            GameObject sldBg = new GameObject("Background", typeof(RectTransform));
            sldBg.transform.SetParent(sliderObj.transform, false);
            RectTransform sldBgRect = sldBg.GetComponent<RectTransform>();
            StretchFull(sldBgRect);
            Image sldBgImg = sldBg.AddComponent<Image>();
            sldBgImg.color = new Color(0.12f, 0.14f, 0.18f, 1f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRect = fillArea.GetComponent<RectTransform>();
            StretchFull(faRect);

            GameObject sldFill = new GameObject("Fill", typeof(RectTransform));
            sldFill.transform.SetParent(fillArea.transform, false);
            RectTransform sfRect = sldFill.GetComponent<RectTransform>();
            StretchFull(sfRect);
            Image sfImg = sldFill.AddComponent<Image>();
            sfImg.color = new Color(0.22f, 0.45f, 0.82f);

            Slider volSlider = sliderObj.AddComponent<Slider>();
            volSlider.targetGraphic = sfImg;
            volSlider.fillRect = sfRect;
            volSlider.minValue = 0f;
            volSlider.maxValue = 1f;
            volSlider.value = 1f;

            GameObject setBtnObj = new GameObject("Btn_SaveSettings", typeof(RectTransform));
            setBtnObj.transform.SetParent(pdaFooter.transform, false);
            RectTransform sbtnRect = setBtnObj.GetComponent<RectTransform>();
            sbtnRect.anchorMin = new Vector2(0f, 0f); sbtnRect.anchorMax = new Vector2(1f, 0.5f);
            sbtnRect.offsetMin = Vector2.zero; sbtnRect.offsetMax = Vector2.zero;
            Image sbtnBg = setBtnObj.AddComponent<Image>();
            sbtnBg.color = new Color(0.08f, 0.09f, 0.11f, 0.98f);
            Button btnSettings = setBtnObj.AddComponent<Button>();
            btnSettings.targetGraphic = sbtnBg;

            TextMeshProUGUI sbTmp = CreateTMPText(setBtnObj.transform, "Text", "SAVE / LOAD / SETTINGS", 11.5f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform sbtr = sbTmp.rectTransform;
            StretchFull(sbtr);

            // Right Sub-Panel (1220px): Tab Content Area
            GameObject pdaContentObj = new GameObject("PDAContentArea", typeof(RectTransform));
            pdaContentObj.transform.SetParent(dockPDA.transform, false);
            RectTransform pcRect = pdaContentObj.GetComponent<RectTransform>();
            pcRect.anchorMin = new Vector2(0f, 0f); pcRect.anchorMax = new Vector2(1f, 1f);
            pcRect.offsetMin = new Vector2(395f, 0f);
            pcRect.offsetMax = Vector2.zero;
            Image pcBg = pdaContentObj.AddComponent<Image>();
            pcBg.color = new Color(0.05f, 0.06f, 0.08f, 0.98f);
            Outline pcOl = pdaContentObj.AddComponent<Outline>();
            pcOl.effectColor = new Color(0.20f, 0.24f, 0.30f, 0.85f);
            pcOl.effectDistance = new Vector2(1f, -1f);

            // TAB 1: STATUS PANEL (Widescreen 2-column layout inside content area)
            GameObject pStatusObj = new GameObject("Panel_Status", typeof(RectTransform));
            pStatusObj.transform.SetParent(pdaContentObj.transform, false);
            RectTransform psRect = pStatusObj.GetComponent<RectTransform>();
            StretchFull(psRect);
            psRect.offsetMin = new Vector2(16f, 14f); psRect.offsetMax = new Vector2(-16f, -14f);

            // Left status col (580px): Active Logs
            GameObject logsCard = new GameObject("ActiveLogsCard", typeof(RectTransform));
            logsCard.transform.SetParent(pStatusObj.transform, false);
            RectTransform lcRect = logsCard.GetComponent<RectTransform>();
            lcRect.anchorMin = new Vector2(0f, 0f); lcRect.anchorMax = new Vector2(0.5f, 1f);
            lcRect.offsetMin = Vector2.zero; lcRect.offsetMax = new Vector2(-8f, 0f);
            Image lcBg = logsCard.AddComponent<Image>();
            lcBg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            Outline lcOl = logsCard.AddComponent<Outline>();
            lcOl.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
            lcOl.effectDistance = new Vector2(1f, -1f);

            GameObject logBar = new GameObject("CyanAccent", typeof(RectTransform));
            logBar.transform.SetParent(logsCard.transform, false);
            RectTransform lbrt = logBar.GetComponent<RectTransform>();
            lbrt.anchorMin = new Vector2(0f, 0f); lbrt.anchorMax = new Vector2(0f, 1f);
            lbrt.pivot = new Vector2(0f, 0.5f);
            lbrt.sizeDelta = new Vector2(3f, 0f);
            lbrt.anchoredPosition = Vector2.zero;
            Image lbImg = logBar.AddComponent<Image>();
            lbImg.color = new Color(0.22f, 0.74f, 0.97f);

            TextMeshProUGUI logHeadTmp = CreateTMPText(logsCard.transform, "Header", "ACTIVE LOGS", 12f, new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            RectTransform lhr = logHeadTmp.rectTransform;
            lhr.anchorMin = new Vector2(0f, 1f); lhr.anchorMax = new Vector2(1f, 1f);
            lhr.pivot = new Vector2(0f, 1f);
            lhr.anchoredPosition = new Vector2(14f, -10f);
            lhr.sizeDelta = new Vector2(0f, 22f);

            TextMeshProUGUI pdaActiveLogsTmp = CreateTMPText(logsCard.transform, "Body", "[ ] Gather your starter gear from the table\n[ ] Leave your apartment and head outside", 12f, new Color(0.85f, 0.88f, 0.92f), TextAlignmentOptions.TopLeft);
            pdaActiveLogsTmp.lineSpacing = 6f;
            RectTransform lbrct = pdaActiveLogsTmp.rectTransform;
            StretchFull(lbrct);
            lbrct.offsetMin = new Vector2(14f, 12f);
            lbrct.offsetMax = new Vector2(-12f, -36f);

            // Right status col (580px): LVL, Humanity, Tactical Condition
            GameObject rightStatusCol = new GameObject("RightStatusCol", typeof(RectTransform));
            rightStatusCol.transform.SetParent(pStatusObj.transform, false);
            RectTransform rscRect = rightStatusCol.GetComponent<RectTransform>();
            rscRect.anchorMin = new Vector2(0.5f, 0f); rscRect.anchorMax = new Vector2(1f, 1f);
            rscRect.offsetMin = new Vector2(8f, 0f); rscRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup rscVlg = rightStatusCol.AddComponent<VerticalLayoutGroup>();
            rscVlg.spacing = 10f;
            rscVlg.childControlWidth = true; rscVlg.childControlHeight = true;
            rscVlg.childForceExpandWidth = true; rscVlg.childForceExpandHeight = false;

            // LVL 1 & XP Card
            GameObject lvlCard = new GameObject("LvlCard", typeof(RectTransform));
            lvlCard.transform.SetParent(rightStatusCol.transform, false);
            LayoutElement lvlLe = lvlCard.AddComponent<LayoutElement>();
            lvlLe.preferredHeight = 48f;
            Image lvlBg = lvlCard.AddComponent<Image>();
            lvlBg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);

            TextMeshProUGUI pdaLevelTmp = CreateTMPText(lvlCard.transform, "LvlLbl", "LVL 1", 14f, new Color(0.95f, 0.48f, 0.12f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform llr = pdaLevelTmp.rectTransform;
            llr.anchorMin = new Vector2(0f, 0.5f); llr.anchorMax = new Vector2(0f, 0.5f);
            llr.pivot = new Vector2(0f, 0.5f);
            llr.anchoredPosition = new Vector2(12f, 6f);
            llr.sizeDelta = new Vector2(100f, 20f);

            TextMeshProUGUI pdaXpTmp = CreateTMPText(lvlCard.transform, "XpLbl", "0/100 XP", 11f, new Color(0.55f, 0.58f, 0.62f), TextAlignmentOptions.MidlineRight);
            RectTransform xlr = pdaXpTmp.rectTransform;
            xlr.anchorMin = new Vector2(1f, 0.5f); xlr.anchorMax = new Vector2(1f, 0.5f);
            xlr.pivot = new Vector2(1f, 0.5f);
            xlr.anchoredPosition = new Vector2(-12f, 6f);
            xlr.sizeDelta = new Vector2(120f, 20f);

            GameObject xpTrack = new GameObject("XPTrack", typeof(RectTransform));
            xpTrack.transform.SetParent(lvlCard.transform, false);
            RectTransform xptRect = xpTrack.GetComponent<RectTransform>();
            xptRect.anchorMin = new Vector2(0f, 0f); xptRect.anchorMax = new Vector2(1f, 0f);
            xptRect.pivot = new Vector2(0.5f, 0f);
            xptRect.anchoredPosition = new Vector2(0f, 6f);
            xptRect.sizeDelta = new Vector2(-24f, 6f);
            Image xptBg = xpTrack.AddComponent<Image>();
            xptBg.color = new Color(0.12f, 0.14f, 0.18f, 1f);

            GameObject xpFillObj = new GameObject("Fill", typeof(RectTransform));
            xpFillObj.transform.SetParent(xpTrack.transform, false);
            RectTransform xpfRect = xpFillObj.GetComponent<RectTransform>();
            StretchFull(xpfRect);
            Image pdaXpFillImg = xpFillObj.AddComponent<Image>();
            pdaXpFillImg.color = new Color(0.95f, 0.48f, 0.12f);
            pdaXpFillImg.type = Image.Type.Filled;
            pdaXpFillImg.fillMethod = Image.FillMethod.Horizontal;
            pdaXpFillImg.fillAmount = 0f;

            // Humanity Card
            GameObject humCard = new GameObject("HumanityCard", typeof(RectTransform));
            humCard.transform.SetParent(rightStatusCol.transform, false);
            LayoutElement hcLe = humCard.AddComponent<LayoutElement>();
            hcLe.preferredHeight = 40f;
            Image hcBg = humCard.AddComponent<Image>();
            hcBg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);

            GameObject humTrack = new GameObject("Track", typeof(RectTransform));
            humTrack.transform.SetParent(humCard.transform, false);
            RectTransform htRect = humTrack.GetComponent<RectTransform>();
            StretchFull(htRect);
            htRect.offsetMin = new Vector2(8f, 6f); htRect.offsetMax = new Vector2(-8f, -6f);
            Image htBg = humTrack.AddComponent<Image>();
            htBg.color = new Color(0.10f, 0.12f, 0.15f, 1f);

            GameObject humFillObj = new GameObject("Fill", typeof(RectTransform));
            humFillObj.transform.SetParent(humTrack.transform, false);
            RectTransform hfRect = humFillObj.GetComponent<RectTransform>();
            StretchFull(hfRect);
            Image pdaHumFillImg = humFillObj.AddComponent<Image>();
            pdaHumFillImg.color = new Color(0.36f, 0.61f, 0.84f);
            pdaHumFillImg.type = Image.Type.Filled;
            pdaHumFillImg.fillMethod = Image.FillMethod.Horizontal;
            pdaHumFillImg.fillAmount = 0.35f;

            TextMeshProUGUI humLblTmp = CreateTMPText(humTrack.transform, "Label", "HUMANITY", 11f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform hlr = humLblTmp.rectTransform;
            hlr.anchorMin = new Vector2(0f, 0f); hlr.anchorMax = new Vector2(0.5f, 1f);
            hlr.offsetMin = new Vector2(10f, 0f); hlr.offsetMax = Vector2.zero;

            TextMeshProUGUI pdaHumValTmp = CreateTMPText(humTrack.transform, "Value", "35/100", 11f, Color.white, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            RectTransform hvr = pdaHumValTmp.rectTransform;
            hvr.anchorMin = new Vector2(0.5f, 0f); hvr.anchorMax = new Vector2(1f, 1f);
            hvr.offsetMin = Vector2.zero; hvr.offsetMax = new Vector2(-10f, 0f);

            // Condition Card
            GameObject condCard = new GameObject("ConditionCard", typeof(RectTransform));
            condCard.transform.SetParent(rightStatusCol.transform, false);
            LayoutElement ccLe = condCard.AddComponent<LayoutElement>();
            ccLe.preferredHeight = 120f;
            Image ccCardBg = condCard.AddComponent<Image>();
            ccCardBg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            Outline ccOl = condCard.AddComponent<Outline>();
            ccOl.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
            ccOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI condTmp = CreateTMPText(condCard.transform, "Text",
                "<color=#f39c12><b>TACTICAL CONDITION:</b></color>\n" +
                "• Shelter: Unit 404 (Living Room)\n" +
                "• Status: Unprepared / In Pajamas\n" +
                "• Threat: Civil Danger Warning (Category 5)\n" +
                "• Time to Sector Purge: 06:15 remaining", 11f, new Color(0.75f, 0.78f, 0.84f));
            condTmp.lineSpacing = 6f;
            RectTransform ctr = condTmp.rectTransform;
            StretchFull(ctr);
            ctr.offsetMin = new Vector2(12f, 8f);
            ctr.offsetMax = new Vector2(-12f, -8f);

            // TAB 2: SKILLS PANEL (High-Tech Illuminated HUD Matrix)
            GameObject pSkillsObj = new GameObject("Panel_Skills", typeof(RectTransform));
            pSkillsObj.transform.SetParent(pdaContentObj.transform, false);
            RectTransform pskRect = pSkillsObj.GetComponent<RectTransform>();
            StretchFull(pskRect);
            pskRect.offsetMin = new Vector2(16f, 14f); pskRect.offsetMax = new Vector2(-16f, -14f);
            VerticalLayoutGroup pskVlg = pSkillsObj.AddComponent<VerticalLayoutGroup>();
            pskVlg.spacing = 8f;
            pskVlg.childControlWidth = true; pskVlg.childControlHeight = true;
            pskVlg.childForceExpandWidth = true; pskVlg.childForceExpandHeight = false;
            pSkillsObj.SetActive(false);

            TacticalSkillsViewController skillsCtrl = BuildTacticalSkillsHUD(pSkillsObj, out TextMeshProUGUI pdaAttributesTmp, out TextMeshProUGUI pdaSkillsTmp);

            // TAB 3: TRAITS PANEL
            GameObject pTraitsObj = new GameObject("Panel_Traits", typeof(RectTransform));
            pTraitsObj.transform.SetParent(pdaContentObj.transform, false);
            RectTransform ptrRect = pTraitsObj.GetComponent<RectTransform>();
            StretchFull(ptrRect);
            ptrRect.offsetMin = new Vector2(16f, 14f); ptrRect.offsetMax = new Vector2(-16f, -14f);
            VerticalLayoutGroup ptrVlg = pTraitsObj.AddComponent<VerticalLayoutGroup>();
            ptrVlg.spacing = 8f;
            ptrVlg.childControlWidth = true; ptrVlg.childControlHeight = true;
            ptrVlg.childForceExpandWidth = true; ptrVlg.childForceExpandHeight = false;
            pTraitsObj.SetActive(false);

            CreateSectionBadge(pTraitsObj.transform, "TRAITS & SPECIAL PERKS");
            GameObject traitsScroll = new GameObject("TraitsScroll", typeof(RectTransform));
            traitsScroll.transform.SetParent(pTraitsObj.transform, false);
            LayoutElement tsLe = traitsScroll.AddComponent<LayoutElement>();
            tsLe.flexibleWidth = 1f;
            tsLe.flexibleHeight = 1f;
            RectTransform tsScrollRect = traitsScroll.GetComponent<RectTransform>();
            StretchFull(tsScrollRect);

            GameObject traitsContent = new GameObject("Content", typeof(RectTransform));
            traitsContent.transform.SetParent(traitsScroll.transform, false);
            RectTransform tcRect = traitsContent.GetComponent<RectTransform>();
            tcRect.anchorMin = new Vector2(0f, 1f); tcRect.anchorMax = new Vector2(1f, 1f);
            tcRect.pivot = new Vector2(0.5f, 1f);
            tcRect.sizeDelta = new Vector2(0f, 0f);
            VerticalLayoutGroup tcVlg = traitsContent.AddComponent<VerticalLayoutGroup>();
            tcVlg.spacing = 8f;
            tcVlg.childControlWidth = true; tcVlg.childControlHeight = false;
            ContentSizeFitter tcCsf = traitsContent.AddComponent<ContentSizeFitter>();
            tcCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // TAB 4: MAP PANEL (Architectural Blueprint & Bio-Radar Scanner)
            GameObject pMapObj = new GameObject("Panel_Map", typeof(RectTransform));
            pMapObj.transform.SetParent(pdaContentObj.transform, false);
            RectTransform pmRect = pMapObj.GetComponent<RectTransform>();
            StretchFull(pmRect);
            pmRect.offsetMin = new Vector2(16f, 14f); pmRect.offsetMax = new Vector2(-16f, -14f);
            pMapObj.SetActive(false);

            TacticalMapViewController mapCtrl = BuildTacticalConnectedMap(pMapObj);

            // TAB 5: LORE PANEL
            GameObject pLoreObj = new GameObject("Panel_Lore", typeof(RectTransform));
            pLoreObj.transform.SetParent(pdaContentObj.transform, false);
            RectTransform plRect = pLoreObj.GetComponent<RectTransform>();
            StretchFull(plRect);
            plRect.offsetMin = new Vector2(16f, 14f); plRect.offsetMax = new Vector2(-16f, -14f);
            pLoreObj.SetActive(false);

            CreateSectionBadge(pLoreObj.transform, "RECORDED INTEL & LOGS");
            GameObject loreCard = new GameObject("LoreCard", typeof(RectTransform));
            loreCard.transform.SetParent(pLoreObj.transform, false);
            RectTransform lc2Rect = loreCard.GetComponent<RectTransform>();
            lc2Rect.anchorMin = new Vector2(0f, 0f); lc2Rect.anchorMax = new Vector2(1f, 1f);
            lc2Rect.offsetMin = new Vector2(0f, 0f); lc2Rect.offsetMax = new Vector2(0f, -32f);
            Image lc2Bg = loreCard.AddComponent<Image>();
            lc2Bg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);

            TextMeshProUGUI pdaLoreTmp = CreateTMPText(loreCard.transform, "LoreText", "", 11.5f, new Color(0.85f, 0.88f, 0.92f));
            pdaLoreTmp.lineSpacing = 6f;
            RectTransform ltr = pdaLoreTmp.rectTransform;
            StretchFull(ltr);
            ltr.offsetMin = new Vector2(14f, 10f); ltr.offsetMax = new Vector2(-14f, -10f);

            // -------------------------------------------------------------------------
            // 4C. DOCK CONTAINER 3: TACTICAL GEAR (MODE C - Widescreen Workstation Layout)
            // -------------------------------------------------------------------------
            GameObject dockGear = new GameObject("Dock_TacticalGear", typeof(RectTransform));
            dockGear.transform.SetParent(botScreen.transform, false);
            RectTransform dgRect = dockGear.GetComponent<RectTransform>();
            dgRect.anchorMin = new Vector2(0.5f, 0.5f); dgRect.anchorMax = new Vector2(0.5f, 0.5f);
            dgRect.pivot = new Vector2(0.5f, 0.5f);
            dgRect.sizeDelta = new Vector2(1620f, 400f);
            dgRect.anchoredPosition = Vector2.zero;
            dockGear.SetActive(false);

            TacticalGearViewController gearCtrl = dockGear.AddComponent<TacticalGearViewController>();

            // Sub-Panel 1 (Left 420px): Active Loadout Card
            GameObject loadoutCard = new GameObject("Card_ActiveLoadout", typeof(RectTransform));
            loadoutCard.transform.SetParent(dockGear.transform, false);
            RectTransform lcCardRect = loadoutCard.GetComponent<RectTransform>();
            lcCardRect.anchorMin = new Vector2(0f, 0f); lcCardRect.anchorMax = new Vector2(0f, 1f);
            lcCardRect.pivot = new Vector2(0f, 0.5f);
            lcCardRect.offsetMin = Vector2.zero; lcCardRect.offsetMax = new Vector2(420f, 0f);
            Image lcBgImg = loadoutCard.AddComponent<Image>();
            lcBgImg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            Outline lcCardOl = loadoutCard.AddComponent<Outline>();
            lcCardOl.effectColor = new Color(0.95f, 0.48f, 0.12f, 0.55f);
            lcCardOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI lheadTmp = CreateTMPText(loadoutCard.transform, "Header", "ACTIVE LOADOUT", 12.5f, new Color(0.95f, 0.48f, 0.12f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform lhr2 = lheadTmp.rectTransform;
            lhr2.anchorMin = new Vector2(0f, 1f); lhr2.anchorMax = new Vector2(1f, 1f);
            lhr2.pivot = new Vector2(0f, 1f);
            lhr2.anchoredPosition = new Vector2(14f, -10f);
            lhr2.sizeDelta = new Vector2(-28f, 22f);

            // Wireframe Human Body Silhouette
            GameObject silObj = new GameObject("BodySilhouette", typeof(RectTransform));
            silObj.transform.SetParent(loadoutCard.transform, false);
            RectTransform silRect = silObj.GetComponent<RectTransform>();
            silRect.anchorMin = new Vector2(0.5f, 0.5f); silRect.anchorMax = new Vector2(0.5f, 0.5f);
            silRect.pivot = new Vector2(0.5f, 0.5f);
            silRect.anchoredPosition = new Vector2(0f, -8f);
            silRect.sizeDelta = new Vector2(270f, 360f);
            Image silImg = silObj.AddComponent<Image>();
            silImg.preserveAspect = true;
            silImg.color = Color.white;
            silImg.raycastTarget = false;
            Sprite silSprite = LoadSpriteSafe("Assets/Sprites/UI/silhouette.png");
            if (silSprite != null) silImg.sprite = silSprite;

            CreateEquipSlot(silObj.transform, "Slot_HEAD", new Vector2(0f, 142f), "HEAD", out Button headBtn, out Image headIcon, out TextMeshProUGUI headLbl, out Outline headOl);
            CreateEquipSlot(silObj.transform, "Slot_BODY", new Vector2(0f, 80f), "BODY", out Button bodyBtn, out Image bodyIcon, out TextMeshProUGUI bodyLbl, out Outline bodyOl);
            CreateEquipSlot(silObj.transform, "Slot_WPN", new Vector2(-76f, -10f), "WPN", out Button wpnBtn, out Image wpnIcon, out TextMeshProUGUI wpnLbl, out Outline wpnOl);
            CreateEquipSlot(silObj.transform, "Slot_LEGS", new Vector2(0f, -58f), "LEGS", out Button legsBtn, out Image legsIcon, out TextMeshProUGUI legsLbl, out Outline legsOl);
            CreateEquipSlot(silObj.transform, "Slot_FEET", new Vector2(0f, -155f), "FEET", out Button feetBtn, out Image feetIcon, out TextMeshProUGUI feetLbl, out Outline feetOl);

            // Sub-Panel 2 (Center 680px): Backpack Grid Card
            GameObject bagCard = new GameObject("Card_Backpack", typeof(RectTransform));
            bagCard.transform.SetParent(dockGear.transform, false);
            RectTransform bcRect = bagCard.GetComponent<RectTransform>();
            bcRect.anchorMin = new Vector2(0.5f, 0.5f); bcRect.anchorMax = new Vector2(0.5f, 0.5f);
            bcRect.pivot = new Vector2(0.5f, 0.5f);
            bcRect.sizeDelta = new Vector2(680f, 400f);
            bcRect.anchoredPosition = new Vector2(-10f, 0f);
            Image bcCardBg = bagCard.AddComponent<Image>();
            bcCardBg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            Outline bcOl = bagCard.AddComponent<Outline>();
            bcOl.effectColor = new Color(0.95f, 0.48f, 0.12f, 0.55f);
            bcOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI bheadTmp = CreateTMPText(bagCard.transform, "Header", "BACKPACK", 12.5f, new Color(0.95f, 0.48f, 0.12f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform bhr = bheadTmp.rectTransform;
            bhr.anchorMin = new Vector2(0f, 1f); bhr.anchorMax = new Vector2(0.4f, 1f);
            bhr.pivot = new Vector2(0f, 1f);
            bhr.anchoredPosition = new Vector2(16f, -10f);
            bhr.sizeDelta = new Vector2(0f, 22f);

            TextMeshProUGUI pdaBackpackCountTmp = CreateTMPText(bagCard.transform, "Count", "[ 0 / 12 SLOTS ]", 11.5f, new Color(0.65f, 0.68f, 0.72f), TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            RectTransform bcr = pdaBackpackCountTmp.rectTransform;
            bcr.anchorMin = new Vector2(0.6f, 1f); bcr.anchorMax = new Vector2(1f, 1f);
            bcr.pivot = new Vector2(1f, 1f);
            bcr.anchoredPosition = new Vector2(-16f, -10f);
            bcr.sizeDelta = new Vector2(0f, 22f);

            // Capacity fill bar
            GameObject capTrackObj = new GameObject("CapacityTrack", typeof(RectTransform));
            capTrackObj.transform.SetParent(bagCard.transform, false);
            RectTransform capTrackRt = capTrackObj.GetComponent<RectTransform>();
            capTrackRt.anchorMin = new Vector2(0f, 1f); capTrackRt.anchorMax = new Vector2(1f, 1f);
            capTrackRt.pivot = new Vector2(0.5f, 1f);
            capTrackRt.anchoredPosition = new Vector2(0f, -34f);
            capTrackRt.sizeDelta = new Vector2(-32f, 4f);
            Image ctImg = capTrackObj.AddComponent<Image>();
            ctImg.color = new Color(0.12f, 0.14f, 0.18f, 1f);

            GameObject capFillObj = new GameObject("Fill", typeof(RectTransform));
            capFillObj.transform.SetParent(capTrackObj.transform, false);
            RectTransform cft = capFillObj.GetComponent<RectTransform>();
            StretchFull(cft);
            Image capFillImg = capFillObj.AddComponent<Image>();
            capFillImg.color = new Color(0.22f, 0.74f, 0.97f, 1f);
            capFillImg.type = Image.Type.Filled;
            capFillImg.fillMethod = Image.FillMethod.Horizontal;
            capFillImg.fillAmount = 0.5f;

            // Filter tabs row
            GameObject filterRowObj = new GameObject("FilterRow", typeof(RectTransform));
            filterRowObj.transform.SetParent(bagCard.transform, false);
            RectTransform frt = filterRowObj.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0f, 1f); frt.anchorMax = new Vector2(1f, 1f);
            frt.pivot = new Vector2(0.5f, 1f);
            frt.anchoredPosition = new Vector2(0f, -44f);
            frt.sizeDelta = new Vector2(-32f, 24f);
            HorizontalLayoutGroup frHlg = filterRowObj.AddComponent<HorizontalLayoutGroup>();
            frHlg.spacing = 6f;
            frHlg.childAlignment = TextAnchor.MiddleLeft;
            frHlg.childControlWidth = true; frHlg.childControlHeight = true;
            frHlg.childForceExpandWidth = true; frHlg.childForceExpandHeight = true;

            CreateFilterButton(filterRowObj.transform, "Btn_All", "ALL", out Button btnFAll);
            CreateFilterButton(filterRowObj.transform, "Btn_Wpn", "WEAPONS", out Button btnFWpn);
            CreateFilterButton(filterRowObj.transform, "Btn_Med", "MED / FOOD", out Button btnFMed);
            CreateFilterButton(filterRowObj.transform, "Btn_App", "APPAREL", out Button btnFApp);
            CreateFilterButton(filterRowObj.transform, "Btn_Tool", "TOOLS", out Button btnFTool);

            // Backpack Grid
            GameObject bagGridObj = new GameObject("BackpackGrid", typeof(RectTransform));
            bagGridObj.transform.SetParent(bagCard.transform, false);
            RectTransform bgr = bagGridObj.GetComponent<RectTransform>();
            StretchFull(bgr);
            bgr.offsetMin = new Vector2(16f, 32f);
            bgr.offsetMax = new Vector2(-16f, -74f);
            GridLayoutGroup bgGlg = bagGridObj.AddComponent<GridLayoutGroup>();
            bgGlg.cellSize = new Vector2(74f, 74f);
            bgGlg.spacing = new Vector2(10f, 10f);
            bgGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            bgGlg.constraintCount = 5;

            // Weight text
            TextMeshProUGUI weightTmp = CreateTMPText(bagCard.transform, "WeightText", "ENCUMBRANCE: 0.0 KG", 10.5f, new Color(0.55f, 0.60f, 0.68f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform wtr = weightTmp.rectTransform;
            wtr.anchorMin = new Vector2(0f, 0f); wtr.anchorMax = new Vector2(1f, 0f);
            wtr.pivot = new Vector2(0f, 0f);
            wtr.anchoredPosition = new Vector2(16f, 8f);
            wtr.sizeDelta = new Vector2(-32f, 20f);

            // Sub-Panel 3 (Right 490px): Dual Inspector / Room Loot Stash Card
            GameObject rightCard = new GameObject("Card_RightInspectorStash", typeof(RectTransform));
            rightCard.transform.SetParent(dockGear.transform, false);
            RectTransform rcRect = rightCard.GetComponent<RectTransform>();
            rcRect.anchorMin = new Vector2(1f, 0f); rcRect.anchorMax = new Vector2(1f, 1f);
            rcRect.pivot = new Vector2(1f, 0.5f);
            rcRect.offsetMin = new Vector2(-490f, 0f); rcRect.offsetMax = Vector2.zero;
            Image rcCardBg = rightCard.AddComponent<Image>();
            rcCardBg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            Outline rcOl = rightCard.AddComponent<Outline>();
            rcOl.effectColor = new Color(0.20f, 0.24f, 0.30f, 0.85f);
            rcOl.effectDistance = new Vector2(1f, -1f);

            // Top Dual Tabs
            GameObject rightTabsObj = new GameObject("RightTabs", typeof(RectTransform));
            rightTabsObj.transform.SetParent(rightCard.transform, false);
            RectTransform rtr = rightTabsObj.GetComponent<RectTransform>();
            rtr.anchorMin = new Vector2(0f, 1f); rtr.anchorMax = new Vector2(1f, 1f);
            rtr.pivot = new Vector2(0.5f, 1f);
            rtr.anchoredPosition = new Vector2(0f, -8f);
            rtr.sizeDelta = new Vector2(-20f, 26f);
            HorizontalLayoutGroup rtHlg = rightTabsObj.AddComponent<HorizontalLayoutGroup>();
            rtHlg.spacing = 8f;
            rtHlg.childAlignment = TextAnchor.MiddleCenter;
            rtHlg.childControlWidth = true; rtHlg.childControlHeight = true;
            rtHlg.childForceExpandWidth = true; rtHlg.childForceExpandHeight = true;

            CreateTabPillButton(rightTabsObj.transform, "Tab_Dossier", "ITEM DOSSIER", out Button tabDossierBtn, out TextMeshProUGUI _);
            CreateTabPillButton(rightTabsObj.transform, "Tab_Floor", "ROOM ITEMS", out Button tabFloorBtn, out TextMeshProUGUI tabFloorCountTmp);

            // 1. Panel: Item Dossier
            GameObject dossierPanelObj = new GameObject("Panel_ItemDossier", typeof(RectTransform));
            dossierPanelObj.transform.SetParent(rightCard.transform, false);
            RectTransform dpr = dossierPanelObj.GetComponent<RectTransform>();
            StretchFull(dpr);
            dpr.offsetMin = new Vector2(12f, 10f);
            dpr.offsetMax = new Vector2(-12f, -38f);

            // Empty Notice
            GameObject dEmptyObj = new GameObject("EmptyState", typeof(RectTransform));
            dEmptyObj.transform.SetParent(dossierPanelObj.transform, false);
            RectTransform der = dEmptyObj.GetComponent<RectTransform>();
            StretchFull(der);
            TextMeshProUGUI dEmptyTmp = CreateTMPText(dEmptyObj.transform, "Text", "(Select an item in your backpack or loadout slot to inspect tactical intel & specs)", 11f, new Color(0.5f, 0.55f, 0.6f), TextAlignmentOptions.Center, FontStyles.Italic);
            RectTransform det = dEmptyTmp.rectTransform;
            StretchFull(det);

            // Detail Content Container
            GameObject dContentObj = new GameObject("DetailContent", typeof(RectTransform));
            dContentObj.transform.SetParent(dossierPanelObj.transform, false);
            RectTransform dcr = dContentObj.GetComponent<RectTransform>();
            StretchFull(dcr);
            dContentObj.SetActive(false);

            // Header Row (Icon + Name + Category + Caliber/Weight)
            GameObject dTopRow = new GameObject("TopRow", typeof(RectTransform));
            dTopRow.transform.SetParent(dContentObj.transform, false);
            RectTransform dtr = dTopRow.GetComponent<RectTransform>();
            dtr.anchorMin = new Vector2(0f, 1f); dtr.anchorMax = new Vector2(1f, 1f);
            dtr.pivot = new Vector2(0f, 1f);
            dtr.anchoredPosition = new Vector2(0f, 0f);
            dtr.sizeDelta = new Vector2(0f, 76f);

            // Holographic Icon Reticle Frame
            GameObject iconFrame = new GameObject("IconFrame", typeof(RectTransform));
            iconFrame.transform.SetParent(dTopRow.transform, false);
            RectTransform ifr = iconFrame.GetComponent<RectTransform>();
            ifr.anchorMin = new Vector2(0f, 0.5f); ifr.anchorMax = new Vector2(0f, 0.5f);
            ifr.pivot = new Vector2(0f, 0.5f);
            ifr.anchoredPosition = new Vector2(0f, 0f);
            ifr.sizeDelta = new Vector2(72f, 72f);
            Image ifBg = iconFrame.AddComponent<Image>();
            ifBg.color = new Color(0.08f, 0.10f, 0.14f, 0.95f);
            Outline ifOl = iconFrame.AddComponent<Outline>();
            ifOl.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.75f);
            ifOl.effectDistance = new Vector2(1f, -1f);

            GameObject dIconObj = new GameObject("Icon");
            dIconObj.transform.SetParent(iconFrame.transform, false);
            Image dIconImg = dIconObj.AddComponent<Image>();
            dIconImg.preserveAspect = true;
            dIconImg.raycastTarget = false;
            RectTransform dir = dIconObj.GetComponent<RectTransform>();
            StretchFull(dir);
            dir.offsetMin = new Vector2(6f, 6f);
            dir.offsetMax = new Vector2(-6f, -6f);

            // Title Column
            GameObject titleCol = new GameObject("TitleCol", typeof(RectTransform));
            titleCol.transform.SetParent(dTopRow.transform, false);
            RectTransform tcr = titleCol.GetComponent<RectTransform>();
            tcr.anchorMin = new Vector2(0f, 0f); tcr.anchorMax = new Vector2(1f, 1f);
            tcr.offsetMin = new Vector2(82f, 0f);
            tcr.offsetMax = Vector2.zero;

            TextMeshProUGUI dNameTmp = CreateTMPText(titleCol.transform, "ItemName", "GLOCK 19", 14f, new Color(0.95f, 0.75f, 0.20f), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            RectTransform dnt = dNameTmp.rectTransform;
            dnt.anchorMin = new Vector2(0f, 1f); dnt.anchorMax = new Vector2(1f, 1f);
            dnt.pivot = new Vector2(0f, 1f);
            dnt.anchoredPosition = new Vector2(0f, 0f);
            dnt.sizeDelta = new Vector2(0f, 20f);

            TextMeshProUGUI dCatTmp = CreateTMPText(titleCol.transform, "Category", "<color=#38BDF8>[RANGED WEAPON]</color>", 10.5f, Color.white, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            RectTransform dct = dCatTmp.rectTransform;
            dct.anchorMin = new Vector2(0f, 1f); dct.anchorMax = new Vector2(1f, 1f);
            dct.pivot = new Vector2(0f, 1f);
            dct.anchoredPosition = new Vector2(0f, -22f);
            dct.sizeDelta = new Vector2(0f, 18f);

            TextMeshProUGUI dSubTmp = CreateTMPText(titleCol.transform, "SubInfo", "SLOT: WPN | CALIBER: 9mm Ammo | 1.2 KG", 9.5f, new Color(0.65f, 0.70f, 0.75f), TextAlignmentOptions.TopLeft);
            RectTransform dst = dSubTmp.rectTransform;
            dst.anchorMin = new Vector2(0f, 1f); dst.anchorMax = new Vector2(1f, 1f);
            dst.pivot = new Vector2(0f, 1f);
            dst.anchoredPosition = new Vector2(0f, -42f);
            dst.sizeDelta = new Vector2(0f, 18f);

            // Stats Sheet Card
            GameObject statsBox = new GameObject("StatsBox", typeof(RectTransform));
            statsBox.transform.SetParent(dContentObj.transform, false);
            RectTransform sbr = statsBox.GetComponent<RectTransform>();
            sbr.anchorMin = new Vector2(0f, 1f); sbr.anchorMax = new Vector2(1f, 1f);
            sbr.pivot = new Vector2(0f, 1f);
            sbr.anchoredPosition = new Vector2(0f, -84f);
            sbr.sizeDelta = new Vector2(0f, 76f);
            Image statsBoxBg = statsBox.AddComponent<Image>();
            statsBoxBg.color = new Color(0.06f, 0.08f, 0.10f, 0.95f);
            Outline sbOl = statsBox.AddComponent<Outline>();
            sbOl.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.75f);
            sbOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI dStatsTmp = CreateTMPText(statsBox.transform, "StatsText", "TACTICAL ACCURACY: 60%\nMAX TARGET ENGAGEMENT: 1 Hostile\nMAGAZINE SPEC: 9mm Ammo", 10f, new Color(0.85f, 0.88f, 0.92f), TextAlignmentOptions.TopLeft);
            RectTransform dsr = dStatsTmp.rectTransform;
            StretchFull(dsr);
            dsr.offsetMin = new Vector2(10f, 6f);
            dsr.offsetMax = new Vector2(-10f, -6f);

            // Lore description
            GameObject descBox = new GameObject("DescBox", typeof(RectTransform));
            descBox.transform.SetParent(dContentObj.transform, false);
            RectTransform dbr = descBox.GetComponent<RectTransform>();
            dbr.anchorMin = new Vector2(0f, 1f); dbr.anchorMax = new Vector2(1f, 1f);
            dbr.pivot = new Vector2(0f, 1f);
            dbr.anchoredPosition = new Vector2(0f, -166f);
            dbr.sizeDelta = new Vector2(0f, 76f);

            TextMeshProUGUI dDescTmp = CreateTMPText(descBox.transform, "DescText", "\"Reliable 9mm police service pistol.\"", 10.5f, new Color(0.70f, 0.75f, 0.80f), TextAlignmentOptions.TopLeft, FontStyles.Italic);
            RectTransform ddr = dDescTmp.rectTransform;
            StretchFull(ddr);

            // Actions Row
            GameObject actRow = new GameObject("ActionButtonsRow", typeof(RectTransform));
            actRow.transform.SetParent(dContentObj.transform, false);
            RectTransform arr = actRow.GetComponent<RectTransform>();
            arr.anchorMin = new Vector2(0f, 0f); arr.anchorMax = new Vector2(1f, 0f);
            arr.pivot = new Vector2(0.5f, 0f);
            arr.anchoredPosition = new Vector2(0f, 28f);
            arr.sizeDelta = new Vector2(0f, 32f);
            HorizontalLayoutGroup arHlg = actRow.AddComponent<HorizontalLayoutGroup>();
            arHlg.spacing = 8f;
            arHlg.childAlignment = TextAnchor.MiddleCenter;
            arHlg.childControlWidth = true; arHlg.childControlHeight = true;
            arHlg.childForceExpandWidth = true; arHlg.childForceExpandHeight = true;

            CreateDossierActionButton(actRow.transform, "Btn_Equip", "EQUIP GEAR", new Color(0.14f, 0.54f, 0.78f), out Button btnEquip);
            CreateDossierActionButton(actRow.transform, "Btn_Unequip", "UNEQUIP", new Color(0.85f, 0.52f, 0.12f), out Button btnUnequip);
            CreateDossierActionButton(actRow.transform, "Btn_Use", "CONSUME / USE", new Color(0.13f, 0.65f, 0.32f), out Button btnUse);
            CreateDossierActionButton(actRow.transform, "Btn_Drop", "DROP TO ROOM", new Color(0.72f, 0.22f, 0.22f), out Button btnDrop);
            CreateDossierActionButton(actRow.transform, "Btn_Take", "TAKE ITEM", new Color(0.13f, 0.65f, 0.32f), out Button btnTake);

            // Toast text
            TextMeshProUGUI dToastTmp = CreateTMPText(dContentObj.transform, "ToastText", "", 10f, new Color(0.95f, 0.85f, 0.45f), TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform dttr = dToastTmp.rectTransform;
            dttr.anchorMin = new Vector2(0f, 0f); dttr.anchorMax = new Vector2(1f, 0f);
            dttr.pivot = new Vector2(0.5f, 0f);
            dttr.anchoredPosition = new Vector2(0f, 4f);
            dttr.sizeDelta = new Vector2(0f, 20f);

            // 2. Panel: Floor / Room Loot Stash
            GameObject floorPanelObj = new GameObject("Panel_FloorItems", typeof(RectTransform));
            floorPanelObj.transform.SetParent(rightCard.transform, false);
            RectTransform fpr = floorPanelObj.GetComponent<RectTransform>();
            StretchFull(fpr);
            fpr.offsetMin = new Vector2(12f, 10f);
            fpr.offsetMax = new Vector2(-12f, -38f);
            floorPanelObj.SetActive(false);

            GameObject fheadObj = new GameObject("FloorHeaderRow", typeof(RectTransform));
            fheadObj.transform.SetParent(floorPanelObj.transform, false);
            RectTransform fhrr = fheadObj.GetComponent<RectTransform>();
            fhrr.anchorMin = new Vector2(0f, 1f); fhrr.anchorMax = new Vector2(1f, 1f);
            fhrr.pivot = new Vector2(0f, 1f);
            fhrr.anchoredPosition = new Vector2(0f, 0f);
            fhrr.sizeDelta = new Vector2(0f, 28f);

            TextMeshProUGUI fheadTmp = CreateTMPText(fheadObj.transform, "Header", "ROOM / TABLE ITEMS", 11.5f, new Color(0.91f, 0.30f, 0.24f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform fhr = fheadTmp.rectTransform;
            fhr.anchorMin = new Vector2(0f, 0f); fhr.anchorMax = new Vector2(0.55f, 1f);
            fhr.offsetMin = Vector2.zero; fhr.offsetMax = Vector2.zero;

            GameObject fBtnObj = new GameObject("Btn_GatherFloor", typeof(RectTransform));
            fBtnObj.transform.SetParent(fheadObj.transform, false);
            RectTransform fbr = fBtnObj.GetComponent<RectTransform>();
            fbr.anchorMin = new Vector2(0.55f, 0f); fbr.anchorMax = new Vector2(1f, 1f);
            fbr.offsetMin = Vector2.zero; fbr.offsetMax = Vector2.zero;
            Image fbImg = fBtnObj.AddComponent<Image>();
            fbImg.color = new Color(0.78f, 0.32f, 0.26f, 0.95f);
            Button btnFloorGather = fBtnObj.AddComponent<Button>();
            btnFloorGather.targetGraphic = fbImg;

            TextMeshProUGUI floorCountTmp = CreateTMPText(fBtnObj.transform, "Text", "TAKE ALL", 10.5f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform fbtRect = floorCountTmp.rectTransform;
            StretchFull(fbtRect);

            GameObject floorGridObj = new GameObject("FloorGrid", typeof(RectTransform));
            floorGridObj.transform.SetParent(floorPanelObj.transform, false);
            RectTransform fgr = floorGridObj.GetComponent<RectTransform>();
            StretchFull(fgr);
            fgr.offsetMin = new Vector2(0f, 0f);
            fgr.offsetMax = new Vector2(0f, -34f);
            GridLayoutGroup fgGlg = floorGridObj.AddComponent<GridLayoutGroup>();
            fgGlg.cellSize = new Vector2(72f, 72f);
            fgGlg.spacing = new Vector2(8f, 8f);
            fgGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            fgGlg.constraintCount = 5;

            TextMeshProUGUI pdaFloorEmptyTmp = CreateTMPText(floorPanelObj.transform, "EmptyNotice", "(Table is empty)", 11.5f, new Color(0.55f, 0.58f, 0.62f), TextAlignmentOptions.Center, FontStyles.Italic);
            RectTransform fer = pdaFloorEmptyTmp.rectTransform;
            StretchFull(fer);

            // Configure Serialized Properties for TacticalGearViewController
            SerializedObject soGear = new SerializedObject(gearCtrl);
            soGear.FindProperty("slotHeadBtn").objectReferenceValue = headBtn;
            soGear.FindProperty("slotHeadIcon").objectReferenceValue = headIcon;
            soGear.FindProperty("slotHeadLabel").objectReferenceValue = headLbl;
            soGear.FindProperty("slotHeadOutline").objectReferenceValue = headOl;

            soGear.FindProperty("slotBodyBtn").objectReferenceValue = bodyBtn;
            soGear.FindProperty("slotBodyIcon").objectReferenceValue = bodyIcon;
            soGear.FindProperty("slotBodyLabel").objectReferenceValue = bodyLbl;
            soGear.FindProperty("slotBodyOutline").objectReferenceValue = bodyOl;

            soGear.FindProperty("slotWpnBtn").objectReferenceValue = wpnBtn;
            soGear.FindProperty("slotWpnIcon").objectReferenceValue = wpnIcon;
            soGear.FindProperty("slotWpnLabel").objectReferenceValue = wpnLbl;
            soGear.FindProperty("slotWpnOutline").objectReferenceValue = wpnOl;

            soGear.FindProperty("slotLegsBtn").objectReferenceValue = legsBtn;
            soGear.FindProperty("slotLegsIcon").objectReferenceValue = legsIcon;
            soGear.FindProperty("slotLegsLabel").objectReferenceValue = legsLbl;
            soGear.FindProperty("slotLegsOutline").objectReferenceValue = legsOl;

            soGear.FindProperty("slotFeetBtn").objectReferenceValue = feetBtn;
            soGear.FindProperty("slotFeetIcon").objectReferenceValue = feetIcon;
            soGear.FindProperty("slotFeetLabel").objectReferenceValue = feetLbl;
            soGear.FindProperty("slotFeetOutline").objectReferenceValue = feetOl;

            soGear.FindProperty("backpackGridRoot").objectReferenceValue = bagGridObj.transform;
            soGear.FindProperty("backpackCountText").objectReferenceValue = pdaBackpackCountTmp;
            soGear.FindProperty("backpackCapacityFill").objectReferenceValue = capFillImg;
            soGear.FindProperty("weightText").objectReferenceValue = weightTmp;

            soGear.FindProperty("btnFilterAll").objectReferenceValue = btnFAll;
            soGear.FindProperty("btnFilterWeapons").objectReferenceValue = btnFWpn;
            soGear.FindProperty("btnFilterConsumables").objectReferenceValue = btnFMed;
            soGear.FindProperty("btnFilterApparel").objectReferenceValue = btnFApp;
            soGear.FindProperty("btnFilterTools").objectReferenceValue = btnFTool;

            soGear.FindProperty("tabBtnDossier").objectReferenceValue = tabDossierBtn;
            soGear.FindProperty("tabBtnFloor").objectReferenceValue = tabFloorBtn;
            soGear.FindProperty("tabFloorCountText").objectReferenceValue = tabFloorCountTmp;
            soGear.FindProperty("dossierPanel").objectReferenceValue = dossierPanelObj;
            soGear.FindProperty("floorPanel").objectReferenceValue = floorPanelObj;

            soGear.FindProperty("dossierEmptyState").objectReferenceValue = dEmptyObj;
            soGear.FindProperty("dossierDetailContent").objectReferenceValue = dContentObj;
            soGear.FindProperty("dossierItemName").objectReferenceValue = dNameTmp;
            soGear.FindProperty("dossierCategoryBadge").objectReferenceValue = dCatTmp;
            soGear.FindProperty("dossierIcon").objectReferenceValue = dIconImg;
            soGear.FindProperty("dossierSubInfo").objectReferenceValue = dSubTmp;
            soGear.FindProperty("dossierStatsText").objectReferenceValue = dStatsTmp;
            soGear.FindProperty("dossierDescription").objectReferenceValue = dDescTmp;
            soGear.FindProperty("dossierToast").objectReferenceValue = dToastTmp;

            soGear.FindProperty("btnActionEquip").objectReferenceValue = btnEquip;
            soGear.FindProperty("btnActionUnequip").objectReferenceValue = btnUnequip;
            soGear.FindProperty("btnActionUse").objectReferenceValue = btnUse;
            soGear.FindProperty("btnActionDrop").objectReferenceValue = btnDrop;
            soGear.FindProperty("btnActionTake").objectReferenceValue = btnTake;

            soGear.FindProperty("floorGridRoot").objectReferenceValue = floorGridObj.transform;
            soGear.FindProperty("floorEmptyNotice").objectReferenceValue = pdaFloorEmptyTmp;
            soGear.FindProperty("btnTakeAllFloor").objectReferenceValue = btnFloorGather;

            soGear.ApplyModifiedPropertiesWithoutUndo();

            // =========================================================================
            // 5. MODAL OVERLAYS (Phone, Radio, Hacking)
            // =========================================================================
            GameObject modalsRoot = new GameObject("ModalOverlays", typeof(RectTransform));
            modalsRoot.transform.SetParent(obj.transform, false);
            RectTransform modRect = modalsRoot.GetComponent<RectTransform>();
            StretchFull(modRect);

            GameObject phoneObj = BuildPhoneModal(modalsRoot.transform, out Button closePhoneBtn);
            phoneObj.SetActive(false);

            GameObject radioObj = BuildRadioModal(modalsRoot.transform, out Button closeRadioBtn);
            radioObj.SetActive(false);

            GameObject hackObj = BuildHackingModal(modalsRoot.transform, out TextMeshProUGUI hackTerminalTmp, out GameObject hackFinishObj, out Button closeHackBtn);
            hackObj.SetActive(false);

            GameObject storyLogObj = BuildStoryLogModal(modalsRoot.transform, out TextMeshProUGUI logContentTmp, out Button closeLogBtn, out Button closeLogBotBtn);
            storyLogObj.SetActive(false);

            // Audio Clips
            AudioClip atmosphereClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/atmosphere.mp3");
            AudioClip ebsWarningClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/ebs_warning.mp3");
            AudioClip staticClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/static.mp3");
            AudioClip policeRadioClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/police_radio.mp3");
            AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/terminal_beep.mp3");

            // Wire Serialized Properties
            SerializedObject so = new SerializedObject(screen);

            // 3DS Switchable Console Mode Buttons & Containers
            so.FindProperty("btnModeNarrative").objectReferenceValue = btnNarrative;
            so.FindProperty("btnModePDA").objectReferenceValue = btnPDA;
            so.FindProperty("btnModeGear").objectReferenceValue = btnGear;
            so.FindProperty("btnModeGearLabel").objectReferenceValue = gearLabelTmp;
            so.FindProperty("btnModeGearBadge").objectReferenceValue = gearBadgeObj;
            so.FindProperty("modeIndicatorNarrative").objectReferenceValue = indNarrative;
            so.FindProperty("modeIndicatorPDA").objectReferenceValue = indPDA;
            so.FindProperty("modeIndicatorGear").objectReferenceValue = indGear;
            so.FindProperty("dockContainerNarrative").objectReferenceValue = dockNarrative;
            so.FindProperty("dockContainerPDA").objectReferenceValue = dockPDA;
            so.FindProperty("dockContainerGear").objectReferenceValue = dockGear;

            // Top HUD
            so.FindProperty("hpBarFill").objectReferenceValue = hpFill;
            so.FindProperty("hpValueText").objectReferenceValue = hpValTmp;
            so.FindProperty("staminaBarFill").objectReferenceValue = stmFill;
            so.FindProperty("staminaValueText").objectReferenceValue = stmValTmp;
            so.FindProperty("hungerBarFill").objectReferenceValue = hngFill;
            so.FindProperty("hungerValueText").objectReferenceValue = hngValTmp;
            so.FindProperty("thirstBarFill").objectReferenceValue = thrFill;
            so.FindProperty("thirstValueText").objectReferenceValue = thrValTmp;
            so.FindProperty("clockTimeText").objectReferenceValue = clockTimeTmp;
            so.FindProperty("clockDayText").objectReferenceValue = clockDayTmp;
            so.FindProperty("floorItemsCard").objectReferenceValue = rightCard;
            so.FindProperty("floorItemsButton").objectReferenceValue = btnFloorGather;
            so.FindProperty("floorItemsCountText").objectReferenceValue = floorCountTmp;

            // Left Wing / PDA Properties
            so.FindProperty("pdaTabStatusBtn").objectReferenceValue = btnTabStatus;
            so.FindProperty("pdaTabSkillsBtn").objectReferenceValue = btnTabSkills;
            so.FindProperty("pdaTabTraitsBtn").objectReferenceValue = btnTabTraits;
            so.FindProperty("pdaTabMapBtn").objectReferenceValue = btnTabMap;
            so.FindProperty("pdaTabLoreBtn").objectReferenceValue = btnTabLore;
            so.FindProperty("pdaPanelStatus").objectReferenceValue = pStatusObj;
            so.FindProperty("pdaPanelSkills").objectReferenceValue = pSkillsObj;
            so.FindProperty("pdaPanelTraits").objectReferenceValue = pTraitsObj;
            so.FindProperty("pdaPanelMap").objectReferenceValue = pMapObj;
            so.FindProperty("tacticalMapController").objectReferenceValue = mapCtrl;
            so.FindProperty("tacticalSkillsController").objectReferenceValue = skillsCtrl;
            so.FindProperty("tacticalGearController").objectReferenceValue = gearCtrl;
            so.FindProperty("pdaPanelLore").objectReferenceValue = pLoreObj;

            so.FindProperty("pdaCharNameText").objectReferenceValue = pdaCharNameTmp;
            so.FindProperty("pdaCharSubText").objectReferenceValue = pdaCharSubTmp;
            so.FindProperty("pdaActiveLogsText").objectReferenceValue = pdaActiveLogsTmp;
            so.FindProperty("pdaLevelText").objectReferenceValue = pdaLevelTmp;
            so.FindProperty("pdaXpText").objectReferenceValue = pdaXpTmp;
            so.FindProperty("pdaXpBarFill").objectReferenceValue = pdaXpFillImg;
            so.FindProperty("pdaHumanityBarFill").objectReferenceValue = pdaHumFillImg;
            so.FindProperty("pdaHumanityValueText").objectReferenceValue = pdaHumValTmp;

            so.FindProperty("pdaAttributesText").objectReferenceValue = pdaAttributesTmp;
            so.FindProperty("pdaSkillsText").objectReferenceValue = pdaSkillsTmp;
            so.FindProperty("pdaTraitsListRoot").objectReferenceValue = traitsContent.transform;
            so.FindProperty("pdaLoreText").objectReferenceValue = pdaLoreTmp;
            so.FindProperty("pdaVolumeSlider").objectReferenceValue = volSlider;
            so.FindProperty("pdaSaveSettingsBtn").objectReferenceValue = btnSettings;

            // Tactical Gear Properties
            so.FindProperty("equipHeadIcon").objectReferenceValue = headIcon;
            so.FindProperty("equipHeadLabel").objectReferenceValue = headLbl;
            so.FindProperty("equipBodyIcon").objectReferenceValue = bodyIcon;
            so.FindProperty("equipBodyLabel").objectReferenceValue = bodyLbl;
            so.FindProperty("equipWpnIcon").objectReferenceValue = wpnIcon;
            so.FindProperty("equipWpnLabel").objectReferenceValue = wpnLbl;
            so.FindProperty("equipLegsIcon").objectReferenceValue = legsIcon;
            so.FindProperty("equipLegsLabel").objectReferenceValue = legsLbl;
            so.FindProperty("equipFeetIcon").objectReferenceValue = feetIcon;
            so.FindProperty("equipFeetLabel").objectReferenceValue = feetLbl;
            so.FindProperty("pdaBackpackCountText").objectReferenceValue = pdaBackpackCountTmp;
            so.FindProperty("pdaBackpackGrid").objectReferenceValue = bagGridObj.transform;
            so.FindProperty("pdaFloorItemsGrid").objectReferenceValue = floorGridObj.transform;
            so.FindProperty("pdaFloorEmptyText").objectReferenceValue = pdaFloorEmptyTmp;

            // Stage Visuals
            so.FindProperty("stageBackdropImage").objectReferenceValue = roomImg;
            so.FindProperty("livingRoomAnimPlayer").objectReferenceValue = roomAnim;

            so.FindProperty("awakeningContainer").objectReferenceValue = awakeObj;

            // Beat 1
            so.FindProperty("beat1Container").objectReferenceValue = b1Obj;
            so.FindProperty("beat1IntroText").objectReferenceValue = b1Tmp;
            so.FindProperty("easAlertBox").objectReferenceValue = easBox;
            so.FindProperty("easAlertText").objectReferenceValue = easTmp;

            // Beat 2
            so.FindProperty("beat2Container").objectReferenceValue = b2Obj;
            so.FindProperty("beat2IntroText").objectReferenceValue = b2Tmp;
            so.FindProperty("monologueBox").objectReferenceValue = monoBox;
            so.FindProperty("monologueAvatar").objectReferenceValue = mAvatarImg;
            so.FindProperty("monologueSpeakerText").objectReferenceValue = mSpkTmp;
            so.FindProperty("monologueSpeechText").objectReferenceValue = mTextTmp;
            so.FindProperty("beat2FootnoteText").objectReferenceValue = b2fTmp;

            // Beat 3
            so.FindProperty("beat3Container").objectReferenceValue = b3Obj;
            so.FindProperty("beat3NarrativeText").objectReferenceValue = b3Tmp;
            so.FindProperty("tableStashDisplay").objectReferenceValue = stashObj;
            so.FindProperty("tableStashGrid").objectReferenceValue = sGridObj.transform;
            so.FindProperty("tableEmptyNotice").objectReferenceValue = seTmp.gameObject;

            so.FindProperty("continuePrompt").objectReferenceValue = prTmp.gameObject;
            so.FindProperty("fullScreenClickCatcher").objectReferenceValue = ccBtn;

            so.FindProperty("hubMenuContainer").objectReferenceValue = menuObj;
            so.FindProperty("packGearButton").objectReferenceValue = btnPack;
            so.FindProperty("packedSuccessBanner").objectReferenceValue = pBanner;
            so.FindProperty("packedSuccessText").objectReferenceValue = pbTmp;
            so.FindProperty("takeFloorItemsButton").objectReferenceValue = btnTakeFloor;
            so.FindProperty("takeFloorItemsLabel").objectReferenceValue = takeFloorLbl;
            so.FindProperty("takeFloorItemsSub").objectReferenceValue = takeFloorSub;
            so.FindProperty("checkPhoneButton").objectReferenceValue = btnPhone;
            so.FindProperty("listenRadioButton").objectReferenceValue = btnRadio;
            so.FindProperty("listenRadioLabel").objectReferenceValue = radioLbl;
            so.FindProperty("listenRadioSub").objectReferenceValue = radioSub;
            so.FindProperty("hackTerminalButton").objectReferenceValue = btnHacking;
            so.FindProperty("hackTerminalLabel").objectReferenceValue = hackLbl;
            so.FindProperty("hackTerminalSub").objectReferenceValue = hackSub;
            so.FindProperty("leaveApartmentButton").objectReferenceValue = btnLeave;

            so.FindProperty("phoneModal").objectReferenceValue = phoneObj;
            so.FindProperty("closePhoneButton").objectReferenceValue = closePhoneBtn;
            so.FindProperty("radioModal").objectReferenceValue = radioObj;
            so.FindProperty("closeRadioButton").objectReferenceValue = closeRadioBtn;
            so.FindProperty("hackingModal").objectReferenceValue = hackObj;
            so.FindProperty("hackingTerminalText").objectReferenceValue = hackTerminalTmp;
            so.FindProperty("hackingFinishContainer").objectReferenceValue = hackFinishObj;
            so.FindProperty("closeHackingButton").objectReferenceValue = closeHackBtn;

            so.FindProperty("situationBox").objectReferenceValue = sitBox;
            so.FindProperty("situationBoxText").objectReferenceValue = sitBodyTmp;
            so.FindProperty("openStoryLogButton").objectReferenceValue = btnOpenLog;
            so.FindProperty("storyLogModal").objectReferenceValue = storyLogObj;
            so.FindProperty("closeStoryLogButton").objectReferenceValue = closeLogBtn;
            so.FindProperty("closeStoryLogBottomButton").objectReferenceValue = closeLogBotBtn;
            so.FindProperty("storyLogContentText").objectReferenceValue = logContentTmp;

            if (atmosphereClip != null) so.FindProperty("atmosphereClip").objectReferenceValue = atmosphereClip;
            if (ebsWarningClip != null) so.FindProperty("ebsWarningClip").objectReferenceValue = ebsWarningClip;
            if (staticClip != null) so.FindProperty("staticClip").objectReferenceValue = staticClip;
            if (policeRadioClip != null) so.FindProperty("policeRadioClip").objectReferenceValue = policeRadioClip;
            if (clickClip != null) so.FindProperty("clickClip").objectReferenceValue = clickClip;

            so.ApplyModifiedProperties();

            // Register item sprites with ItemDatabase
            RegisterItemSprites();

            return screen;
        }

        private static void CreateConsoleModeButton(Transform parent, string name, string label, out Button button, out Image activeIndicator, out TextMeshProUGUI labelTmp, bool isActiveDefault)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 32f;
            le.flexibleWidth = 1f;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.07f, 0.09f, 0.95f);
            button = btnObj.AddComponent<Button>();
            button.targetGraphic = bg;

            ColorBlock cb = button.colors;
            cb.normalColor = bg.color;
            cb.highlightedColor = new Color(0.14f, 0.16f, 0.20f, 1f);
            cb.pressedColor = new Color(0.04f, 0.05f, 0.06f, 1f);
            button.colors = cb;

            Outline ol = btnObj.AddComponent<Outline>();
            ol.effectColor = new Color(0.24f, 0.28f, 0.35f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            labelTmp = CreateTMPText(btnObj.transform, "Label", label, 11f, new Color(0.88f, 0.90f, 0.94f), TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform tr = labelTmp.rectTransform;
            StretchFull(tr);

            // Active Bottom Neon Line
            GameObject indObj = new GameObject("ActiveIndicator", typeof(RectTransform));
            indObj.transform.SetParent(btnObj.transform, false);
            RectTransform ir = indObj.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0f, 0f); ir.anchorMax = new Vector2(1f, 0f);
            ir.pivot = new Vector2(0.5f, 0f);
            ir.sizeDelta = new Vector2(0f, 3f);
            ir.anchoredPosition = Vector2.zero;
            activeIndicator = indObj.AddComponent<Image>();
            activeIndicator.color = new Color(0.95f, 0.48f, 0.12f); // Neon orange
            indObj.SetActive(isActiveDefault);
        }

        private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, float fontSize, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft, FontStyles style = FontStyles.Normal, bool wrap = true)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = style;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void CreatePDATabButton(Transform parent, string name, string label, out Button button)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 28f;
            le.flexibleWidth = 1f;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.09f, 0.11f, 0.95f);
            button = btnObj.AddComponent<Button>();
            button.targetGraphic = bg;

            Outline ol = btnObj.AddComponent<Outline>();
            ol.effectColor = new Color(0.20f, 0.22f, 0.26f, 0.75f);
            ol.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI tmp = CreateTMPText(btnObj.transform, "Text", label, 10.5f, new Color(0.55f, 0.58f, 0.62f), TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform tr = tmp.rectTransform;
            StretchFull(tr);

            GameObject accent = new GameObject("BottomAccent");
            accent.transform.SetParent(btnObj.transform, false);
            RectTransform ar = accent.AddComponent<RectTransform>();
            ar.anchorMin = new Vector2(0f, 0f); ar.anchorMax = new Vector2(1f, 0f);
            ar.pivot = new Vector2(0.5f, 0f);
            ar.sizeDelta = new Vector2(0f, 2f);
            ar.anchoredPosition = Vector2.zero;
            Image aImg = accent.AddComponent<Image>();
            aImg.color = new Color(0.95f, 0.48f, 0.12f);
            accent.SetActive(false);
        }

        private static void CreateSectionBadge(Transform parent, string title)
        {
            GameObject badge = new GameObject("SectionBadge", typeof(RectTransform));
            badge.transform.SetParent(parent, false);
            RectTransform br = badge.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0f, 1f);
            br.anchorMax = new Vector2(1f, 1f);
            br.pivot = new Vector2(0.5f, 1f);
            br.anchoredPosition = Vector2.zero;
            br.sizeDelta = new Vector2(0f, 24f);
            LayoutElement le = badge.AddComponent<LayoutElement>();
            le.minHeight = 24f;
            le.preferredHeight = 24f;
            Image bg = badge.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.10f, 0.14f, 0.95f);

            TextMeshProUGUI tmp = CreateTMPText(badge.transform, "Text", title, 10.5f, new Color(0.95f, 0.48f, 0.12f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform tr = tmp.rectTransform;
            StretchFull(tr);
            tr.offsetMin = new Vector2(10f, 0f);
            tr.offsetMax = new Vector2(-10f, 0f);
        }

        private static void CreateEquipSlot(Transform parent, string name, Vector2 pos, string slotName,
            out Button slotBtn, out Image iconImg, out TextMeshProUGUI slotLabel, out Outline outline)
        {
            GameObject slotObj = new GameObject(name);
            slotObj.transform.SetParent(parent, false);
            RectTransform sr = slotObj.AddComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.5f, 0.5f);
            sr.anchorMax = new Vector2(0.5f, 0.5f);
            sr.pivot = new Vector2(0.5f, 0.5f);
            sr.anchoredPosition = pos;
            sr.sizeDelta = new Vector2(48f, 48f);

            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            outline = slotObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.7f); // Neon cyan outline
            outline.effectDistance = new Vector2(1f, -1f);

            slotBtn = slotObj.AddComponent<Button>();
            slotBtn.targetGraphic = bg;

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);
            iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            StretchFull(ir);
            ir.offsetMin = new Vector2(4f, 4f);
            ir.offsetMax = new Vector2(-4f, -4f);
            iconObj.SetActive(false);

            slotLabel = CreateTMPText(slotObj.transform, "SlotLabel", slotName, 9.5f, new Color(0.45f, 0.65f, 0.85f), TextAlignmentOptions.Center, FontStyles.Bold);
            slotLabel.raycastTarget = false;
            RectTransform lr = slotLabel.rectTransform;
            StretchFull(lr);
        }

        private static void CreateFilterButton(Transform parent, string name, string label, out Button btn)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.12f, 0.16f, 0.95f);
            Outline ol = obj.AddComponent<Outline>();
            ol.effectColor = new Color(0.22f, 0.28f, 0.36f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);
            btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;

            TextMeshProUGUI tmp = CreateTMPText(obj.transform, "Text", label, 9.5f, new Color(0.80f, 0.85f, 0.90f), TextAlignmentOptions.Center, FontStyles.Bold);
            tmp.raycastTarget = false;
            RectTransform tr = tmp.rectTransform;
            StretchFull(tr);
        }

        private static void CreateTabPillButton(Transform parent, string name, string label, out Button btn, out TextMeshProUGUI textTmp)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);
            Outline ol = obj.AddComponent<Outline>();
            ol.effectColor = new Color(0.25f, 0.35f, 0.45f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);
            btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;

            textTmp = CreateTMPText(obj.transform, "Text", label, 10f, new Color(0.85f, 0.90f, 0.95f), TextAlignmentOptions.Center, FontStyles.Bold);
            textTmp.raycastTarget = false;
            RectTransform tr = textTmp.rectTransform;
            StretchFull(tr);
        }

        private static void CreateDossierActionButton(Transform parent, string name, string label, Color color, out Button btn)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            Image bg = obj.AddComponent<Image>();
            bg.color = color;
            Outline ol = obj.AddComponent<Outline>();
            ol.effectColor = new Color(0f, 0f, 0f, 0.6f);
            ol.effectDistance = new Vector2(1f, -1f);
            btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;

            TextMeshProUGUI tmp = CreateTMPText(obj.transform, "Text", label, 10f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            tmp.raycastTarget = false;
            RectTransform tr = tmp.rectTransform;
            StretchFull(tr);
        }

        private static void CreateVitalBar(Transform parent, string name, Sprite iconSprite, Sprite fillSprite, Color fillColor,
            out Image barFill, out TextMeshProUGUI valueText, string defaultValue = "100")
        {
            GameObject barObj = new GameObject(name);
            barObj.transform.SetParent(parent, false);
            LayoutElement le = barObj.AddComponent<LayoutElement>();
            le.preferredHeight = 22f;
            le.flexibleWidth = 1f;

            Image bg = barObj.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.10f, 0.12f, 1f);
            bg.raycastTarget = false;

            Outline ol = barObj.AddComponent<Outline>();
            ol.effectColor = new Color(0.20f, 0.20f, 0.24f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            // Fill Image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barObj.transform, false);
            RectTransform fr = fillObj.AddComponent<RectTransform>();
            StretchFull(fr);
            barFill = fillObj.AddComponent<Image>();
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            barFill.fillAmount = 1.0f;
            barFill.color = fillColor;
            if (fillSprite != null) barFill.sprite = fillSprite;
            barFill.raycastTarget = false;

            // Overlay content (Icon Left, Numeric Value Right)
            GameObject overlayObj = new GameObject("Overlay");
            overlayObj.transform.SetParent(barObj.transform, false);
            RectTransform or = overlayObj.AddComponent<RectTransform>();
            StretchFull(or);

            if (iconSprite != null)
            {
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(overlayObj.transform, false);
                RectTransform ir = iconObj.AddComponent<RectTransform>();
                ir.anchorMin = new Vector2(0f, 0.5f);
                ir.anchorMax = new Vector2(0f, 0.5f);
                ir.pivot = new Vector2(0f, 0.5f);
                ir.anchoredPosition = new Vector2(6f, 0f);
                ir.sizeDelta = new Vector2(14f, 14f);
                Image ii = iconObj.AddComponent<Image>();
                ii.sprite = iconSprite;
                ii.preserveAspect = true;
                ii.raycastTarget = false;
            }

            GameObject valObj = new GameObject("Value");
            valObj.transform.SetParent(overlayObj.transform, false);
            RectTransform vr = valObj.AddComponent<RectTransform>();
            vr.anchorMin = new Vector2(0f, 0f);
            vr.anchorMax = new Vector2(1f, 1f);
            vr.offsetMin = new Vector2(24f, 0f);
            vr.offsetMax = new Vector2(-8f, 0f);
            valueText = valObj.AddComponent<TextMeshProUGUI>();
            valueText.fontSize = 11f;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.color = Color.white;
            valueText.text = defaultValue;
            valueText.raycastTarget = false;
        }

        private static void CreateApartmentMenuButton(Transform parent, string name, string mainText, string subText,
            Color accentColor, out Button button, out TextMeshProUGUI mainLabel, out TextMeshProUGUI subLabel, Color? bgColor = null)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 54f;
            le.flexibleWidth = 1f;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor ?? new Color(0.08f, 0.09f, 0.11f, 0.98f);
            bg.raycastTarget = true;

            Outline ol = btnObj.AddComponent<Outline>();
            ol.effectColor = new Color(0.24f, 0.16f, 0.16f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            button = btnObj.AddComponent<Button>();
            button.targetGraphic = bg;

            ColorBlock cb = button.colors;
            cb.normalColor = bg.color;
            cb.highlightedColor = new Color(0.14f, 0.16f, 0.20f, 1f);
            cb.pressedColor = new Color(0.05f, 0.06f, 0.08f, 1f);
            cb.disabledColor = new Color(0.03f, 0.04f, 0.05f, 0.5f);
            button.colors = cb;

            // Mask so sheen sweep stays strictly within button borders
            btnObj.AddComponent<RectMask2D>();

            // Sweeping Shine Sheen (replicating Twine CSS button::after radar sweep)
            GameObject shineObj = new GameObject("ShineSweep", typeof(RectTransform));
            shineObj.transform.SetParent(btnObj.transform, false);
            RectTransform shineRect = shineObj.GetComponent<RectTransform>();
            shineRect.anchorMin = new Vector2(0f, 0f);
            shineRect.anchorMax = new Vector2(0f, 1f);
            shineRect.pivot = new Vector2(0.5f, 0.5f);
            shineRect.sizeDelta = new Vector2(60f, 40f);
            shineRect.anchoredPosition = new Vector2(-120f, 0f);
            shineRect.localRotation = Quaternion.Euler(0f, 0f, -25f);
            Image shineImg = shineObj.AddComponent<Image>();
            shineImg.color = new Color(1f, 1f, 1f, 0.14f);
            shineImg.raycastTarget = false;
            shineObj.SetActive(false);

            // 5px Accent Bar on Left (matching Twine border-left: 5px solid var(--accent-gold))
            GameObject barObj = new GameObject("AccentBar");
            barObj.transform.SetParent(btnObj.transform, false);
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0f, 0.5f);
            barRect.sizeDelta = new Vector2(5f, 0f);
            barRect.anchoredPosition = Vector2.zero;
            Image barImg = barObj.AddComponent<Image>();
            barImg.color = accentColor;
            barImg.raycastTarget = false;

            // Text Container
            GameObject textCont = new GameObject("TextContainer");
            textCont.transform.SetParent(btnObj.transform, false);
            RectTransform tcRect = textCont.AddComponent<RectTransform>();
            StretchFull(tcRect);
            tcRect.offsetMin = new Vector2(18f, 6f);
            tcRect.offsetMax = new Vector2(-16f, -6f);

            GameObject mLblObj = new GameObject("MainLabel");
            mLblObj.transform.SetParent(textCont.transform, false);
            RectTransform mlRect = mLblObj.AddComponent<RectTransform>();
            mlRect.anchorMin = new Vector2(0f, 0.45f);
            mlRect.anchorMax = new Vector2(1f, 1f);
            mlRect.offsetMin = Vector2.zero;
            mlRect.offsetMax = Vector2.zero;
            mainLabel = mLblObj.AddComponent<TextMeshProUGUI>();
            mainLabel.fontSize = 13.5f;
            mainLabel.fontStyle = FontStyles.Bold;
            mainLabel.alignment = TextAlignmentOptions.MidlineLeft;
            mainLabel.color = Color.white;
            mainLabel.text = mainText;
            mainLabel.raycastTarget = false;

            GameObject sLblObj = new GameObject("SubLabel");
            sLblObj.transform.SetParent(textCont.transform, false);
            RectTransform slRect = sLblObj.AddComponent<RectTransform>();
            slRect.anchorMin = new Vector2(0f, 0f);
            slRect.anchorMax = new Vector2(1f, 0.45f);
            slRect.offsetMin = Vector2.zero;
            slRect.offsetMax = Vector2.zero;
            subLabel = sLblObj.AddComponent<TextMeshProUGUI>();
            subLabel.fontSize = 10.5f;
            subLabel.alignment = TextAlignmentOptions.MidlineLeft;
            subLabel.color = new Color(0.68f, 0.70f, 0.74f);
            subLabel.text = subText;
            subLabel.raycastTarget = false;

            // Attach TacticalChoiceButton for 10px hover slide, border glow, sheen sweep, and audio triggers
            TacticalChoiceButton tcb = btnObj.AddComponent<TacticalChoiceButton>();
            tcb.Initialize(button, tcRect, bg, barImg, ol, shineRect, accentColor);
        }

        private static GameObject BuildStoryLogModal(Transform parent, out TextMeshProUGUI logContentTmp, out Button closeBtn, out Button closeBottomBtn)
        {
            GameObject modalObj = new GameObject("StoryLogModal");
            modalObj.transform.SetParent(parent, false);
            RectTransform rect = modalObj.AddComponent<RectTransform>();
            StretchFull(rect);

            Image backdrop = modalObj.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.88f);
            backdrop.raycastTarget = true;

            GameObject panel = new GameObject("LogPanel");
            panel.transform.SetParent(modalObj.transform, false);
            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(920f, 580f);
            pRect.anchoredPosition = Vector2.zero;
            Image pBg = panel.AddComponent<Image>();
            pBg.color = new Color(0.04f, 0.05f, 0.07f, 0.98f);
            Outline pOl = panel.AddComponent<Outline>();
            pOl.effectColor = new Color(0.24f, 0.36f, 0.48f, 0.85f);
            pOl.effectDistance = new Vector2(1.5f, -1.5f);

            // Header Bar (56px)
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panel.transform, false);
            RectTransform hRect = headerObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f); hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(0f, 56f);
            hRect.anchoredPosition = Vector2.zero;
            Image hBg = headerObj.AddComponent<Image>();
            hBg.color = new Color(0.07f, 0.10f, 0.14f, 0.98f);

            TextMeshProUGUI hTmp = CreateTMPText(headerObj.transform, "Title",
                "<b><color=#4FC3F7>SURVIVOR CHRONICLE // INCIDENT & STORY LOG</color></b>\n<size=75%><color=#90A4AE>ARCHIVED TRANSCRIPTS, BROADCAST RECORDS, AND OBSERVATIONS</color></size>",
                14f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform htRect = hTmp.rectTransform;
            htRect.anchorMin = new Vector2(0f, 0f); htRect.anchorMax = new Vector2(1f, 1f);
            htRect.offsetMin = new Vector2(20f, 0f); htRect.offsetMax = new Vector2(-60f, 0f);

            // Close Button [ X ] on Header
            GameObject closeObj = new GameObject("Btn_CloseTop");
            closeObj.transform.SetParent(headerObj.transform, false);
            RectTransform cRect = closeObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(1f, 0.5f); cRect.anchorMax = new Vector2(1f, 0.5f);
            cRect.pivot = new Vector2(1f, 0.5f);
            cRect.sizeDelta = new Vector2(34f, 34f);
            cRect.anchoredPosition = new Vector2(-12f, 0f);
            Image cBg = closeObj.AddComponent<Image>();
            cBg.color = new Color(0.16f, 0.08f, 0.08f, 0.95f);
            Outline cOl = closeObj.AddComponent<Outline>();
            cOl.effectColor = new Color(0.85f, 0.25f, 0.25f, 0.8f);
            closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = cBg;
            TextMeshProUGUI cTmp = CreateTMPText(closeObj.transform, "Label", "✕", 18f, new Color(0.95f, 0.4f, 0.4f), TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform ctRect = cTmp.rectTransform;
            StretchFull(ctRect);

            // Scroll View for Log Text
            GameObject scrollObj = CreateScrollView(panel.transform, "LogScrollView", new Vector2(880f, 440f), true, out Transform content);
            RectTransform sRect = scrollObj.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0f); sRect.anchorMax = new Vector2(1f, 1f);
            sRect.offsetMin = new Vector2(20f, 56f);
            sRect.offsetMax = new Vector2(-20f, -64f);

            VerticalLayoutGroup vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Content text
            logContentTmp = CreateTMPText(content, "LogContentText", "", 13.5f, new Color(0.88f, 0.91f, 0.96f), TextAlignmentOptions.TopLeft);
            logContentTmp.lineSpacing = 6f;
            logContentTmp.textWrappingMode = TextWrappingModes.Normal;
            RectTransform lcRect = logContentTmp.rectTransform;
            lcRect.anchorMin = new Vector2(0f, 1f); lcRect.anchorMax = new Vector2(1f, 1f);
            lcRect.pivot = new Vector2(0.5f, 1f);

            // Footer Bar
            GameObject footObj = new GameObject("Footer");
            footObj.transform.SetParent(panel.transform, false);
            RectTransform fRect = footObj.AddComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0f, 0f); fRect.anchorMax = new Vector2(1f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.sizeDelta = new Vector2(0f, 48f);
            fRect.anchoredPosition = Vector2.zero;
            Image fBg = footObj.AddComponent<Image>();
            fBg.color = new Color(0.06f, 0.08f, 0.11f, 0.98f);

            GameObject botCloseObj = new GameObject("Btn_CloseBottom");
            botCloseObj.transform.SetParent(footObj.transform, false);
            RectTransform bcRect = botCloseObj.AddComponent<RectTransform>();
            bcRect.anchorMin = new Vector2(0.5f, 0.5f); bcRect.anchorMax = new Vector2(0.5f, 0.5f);
            bcRect.pivot = new Vector2(0.5f, 0.5f);
            bcRect.sizeDelta = new Vector2(240f, 36f);
            bcRect.anchoredPosition = Vector2.zero;
            Image bcBg = botCloseObj.AddComponent<Image>();
            bcBg.color = new Color(0.12f, 0.16f, 0.22f, 0.95f);
            Outline bcOl = botCloseObj.AddComponent<Outline>();
            bcOl.effectColor = new Color(0.35f, 0.65f, 0.85f, 0.7f);
            closeBottomBtn = botCloseObj.AddComponent<Button>();
            closeBottomBtn.targetGraphic = bcBg;
            TextMeshProUGUI bcTmp = CreateTMPText(botCloseObj.transform, "Label", "<b>[ CLOSE ARCHIVE ]</b>", 12f, new Color(0.85f, 0.90f, 0.95f), TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform bctRect = bcTmp.rectTransform;
            StretchFull(bctRect);

            modalObj.SetActive(false);
            return modalObj;
        }

        private static GameObject BuildPhoneModal(Transform parent, out Button closeButton)
        {
            GameObject modalObj = new GameObject("PhoneModal");
            modalObj.transform.SetParent(parent, false);
            RectTransform rect = modalObj.AddComponent<RectTransform>();
            StretchFull(rect);

            Image backdrop = modalObj.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.88f);
            backdrop.raycastTarget = true;

            GameObject phonePanel = new GameObject("PhonePanel");
            phonePanel.transform.SetParent(modalObj.transform, false);
            RectTransform pRect = phonePanel.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(440f, 620f);
            pRect.anchoredPosition = Vector2.zero;
            Image pBg = phonePanel.AddComponent<Image>();
            pBg.color = new Color(0.04f, 0.04f, 0.05f, 0.98f);

            VerticalLayoutGroup vlg = phonePanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 14, 14);
            vlg.spacing = 10f;

            // Status bar
            GameObject sbObj = new GameObject("StatusBar");
            sbObj.transform.SetParent(phonePanel.transform, false);
            TextMeshProUGUI sbTmp = sbObj.AddComponent<TextMeshProUGUI>();
            sbTmp.fontSize = 11f;
            sbTmp.text = "<color=#ff4444>[!] NO SIGNAL</color>                       <color=#ff4444>12%</color> [BAT]";
            sbTmp.alignment = TextAlignmentOptions.Center;

            // Header
            GameObject hObj = new GameObject("Header");
            hObj.transform.SetParent(phonePanel.transform, false);
            TextMeshProUGUI hTmp = hObj.AddComponent<TextMeshProUGUI>();
            hTmp.fontSize = 14f;
            hTmp.fontStyle = FontStyles.Bold;
            hTmp.text = "Trending: <color=#e74c3c>#SeattleLockdown</color>";

            // Tweets Text Container
            GameObject feedObj = new GameObject("FeedText");
            feedObj.transform.SetParent(phonePanel.transform, false);
            LayoutElement fle = feedObj.AddComponent<LayoutElement>();
            fle.preferredHeight = 360f;
            fle.flexibleHeight = 1f;
            TextMeshProUGUI fTmp = feedObj.AddComponent<TextMeshProUGUI>();
            fTmp.fontSize = 12f;
            fTmp.color = new Color(0.85f, 0.85f, 0.9f);
            fTmp.lineSpacing = 12f;
            fTmp.text = "<b>CityScanner</b> @CityScanner_WA • 2m\n" +
                        "Does anyone know what's happening at the 5th St Hospital? Seeing police in gas masks and riot gear. They aren't letting ambulances through. Stay away!!\n\n" +
                        "<color=#5dade2><b>NewsWire Breaking [VERIFIED]</b></color> @NewsWire_Alerts • 4m\n" +
                        "EMERGENCY: CDC reporting 'unprecedented neuro-aggressive pathogen' causing extreme psychosis and unprovoked violence. Authorities urge citizens to avoid public spaces.\n\n" +
                        "<b>User_9921</b> @anon_9921 • 11m\n" +
                        "<color=#ff5555>[ATTACHED VIDEO: BLURRY]</color>\n" +
                        "Look at his face! That's not rabies! The cops shot him three times in the chest and he kept running! Oh my god, he's— <i>[VIDEO ENDS]</i>\n\n" +
                        "<color=#666666><i>Connection lost. Tap to retry.</i></color>";

            // Outro note
            GameObject oObj = new GameObject("Outro");
            oObj.transform.SetParent(phonePanel.transform, false);
            TextMeshProUGUI oTmp = oObj.AddComponent<TextMeshProUGUI>();
            oTmp.fontSize = 11f;
            oTmp.color = new Color(0.7f, 0.7f, 0.7f);
            oTmp.text = "A traffic map shows military checkpoints barricading bridges out of Seattle. Then the signal dies. The internet is gone.";

            // Close Button
            CreateApartmentMenuButton(phonePanel.transform, "Btn_ClosePhone", "LOWER THE PHONE AND RETURN",
                "Add Intel to Lore Database.", new Color(0.35f, 0.75f, 0.95f), out closeButton, out TextMeshProUGUI _, out TextMeshProUGUI _);

            return modalObj;
        }

        private static GameObject BuildRadioModal(Transform parent, out Button closeButton)
        {
            GameObject modalObj = new GameObject("RadioModal");
            modalObj.transform.SetParent(parent, false);
            RectTransform rect = modalObj.AddComponent<RectTransform>();
            StretchFull(rect);

            Image backdrop = modalObj.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.88f);
            backdrop.raycastTarget = true;

            GameObject radioPanel = new GameObject("RadioPanel");
            radioPanel.transform.SetParent(modalObj.transform, false);
            RectTransform pRect = radioPanel.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(560f, 480f);
            pRect.anchoredPosition = Vector2.zero;
            Image pBg = radioPanel.AddComponent<Image>();
            pBg.color = new Color(0.04f, 0.05f, 0.07f, 0.98f);

            VerticalLayoutGroup vlg = radioPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(18, 18, 14, 14);
            vlg.spacing = 10f;

            // Freq Header
            GameObject hObj = new GameObject("FreqHeader");
            hObj.transform.SetParent(radioPanel.transform, false);
            TextMeshProUGUI hTmp = hObj.AddComponent<TextMeshProUGUI>();
            hTmp.fontSize = 13f;
            hTmp.fontStyle = FontStyles.Bold;
            hTmp.color = new Color(0.95f, 0.75f, 0.35f);
            hTmp.text = "FREQ: 154.790 MHz                     <color=#ff4444>● REC</color>";

            // Transcript Body
            GameObject tObj = new GameObject("Transcript");
            tObj.transform.SetParent(radioPanel.transform, false);
            LayoutElement tle = tObj.AddComponent<LayoutElement>();
            tle.preferredHeight = 260f;
            tle.flexibleHeight = 1f;
            TextMeshProUGUI tTmp = tObj.AddComponent<TextMeshProUGUI>();
            tTmp.fontSize = 12f;
            tTmp.color = new Color(0.65f, 0.85f, 0.85f);
            tTmp.lineSpacing = 12f;
            tTmp.text = "<color=#888888>[STATIC HISS]</color>\n\n" +
                        "<b>UNIT 4-CHARLIE:</b>\n" +
                        "<i>\"Dispatch, 4-Charlie! We've lost the 5th Street barricade! They are not stopping! I repeat, shooting them in the chest does nothing! They just keep coming!\"</i>\n\n" +
                        "<color=#888888>[HEAVY STRUGGLE / THUD]</color>\n\n" +
                        "<b>DISPATCH (CALM):</b>\n" +
                        "<i>\"4-Charlie, unrestricted lethal force is authorized. All units: Standard rules of engagement are suspended. Body shots will not stop them. You must destroy the brain. Aim for the head. Avoid all physical contact.\"</i>\n\n" +
                        "<color=#888888>[SIGNAL CUTS OUT]</color>";

            // Outro
            GameObject oObj = new GameObject("Outro");
            oObj.transform.SetParent(radioPanel.transform, false);
            TextMeshProUGUI oTmp = oObj.AddComponent<TextMeshProUGUI>();
            oTmp.fontSize = 11f;
            oTmp.color = new Color(0.7f, 0.7f, 0.7f);
            oTmp.text = "<color=#ff5555>'Aim for the head.'</color> Total lethal force. Standard body shots are useless. Whatever is outside, the law is officially dead.";

            // Close Button
            CreateApartmentMenuButton(radioPanel.transform, "Btn_CloseRadio", "KILL THE RADIO AND RETURN",
                "Add tactical intel to your PDA.", new Color(0.95f, 0.75f, 0.35f), out closeButton, out TextMeshProUGUI _, out TextMeshProUGUI _);

            return modalObj;
        }

        private static GameObject BuildHackingModal(Transform parent, out TextMeshProUGUI terminalText, out GameObject finishContainer, out Button closeButton)
        {
            GameObject modalObj = new GameObject("HackingModal");
            modalObj.transform.SetParent(parent, false);
            RectTransform rect = modalObj.AddComponent<RectTransform>();
            StretchFull(rect);

            Image backdrop = modalObj.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.88f);
            backdrop.raycastTarget = true;

            GameObject hackPanel = new GameObject("HackingPanel");
            hackPanel.transform.SetParent(modalObj.transform, false);
            RectTransform pRect = hackPanel.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(660f, 520f);
            pRect.anchoredPosition = Vector2.zero;
            Image pBg = hackPanel.AddComponent<Image>();
            pBg.color = new Color(0.02f, 0.02f, 0.02f, 0.98f);

            VerticalLayoutGroup vlg = hackPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(18, 18, 14, 14);
            vlg.spacing = 10f;

            // Terminal Header
            GameObject hObj = new GameObject("Header");
            hObj.transform.SetParent(hackPanel.transform, false);
            TextMeshProUGUI hTmp = hObj.AddComponent<TextMeshProUGUI>();
            hTmp.fontSize = 12f;
            hTmp.fontStyle = FontStyles.Bold;
            hTmp.color = new Color(0.3f, 1f, 0.3f);
            hTmp.text = "SYS_ADMIN - SECURE_NODE_04                         <color=#ff4444>[ENCRYPTED]</color>";

            // Terminal Text Body
            GameObject tObj = new GameObject("TerminalText");
            tObj.transform.SetParent(hackPanel.transform, false);
            LayoutElement tle = tObj.AddComponent<LayoutElement>();
            tle.preferredHeight = 260f;
            tle.flexibleHeight = 1f;
            terminalText = tObj.AddComponent<TextMeshProUGUI>();
            terminalText.fontSize = 12f;
            terminalText.color = new Color(0.35f, 0.95f, 0.35f);
            terminalText.lineSpacing = 10f;
            terminalText.text = "> BYPASSING 256-BIT AES ENCRYPTION...\n> ACCESSING SECURE MILITARY NODE...";

            // Finish Container
            finishContainer = new GameObject("FinishContainer");
            finishContainer.transform.SetParent(hackPanel.transform, false);
            VerticalLayoutGroup fVlg = finishContainer.AddComponent<VerticalLayoutGroup>();
            fVlg.spacing = 8f;

            GameObject foObj = new GameObject("Outro");
            foObj.transform.SetParent(finishContainer.transform, false);
            TextMeshProUGUI foTmp = foObj.AddComponent<TextMeshProUGUI>();
            foTmp.fontSize = 11f;
            foTmp.color = new Color(0.85f, 0.85f, 0.85f);
            foTmp.text = "<color=#ff4444><b>'SECTOR PURGE AT 0600 HOURS.'</b></color> The military is firebombing the city at dawn. You secured a <b><color=#ffd700>Tier-1 Military Clearance Passkey</color></b> to your device.";

            CreateApartmentMenuButton(finishContainer.transform, "Btn_CloseHacking", "WIPE DRIVE & CLOSE LAPTOP",
                "Save the Tier-1 Passkey to your PDA and log off.", new Color(0.35f, 0.95f, 0.45f), out closeButton, out TextMeshProUGUI _, out TextMeshProUGUI _);

            return modalObj;
        }

        private static void RegisterItemSprites()
        {
            string itemsFolder = "Assets/Sprites/Items";
            if (!Directory.Exists(itemsFolder)) return;

            string[] files = Directory.GetFiles(itemsFolder, "*.png");
            foreach (string f in files)
            {
                string relPath = f.Replace("\\", "/");
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(relPath);
                if (s != null)
                {
                    string idName = Path.GetFileNameWithoutExtension(f);
                    ItemDatabase.RegisterIcon(idName, s);
                }
            }
        }

        // --- SPLASH ---
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

        #region Tactical Floorplan Map Builder (Phase 2 — Architectural Blueprint & Bio-Radar Scanner)
        private static TacticalMapViewController BuildTacticalConnectedMap(GameObject pMapObj)
        {
            TacticalMapViewController mapCtrl = pMapObj.AddComponent<TacticalMapViewController>();

            // =========================================================================
            // 1. TOP COMMAND TELEMETRY BAR (Explicit RectTransform: 24px tall)
            // =========================================================================
            GameObject topBar = new GameObject("TopTelemetryBar", typeof(RectTransform));
            topBar.transform.SetParent(pMapObj.transform, false);
            RectTransform tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.offsetMin = new Vector2(0f, -24f);
            tbRect.offsetMax = new Vector2(0f, 0f);

            Image tbBg = topBar.AddComponent<Image>();
            tbBg.color = new Color(0.05f, 0.08f, 0.13f, 0.96f);
            Outline tbOl = topBar.AddComponent<Outline>();
            tbOl.effectColor = new Color(0.16f, 0.30f, 0.45f, 0.85f);
            tbOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI tbTitle = CreateTMPText(topBar.transform, "BlueprintTitle",
                "ARCHITECTURAL BLUEPRINT // ASCOTT RESIDENCY — 4TH FLOOR WEST WING",
                10f, new Color(0.0f, 0.90f, 1.0f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            StretchFull(tbTitle.rectTransform);
            tbTitle.rectTransform.offsetMin = new Vector2(10f, 0f);
            tbTitle.rectTransform.offsetMax = new Vector2(-340f, 0f);

            TextMeshProUGUI coordBannerTmp = CreateTMPText(topBar.transform, "CoordTelemetry",
                "GRID [W-01:R-01] // BIO-RADAR: ACTIVE",
                9.5f, new Color(0.52f, 0.72f, 0.88f), TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            StretchFull(coordBannerTmp.rectTransform);
            coordBannerTmp.rectTransform.offsetMin = new Vector2(560f, 0f);
            coordBannerTmp.rectTransform.offsetMax = new Vector2(-10f, 0f);

            // =========================================================================
            // 2. LEFT COLUMN: ARCHITECTURAL BLUEPRINT VIEWPORT (648px wide x 342px tall)
            // =========================================================================
            GameObject bpViewport = new GameObject("BlueprintViewport", typeof(RectTransform));
            bpViewport.transform.SetParent(pMapObj.transform, false);
            RectTransform bpRect = bpViewport.GetComponent<RectTransform>();
            bpRect.anchorMin = new Vector2(0f, 0f);
            bpRect.anchorMax = new Vector2(0f, 1f);
            bpRect.pivot = new Vector2(0f, 0.5f);
            bpRect.offsetMin = new Vector2(0f, 0f);
            bpRect.offsetMax = new Vector2(648f, -30f);

            Image bpBg = bpViewport.AddComponent<Image>();
            bpBg.color = new Color(0.025f, 0.04f, 0.07f, 0.98f);
            Outline bpOl = bpViewport.AddComponent<Outline>();
            bpOl.effectColor = new Color(0.16f, 0.30f, 0.45f, 0.90f);
            bpOl.effectDistance = new Vector2(1f, -1f);

            // Procedural CAD Blueprint Grid Overlay
            GameObject gridOverlayObj = new GameObject("BlueprintGridOverlay", typeof(RectTransform));
            gridOverlayObj.transform.SetParent(bpViewport.transform, false);
            StretchFull(gridOverlayObj.GetComponent<RectTransform>());
            Image gridOverlayImg = gridOverlayObj.AddComponent<Image>();
            gridOverlayImg.sprite = TacticalMapViewController.GetBlueprintGridSprite();
            gridOverlayImg.type = Image.Type.Tiled;
            gridOverlayImg.color = new Color(1f, 1f, 1f, 0.90f);
            gridOverlayImg.raycastTarget = false;

            // Blueprint Sub-Header Bar (Top 22px inside viewport)
            GameObject bpSubBar = new GameObject("BlueprintSubHeader", typeof(RectTransform));
            bpSubBar.transform.SetParent(bpViewport.transform, false);
            RectTransform bsbRect = bpSubBar.GetComponent<RectTransform>();
            bsbRect.anchorMin = new Vector2(0f, 1f);
            bsbRect.anchorMax = new Vector2(1f, 1f);
            bsbRect.pivot = new Vector2(0.5f, 1f);
            bsbRect.offsetMin = new Vector2(10f, -22f);
            bsbRect.offsetMax = new Vector2(-10f, -2f);

            TextMeshProUGUI scaleTmp = CreateTMPText(bpSubBar.transform, "ScaleLabel",
                "SCHEMATIC SCALE 1:100 // CONTIGUOUS FLOORPLAN",
                8f, new Color(0.38f, 0.56f, 0.72f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            StretchFull(scaleTmp.rectTransform);
            scaleTmp.rectTransform.offsetMax = new Vector2(-340f, 0f);

            TextMeshProUGUI footerLocTmp = CreateTMPText(bpSubBar.transform, "ActivePosBanner",
                "<color=#00e5ff><b>PLAYER POS:</b> UNIT 407</color>",
                8.5f, new Color(0.75f, 0.88f, 0.96f), TextAlignmentOptions.MidlineRight);
            StretchFull(footerLocTmp.rectTransform);
            footerLocTmp.rectTransform.offsetMin = new Vector2(240f, 0f);

            mapCtrl.BindBlueprintBanners(coordBannerTmp, footerLocTmp, gridOverlayImg);

            // =========================================================================
            // 3. CONTIGUOUS FLOORPLAN CANVAS (616x274 centered inside BlueprintViewport)
            // =========================================================================
            GameObject fpCanvas = new GameObject("FloorplanCanvas", typeof(RectTransform));
            fpCanvas.transform.SetParent(bpViewport.transform, false);
            RectTransform fpRect = fpCanvas.GetComponent<RectTransform>();
            fpRect.anchorMin = new Vector2(0.5f, 0.5f);
            fpRect.anchorMax = new Vector2(0.5f, 0.5f);
            fpRect.pivot = new Vector2(0.5f, 0.5f);
            fpRect.anchoredPosition = new Vector2(0f, 2f);
            fpRect.sizeDelta = new Vector2(616f, 274f);

            // Outer Structural Wing Perimeter Wall
            GameObject perimeterObj = new GameObject("StructuralPerimeter", typeof(RectTransform));
            perimeterObj.transform.SetParent(fpCanvas.transform, false);
            RectTransform perimRect = perimeterObj.GetComponent<RectTransform>();
            perimRect.anchorMin = new Vector2(0.5f, 0.5f);
            perimRect.anchorMax = new Vector2(0.5f, 0.5f);
            perimRect.pivot = new Vector2(0.5f, 0.5f);
            perimRect.anchoredPosition = Vector2.zero;
            perimRect.sizeDelta = new Vector2(560f, 260f);
            Image perimBg = perimeterObj.AddComponent<Image>();
            perimBg.color = new Color(0.02f, 0.035f, 0.06f, 0.94f);
            perimBg.raycastTarget = false;
            Outline perimOl = perimeterObj.AddComponent<Outline>();
            perimOl.effectColor = new Color(0.22f, 0.40f, 0.58f, 0.90f);
            perimOl.effectDistance = new Vector2(1.5f, -1.5f);

            // Central Spine Corridor Continuous Structural Shaft
            GameObject spineShaft = new GameObject("CentralCorridorShaft", typeof(RectTransform));
            spineShaft.transform.SetParent(fpCanvas.transform, false);
            RectTransform ssRect = spineShaft.GetComponent<RectTransform>();
            ssRect.anchorMin = new Vector2(0.5f, 0.5f);
            ssRect.anchorMax = new Vector2(0.5f, 0.5f);
            ssRect.pivot = new Vector2(0.5f, 0.5f);
            ssRect.anchoredPosition = Vector2.zero;
            ssRect.sizeDelta = new Vector2(64f, 248f);
            Image ssBg = spineShaft.AddComponent<Image>();
            ssBg.color = new Color(0.035f, 0.055f, 0.09f, 0.96f);
            ssBg.raycastTarget = false;
            Outline ssOl = spineShaft.AddComponent<Outline>();
            ssOl.effectColor = new Color(0.18f, 0.32f, 0.46f, 0.75f);
            ssOl.effectDistance = new Vector2(1f, -1f);

            // 5 Contiguous Floorplan Rows (y = +104, +52, 0, -52, -104)
            CreateBlueprintFloorRow(mapCtrl, fpCanvas.transform, 100f,
                "unit_407", "UNIT 407", "SAFEHOUSE",
                "hallway_1", "CORRIDOR N", "NORTH SPINE",
                "unit_406", "UNIT 406", "RESIDENTIAL",
                hasSouthCorridorLink: true);

            CreateBlueprintFloorRow(mapCtrl, fpCanvas.transform, 50f,
                "unit_405", "UNIT 405", "RESIDENTIAL",
                "hallway_2", "CORRIDOR MN", "MID-NORTH",
                "unit_404", "UNIT 404", "RESIDENTIAL",
                hasSouthCorridorLink: true);

            CreateBlueprintFloorRow(mapCtrl, fpCanvas.transform, 0f,
                "unit_403", "UNIT 403", "RESIDENTIAL",
                "hallway_3", "CORRIDOR MS", "MID-SOUTH",
                "unit_402", "UNIT 402", "RESIDENTIAL",
                hasSouthCorridorLink: true);

            CreateBlueprintFloorRow(mapCtrl, fpCanvas.transform, -50f,
                "unit_401", "UNIT 401", "RESIDENCE",
                "hallway_4", "CORRIDOR S", "SOUTH SPINE",
                "utility", "UTILITY BAY", "SERVICE ROOM",
                hasSouthCorridorLink: true);

            CreateBlueprintFloorRow(mapCtrl, fpCanvas.transform, -100f,
                "stairs", "STAIRWELL W", "FIRE EXIT",
                "hallway_end", "STAIR FOYER", "SOUTH FOYER",
                "empty_bot", "EAST WING", "EAST WING",
                hasSouthCorridorLink: false);

            // Bottom Clearance Legend Strip (Bottom 22px inside BlueprintViewport)
            GameObject legendBar = new GameObject("LegendBar", typeof(RectTransform));
            legendBar.transform.SetParent(bpViewport.transform, false);
            RectTransform lbRect = legendBar.GetComponent<RectTransform>();
            lbRect.anchorMin = new Vector2(0f, 0f);
            lbRect.anchorMax = new Vector2(1f, 0f);
            lbRect.pivot = new Vector2(0.5f, 0f);
            lbRect.offsetMin = new Vector2(10f, 3f);
            lbRect.offsetMax = new Vector2(-10f, 23f);

            TextMeshProUGUI legendTmp = CreateTMPText(legendBar.transform, "LegendText",
                "<color=#00e5ff>■</color> CURRENT SECTOR     <color=#14b87a>■</color> CLEARED / OPEN     <color=#f59e0b>■</color> LOCKED     <color=#f43f5e>■</color> BIOHAZARD / BLOCKED     <color=#64748b>■</color> COLLAPSED",
                8f, new Color(0.65f, 0.76f, 0.86f), TextAlignmentOptions.Center, FontStyles.Bold);
            StretchFull(legendTmp.rectTransform);

            // =========================================================================
            // 4. RIGHT COLUMN: BIO-RADAR & RECON DOSSIER (533px wide x 342px tall)
            // =========================================================================
            GameObject intelCard = new GameObject("IntelCard", typeof(RectTransform));
            intelCard.transform.SetParent(pMapObj.transform, false);
            RectTransform icRect = intelCard.GetComponent<RectTransform>();
            icRect.anchorMin = new Vector2(0f, 0f);
            icRect.anchorMax = new Vector2(1f, 1f);
            icRect.pivot = new Vector2(1f, 0.5f);
            icRect.offsetMin = new Vector2(660f, 0f);
            icRect.offsetMax = new Vector2(0f, -30f);

            Image icBg = intelCard.AddComponent<Image>();
            icBg.color = new Color(0.03f, 0.05f, 0.09f, 0.98f);
            Outline icOl = intelCard.AddComponent<Outline>();
            icOl.effectColor = new Color(0.16f, 0.30f, 0.45f, 0.90f);
            icOl.effectDistance = new Vector2(1f, -1f);

            // Dossier Top Badge (y: -6 to -28)
            GameObject dosBadge = new GameObject("DossierHeader", typeof(RectTransform));
            dosBadge.transform.SetParent(intelCard.transform, false);
            RectTransform dbRect = dosBadge.GetComponent<RectTransform>();
            dbRect.anchorMin = new Vector2(0f, 1f);
            dbRect.anchorMax = new Vector2(1f, 1f);
            dbRect.pivot = new Vector2(0.5f, 1f);
            dbRect.offsetMin = new Vector2(12f, -28f);
            dbRect.offsetMax = new Vector2(-12f, -6f);
            Image dbBg = dosBadge.AddComponent<Image>();
            dbBg.color = new Color(0.06f, 0.10f, 0.16f, 0.96f);
            TextMeshProUGUI dbTxt = CreateTMPText(dosBadge.transform, "Text",
                "BIO-RADAR & RECON DOSSIER // SECTOR TELEMETRY",
                9.5f, new Color(0.95f, 0.55f, 0.15f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            StretchFull(dbTxt.rectTransform);
            dbTxt.rectTransform.offsetMin = new Vector2(10f, 0f);
            dbTxt.rectTransform.offsetMax = new Vector2(-10f, 0f);

            // Sector Title & Procedural Icon Row (y: -34 to -58)
            GameObject titleRow = new GameObject("SectorTitleRow", typeof(RectTransform));
            titleRow.transform.SetParent(intelCard.transform, false);
            RectTransform trRect = titleRow.GetComponent<RectTransform>();
            trRect.anchorMin = new Vector2(0f, 1f);
            trRect.anchorMax = new Vector2(1f, 1f);
            trRect.pivot = new Vector2(0.5f, 1f);
            trRect.offsetMin = new Vector2(14f, -58f);
            trRect.offsetMax = new Vector2(-14f, -34f);

            GameObject hdrIconObj = new GameObject("HeaderIcon", typeof(RectTransform));
            hdrIconObj.transform.SetParent(titleRow.transform, false);
            RectTransform hiRect = hdrIconObj.GetComponent<RectTransform>();
            hiRect.anchorMin = new Vector2(0f, 0.5f);
            hiRect.anchorMax = new Vector2(0f, 0.5f);
            hiRect.pivot = new Vector2(0f, 0.5f);
            hiRect.anchoredPosition = Vector2.zero;
            hiRect.sizeDelta = new Vector2(20f, 20f);
            Image hdrIconImg = hdrIconObj.AddComponent<Image>();
            hdrIconImg.sprite = TacticalMapViewController.GetRoomIconSprite(SundayLastBreath.Data.TacticalRoomIconType.Home);
            hdrIconImg.color = new Color(0.0f, 0.90f, 1.0f);
            hdrIconImg.preserveAspect = true;

            TextMeshProUGUI intelTitleTmp = CreateTMPText(titleRow.transform, "IntelSectorTitle",
                "<b>UNIT 407 // YOUR RESIDENCE</b>",
                13f, new Color(0.0f, 0.90f, 1.0f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            StretchFull(intelTitleTmp.rectTransform);
            intelTitleTmp.rectTransform.offsetMin = new Vector2(28f, 0f);

            // Clearance Status Pill (y: -60 to -78)
            TextMeshProUGUI intelStatusTmp = CreateTMPText(intelCard.transform, "StatusTag",
                "<color=#00e5ff><b>[ CURRENT SECTOR ]</b></color>",
                9.5f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform stRect = intelStatusTmp.rectTransform;
            stRect.anchorMin = new Vector2(0f, 1f);
            stRect.anchorMax = new Vector2(1f, 1f);
            stRect.pivot = new Vector2(0.5f, 1f);
            stRect.offsetMin = new Vector2(14f, -78f);
            stRect.offsetMax = new Vector2(-14f, -60f);

            // 3-Metric Telemetry Grid Card (y: -82 to -152, height: 70px)
            GameObject metricsCard = new GameObject("TelemetryMetricsCard", typeof(RectTransform));
            metricsCard.transform.SetParent(intelCard.transform, false);
            RectTransform mcRect = metricsCard.GetComponent<RectTransform>();
            mcRect.anchorMin = new Vector2(0f, 1f);
            mcRect.anchorMax = new Vector2(1f, 1f);
            mcRect.pivot = new Vector2(0.5f, 1f);
            mcRect.offsetMin = new Vector2(14f, -152f);
            mcRect.offsetMax = new Vector2(-14f, -82f);
            Image mcBg = metricsCard.AddComponent<Image>();
            mcBg.color = new Color(0.02f, 0.032f, 0.055f, 0.96f);
            Outline mcOl = metricsCard.AddComponent<Outline>();
            mcOl.effectColor = new Color(0.14f, 0.24f, 0.36f, 0.85f);
            mcOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI intelThreatTmp = CreateTMPText(metricsCard.transform, "ThreatMetric",
                "<color=#64748b>THREAT:</color> <color=#2ecc71><b>NONE</b></color>",
                9.5f, new Color(0.82f, 0.88f, 0.95f), TextAlignmentOptions.MidlineLeft);
            intelThreatTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            intelThreatTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            intelThreatTmp.rectTransform.pivot = new Vector2(0.5f, 1f);
            intelThreatTmp.rectTransform.offsetMin = new Vector2(10f, -24f);
            intelThreatTmp.rectTransform.offsetMax = new Vector2(-10f, -4f);

            TextMeshProUGUI intelScavengeTmp = CreateTMPText(metricsCard.transform, "ScavengeMetric",
                "<color=#64748b>SALVAGE:</color> <color=#f39c12><b>DEPLETED / SECURED</b></color>",
                9.5f, new Color(0.82f, 0.88f, 0.95f), TextAlignmentOptions.MidlineLeft);
            intelScavengeTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            intelScavengeTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            intelScavengeTmp.rectTransform.pivot = new Vector2(0.5f, 1f);
            intelScavengeTmp.rectTransform.offsetMin = new Vector2(10f, -45f);
            intelScavengeTmp.rectTransform.offsetMax = new Vector2(-10f, -25f);

            TextMeshProUGUI intelAcousticTmp = CreateTMPText(metricsCard.transform, "AcousticMetric",
                "<color=#64748b>AUDIO:</color> <color=#38bdf8><b>60Hz TV STATIC HUM</b></color>",
                9.5f, new Color(0.82f, 0.88f, 0.95f), TextAlignmentOptions.MidlineLeft);
            intelAcousticTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            intelAcousticTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            intelAcousticTmp.rectTransform.pivot = new Vector2(0.5f, 1f);
            intelAcousticTmp.rectTransform.offsetMin = new Vector2(10f, -66f);
            intelAcousticTmp.rectTransform.offsetMax = new Vector2(-10f, -46f);

            // Architectural & Recon Notes Header (y: -156 to -172)
            TextMeshProUGUI notesHdrTmp = CreateTMPText(intelCard.transform, "NotesHeader",
                "ARCHITECTURAL & RECON NOTES:",
                8.5f, new Color(0.45f, 0.58f, 0.72f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            notesHdrTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            notesHdrTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            notesHdrTmp.rectTransform.pivot = new Vector2(0.5f, 1f);
            notesHdrTmp.rectTransform.offsetMin = new Vector2(14f, -172f);
            notesHdrTmp.rectTransform.offsetMax = new Vector2(-14f, -156f);

            // Recon Description Card (y: -174 to -252, height: 78px)
            GameObject descBox = new GameObject("ReconDescriptionBox", typeof(RectTransform));
            descBox.transform.SetParent(intelCard.transform, false);
            RectTransform dbxRect = descBox.GetComponent<RectTransform>();
            dbxRect.anchorMin = new Vector2(0f, 1f);
            dbxRect.anchorMax = new Vector2(1f, 1f);
            dbxRect.pivot = new Vector2(0.5f, 1f);
            dbxRect.offsetMin = new Vector2(14f, -252f);
            dbxRect.offsetMax = new Vector2(-14f, -174f);
            Image dbxBg = descBox.AddComponent<Image>();
            dbxBg.color = new Color(0.02f, 0.032f, 0.055f, 0.96f);
            Outline dbxOl = descBox.AddComponent<Outline>();
            dbxOl.effectColor = new Color(0.14f, 0.24f, 0.36f, 0.85f);
            dbxOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI intelDescTmp = CreateTMPText(descBox.transform, "IntelDescription",
                "Your residential apartment. The television flickers with civil defense emergency alert white noise. Heavy reinforced deadbolt held firm during the initial outbreak chaos. Direct exit to North Spine Corridor.",
                10f, new Color(0.78f, 0.86f, 0.95f), TextAlignmentOptions.TopLeft);
            intelDescTmp.lineSpacing = 3f;
            StretchFull(intelDescTmp.rectTransform);
            intelDescTmp.rectTransform.offsetMin = new Vector2(10f, 6f);
            intelDescTmp.rectTransform.offsetMax = new Vector2(-10f, -6f);

            // Tactical Doorway & Sector Clearance Bar (Informational — pure reference map)
            GameObject transitBtnObj = new GameObject("TransitActionButton", typeof(RectTransform));
            transitBtnObj.transform.SetParent(intelCard.transform, false);
            RectTransform tboRect = transitBtnObj.GetComponent<RectTransform>();
            tboRect.anchorMin = new Vector2(0f, 1f);
            tboRect.anchorMax = new Vector2(1f, 1f);
            tboRect.pivot = new Vector2(0.5f, 1f);
            tboRect.offsetMin = new Vector2(14f, -296f);
            tboRect.offsetMax = new Vector2(-14f, -258f);

            Image transitBg = transitBtnObj.AddComponent<Image>();
            transitBg.color = new Color(0.04f, 0.14f, 0.18f, 0.95f);
            Outline transitOl = transitBtnObj.AddComponent<Outline>();
            transitOl.effectColor = new Color(0.0f, 0.78f, 0.95f, 0.85f);
            transitOl.effectDistance = new Vector2(1f, -1f);

            Button transitBtn = transitBtnObj.AddComponent<Button>();
            transitBtn.targetGraphic = transitBg;
            transitBtn.interactable = false;

            TextMeshProUGUI transitTxt = CreateTMPText(transitBtnObj.transform, "ButtonText",
                "<color=#00e5ff><b>[ CURRENT LOCATION // SAFEHOUSE 407 ]</b></color>",
                10f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            StretchFull(transitTxt.rectTransform);

            // Quick-Nav Telemetry Footer (y: -302 to -334, height: 32px)
            GameObject navFooter = new GameObject("QuickNavFooter", typeof(RectTransform));
            navFooter.transform.SetParent(intelCard.transform, false);
            RectTransform nfRect = navFooter.GetComponent<RectTransform>();
            nfRect.anchorMin = new Vector2(0f, 1f);
            nfRect.anchorMax = new Vector2(1f, 1f);
            nfRect.pivot = new Vector2(0.5f, 1f);
            nfRect.offsetMin = new Vector2(14f, -334f);
            nfRect.offsetMax = new Vector2(-14f, -302f);
            Image nfBg = navFooter.AddComponent<Image>();
            nfBg.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);

            TextMeshProUGUI quickNavTmp = CreateTMPText(navFooter.transform, "PromptText",
                "<color=#94a3b8>Click any sector on the blueprint or use arrow keys to inspect recon telemetry & architectural notes.</color>",
                8.5f, new Color(0.75f, 0.84f, 0.92f), TextAlignmentOptions.Center);
            StretchFull(quickNavTmp.rectTransform);
            quickNavTmp.rectTransform.offsetMin = new Vector2(8f, 0f);
            quickNavTmp.rectTransform.offsetMax = new Vector2(-8f, 0f);

            mapCtrl.BindIntelReferences(hdrIconImg, intelTitleTmp, intelStatusTmp,
                intelThreatTmp, intelScavengeTmp, intelAcousticTmp, intelDescTmp,
                quickNavTmp, transitBtn, transitBg, transitTxt);

            return mapCtrl;
        }

        private static void CreateBlueprintFloorRow(TacticalMapViewController mapCtrl, Transform canvasParent, float yPos,
            string leftId, string leftCode, string leftSub,
            string hallId, string hallCode, string hallSub,
            string rightId, string rightCode, string rightSub,
            bool hasSouthCorridorLink)
        {
            // Left Wing Doorway Threshold Tick (at x = -74, y = yPos)
            Image leftDoorTick = CreateDoorwayThreshold(canvasParent, "Door_" + leftId, new Vector2(-54f, yPos), new Vector2(44f, 4f));

            // Right Wing Doorway Threshold Tick (at x = +74, y = yPos)
            Image rightDoorTick = CreateDoorwayThreshold(canvasParent, "Door_" + rightId, new Vector2(54f, yPos), new Vector2(44f, 4f));

            // Corridor South Archway Tick (at x = 0, y = yPos - 26)
            Image hallDoorTick = null;
            if (hasSouthCorridorLink)
            {
                hallDoorTick = CreateDoorwayThreshold(canvasParent, "Arch_" + hallId, new Vector2(0f, yPos - 25f), new Vector2(28f, 3f));
            }

            // Left Wing Room Module (x = -196, size = 220 x 48)
            CreateBlueprintRoomNode(mapCtrl, canvasParent, leftId, leftCode, leftSub,
                new Vector2(-160f, yPos), new Vector2(168f, 44f), isHallway: false, doorTickImg: leftDoorTick);

            // Central Spine Corridor Module (x = 0, size = 124 x 48)
            CreateBlueprintRoomNode(mapCtrl, canvasParent, hallId, hallCode, hallSub,
                new Vector2(0f, yPos), new Vector2(64f, 44f), isHallway: true, doorTickImg: hallDoorTick);

            // Right Wing Room Module (x = +196, size = 220 x 48)
            CreateBlueprintRoomNode(mapCtrl, canvasParent, rightId, rightCode, rightSub,
                new Vector2(160f, yPos), new Vector2(168f, 44f), isHallway: false, doorTickImg: rightDoorTick);
        }

        private static Image CreateDoorwayThreshold(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject frameObj = new GameObject(name, typeof(RectTransform));
            frameObj.transform.SetParent(parent, false);
            RectTransform fRect = frameObj.GetComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0.5f, 0.5f);
            fRect.anchorMax = new Vector2(0.5f, 0.5f);
            fRect.pivot = new Vector2(0.5f, 0.5f);
            fRect.anchoredPosition = pos;
            fRect.sizeDelta = new Vector2(size.x, size.y + 4f);

            Image frameBg = frameObj.AddComponent<Image>();
            frameBg.color = new Color(0.02f, 0.03f, 0.05f, 0.95f);
            frameBg.raycastTarget = false;

            GameObject tickObj = new GameObject("Tick", typeof(RectTransform));
            tickObj.transform.SetParent(frameObj.transform, false);
            RectTransform tRect = tickObj.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 0.5f);
            tRect.anchorMax = new Vector2(0.5f, 0.5f);
            tRect.pivot = new Vector2(0.5f, 0.5f);
            tRect.anchoredPosition = Vector2.zero;
            tRect.sizeDelta = size;

            Image tickImg = tickObj.AddComponent<Image>();
            tickImg.color = new Color(0.24f, 0.42f, 0.58f, 0.90f);
            tickImg.raycastTarget = false;
            return tickImg;
        }

        private static void CreateBlueprintRoomNode(TacticalMapViewController mapCtrl, Transform parent,
            string id, string code, string subLabel, Vector2 pos, Vector2 size,
            bool isHallway, Image doorTickImg)
        {
            GameObject nodeObj = new GameObject("Node_" + id, typeof(RectTransform));
            nodeObj.transform.SetParent(parent, false);
            RectTransform nRect = nodeObj.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0.5f, 0.5f);
            nRect.anchorMax = new Vector2(0.5f, 0.5f);
            nRect.pivot = new Vector2(0.5f, 0.5f);
            nRect.anchoredPosition = pos;
            nRect.sizeDelta = size;

            Image bg = nodeObj.AddComponent<Image>();
            bg.color = new Color(0.035f, 0.055f, 0.085f, 0.96f);

            Outline ol = nodeObj.AddComponent<Outline>();
            ol.effectColor = new Color(0.20f, 0.34f, 0.48f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = nodeObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Procedural Pattern Overlay (Softened tactical wireframe hatch)
            GameObject patObj = new GameObject("PatternOverlay", typeof(RectTransform));
            patObj.transform.SetParent(nodeObj.transform, false);
            StretchFull(patObj.GetComponent<RectTransform>());
            Image patImg = patObj.AddComponent<Image>();
            patImg.type = Image.Type.Tiled;
            patImg.color = new Color(1f, 1f, 1f, 0.12f);
            patImg.raycastTarget = false;
            patObj.SetActive(false);

            // Procedural Vector Room Icon (16x16 on left)
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(nodeObj.transform, false);
            RectTransform icRect = iconObj.GetComponent<RectTransform>();
            icRect.anchorMin = new Vector2(0f, 0.5f);
            icRect.anchorMax = new Vector2(0f, 0.5f);
            icRect.pivot = new Vector2(0f, 0.5f);
            icRect.anchoredPosition = new Vector2(isHallway ? 6f : 8f, isHallway ? 6f : 0f);
            icRect.sizeDelta = isHallway ? new Vector2(12f, 12f) : new Vector2(16f, 16f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Room Code Label (Top Line) — Pure crisp white for 16:1 contrast
            TextMeshProUGUI codeTmp = CreateTMPText(nodeObj.transform, "Code", code,
                isHallway ? 8.5f : 10f,
                Color.white,
                isHallway ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft,
                FontStyles.Bold);
            RectTransform cRect = codeTmp.rectTransform;
            cRect.anchorMin = new Vector2(0f, 0.5f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.offsetMin = new Vector2(isHallway ? 4f : 28f, -1f);
            cRect.offsetMax = new Vector2(isHallway ? -4f : -48f, -3f);

            // Sub-Label / Clearance Readout (Bottom Line)
            TextMeshProUGUI subTmp = CreateTMPText(nodeObj.transform, "SubLabel", subLabel,
                isHallway ? 7f : 8f,
                new Color(0.65f, 0.78f, 0.88f),
                isHallway ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft);
            RectTransform sRect = subTmp.rectTransform;
            sRect.anchorMin = new Vector2(0f, 0f);
            sRect.anchorMax = new Vector2(1f, 0.5f);
            sRect.offsetMin = new Vector2(isHallway ? 4f : 28f, 3f);
            sRect.offsetMax = new Vector2(isHallway ? -4f : -48f, 1f);

            // Tactical Status Pill Badge (Top-Right Corner, e.g. [LOCK], [HERE], [EXIT])
            GameObject navBadge = new GameObject("NavBadge", typeof(RectTransform));
            navBadge.transform.SetParent(nodeObj.transform, false);
            RectTransform nbRect = navBadge.GetComponent<RectTransform>();
            nbRect.anchorMin = new Vector2(1f, 1f);
            nbRect.anchorMax = new Vector2(1f, 1f);
            nbRect.pivot = new Vector2(1f, 1f);
            nbRect.anchoredPosition = new Vector2(-4f, -4f);
            nbRect.sizeDelta = new Vector2(36f, 14f);
            Image nbBg = navBadge.AddComponent<Image>();
            nbBg.color = new Color(0.04f, 0.14f, 0.18f, 0.95f);
            nbBg.raycastTarget = false;
            Outline nbOl = navBadge.AddComponent<Outline>();
            nbOl.effectColor = new Color(0.18f, 0.85f, 0.55f, 0.80f);
            nbOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI navTmp = CreateTMPText(navBadge.transform, "NavText", "[INFO]",
                7.5f, new Color(0.25f, 0.95f, 0.65f), TextAlignmentOptions.Center, FontStyles.Bold);
            StretchFull(navTmp.rectTransform);
            navBadge.SetActive(false);

            // Player Bio-Signal Beacon (Bottom-Right Corner on Current Room)
            GameObject haloGlow = new GameObject("PlayerHaloGlow", typeof(RectTransform));
            haloGlow.transform.SetParent(nodeObj.transform, false);
            haloGlow.transform.SetAsFirstSibling();
            RectTransform hgRect = haloGlow.GetComponent<RectTransform>();
            StretchFull(hgRect);
            hgRect.offsetMin = new Vector2(-5f, -5f);
            hgRect.offsetMax = new Vector2(5f, 5f);




            Image hgBg = haloGlow.AddComponent<Image>();
            hgBg.color = new Color(0.0f, 0.85f, 1.0f, 0.16f);
            hgBg.raycastTarget = false;
            Outline hgOl = haloGlow.AddComponent<Outline>();
            hgOl.effectColor = new Color(0.0f, 0.95f, 1.0f, 0.85f);
            hgOl.effectDistance = new Vector2(1.5f, -1.5f);




            haloGlow.AddComponent<PulsingBadge>();
            haloGlow.SetActive(false);

            if (isHallway)
            {
                iconObj.SetActive(false);
                codeTmp.text = "";
                subTmp.text = "";
            }

            mapCtrl.BindUIHolder(id, nodeObj, bg, patImg, ol, iconImg, codeTmp, subTmp, navBadge, navTmp, haloGlow, doorTickImg, btn);
        }
        #endregion

        #region Tactical Skills HUD Builder (Phase 2)
        private static TacticalSkillsViewController BuildTacticalSkillsHUD(GameObject pSkillsObj,
            out TextMeshProUGUI legacyAttrTmp, out TextMeshProUGUI legacySkillsTmp)
        {
            TacticalSkillsViewController skillsCtrl = pSkillsObj.AddComponent<TacticalSkillsViewController>();

            GameObject legAttrObj = new GameObject("LegacyAttributesText", typeof(RectTransform));
            legAttrObj.transform.SetParent(pSkillsObj.transform, false);
            legacyAttrTmp = legAttrObj.AddComponent<TextMeshProUGUI>();
            legAttrObj.SetActive(false);

            GameObject legSkObj = new GameObject("LegacySkillsText", typeof(RectTransform));
            legSkObj.transform.SetParent(pSkillsObj.transform, false);
            legacySkillsTmp = legSkObj.AddComponent<TextMeshProUGUI>();
            legSkObj.SetActive(false);

            CreateSectionBadge(pSkillsObj.transform, "CORE ATTRIBUTES // M.O.R.T.A.L. SURVIVOR MATRIX");

            GameObject attrRow1 = CreateHUDCardRow(pSkillsObj.transform, "AttrRow1");
            CreateAttributeCard(skillsCtrl, attrRow1.transform, "STR", "[M] MIGHT", "Carry capacity, melee impact, and blunt leverage.", new Color(0.0f, 0.90f, 1.0f));
            CreateAttributeCard(skillsCtrl, attrRow1.transform, "PER", "[O] OBSERVATION", "Acoustic perception, trap detection, and ranged aim.", new Color(0.0f, 0.90f, 1.0f));
            CreateAttributeCard(skillsCtrl, attrRow1.transform, "AGI", "[R] REFLEX", "Sprint agility, evasion speed, and reaction frames.", new Color(0.0f, 0.90f, 1.0f));

            GameObject attrRow2 = CreateHUDCardRow(pSkillsObj.transform, "AttrRow2");
            CreateAttributeCard(skillsCtrl, attrRow2.transform, "INT", "[T] TACTICS", "Technical problem solving, medicine, and terminal hacking.", new Color(0.0f, 0.90f, 1.0f));
            CreateAttributeCard(skillsCtrl, attrRow2.transform, "CHA", "[A] LEADERSHIP", "Morale stability, negotiation, and bargain discounts.", new Color(0.0f, 0.90f, 1.0f));
            CreateAttributeCard(skillsCtrl, attrRow2.transform, "LCK", "[L] AUSPICE", "Critical hit rate, rare scavenge drops, and lucky saves.", new Color(0.0f, 0.90f, 1.0f));

            CreateSectionBadge(pSkillsObj.transform, "COMBAT & SURVIVAL DISCIPLINES // TACTICAL PROFICIENCY");

            GameObject skRow1 = CreateHUDCardRow(pSkillsObj.transform, "SkillRow1");
            CreateSkillCard(skillsCtrl, skRow1.transform, "SHOOTING", "Firearm recoil control, precision spread, and reload speed.");
            CreateSkillCard(skillsCtrl, skRow1.transform, "MELEE", "Blunt/blade kinetic leverage, parry window, and stagger.");
            CreateSkillCard(skillsCtrl, skRow1.transform, "STEALTH", "Footstep acoustic dampening and shadow concealment.");

            GameObject skRow2 = CreateHUDCardRow(pSkillsObj.transform, "SkillRow2");
            CreateSkillCard(skillsCtrl, skRow2.transform, "TECH", "Terminal bypassing, jury-rigging, and generator repairs.");
            CreateSkillCard(skillsCtrl, skRow2.transform, "MEDICINE", "Treatment efficacy, infection triage, and drug synthesis.");
            CreateSkillCard(skillsCtrl, skRow2.transform, "SCAVENGE", "Search speed, hidden caches, and component reclamation.");

            return skillsCtrl;
        }

        private static GameObject CreateHUDCardRow(Transform parent, string name)
        {
            GameObject rowObj = new GameObject(name, typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            rowObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 64f);
            HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            LayoutElement le = rowObj.AddComponent<LayoutElement>();
            le.minHeight = 64f;
            le.preferredHeight = 64f;
            return rowObj;
        }

        private static void CreateAttributeCard(TacticalSkillsViewController skillsCtrl, Transform parent,
            string key, string title, string subDesc, Color filledCol)
        {
            GameObject card = new GameObject("Card_" + key, typeof(RectTransform));
            card.transform.SetParent(parent, false);
            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI titleTmp = CreateTMPText(card.transform, "Title",
                $"<color=#00e5ff><b>{title}</b></color>", 11f, Color.white, TextAlignmentOptions.MidlineLeft);
            RectTransform tRect = titleTmp.rectTransform;
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(0.72f, 1f);
            tRect.pivot = new Vector2(0f, 1f);
            tRect.offsetMin = new Vector2(10f, -22f);
            tRect.offsetMax = new Vector2(0f, -4f);

            TextMeshProUGUI valTmp = CreateTMPText(card.transform, "Value",
                "<b>0</b> <color=#55606e>/ 10</color>", 11f, Color.white, TextAlignmentOptions.MidlineRight);
            RectTransform vRect = valTmp.rectTransform;
            vRect.anchorMin = new Vector2(0.72f, 1f);
            vRect.anchorMax = new Vector2(1f, 1f);
            vRect.pivot = new Vector2(1f, 1f);
            vRect.offsetMin = new Vector2(0f, -22f);
            vRect.offsetMax = new Vector2(-10f, -4f);

            GameObject pipsBar = new GameObject("PipsBar", typeof(RectTransform));
            pipsBar.transform.SetParent(card.transform, false);
            RectTransform pbRect = pipsBar.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0f, 1f);
            pbRect.anchorMax = new Vector2(1f, 1f);
            pbRect.pivot = new Vector2(0.5f, 1f);
            pbRect.offsetMin = new Vector2(10f, -33f);
            pbRect.offsetMax = new Vector2(-10f, -26f);

            HorizontalLayoutGroup phlg = pipsBar.AddComponent<HorizontalLayoutGroup>();
            phlg.spacing = 3f;
            phlg.childControlWidth = true;
            phlg.childControlHeight = true;
            phlg.childForceExpandWidth = true;
            phlg.childForceExpandHeight = true;

            Color emptyCol = new Color(0.12f, 0.15f, 0.20f, 1f);
            List<Image> pips = new List<Image>();
            for (int i = 0; i < 10; i++)
            {
                GameObject pip = new GameObject("Pip_" + i, typeof(RectTransform));
                pip.transform.SetParent(pipsBar.transform, false);
                Image pipImg = pip.AddComponent<Image>();
                pipImg.color = emptyCol;
                pips.Add(pipImg);
            }

            TextMeshProUGUI descTmp = CreateTMPText(card.transform, "Desc", subDesc, 8.5f, new Color(0.55f, 0.60f, 0.68f), TextAlignmentOptions.MidlineLeft);
            RectTransform dRect = descTmp.rectTransform;
            dRect.anchorMin = new Vector2(0f, 0f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.offsetMin = new Vector2(10f, 4f);
            dRect.offsetMax = new Vector2(-10f, -35f);

            skillsCtrl.BindAttributeRow(key, key, title, valTmp, pips, filledCol, emptyCol);
        }

        private static void CreateSkillCard(TacticalSkillsViewController skillsCtrl, Transform parent,
            string key, string subDesc)
        {
            GameObject card = new GameObject("Card_" + key, typeof(RectTransform));
            card.transform.SetParent(parent, false);
            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);
            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI titleTmp = CreateTMPText(card.transform, "Title",
                $"<color=#f39c12><b>{key}</b></color>", 11f, Color.white, TextAlignmentOptions.MidlineLeft);
            RectTransform tRect = titleTmp.rectTransform;
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(0.42f, 1f);
            tRect.offsetMin = new Vector2(10f, -22f);
            tRect.offsetMax = new Vector2(0f, -4f);

            TextMeshProUGUI tierTmp = CreateTMPText(card.transform, "Tier",
                "<color=#7f8c8d><b>UNTRAINED</b></color>", 9.5f, Color.white, TextAlignmentOptions.Center);
            RectTransform trRect = tierTmp.rectTransform;
            trRect.anchorMin = new Vector2(0.42f, 1f);
            trRect.anchorMax = new Vector2(0.76f, 1f);
            trRect.offsetMin = new Vector2(0f, -22f);
            trRect.offsetMax = new Vector2(0f, -4f);

            TextMeshProUGUI valTmp = CreateTMPText(card.transform, "Value",
                "<b>0</b> <color=#55606e>/ 5</color>", 11f, Color.white, TextAlignmentOptions.MidlineRight);
            RectTransform vRect = valTmp.rectTransform;
            vRect.anchorMin = new Vector2(0.76f, 1f);
            vRect.anchorMax = new Vector2(1f, 1f);
            vRect.offsetMin = new Vector2(0f, -22f);
            vRect.offsetMax = new Vector2(-10f, -4f);

            GameObject pipsBar = new GameObject("PipsBar", typeof(RectTransform));
            pipsBar.transform.SetParent(card.transform, false);
            RectTransform pbRect = pipsBar.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0f, 1f);
            pbRect.anchorMax = new Vector2(1f, 1f);
            pbRect.pivot = new Vector2(0.5f, 1f);
            pbRect.offsetMin = new Vector2(10f, -33f);
            pbRect.offsetMax = new Vector2(-10f, -26f);

            HorizontalLayoutGroup phlg = pipsBar.AddComponent<HorizontalLayoutGroup>();
            phlg.spacing = 4f;
            phlg.childControlWidth = true;
            phlg.childControlHeight = true;
            phlg.childForceExpandWidth = true;
            phlg.childForceExpandHeight = true;

            Color filledCol = new Color(0.95f, 0.55f, 0.12f, 1f);
            Color emptyCol = new Color(0.12f, 0.15f, 0.20f, 1f);
            List<Image> pips = new List<Image>();
            for (int i = 0; i < 5; i++)
            {
                GameObject pip = new GameObject("Pip_" + i, typeof(RectTransform));
                pip.transform.SetParent(pipsBar.transform, false);
                Image pipImg = pip.AddComponent<Image>();
                pipImg.color = emptyCol;
                pips.Add(pipImg);
            }

            TextMeshProUGUI descTmp = CreateTMPText(card.transform, "Desc", subDesc, 8.5f, new Color(0.55f, 0.60f, 0.68f), TextAlignmentOptions.MidlineLeft);
            RectTransform dRect = descTmp.rectTransform;
            dRect.anchorMin = new Vector2(0f, 0f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.offsetMin = new Vector2(10f, 4f);
            dRect.offsetMax = new Vector2(-10f, -35f);

            skillsCtrl.BindSkillRow(key, key, valTmp, tierTmp, pips, filledCol, emptyCol);
        }
        #endregion

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
