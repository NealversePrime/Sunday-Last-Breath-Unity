using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SundayLastBreath.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Modal Window: System Menu (ESC Menu).
    /// Handles Multi-Slot Save/Load via Easy Save 3, Audio volume sliders, and Exit to Main Menu.
    /// Completely extracted from GameStageScreen.cs to keep the Stage lightweight.
    /// </summary>
    public class SystemMenuModal : MonoBehaviour
    {
        [Header("Stage Reference")]
        [SerializeField] private GameStageScreen stage;

        public void Initialize(GameStageScreen stageController)
        {
            stage = stageController;
        }

        private void PlayClickSFX()
        {
            if (stage != null) stage.PlayClickSFX();
        }

        private void AppendStoryLog(string timestamp, string header, string content)
        {
            if (stage != null) stage.AppendStoryLog(timestamp, header, content);
        }

        private void StopAllAudioSources()
        {
            if (stage != null) stage.StopAllAudioSources();
        }

        private void RefreshLocationBGM()
        {
            if (stage != null) stage.RefreshLocationBGM();
        }

        private GameClock gameClock => stage != null ? stage.Clock : null;

        private bool isBuilt = false;

public enum SystemMenuView { Main, SaveSlots, LoadSlots, Settings, ExitConfirm }

private SystemMenuView currentSysView = SystemMenuView.Main;

private GameObject systemMenuModal;

private GameObject sysMainPanel;

private GameObject sysSlotsPanel;

private GameObject sysSettingsPanel;

private GameObject sysExitConfirmPanel;

private bool isSlotPickerSaveMode = true;

private TextMeshProUGUI[] sysSlotTitles = new TextMeshProUGUI[SaveManager.TotalSlotCount];

private TextMeshProUGUI[] sysSlotDetails = new TextMeshProUGUI[SaveManager.TotalSlotCount];

private Button[] sysSlotActionBtns = new Button[SaveManager.TotalSlotCount];

private TextMeshProUGUI[] sysSlotActionLabels = new TextMeshProUGUI[SaveManager.TotalSlotCount];

private Button[] sysSlotDeleteBtns = new Button[SaveManager.TotalSlotCount];

private Image[] sysSlotCardBgs = new Image[SaveManager.TotalSlotCount];

private Outline[] sysSlotCardOutlines = new Outline[SaveManager.TotalSlotCount];

private TextMeshProUGUI sysHeaderSubText;

private TextMeshProUGUI sysTelemetryStatusText;

private TextMeshProUGUI sysSaveBtnLabel;

private Button sysLoadGameBtn;

private TextMeshProUGUI sysLoadBtnLabel;

private TextMeshProUGUI sysExitWarningDetailText;

private Slider sysMasterVolSlider;

private Slider sysBgmVolSlider;

private Slider sysSfxVolSlider;

private TextMeshProUGUI sysMasterVolValueText;

private TextMeshProUGUI sysBgmVolValueText;

private TextMeshProUGUI sysSfxVolValueText;

private Coroutine sysSaveFeedbackRoutine;

private void OnPDASaveClicked()

{
PlayClickSFX();

Open();
}

public void Open()
{
    EnsureSystemMenuModalBuilt();
    if (stage != null && systemMenuModal != null && systemMenuModal.transform.parent != stage.transform)
    {
        systemMenuModal.transform.SetParent(stage.transform, false);
    }
    RefreshSystemMenuTelemetry();
    SwitchSystemMenuView(SystemMenuView.Main);
    if (systemMenuModal != null)
    {
        systemMenuModal.transform.SetAsLastSibling();
        systemMenuModal.SetActive(true);
    }
}

public void Close()
{
    PlayClickSFX();
    if (systemMenuModal != null)
    {
        systemMenuModal.SetActive(false);
    }
}

private void SwitchSystemMenuView(SystemMenuView view)

{
PlayClickSFX();

currentSysView = view;

isSlotPickerSaveMode = (view == SystemMenuView.SaveSlots);

if (sysMainPanel != null) sysMainPanel.SetActive(view == SystemMenuView.Main);

if (sysSlotsPanel != null) sysSlotsPanel.SetActive(view == SystemMenuView.SaveSlots || view == SystemMenuView.LoadSlots);

if (sysSettingsPanel != null) sysSettingsPanel.SetActive(view == SystemMenuView.Settings);

if (sysExitConfirmPanel != null) sysExitConfirmPanel.SetActive(view == SystemMenuView.ExitConfirm);

bool styleD = StyleDThemeUtility.IsStyleDActive;

switch (view)

{
case SystemMenuView.Main:

if (sysHeaderSubText != null)

sysHeaderSubText.text = styleD
    ? "SYSTEM MENU - ARCHIVE, AUDIO & SESSION"
    : "SYSTEM // ARCHIVE, AUDIO CONFIGURATION & SESSION CONTROL";

RefreshSystemMenuTelemetry();

break;

case SystemMenuView.SaveSlots:

if (sysHeaderSubText != null)

sysHeaderSubText.text = styleD
    ? "SAVE GAME - SELECT ARCHIVE SLOT"
    : "SAVE GAME // SELECT ARCHIVE SLOT TO RECORD PROGRESS";

RefreshSystemSlotCards();

break;

case SystemMenuView.LoadSlots:

if (sysHeaderSubText != null)

sysHeaderSubText.text = styleD
    ? "LOAD GAME - SELECT ARCHIVE SLOT"
    : "LOAD GAME // SELECT ARCHIVED SLOT TO RESTORE";

RefreshSystemSlotCards();

break;

case SystemMenuView.Settings:

if (sysHeaderSubText != null)

sysHeaderSubText.text = styleD
    ? "SETTINGS - AUDIO MIXER CALIBRATION"
    : "SETTINGS // AUDIO MIXER & VOLUME CALIBRATION";

SyncSettingsSlidersFromAudio();

break;

case SystemMenuView.ExitConfirm:

if (sysHeaderSubText != null)

sysHeaderSubText.text = styleD
    ? "WARNING - CONFIRM SESSION EXIT"
    : "WARNING // CONFIRM SESSION TERMINATION";

UpdateExitWarningDetails();

break;
}
}

private void RefreshSystemSlotCards()

{
bool styleD = StyleDThemeUtility.IsStyleDActive;

for (int i = 0; i < SaveManager.TotalSlotCount; i++)

{
int slotIdx = i;

var s = SaveManager.Instance != null ? SaveManager.Instance.GetSlotSummary(slotIdx) : default;

string slotTitle = SaveManager.Instance != null ? SaveManager.Instance.GetSlotTitle(slotIdx) : $"SLOT {slotIdx}";

if (s.exists)

{
string savedBadge = !string.IsNullOrEmpty(s.realSaveTime)

? (styleD ? $"<color=#8C9486>[{s.realSaveTime}]</color>" : $"<color=#64748B>[{s.realSaveTime}]</color>")

: "";

if (sysSlotTitles[slotIdx] != null)

{
string titleHex = styleD
    ? (slotIdx == 0 ? "#E0A43A" : "#D95829")
    : (slotIdx == 0 ? "#38BDF8" : "#F59E0B");
string nameHex = styleD ? "#E4DFD1" : "#F8FAFC";
string jobHex = styleD ? "#8C9486" : "#94A3B8";

sysSlotTitles[slotIdx].text =

$"<color={titleHex}><b>[{slotTitle}]</b></color>   <color={nameHex}><b>{s.characterName}</b></color> <color={jobHex}>({s.job})</color>  {savedBadge}";

sysSlotTitles[slotIdx].GetComponent<HorizontalMarqueeText>()?.ResetScroll();
}

if (sysSlotDetails[slotIdx] != null)

{
string clockText = styleD && !string.IsNullOrEmpty(s.clockDisplay)
    ? s.clockDisplay.Replace(" // ", " - ")
    : s.clockDisplay;

if (styleD)
{
    sysSlotDetails[slotIdx].text =
        $"<color=#E0A43A>{s.roomDisplayName}</color>  |  <color=#E4DFD1>{clockText}</color>  |  " +
        $"HP <color=#D95829>{s.hp}/{s.hpMax}</color>  STM <color=#6EE7B7>{s.stamina}/{s.staminaMax}</color>  HUM <color=#B4A7D6>{s.humanity}%</color>";
}
else
{
    sysSlotDetails[slotIdx].text =
        $"<color=#FBBF24>{s.roomDisplayName}</color>  |  <color=#E2E8F0>{clockText}</color>  |  " +
        $"HP <color=#EF4444>{s.hp}/{s.hpMax}</color>  STM <color=#10B981>{s.stamina}/{s.staminaMax}</color>  HUM <color=#A78BFA>{s.humanity}%</color>";
}

sysSlotDetails[slotIdx].GetComponent<HorizontalMarqueeText>()?.ResetScroll();
}

if (sysSlotActionLabels[slotIdx] != null)

{
sysSlotActionLabels[slotIdx].text = isSlotPickerSaveMode ? "<b>OVERWRITE</b>" : "<b>> LOAD</b>";

sysSlotActionLabels[slotIdx].color = styleD
    ? StyleDThemeUtility.ColBone
    : (isSlotPickerSaveMode ? new Color(0.98f, 0.78f, 0.28f) : new Color(0.38f, 0.85f, 0.98f));
}

if (sysSlotActionBtns[slotIdx] != null)

{
sysSlotActionBtns[slotIdx].interactable = true;

if (styleD)
{
    Color tint = isSlotPickerSaveMode
        ? StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColAmber, 0.55f)
        : StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColSage, 0.55f);
    Color hov = isSlotPickerSaveMode
        ? StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColAmber, 0.78f)
        : StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColSage, 0.78f);
    StyleDThemeUtility.ApplyMetalButton(sysSlotActionBtns[slotIdx], tint, hov, 2.8f, StyleDThemeUtility.ColBone);
}
else
{
    Image btnBg = sysSlotActionBtns[slotIdx].GetComponent<Image>();

    if (btnBg != null)

    {
        btnBg.color = isSlotPickerSaveMode
            ? new Color(0.24f, 0.16f, 0.04f, 0.98f)
            : new Color(0.07f, 0.25f, 0.36f, 0.98f);
    }
}
}

if (sysSlotDeleteBtns[slotIdx] != null)

{
sysSlotDeleteBtns[slotIdx].gameObject.SetActive(true);
}

if (sysSlotCardBgs[slotIdx] != null)

{
if (styleD)
{
    StyleDThemeUtility.ApplySliced(sysSlotCardBgs[slotIdx], StyleDThemeUtility.WellRecessed, 3.2f, Color.white);
    StyleDThemeUtility.AddLeftAccentBar(sysSlotCardBgs[slotIdx].transform, slotIdx == 0 ? StyleDThemeUtility.ColAmber : StyleDThemeUtility.ColRust, 4f);
}
else
{
    sysSlotCardBgs[slotIdx].color = new Color(0.07f, 0.10f, 0.15f, 0.96f);
}
}

if (!styleD && sysSlotCardOutlines[slotIdx] != null)

{
sysSlotCardOutlines[slotIdx].effectColor = slotIdx == 0

? new Color(0.22f, 0.74f, 0.97f, 0.65f)

: new Color(0.95f, 0.62f, 0.12f, 0.65f);
}
}

else

{
if (sysSlotTitles[slotIdx] != null)

{
string emptyHex = styleD ? "#8C9486" : "#64748B";
sysSlotTitles[slotIdx].text = $"<color={emptyHex}><b>[{slotTitle}]</b>   -   [ EMPTY ARCHIVE SLOT ]</color>";
sysSlotTitles[slotIdx].GetComponent<HorizontalMarqueeText>()?.ResetScroll();
}

if (sysSlotDetails[slotIdx] != null)

{
string hintHex = styleD ? "#8C9486" : (isSlotPickerSaveMode ? "#64748B" : "#475569");
sysSlotDetails[slotIdx].text = isSlotPickerSaveMode

? $"<color={hintHex}><i>Available slot — click SAVE HERE to record current progress.</i></color>"

: $"<color={hintHex}><i>No survivor data recorded in this slot.</i></color>";
sysSlotDetails[slotIdx].GetComponent<HorizontalMarqueeText>()?.ResetScroll();
}

if (sysSlotActionLabels[slotIdx] != null)

{
sysSlotActionLabels[slotIdx].text = isSlotPickerSaveMode ? "<b>SAVE HERE</b>" : "EMPTY";

sysSlotActionLabels[slotIdx].color = styleD
    ? (isSlotPickerSaveMode ? StyleDThemeUtility.ColBone : StyleDThemeUtility.ColMuted)
    : (isSlotPickerSaveMode ? new Color(0.28f, 0.95f, 0.58f) : new Color(0.35f, 0.40f, 0.46f));
}

if (sysSlotActionBtns[slotIdx] != null)

{
sysSlotActionBtns[slotIdx].interactable = isSlotPickerSaveMode;

if (styleD)
{
    Color tint = isSlotPickerSaveMode
        ? StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColSage, 0.52f)
        : StyleDThemeUtility.ColMetalDisabled;
    Color hov = isSlotPickerSaveMode
        ? StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColSage, 0.75f)
        : StyleDThemeUtility.ColMetalDisabled;
    StyleDThemeUtility.ApplyMetalButton(sysSlotActionBtns[slotIdx], tint, hov, 2.8f, isSlotPickerSaveMode ? StyleDThemeUtility.ColBone : StyleDThemeUtility.ColMuted);
}
else
{
    Image btnBg = sysSlotActionBtns[slotIdx].GetComponent<Image>();

    if (btnBg != null)

    {
        btnBg.color = isSlotPickerSaveMode
            ? new Color(0.06f, 0.24f, 0.14f, 0.98f)
            : new Color(0.06f, 0.08f, 0.11f, 0.75f);
    }
}
}

if (sysSlotDeleteBtns[slotIdx] != null)

{
sysSlotDeleteBtns[slotIdx].gameObject.SetActive(false);
}

if (sysSlotCardBgs[slotIdx] != null)

{
if (styleD)
{
    StyleDThemeUtility.ApplySliced(sysSlotCardBgs[slotIdx], StyleDThemeUtility.WellRecessed, 3.2f, new Color(0.72f, 0.72f, 0.74f, 0.90f));
    StyleDThemeUtility.AddLeftAccentBar(sysSlotCardBgs[slotIdx].transform, isSlotPickerSaveMode ? StyleDThemeUtility.ColSage : StyleDThemeUtility.ColHairline, 4f);
}
else
{
    sysSlotCardBgs[slotIdx].color = new Color(0.04f, 0.05f, 0.08f, 0.88f);
}
}

if (!styleD && sysSlotCardOutlines[slotIdx] != null)

{
sysSlotCardOutlines[slotIdx].effectColor = new Color(0.18f, 0.22f, 0.28f, 0.5f);
}
}
}
}

private void OnSystemSlotActionClicked(int slotIndex)

{
PlayClickSFX();

if (SaveManager.Instance == null) return;

if (currentSysView == SystemMenuView.LoadSlots || !isSlotPickerSaveMode)

{
if (!SaveManager.Instance.HasSaveInSlot(slotIndex))

{
string slotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);

RefreshSystemMenuTelemetry($"<color=#EF4444><b>{slotTitle} is empty — no survivor data to restore.</b></color>");

return;
}

string restoreSlotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);

Close();

if (LoadingScreen.Instance != null)

{
LoadingScreen.Instance.LoadSlotWithScreen(slotIndex, () =>

{
AppendStoryLog(gameClock != null ? gameClock.GetTimeString() : "23:45", "ARCHIVE RESTORED",

$"Survivor vitals, backpack items, and room progress restored from {restoreSlotTitle}.");
});
}

else

{
bool loaded = SaveManager.Instance.LoadFromSlot(slotIndex);

if (loaded)

{
RefreshLocationBGM();
}
}
}

else

{
bool saved = SaveManager.Instance.SaveToSlot(slotIndex);

if (saved)

{
string slotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);

AppendStoryLog(gameClock != null ? gameClock.GetTimeString() : "23:45", $"MANUAL ARCHIVE // {slotTitle}",

$"Survivor vitals, backpack items, and room progress saved to {slotTitle}.");

var s = SaveManager.Instance.GetSlotSummary(slotIndex);

RefreshSystemSlotCards();

RefreshSystemMenuTelemetry(

$"<color=#10B981><b>[SAVED] {slotTitle}!</b></color>  [{s.saveTimestamp}]  -  {s.characterName} | {s.roomDisplayName}");
}
}
}

private void OnSystemSlotDeleteClicked(int slotIndex)

{
PlayClickSFX();

if (SaveManager.Instance == null) return;

string slotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);

SaveManager.Instance.DeleteSlot(slotIndex);

RefreshSystemSlotCards();

RefreshSystemMenuTelemetry($"<color=#F59E0B>Deleted archive data in {slotTitle}.</color>");
}

private void RefreshSystemMenuTelemetry(string customBanner = null)

{
bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();

if (sysLoadGameBtn != null)

{
sysLoadGameBtn.interactable = true; // Always allow player to click LOAD GAME to inspect and restore slots!
}

bool styleD = StyleDThemeUtility.IsStyleDActive;

if (sysLoadBtnLabel != null)

{
string subHex = styleD ? "#8C9486" : (hasSave ? "#94A3B8" : "#64748B");
sysLoadBtnLabel.text = hasSave

? $"<b>[ 2 ]  LOAD GAME</b>   <size=11><color={subHex}>Choose from 5 archive slots to restore</color></size>"

: $"<b>[ 2 ]  LOAD GAME</b>   <size=11><color={subHex}>[NO ARCHIVE SAVES DETECTED]</color></size>";
}

if (sysTelemetryStatusText != null)

{
if (!string.IsNullOrEmpty(customBanner))

{
sysTelemetryStatusText.text = customBanner;
}

else if (hasSave && SaveManager.Instance != null)

{
var s = SaveManager.Instance.GetSaveSummary();

string stamp = styleD && !string.IsNullOrEmpty(s.saveTimestamp)
    ? s.saveTimestamp.Replace(" // ", " - ")
    : s.saveTimestamp;

sysTelemetryStatusText.text = styleD

? $"<color=#E0A43A><b>LATEST ({s.slotTitle}):</b></color> <color=#8C9486>{stamp}</color>  |  <color=#E4DFD1>{s.characterName}</color>  |  <color=#E0A43A>{s.roomDisplayName}</color>"

: $"<color=#38BDF8><b>LATEST ({s.slotTitle}):</b></color> {stamp}  |  <color=#F8FAFC>{s.characterName}</color>  |  <color=#F59E0B>{s.roomDisplayName}</color>";
}

else

{
sysTelemetryStatusText.text = styleD
    ? "<color=#D95829><b>ARCHIVE STATUS:</b> All save slots are currently empty.</color>"
    : "<color=#EF4444><b>ARCHIVE STATUS:</b> All save slots are currently empty.</color>";
}
}
}

private void UpdateExitWarningDetails()

{
if (sysExitWarningDetailText == null) return;

bool styleD = StyleDThemeUtility.IsStyleDActive;
bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();

if (hasSave && SaveManager.Instance != null)

{
var s = SaveManager.Instance.GetSaveSummary();

if (styleD)
{
    sysExitWarningDetailText.text =
        "Are you sure you want to exit the game?\n" +
        "Any unsaved progress since your last save will be lost.\n\n" +
        $"<color=#E0A43A><b>Latest Save ({s.slotTitle}):</b></color> <color=#E4DFD1>{s.saveTimestamp}</color>\n" +
        $"<color=#8C9486>Citizen: {s.characterName}  |  Location: {s.roomDisplayName} ({s.clockDisplay})</color>";
}
else
{
    sysExitWarningDetailText.text =
        "Are you sure you want to exit the game?\n" +
        "Any unsaved progress since your last save will be lost.\n\n" +
        $"<color=#38BDF8><b>Latest Save ({s.slotTitle}):</b></color> {s.saveTimestamp}\n" +
        $"<color=#94A3B8>Citizen: {s.characterName}  |  Location: {s.roomDisplayName} ({s.clockDisplay})</color>";
}
}

else

{
sysExitWarningDetailText.text =

"Are you sure you want to exit the game?\n" +

(styleD
    ? "<color=#D95829><b>You have not saved your game in any slot yet!</b></color> If you exit without saving, all current progress will be lost."
    : "<color=#EF4444><b>You have not saved your game in any slot yet!</b></color> If you exit without saving, all current progress will be lost.");
}
}

private void OnSystemMenuSaveClicked()

{
SwitchSystemMenuView(SystemMenuView.SaveSlots);
}

private void OnSystemMenuLoadClicked()

{
SwitchSystemMenuView(SystemMenuView.LoadSlots);
}

private void OnSystemMenuSaveAndExitClicked()

{
PlayClickSFX();

if (SaveManager.Instance != null)

{
int targetSlot = SaveManager.Instance.GetMostRecentPopulatedSlotIndex();

if (targetSlot <= 0) targetSlot = 1; // Default to Slot 01 if no save exists yet or if quicksave was recent

SaveManager.Instance.SaveToSlot(targetSlot);

SaveManager.Instance.QuickSave(); // Synchronize QuickSave so both buttons on Title Screen restore this exact moment!
}

ExecuteExitToMainMenu();
}

private void OnSystemMenuExitWithoutSavingClicked()

{
PlayClickSFX();

ExecuteExitToMainMenu();
}

private void ExecuteExitToMainMenu()

{
if (systemMenuModal != null) systemMenuModal.SetActive(false);

StopAllAudioSources();

if (GameManager.Instance != null)

{
GameManager.Instance.GoToTitle();
}

else

{
Application.Quit();
}
}

private void SyncSettingsSlidersFromAudio()

{
float master = AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : AudioListener.volume;

float bgm = AudioManager.Instance != null ? AudioManager.Instance.BgmVolume : 0.75f;

float sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.85f;

if (sysMasterVolSlider != null) sysMasterVolSlider.SetValueWithoutNotify(master);

if (sysBgmVolSlider != null) sysBgmVolSlider.SetValueWithoutNotify(bgm);

if (sysSfxVolSlider != null) sysSfxVolSlider.SetValueWithoutNotify(sfx);

UpdateVolumeLabels(master, bgm, sfx);
}

private void OnMasterVolumeChanged(float val)

{
AudioListener.volume = Mathf.Clamp01(val);

if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(val);

ApplyLocalAudioVolumes();

UpdateVolumeLabels(

val,

sysBgmVolSlider != null ? sysBgmVolSlider.value : 0.75f,

sysSfxVolSlider != null ? sysSfxVolSlider.value : 0.85f);
}

private void OnBgmVolumeChanged(float val)

{
if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(val);

ApplyLocalAudioVolumes();

UpdateVolumeLabels(

sysMasterVolSlider != null ? sysMasterVolSlider.value : 1f,

val,

sysSfxVolSlider != null ? sysSfxVolSlider.value : 0.85f);
}

private void OnSfxVolumeChanged(float val)

{
if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(val);

ApplyLocalAudioVolumes();

UpdateVolumeLabels(

sysMasterVolSlider != null ? sysMasterVolSlider.value : 1f,

sysBgmVolSlider != null ? sysBgmVolSlider.value : 0.75f,

val);
}

private void ApplyLocalAudioVolumes()
{
    float master = sysMasterVolSlider != null ? sysMasterVolSlider.value : (AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : 1f);
    float bgm = sysBgmVolSlider != null ? sysBgmVolSlider.value : (AudioManager.Instance != null ? AudioManager.Instance.BgmVolume : 0.75f);
    float sfx = sysSfxVolSlider != null ? sysSfxVolSlider.value : (AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.85f);

    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.SetMasterVolume(master);
        AudioManager.Instance.SetBGMVolume(bgm);
        AudioManager.Instance.SetSFXVolume(sfx);
    }
}

private void UpdateVolumeLabels(float master, float bgm, float sfx)

{
if (sysMasterVolValueText != null) sysMasterVolValueText.text = $"{Mathf.RoundToInt(master * 100f)}%";

if (sysBgmVolValueText != null) sysBgmVolValueText.text = $"{Mathf.RoundToInt(bgm * 100f)}%";

if (sysSfxVolValueText != null) sysSfxVolValueText.text = $"{Mathf.RoundToInt(sfx * 100f)}%";
}

private void EnsureSystemMenuModalBuilt()
{
    if (isBuilt) return;
    isBuilt = true;

    bool styleD = StyleDThemeUtility.IsStyleDActive;

    systemMenuModal = this.gameObject;
    if (stage != null && systemMenuModal.transform.parent != stage.transform)
    {
        systemMenuModal.transform.SetParent(stage.transform, false);
    }

    TMP_FontAsset sharedFont = !styleD && stage != null && stage.LocationText != null ? stage.LocationText.font : null;

    RectTransform overlayRect = systemMenuModal.GetComponent<RectTransform>();
    if (overlayRect == null) overlayRect = systemMenuModal.AddComponent<RectTransform>();

    overlayRect.anchorMin = Vector2.zero;
    overlayRect.anchorMax = Vector2.one;
    overlayRect.offsetMin = Vector2.zero;
    overlayRect.offsetMax = Vector2.zero;

    Image overlayImg = systemMenuModal.GetComponent<Image>();
    if (overlayImg == null) overlayImg = systemMenuModal.AddComponent<Image>();

    overlayImg.color = styleD ? StyleDThemeUtility.ColBackdrop : new Color(0.01f, 0.02f, 0.04f, 0.88f);
    overlayImg.raycastTarget = true;

    // 2. Center Window Card (760 x 530)
    GameObject card = new GameObject("SystemWindowCard", typeof(RectTransform));
    card.transform.SetParent(systemMenuModal.transform, false);

    RectTransform cardRect = card.GetComponent<RectTransform>();
    cardRect.anchorMin = new Vector2(0.5f, 0.5f);
    cardRect.anchorMax = new Vector2(0.5f, 0.5f);
    cardRect.pivot = new Vector2(0.5f, 0.5f);
    cardRect.sizeDelta = new Vector2(760f, 530f);

    Image cardBg = card.AddComponent<Image>();
    Outline cardOutline = card.AddComponent<Outline>();
    cardOutline.effectColor = new Color(0.95f, 0.62f, 0.05f, 0.85f);
    cardOutline.effectDistance = new Vector2(1.5f, -1.5f);
    if (styleD)
    {
        StyleDThemeUtility.ApplySliced(cardBg, StyleDThemeUtility.PanelChassis, 3.2f, Color.white, disableOutline: true);
    }
    else
    {
        cardBg.color = new Color(0.05f, 0.07f, 0.10f, 0.99f);
    }

    // Top Header Bar
    GameObject headerBar = new GameObject("HeaderBar", typeof(RectTransform));
    headerBar.transform.SetParent(card.transform, false);

    RectTransform hbRect = headerBar.GetComponent<RectTransform>();
    hbRect.anchorMin = new Vector2(0f, 1f);
    hbRect.anchorMax = new Vector2(1f, 1f);
    hbRect.pivot = new Vector2(0.5f, 1f);

    Image hbImg = headerBar.AddComponent<Image>();
    if (styleD)
    {
        hbRect.offsetMin = new Vector2(18f, -62f);
        hbRect.offsetMax = new Vector2(-18f, -18f);
        hbImg.color = Color.clear;
        StyleDThemeUtility.AddHeaderDivider(headerBar.transform);
    }
    else
    {
        hbRect.anchoredPosition = Vector2.zero;
        hbRect.sizeDelta = new Vector2(0f, 52f);
        hbImg.color = new Color(0.09f, 0.12f, 0.17f, 1f);
    }

    sysHeaderSubText = CreateSysTMP(
        headerBar.transform,
        "HeaderTitle",
        styleD ? "SYSTEM MENU - ARCHIVE, AUDIO & SESSION" : "SYSTEM // ARCHIVE, AUDIO CONFIGURATION & SESSION CONTROL",
        styleD ? 13.5f : 13.5f,
        FontStyles.Bold,
        styleD ? StyleDThemeUtility.ColBone : new Color(0.96f, 0.65f, 0.14f),
        TextAlignmentOptions.MidlineLeft,
        sharedFont,
        FontRole.Display);

    RectTransform hTitleRect = sysHeaderSubText.rectTransform;
    hTitleRect.anchorMin = Vector2.zero;
    hTitleRect.anchorMax = Vector2.one;
    hTitleRect.offsetMin = new Vector2(styleD ? 8f : 20f, 0f);
    hTitleRect.offsetMax = new Vector2(-100f, 0f);

    // Close [X] Button in Header
    Button closeTopBtn = CreateSysActionButton(headerBar.transform, "CloseTopBtn", "[ ESC ]",
        new Color(0.16f, 0.19f, 0.24f, 1f), new Color(0.85f, 0.88f, 0.92f), sharedFont, out TextMeshProUGUI closeTopLbl, styleD);

    RectTransform ctbRect = closeTopBtn.GetComponent<RectTransform>();
    ctbRect.anchorMin = new Vector2(1f, 0.5f);
    ctbRect.anchorMax = new Vector2(1f, 0.5f);
    ctbRect.pivot = new Vector2(1f, 0.5f);
    ctbRect.anchoredPosition = new Vector2(styleD ? -4f : -14f, 0f);
    ctbRect.sizeDelta = new Vector2(72f, styleD ? 28f : 30f);
    if (styleD && closeTopLbl != null)
    {
        closeTopLbl.alignment = TextAlignmentOptions.Center;
        closeTopLbl.rectTransform.offsetMin = Vector2.zero;
        closeTopLbl.rectTransform.offsetMax = Vector2.zero;
    }

    closeTopBtn.onClick.AddListener(Close);

    // Telemetry Strip under Header
    GameObject teleStrip = new GameObject("TelemetryStrip", typeof(RectTransform), typeof(RectMask2D));
    teleStrip.transform.SetParent(card.transform, false);

    RectTransform tsRect = teleStrip.GetComponent<RectTransform>();
    tsRect.anchorMin = new Vector2(0f, 1f);
    tsRect.anchorMax = new Vector2(1f, 1f);
    tsRect.pivot = new Vector2(0.5f, 1f);

    Image tsImg = teleStrip.AddComponent<Image>();
    if (styleD)
    {
        tsRect.offsetMin = new Vector2(18f, -96f);
        tsRect.offsetMax = new Vector2(-18f, -66f);
        StyleDThemeUtility.ApplySliced(tsImg, StyleDThemeUtility.WellRecessed, 3.2f, Color.white);
    }
    else
    {
        tsRect.anchoredPosition = new Vector2(0f, -52f);
        tsRect.sizeDelta = new Vector2(0f, 34f);
        tsImg.color = new Color(0.03f, 0.05f, 0.08f, 1f);
    }

    sysTelemetryStatusText = CreateSysTMP(teleStrip.transform, "TelemetryText",
        "ARCHIVE STATUS: READY", 11.5f, FontStyles.Normal,
        styleD ? StyleDThemeUtility.ColMuted : new Color(0.82f, 0.86f, 0.92f),
        TextAlignmentOptions.MidlineLeft, sharedFont, FontRole.Mono);
    if (styleD)
    {
        sysTelemetryStatusText.enableAutoSizing = true;
        sysTelemetryStatusText.fontSizeMin = 8.5f;
        sysTelemetryStatusText.fontSizeMax = 11.5f;
        sysTelemetryStatusText.overflowMode = TextOverflowModes.Ellipsis;
    }

    RectTransform ttRect = sysTelemetryStatusText.rectTransform;
    ttRect.anchorMin = Vector2.zero;
    ttRect.anchorMax = Vector2.one;
    ttRect.offsetMin = new Vector2(styleD ? 12f : 20f, 0f);
    ttRect.offsetMax = new Vector2(styleD ? -12f : -20f, 0f);

// =========================================================================

// VIEW 1: MAIN 4-BUTTON SYSTEM MENU (SAVE, LOAD, SETTINGS, EXIT GAME)

// =========================================================================

sysMainPanel = CreateSubPanelRoot(card.transform, "MainPanel", styleD);

string subHex = styleD ? "#8C9486" : "#94A3B8";

// 1. SAVE GAME

Button saveBtn = CreateSysActionButton(sysMainPanel.transform, "BtnSaveGame",

$"<b>[ 1 ]  SAVE GAME</b>   <size=11><color={subHex}>Choose from 4 archive slots to save progress</color></size>",

new Color(0.06f, 0.22f, 0.14f, 0.98f), new Color(0.25f, 0.95f, 0.58f), sharedFont, out sysSaveBtnLabel, styleD, StyleDThemeUtility.ColSage);

PositionMenuRow(saveBtn.GetComponent<RectTransform>(), 0, styleD);

saveBtn.onClick.AddListener(OnSystemMenuSaveClicked);

// 2. LOAD GAME

sysLoadGameBtn = CreateSysActionButton(sysMainPanel.transform, "BtnLoadGame",

$"<b>[ 2 ]  LOAD GAME</b>   <size=11><color={subHex}>Choose from 4 archive slots to restore</color></size>",

new Color(0.07f, 0.17f, 0.26f, 0.98f), new Color(0.35f, 0.78f, 0.98f), sharedFont, out sysLoadBtnLabel, styleD, StyleDThemeUtility.ColAmber);

PositionMenuRow(sysLoadGameBtn.GetComponent<RectTransform>(), 1, styleD);

sysLoadGameBtn.onClick.AddListener(OnSystemMenuLoadClicked);

// 3. SETTINGS (BGM, SFX, Master Volume)

Button settingsBtn = CreateSysActionButton(sysMainPanel.transform, "BtnSettings",

$"<b>[ 3 ]  SETTINGS</b>   <size=11><color={subHex}>Configure Master, BGM & SFX audio levels</color></size>",

new Color(0.14f, 0.15f, 0.20f, 0.98f), new Color(0.95f, 0.75f, 0.28f), sharedFont, out _, styleD, StyleDThemeUtility.ColBone);

PositionMenuRow(settingsBtn.GetComponent<RectTransform>(), 2, styleD);

settingsBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Settings));

// 4. EXIT GAME (Opens Unsaved Progress Confirmation Window)

string exitSubHex = styleD ? "#D95829" : "#FCA5A5";
Button exitBtn = CreateSysActionButton(sysMainPanel.transform, "BtnExitGame",

$"<b>[ 4 ]  EXIT GAME</b>   <size=11><color={exitSubHex}>Return to Main Menu / Quit session</color></size>",

new Color(0.25f, 0.07f, 0.08f, 0.98f), new Color(0.98f, 0.42f, 0.42f), sharedFont, out _, styleD, StyleDThemeUtility.ColRust);

PositionMenuRow(exitBtn.GetComponent<RectTransform>(), 3, styleD);

exitBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.ExitConfirm));

// Bottom Resume Button

Button resumeBtn = CreateSysActionButton(sysMainPanel.transform, "BtnResume",

"<b>[ RESUME GAME ]</b>",

new Color(0.10f, 0.12f, 0.16f, 1f), new Color(0.82f, 0.86f, 0.92f), sharedFont, out TextMeshProUGUI resLbl, styleD);

RectTransform resRect = resumeBtn.GetComponent<RectTransform>();

resRect.anchorMin = new Vector2(0.5f, 0f);

resRect.anchorMax = new Vector2(0.5f, 0f);

resRect.pivot = new Vector2(0.5f, 0f);

resRect.anchoredPosition = new Vector2(0f, 14f);

resRect.sizeDelta = new Vector2(styleD ? 696f : 710f, 40f);
if (styleD && resLbl != null)
{
    resLbl.alignment = TextAlignmentOptions.Center;
    resLbl.rectTransform.offsetMin = Vector2.zero;
    resLbl.rectTransform.offsetMax = Vector2.zero;
}

resumeBtn.onClick.AddListener(Close);

// =========================================================================

// VIEW 1B: 4-SLOT SAVE / LOAD PICKER SUB-WINDOW

// =========================================================================

sysSlotsPanel = CreateSubPanelRoot(card.transform, "SlotsPickerPanel", styleD);

for (int i = 0; i < SaveManager.TotalSlotCount; i++)

{
            int slotIdx = i;
            GameObject sCard = new GameObject($"SysSlotCard_{slotIdx}", typeof(RectTransform));
            sCard.transform.SetParent(sysSlotsPanel.transform, false);

            RectTransform scRect = sCard.GetComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0.5f, 1f);
            scRect.anchorMax = new Vector2(0.5f, 1f);
            scRect.pivot = new Vector2(0.5f, 1f);
            scRect.anchoredPosition = styleD ? new Vector2(0f, -6f - (slotIdx * 70f)) : new Vector2(0f, -8f - (slotIdx * 74f));
            scRect.sizeDelta = styleD ? new Vector2(696f, 64f) : new Vector2(710f, 68f);

            sysSlotCardBgs[slotIdx] = sCard.AddComponent<Image>();
            sysSlotCardOutlines[slotIdx] = sCard.AddComponent<Outline>();
            sysSlotCardOutlines[slotIdx].effectColor = new Color(0.25f, 0.32f, 0.42f, 0.65f);
            if (styleD)
            {
                StyleDThemeUtility.ApplySliced(sysSlotCardBgs[slotIdx], StyleDThemeUtility.WellRecessed, 3.2f, Color.white, disableOutline: true);
                StyleDThemeUtility.AddLeftAccentBar(sCard.transform, slotIdx == 0 ? StyleDThemeUtility.ColAmber : StyleDThemeUtility.ColRust, 4f);
            }
            else
            {
                sysSlotCardBgs[slotIdx].color = new Color(0.07f, 0.10f, 0.15f, 0.96f);
            }

            // Dedicated Text Container with RectMask2D to prevent overlaying into buttons
            GameObject textContainer = new GameObject("TextContainer", typeof(RectTransform));
            textContainer.transform.SetParent(sCard.transform, false);

            RectTransform tcRect = textContainer.GetComponent<RectTransform>();
            tcRect.anchorMin = Vector2.zero;
            tcRect.anchorMax = Vector2.one;
            tcRect.offsetMin = new Vector2(16f, 4f);
            tcRect.offsetMax = new Vector2(-188f, -4f);
            textContainer.AddComponent<RectMask2D>();

sysSlotTitles[slotIdx] = CreateSysTMP(textContainer.transform, "SlotTitle", "SLOT", 13f, FontStyles.Normal, Color.white, TextAlignmentOptions.MidlineLeft, sharedFont, FontRole.Body);

sysSlotTitles[slotIdx].overflowMode = styleD ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
if (styleD) sysSlotTitles[slotIdx].gameObject.AddComponent<HorizontalMarqueeText>();

RectTransform stRect = sysSlotTitles[slotIdx].rectTransform;

stRect.anchorMin = new Vector2(0f, 0.5f);

stRect.anchorMax = new Vector2(1f, 1f);

stRect.offsetMin = new Vector2(0f, 2f);

stRect.offsetMax = new Vector2(0f, -2f);

sysSlotDetails[slotIdx] = CreateSysTMP(textContainer.transform, "SlotDetails", "Details", 11.5f, FontStyles.Normal, styleD ? StyleDThemeUtility.ColBone : new Color(0.78f, 0.82f, 0.88f), TextAlignmentOptions.MidlineLeft, sharedFont, FontRole.Body);

sysSlotDetails[slotIdx].overflowMode = styleD ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
if (styleD) sysSlotDetails[slotIdx].gameObject.AddComponent<HorizontalMarqueeText>();

RectTransform sdRect = sysSlotDetails[slotIdx].rectTransform;

sdRect.anchorMin = new Vector2(0f, 0f);

sdRect.anchorMax = new Vector2(1f, 0.5f);

sdRect.offsetMin = new Vector2(0f, 2f);

sdRect.offsetMax = new Vector2(0f, -2f);

// Action Button (SAVE HERE / OVERWRITE / LOAD)

Button actBtn = CreateSysActionButton(sCard.transform, "BtnSlotAction", "<b>SAVE HERE</b>",

new Color(0.06f, 0.24f, 0.14f, 0.98f), new Color(0.28f, 0.95f, 0.58f), sharedFont, out sysSlotActionLabels[slotIdx], styleD);

if (!styleD) actBtn.transition = Selectable.Transition.None;

sysSlotActionLabels[slotIdx].alignment = TextAlignmentOptions.Center;

sysSlotActionLabels[slotIdx].fontSize = 12f;

sysSlotActionLabels[slotIdx].rectTransform.offsetMin = Vector2.zero;

sysSlotActionLabels[slotIdx].rectTransform.offsetMax = Vector2.zero;

RectTransform abRect = actBtn.GetComponent<RectTransform>();

abRect.anchorMin = new Vector2(1f, 0.5f);

abRect.anchorMax = new Vector2(1f, 0.5f);

abRect.pivot = new Vector2(1f, 0.5f);

abRect.anchoredPosition = new Vector2(-48f, 0f);

abRect.sizeDelta = new Vector2(120f, 38f);

actBtn.onClick.AddListener(() => OnSystemSlotActionClicked(slotIdx));

sysSlotActionBtns[slotIdx] = actBtn;

// Delete Slot Button [X]

Button delBtn = CreateSysActionButton(sCard.transform, "BtnSlotDelete", "<b>X</b>",

new Color(0.26f, 0.07f, 0.08f, 0.95f), new Color(0.98f, 0.45f, 0.45f), sharedFont, out TextMeshProUGUI delLbl, styleD);

if (!styleD) delBtn.transition = Selectable.Transition.None;
if (styleD)
{
    StyleDThemeUtility.ApplyMetalButton(
        delBtn,
        StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColRust, 0.65f),
        StyleDThemeUtility.WithRgbScale(StyleDThemeUtility.ColRust, 0.88f),
        2.8f,
        StyleDThemeUtility.ColBone);
}

delLbl.alignment = TextAlignmentOptions.Center;

delLbl.fontSize = 12f;

delLbl.rectTransform.offsetMin = Vector2.zero;

delLbl.rectTransform.offsetMax = Vector2.zero;

RectTransform dbRect = delBtn.GetComponent<RectTransform>();

dbRect.anchorMin = new Vector2(1f, 0.5f);

dbRect.anchorMax = new Vector2(1f, 0.5f);

dbRect.pivot = new Vector2(1f, 0.5f);

dbRect.anchoredPosition = new Vector2(-10f, 0f);

dbRect.sizeDelta = new Vector2(32f, 38f);

delBtn.onClick.AddListener(() => OnSystemSlotDeleteClicked(slotIdx));

sysSlotDeleteBtns[slotIdx] = delBtn;
}

Button slotsBackBtn = CreateSysActionButton(sysSlotsPanel.transform, "BtnSlotsBack",

"<b>[ < BACK TO SYSTEM MENU ]</b>",

new Color(0.12f, 0.15f, 0.20f, 1f), new Color(0.95f, 0.75f, 0.28f), sharedFont, out TextMeshProUGUI slotsBackLbl, styleD);

RectTransform slbRect = slotsBackBtn.GetComponent<RectTransform>();

slbRect.anchorMin = new Vector2(0.5f, 0f);

slbRect.anchorMax = new Vector2(0.5f, 0f);

slbRect.pivot = new Vector2(0.5f, 0f);

slbRect.anchoredPosition = new Vector2(0f, styleD ? 8f : 14f);

slbRect.sizeDelta = new Vector2(styleD ? 696f : 710f, 38f);
if (styleD && slotsBackLbl != null)
{
    slotsBackLbl.alignment = TextAlignmentOptions.Center;
    slotsBackLbl.rectTransform.offsetMin = Vector2.zero;
    slotsBackLbl.rectTransform.offsetMax = Vector2.zero;
}

slotsBackBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Main));

// =========================================================================

// VIEW 2: SETTINGS SUB-WINDOW (Master, BGM, SFX Sliders)

// =========================================================================

sysSettingsPanel = CreateSubPanelRoot(card.transform, "SettingsPanel", styleD);

sysMasterVolSlider = CreateVolumeSliderRow(sysSettingsPanel.transform, "MasterVolumeRow",

"MASTER VOLUME", "Global output level for all game audio", -18f, sharedFont, out sysMasterVolValueText, styleD);

sysMasterVolSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

sysBgmVolSlider = CreateVolumeSliderRow(sysSettingsPanel.transform, "BgmVolumeRow",

"BGM & AMBIENCE", "Background music & apartment atmospheric drone", -96f, sharedFont, out sysBgmVolValueText, styleD);

sysBgmVolSlider.onValueChanged.AddListener(OnBgmVolumeChanged);

sysSfxVolSlider = CreateVolumeSliderRow(sysSettingsPanel.transform, "SfxVolumeRow",

"SFX & BROADCASTS", "UI clicks, emergency broadcasts, radio & alarms", -174f, sharedFont, out sysSfxVolValueText, styleD);

sysSfxVolSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

Button settingsBackBtn = CreateSysActionButton(sysSettingsPanel.transform, "BtnSettingsBack",

"<b>[ < BACK TO SYSTEM MENU ]</b>",

new Color(0.12f, 0.15f, 0.20f, 1f), new Color(0.95f, 0.75f, 0.28f), sharedFont, out TextMeshProUGUI setBackLbl, styleD);

RectTransform sbRect = settingsBackBtn.GetComponent<RectTransform>();

sbRect.anchorMin = new Vector2(0.5f, 0f);

sbRect.anchorMax = new Vector2(0.5f, 0f);

sbRect.pivot = new Vector2(0.5f, 0f);

sbRect.anchoredPosition = new Vector2(0f, 14f);

sbRect.sizeDelta = new Vector2(styleD ? 696f : 512f, 40f);
if (styleD && setBackLbl != null)
{
    setBackLbl.alignment = TextAlignmentOptions.Center;
    setBackLbl.rectTransform.offsetMin = Vector2.zero;
    setBackLbl.rectTransform.offsetMax = Vector2.zero;
}

settingsBackBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Main));

// =========================================================================

// VIEW 3: EXIT GAME CONFIRMATION WINDOW (Unsaved Progress Check)

// =========================================================================

sysExitConfirmPanel = CreateSubPanelRoot(card.transform, "ExitConfirmPanel", styleD);

GameObject warnBox = new GameObject("WarningBox", typeof(RectTransform));
warnBox.transform.SetParent(sysExitConfirmPanel.transform, false);

RectTransform wbRect = warnBox.GetComponent<RectTransform>();
wbRect.anchorMin = new Vector2(0f, 1f);
wbRect.anchorMax = new Vector2(1f, 1f);
wbRect.pivot = new Vector2(0.5f, 1f);
wbRect.anchoredPosition = new Vector2(0f, styleD ? -12f : -14f);
wbRect.sizeDelta = new Vector2(styleD ? -28f : -48f, styleD ? 128f : 132f);

Image wbBg = warnBox.AddComponent<Image>();
Outline wbOl = warnBox.AddComponent<Outline>();
wbOl.effectColor = new Color(0.85f, 0.22f, 0.22f, 0.75f);
if (styleD)
{
    StyleDThemeUtility.ApplySliced(wbBg, StyleDThemeUtility.WellRecessed, 3.2f, Color.white, disableOutline: true);
    StyleDThemeUtility.AddLeftAccentBar(warnBox.transform, StyleDThemeUtility.ColRust, 4f);
}
else
{
    wbBg.color = new Color(0.16f, 0.05f, 0.06f, 0.95f);
}

TextMeshProUGUI warnHeader = CreateSysTMP(warnBox.transform, "WarnHeader",

"[!] UNSAVED PROGRESS WARNING — CONFIRM EXIT",

13.5f, FontStyles.Bold, styleD ? StyleDThemeUtility.ColRust : new Color(0.98f, 0.42f, 0.42f), TextAlignmentOptions.TopLeft, sharedFont, FontRole.Display);

RectTransform whRect = warnHeader.rectTransform;

whRect.anchorMin = Vector2.zero;

whRect.anchorMax = Vector2.one;

whRect.offsetMin = new Vector2(16f, 10f);

whRect.offsetMax = new Vector2(-16f, -12f);

sysExitWarningDetailText = CreateSysTMP(warnBox.transform, "WarnDetails",

"Are you sure you want to exit? Any unsaved progress will be lost.",

12f, FontStyles.Normal, styleD ? StyleDThemeUtility.ColBone : new Color(0.90f, 0.92f, 0.95f), TextAlignmentOptions.TopLeft, sharedFont, FontRole.Body);

sysExitWarningDetailText.textWrappingMode = TextWrappingModes.Normal;

RectTransform wdRect = sysExitWarningDetailText.rectTransform;

wdRect.anchorMin = Vector2.zero;

wdRect.anchorMax = Vector2.one;

wdRect.offsetMin = new Vector2(16f, 10f);

wdRect.offsetMax = new Vector2(-16f, -38f);

// Option A: SAVE & EXIT TO MAIN MENU

string saveExitSub = styleD ? "#6EE7B7" : "#A7F3D0";
Button saveAndExitBtn = CreateSysActionButton(sysExitConfirmPanel.transform, "BtnSaveAndExit",

$"<b>[ SAVE & EXIT TO MAIN MENU ]</b>   <size=11><color={saveExitSub}>Save progress first, then exit</color></size>",

new Color(0.06f, 0.24f, 0.15f, 0.98f), new Color(0.28f, 0.95f, 0.60f), sharedFont, out _, styleD, StyleDThemeUtility.ColSage);

RectTransform saeRect = saveAndExitBtn.GetComponent<RectTransform>();

saeRect.anchorMin = new Vector2(0.5f, 1f);

saeRect.anchorMax = new Vector2(0.5f, 1f);

saeRect.pivot = new Vector2(0.5f, 1f);

saeRect.anchoredPosition = new Vector2(0f, styleD ? -154f : -160f);

saeRect.sizeDelta = new Vector2(styleD ? 696f : 512f, 46f);

saveAndExitBtn.onClick.AddListener(OnSystemMenuSaveAndExitClicked);

// Option B: EXIT WITHOUT SAVING

string exitNoSub = styleD ? "#D95829" : "#FECACA";
Button exitNoSaveBtn = CreateSysActionButton(sysExitConfirmPanel.transform, "BtnExitWithoutSaving",

$"<b>[ EXIT WITHOUT SAVING ]</b>   <size=11><color={exitNoSub}>Discard unsaved changes & return to Title</color></size>",

new Color(0.28f, 0.07f, 0.08f, 0.98f), new Color(0.98f, 0.45f, 0.45f), sharedFont, out _, styleD, StyleDThemeUtility.ColRust);

RectTransform ensRect = exitNoSaveBtn.GetComponent<RectTransform>();

ensRect.anchorMin = new Vector2(0.5f, 1f);

ensRect.anchorMax = new Vector2(0.5f, 1f);

ensRect.pivot = new Vector2(0.5f, 1f);

ensRect.anchoredPosition = new Vector2(0f, styleD ? -210f : -218f);

ensRect.sizeDelta = new Vector2(styleD ? 696f : 512f, 46f);

exitNoSaveBtn.onClick.AddListener(OnSystemMenuExitWithoutSavingClicked);

// Option C: CANCEL (Return to System Menu)

Button cancelExitBtn = CreateSysActionButton(sysExitConfirmPanel.transform, "BtnCancelExit",

"<b>[ CANCEL — RETURN TO GAME ]</b>",

new Color(0.12f, 0.15f, 0.20f, 1f), new Color(0.85f, 0.88f, 0.92f), sharedFont, out TextMeshProUGUI cancelLbl, styleD);

RectTransform ceRect = cancelExitBtn.GetComponent<RectTransform>();

ceRect.anchorMin = new Vector2(0.5f, 0f);

ceRect.anchorMax = new Vector2(0.5f, 0f);

ceRect.pivot = new Vector2(0.5f, 0f);

ceRect.anchoredPosition = new Vector2(0f, 14f);

ceRect.sizeDelta = new Vector2(styleD ? 696f : 512f, 42f);
if (styleD && cancelLbl != null)
{
    cancelLbl.alignment = TextAlignmentOptions.Center;
    cancelLbl.rectTransform.offsetMin = Vector2.zero;
    cancelLbl.rectTransform.offsetMax = Vector2.zero;
}

cancelExitBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Main));
}

private GameObject CreateSubPanelRoot(Transform parent, string name, bool styleD = false)
{
    GameObject panel = new GameObject(name, typeof(RectTransform));
    panel.transform.SetParent(parent, false);
    RectTransform rt = panel.GetComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = styleD ? new Vector2(18f, 18f) : new Vector2(0f, 0f);
    rt.offsetMax = styleD ? new Vector2(-18f, -100f) : new Vector2(0f, -86f);
    return panel;
}

private void PositionMenuRow(RectTransform rt, int index, bool styleD = false)
{
    rt.anchorMin = new Vector2(0.5f, 1f);
    rt.anchorMax = new Vector2(0.5f, 1f);
    rt.pivot = new Vector2(0.5f, 1f);
    rt.anchoredPosition = styleD
        ? new Vector2(0f, -12f - (index * 68f))
        : new Vector2(0f, -18f - (index * 72f));
    rt.sizeDelta = styleD ? new Vector2(696f, 58f) : new Vector2(710f, 60f);
}

private Button CreateSysActionButton(Transform parent, string name, string labelText, Color bgColor, Color borderAndTextColor, TMP_FontAsset font, out TextMeshProUGUI labelOut, bool styleD = false, Color? styleDAccent = null)
{
    GameObject btnObj = new GameObject(name, typeof(RectTransform));
    btnObj.transform.SetParent(parent, false);
    Image bg = btnObj.AddComponent<Image>();
    Outline ol = btnObj.AddComponent<Outline>();
    ol.effectColor = new Color(borderAndTextColor.r, borderAndTextColor.g, borderAndTextColor.b, 0.65f);
    ol.effectDistance = new Vector2(1f, -1f);
    Button btn = btnObj.AddComponent<Button>();
    btn.targetGraphic = bg;

    labelOut = CreateSysTMP(
        btnObj.transform,
        "Label",
        labelText,
        13.5f,
        FontStyles.Normal,
        styleD ? StyleDThemeUtility.ColBone : borderAndTextColor,
        TextAlignmentOptions.MidlineLeft,
        font,
        FontRole.Mono);
    labelOut.raycastTarget = false;
    RectTransform lr = labelOut.rectTransform;
    lr.anchorMin = Vector2.zero;
    lr.anchorMax = Vector2.one;
    lr.offsetMin = new Vector2(18f, 0f);
    lr.offsetMax = new Vector2(-18f, 0f);

    if (styleD)
    {
        StyleDThemeUtility.ApplyMetalButton(btn, StyleDThemeUtility.ColMetalBtn, StyleDThemeUtility.ColMetalHover, 2.8f, StyleDThemeUtility.ColBone);
        if (styleDAccent.HasValue)
        {
            StyleDThemeUtility.AddLeftAccentBar(btnObj.transform, styleDAccent.Value, 4f);
        }
    }
    else
    {
        bg.color = bgColor;
    }

    return btn;
}

private Slider CreateVolumeSliderRow(Transform parent, string name, string title, string subtitle, float topOffset, TMP_FontAsset font, out TextMeshProUGUI pctLabel, bool styleD = false)
{
    GameObject row = new GameObject(name, typeof(RectTransform));
    row.transform.SetParent(parent, false);
    RectTransform rr = row.GetComponent<RectTransform>();
    rr.anchorMin = new Vector2(0.5f, 1f);
    rr.anchorMax = new Vector2(0.5f, 1f);
    rr.pivot = new Vector2(0.5f, 1f);
    rr.anchoredPosition = new Vector2(0f, topOffset);
    rr.sizeDelta = new Vector2(styleD ? 696f : 512f, 68f);

    Image rowBg = row.AddComponent<Image>();
    Outline rowOl = row.AddComponent<Outline>();
    rowOl.effectColor = new Color(0.22f, 0.28f, 0.36f, 0.7f);
    if (styleD)
    {
        StyleDThemeUtility.ApplySliced(rowBg, StyleDThemeUtility.WellRecessed, 3.2f, Color.white, disableOutline: true);
    }
    else
    {
        rowBg.color = new Color(0.08f, 0.10f, 0.14f, 0.95f);
    }

    string subHex = styleD ? "#8C9486" : "#64748B";
    TextMeshProUGUI titleTmp = CreateSysTMP(row.transform, "Title", $"<b>{title}</b>  <size=10.5><color={subHex}>{subtitle}</color></size>",
        12.5f, FontStyles.Normal, styleD ? StyleDThemeUtility.ColBone : new Color(0.92f, 0.95f, 0.98f), TextAlignmentOptions.MidlineLeft, font, FontRole.Body);
    RectTransform tr = titleTmp.rectTransform;
    tr.anchorMin = new Vector2(0f, 0.5f);
    tr.anchorMax = new Vector2(1f, 1f);
    tr.offsetMin = new Vector2(14f, 2f);
    tr.offsetMax = new Vector2(-70f, -4f);

    pctLabel = CreateSysTMP(row.transform, "PctValue", "100%", 13f, FontStyles.Bold,
        styleD ? StyleDThemeUtility.ColAmber : new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.MidlineRight, font, FontRole.Mono);
    RectTransform pr = pctLabel.rectTransform;
    pr.anchorMin = new Vector2(1f, 0.5f);
    pr.anchorMax = new Vector2(1f, 1f);
    pr.pivot = new Vector2(1f, 1f);
    pr.offsetMin = new Vector2(-70f, 2f);
    pr.offsetMax = new Vector2(-14f, -4f);

    // Interactive Slider Control
    GameObject sliderObj = new GameObject("Slider", typeof(RectTransform));
    sliderObj.transform.SetParent(row.transform, false);
    RectTransform sr = sliderObj.GetComponent<RectTransform>();
    sr.anchorMin = new Vector2(0f, 0f);
    sr.anchorMax = new Vector2(1f, 0.5f);
    sr.offsetMin = new Vector2(14f, 10f);
    sr.offsetMax = new Vector2(-14f, -4f);

    GameObject bgTrack = new GameObject("Background", typeof(RectTransform));
    bgTrack.transform.SetParent(sliderObj.transform, false);
    RectTransform bgr = bgTrack.GetComponent<RectTransform>();
    bgr.anchorMin = new Vector2(0f, 0.25f);
    bgr.anchorMax = new Vector2(1f, 0.75f);
    bgr.offsetMin = Vector2.zero;
    bgr.offsetMax = Vector2.zero;
    Image bgImg = bgTrack.AddComponent<Image>();
    bgImg.color = styleD ? new Color(0.05f, 0.055f, 0.06f, 1f) : new Color(0.03f, 0.04f, 0.06f, 1f);

    GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
    fillArea.transform.SetParent(sliderObj.transform, false);
    RectTransform far = fillArea.GetComponent<RectTransform>();
    far.anchorMin = new Vector2(0f, 0.25f);
    far.anchorMax = new Vector2(1f, 0.75f);
    far.offsetMin = Vector2.zero;
    far.offsetMax = Vector2.zero;

    GameObject fill = new GameObject("Fill", typeof(RectTransform));
    fill.transform.SetParent(fillArea.transform, false);
    RectTransform fr = fill.GetComponent<RectTransform>();
    fr.anchorMin = Vector2.zero;
    fr.anchorMax = Vector2.one;
    fr.offsetMin = Vector2.zero;
    fr.offsetMax = Vector2.zero;
    Image fillImg = fill.AddComponent<Image>();
    fillImg.color = styleD ? StyleDThemeUtility.ColAmber : new Color(0.95f, 0.62f, 0.08f, 0.95f);

    GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
    handleArea.transform.SetParent(sliderObj.transform, false);
    RectTransform har = handleArea.GetComponent<RectTransform>();
    har.anchorMin = Vector2.zero;
    har.anchorMax = Vector2.one;
    har.offsetMin = new Vector2(6f, 0f);
    har.offsetMax = new Vector2(-6f, 0f);

    GameObject handle = new GameObject("Handle", typeof(RectTransform));
    handle.transform.SetParent(handleArea.transform, false);
    RectTransform hr = handle.GetComponent<RectTransform>();
    hr.sizeDelta = new Vector2(14f, 20f);
    Image handleImg = handle.AddComponent<Image>();
    handleImg.color = styleD ? StyleDThemeUtility.ColBone : new Color(0.98f, 0.98f, 1f, 1f);

    Slider slider = sliderObj.AddComponent<Slider>();
    slider.minValue = 0f;
    slider.maxValue = 1f;
    slider.wholeNumbers = false;
    slider.fillRect = fr;
    slider.handleRect = hr;
    slider.targetGraphic = handleImg;
    slider.direction = Slider.Direction.LeftToRight;
    return slider;
}

private TextMeshProUGUI CreateSysTMP(Transform parent, string name, string text, float size, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font, FontRole fallbackRole = FontRole.Body)
{
    GameObject obj = new GameObject(name, typeof(RectTransform));
    obj.transform.SetParent(parent, false);
    TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
    if (font != null) tmp.font = font;
    else SundayTypography.ApplyFont(tmp, fallbackRole);
    tmp.text = text;
    tmp.fontSize = size;
    tmp.fontStyle = style;
    tmp.color = color;
    tmp.alignment = align;
    tmp.textWrappingMode = TextWrappingModes.NoWrap;
    return tmp;
}

private void HandleSystemMenuModalInput()

{
bool esc = false;

bool spaceOrEnter = false;

int numPressed = -1;

if (Keyboard.current != null)

{
var kb = Keyboard.current;

esc = kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame;

spaceOrEnter = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;

if (kb.digit0Key.wasPressedThisFrame || kb.numpad0Key.wasPressedThisFrame) numPressed = 0;

else if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) numPressed = 1;

else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) numPressed = 2;

else if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) numPressed = 3;

else if (kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame) numPressed = 4;
}

else

{
try

{
esc = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace);

spaceOrEnter = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) numPressed = 0;

else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) numPressed = 1;

else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) numPressed = 2;

else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) numPressed = 3;

else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) numPressed = 4;
}

catch (System.InvalidOperationException) { }
}

switch (currentSysView)

{
case SystemMenuView.Main:

if (numPressed == 1) OnSystemMenuSaveClicked();

else if (numPressed == 2) OnSystemMenuLoadClicked();

else if (numPressed == 3) SwitchSystemMenuView(SystemMenuView.Settings);

else if (numPressed == 4) SwitchSystemMenuView(SystemMenuView.ExitConfirm);

else if (esc || spaceOrEnter) Close();

break;

case SystemMenuView.SaveSlots:

case SystemMenuView.LoadSlots:

if (numPressed >= 0 && numPressed < SaveManager.TotalSlotCount)

{
OnSystemSlotActionClicked(numPressed);
}

else if (esc)

{
SwitchSystemMenuView(SystemMenuView.Main);
}

break;

case SystemMenuView.Settings:

if (esc) SwitchSystemMenuView(SystemMenuView.Main);

break;

case SystemMenuView.ExitConfirm:

if (numPressed == 1) OnSystemMenuSaveAndExitClicked();

else if (numPressed == 2) OnSystemMenuExitWithoutSavingClicked();

else if (esc || numPressed == 3) SwitchSystemMenuView(SystemMenuView.Main);

break;
}
}


        private void Update()
        {
            if (gameObject.activeSelf)
            {
                HandleSystemMenuModalInput();
            }
        }
    }
}
