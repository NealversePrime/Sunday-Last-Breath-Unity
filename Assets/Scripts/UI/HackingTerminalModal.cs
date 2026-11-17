using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Laptop Terminal Decryption Modal.
    /// Manages green monospace command line readouts, blinking LED activities,
    /// and pulling municipal emergency memo dispatches.
    /// </summary>
    public class HackingTerminalModal : MonoBehaviour
    {
        [Header("Stage Reference")]
        [SerializeField] private GameStageScreen stage;

        [Header("UI Displays")]
        [SerializeField] private TextMeshProUGUI hackingTerminalText;
        [SerializeField] private TextMeshProUGUI hackingLedText;
        [SerializeField] private GameObject hackingFinishContainer;

        private Coroutine hackingRoutine;
        private Coroutine hackingLedRoutine;
        private bool isHackingTerminal = false;

        public bool IsHacking => isHackingTerminal;

        public void Initialize(GameStageScreen stageController)
        {
            stage = stageController;
            Transform hackPanel = transform.Find("HackingPanel") ?? transform;
            if (hackingLedText == null)
            {
                var tb = hackPanel.Find("LaptopTopBezel/Leds");
                if (tb != null) hackingLedText = tb.GetComponent<TextMeshProUGUI>();
            }
            if (hackingTerminalText == null)
            {
                var tt = hackPanel.Find("LaptopScreen/TerminalText") ?? hackPanel.Find("TerminalText");
                if (tt != null) hackingTerminalText = tt.GetComponent<TextMeshProUGUI>();
            }
            if (hackingFinishContainer == null)
            {
                var fc = hackPanel.Find("FinishContainer") ?? hackPanel.Find("LaptopScreen/FinishContainer");
                if (fc != null) hackingFinishContainer = fc.gameObject;
            }
        }

        private void PlayClickSFX()
        {
            if (stage != null) stage.PlayClickSFX();
        }

        private void PlayBeepSFX()
        {
            if (stage != null) stage.PlayBeepSFX();
        }

        public void Open()
        {
            PlayClickSFX();
            if (stage != null && stage.Clock != null) stage.Clock.AddTime(8);
            if (stage != null) stage.HackingVisited = true;

            gameObject.SetActive(true);

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(false);
            }

            isHackingTerminal = true;
            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            hackingRoutine = StartCoroutine(HackingTerminalRoutine());

            if (hackingLedRoutine != null) StopCoroutine(hackingLedRoutine);
            hackingLedRoutine = StartCoroutine(HackingLedRoutine());
        }

        public void Close()
        {
            PlayClickSFX();
            isHackingTerminal = false;

            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            if (hackingLedRoutine != null) StopCoroutine(hackingLedRoutine);

            gameObject.SetActive(false);

            if (stage != null)
            {
                if (stage.CurrentPlayer != null)
                {
                    stage.CurrentPlayer.AddItem("Tier-1 Military Clearance Passkey", 1);
                    stage.CurrentPlayer.SetFlag("hacked_terminal_407", true);
                }

                if (stage.Lore != null)
                {
                    stage.Lore.AddLore(
                        "Cached City Traffic (Midnight)",
                        "Pulled cached municipal memos before the fiber line died. Downtown and 5th St Hospital are locked down under 'Code Amber-9', and freeway ramps and bridges are closing by 01:00. Saved a valid checkpoint passkey to my phone.",
                        stage.Clock != null ? stage.Clock.GetTimeString() : "23:45"
                    );
                }

                stage.SetSituationNarration("<color=#EF4444>[CACHED DISPATCH PULLED]</color> You pull a few cached city memos before the connection dies completely. Downtown and 5th Street are locked down, and bridges are closing by 01:00. You grab the attached municipal passkey and shut the laptop—if you're leaving this neighborhood, it has to be now.");
                stage.RefreshAllPDAPanels();
            }
        }

        private IEnumerator HackingLedRoutine()
        {
            int tick = 0;
            while (isHackingTerminal && gameObject.activeSelf)
            {
                if (hackingLedText != null)
                {
                    string hddColor = (tick % 2 == 0) ? "#f59e0b" : "#78350f";
                    string netColor = (tick % 3 == 0) ? "#38bdf8" : "#0284c7";
                    hackingLedText.text = $"<color=#10b981>[PWR]</color>   <color={hddColor}>[HDD]</color>   <color={netColor}>[NET]</color>";
                }
                tick++;
                yield return new WaitForSecondsRealtime(0.12f);
            }

            if (hackingLedText != null)
            {
                hackingLedText.text = "<color=#10b981>[PWR]</color>   <color=#78350f>[HDD]</color>   <color=#ef4444>[NET SEVERED]</color>";
            }
        }

        private IEnumerator HackingTerminalRoutine()
        {
            isHackingTerminal = true;
            if (hackingFinishContainer != null) hackingFinishContainer.SetActive(false);

            string[] hackingTextLines = new string[]
            {
                "<color=#64748b>>_ terminal@unit407: scanning local building gateway...</color>",
                "<color=#38bdf8>[*] SUBNET 192.168.1.1 — SNIFFING CITY EMERGENCY RELAY...</color>",
                "<color=#10b981>[+] LOCAL ROUTER CACHE DUMPED... [OK]</color>",
                "<color=#f59e0b>[*] READING LAST CACHED MUNICIPAL DISPATCH PACKETS...</color>",
                "",
                "<color=#ef4444><b>================== [ CACHED CITY EMERGENCY MEMO ] ==================</b></color>",
                "<color=#f1f5f9><b>INCIDENT CODE  :</b></color> <color=#38bdf8>AMBER-9 (UNCONTROLLED MASS VIOLENCE)</color>",
                "<color=#f1f5f9><b>AFFECTED ZONES :</b></color> <color=#f59e0b>5TH ST HOSPITAL // DOWNTOWN CORRIDOR // I-5 RAMPS</color>",
                "<color=#f1f5f9><b>MEDICAL NOTE   :</b></color> <color=#f59e0b>SUSPECTED ACUTE CONTAGION — BITING / EXTREME AGITATION</color>",
                "<color=#f1f5f9><b>TRAFFIC STATUS :</b></color> <color=#ef4444>BRIDGES & FREEWAY ON-RAMPS CLOSING BY 0100</color>",
                "<color=#f1f5f9><b>DIRECTIVE      :</b></color> <color=#ef4444><b>HOLD IN PLACE UNTIL FURTHER COUNTY ORDERS.</b></color>",
                "<color=#f1f5f9><b>COUNTY STAGING :</b></color> <color=#10b981>EASTBOUND ROUTES / TIMBERLINE CORRIDOR (UNCONFIRMED)</color>",
                "<color=#ef4444><b>====================================================================</b></color>",
                "",
                "<color=#10b981>[+] CACHED ROAD CLOSURE LIST SAVED (ROAD_STATUS.DAT).</color>",
                "<color=#ffd700><b>[+] FOUND UNREVOKED MUNICIPAL GATE PASSKEY... SAVED TO PHONE!</b></color>",
                "",
                "<color=#ef4444><b>[!] UPSTREAM GATEWAY TIMED OUT. NO CARRIER ON WAN PORT.</b></color>",
                "<color=#94a3b8>[X] CONNECTION DROPPED. FIBER LINE DEAD.</color>"
            };

            if (hackingTerminalText != null) hackingTerminalText.text = "";
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hackingTextLines.Length; i++)
            {
                if (!isHackingTerminal) yield break;
                sb.AppendLine(hackingTextLines[i]);
                if (hackingTerminalText != null) hackingTerminalText.text = sb.ToString();
                PlayBeepSFX();
                yield return new WaitForSecondsRealtime(0.18f);
            }

            isHackingTerminal = false;
            PlayBeepSFX();
            yield return new WaitForSecondsRealtime(0.35f);

            if (hackingFinishContainer != null) hackingFinishContainer.SetActive(true);
        }

        public void FastForwardHacking()
        {
            if (!isHackingTerminal) return;
            isHackingTerminal = false;

            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            if (hackingLedRoutine != null) StopCoroutine(hackingLedRoutine);

            if (hackingLedText != null)
            {
                hackingLedText.text = "<color=#10b981>[PWR]</color>   <color=#78350f>[HDD]</color>   <color=#ef4444>[NET SEVERED]</color>";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<color=#64748b>>_ terminal@unit407: scanning local building gateway...</color>");
            sb.AppendLine("<color=#38bdf8>[*] SUBNET 192.168.1.1 — SNIFFING CITY EMERGENCY RELAY...</color>");
            sb.AppendLine("<color=#10b981>[+] LOCAL ROUTER CACHE DUMPED... [OK]</color>");
            sb.AppendLine("<color=#f59e0b>[*] READING LAST CACHED MUNICIPAL DISPATCH PACKETS...</color>");
            sb.AppendLine("");
            sb.AppendLine("<color=#ef4444><b>================== [ CACHED CITY EMERGENCY MEMO ] ==================</b></color>");
            sb.AppendLine("<color=#f1f5f9><b>INCIDENT CODE  :</b></color> <color=#38bdf8>AMBER-9 (UNCONTROLLED MASS VIOLENCE)</color>");
            sb.AppendLine("<color=#f1f5f9><b>AFFECTED ZONES :</b></color> <color=#f59e0b>5TH ST HOSPITAL // DOWNTOWN CORRIDOR // I-5 RAMPS</color>");
            sb.AppendLine("<color=#f1f5f9><b>MEDICAL NOTE   :</b></color> <color=#f59e0b>SUSPECTED ACUTE CONTAGION — BITING / EXTREME AGITATION</color>");
            sb.AppendLine("<color=#f1f5f9><b>TRAFFIC STATUS :</b></color> <color=#ef4444>BRIDGES & FREEWAY ON-RAMPS CLOSING BY 0100</color>");
            sb.AppendLine("<color=#f1f5f9><b>DIRECTIVE      :</b></color> <color=#ef4444><b>HOLD IN PLACE UNTIL FURTHER COUNTY ORDERS.</b></color>");
            sb.AppendLine("<color=#f1f5f9><b>COUNTY STAGING :</b></color> <color=#10b981>EASTBOUND ROUTES / TIMBERLINE CORRIDOR (UNCONFIRMED)</color>");
            sb.AppendLine("<color=#ef4444><b>====================================================================</b></color>");
            sb.AppendLine("");
            sb.AppendLine("<color=#10b981>[+] CACHED ROAD CLOSURE LIST SAVED (ROAD_STATUS.DAT).</color>");
            sb.AppendLine("<color=#ffd700><b>[+] FOUND UNREVOKED MUNICIPAL GATE PASSKEY... SAVED TO PHONE!</b></color>");
            sb.AppendLine("");
            sb.AppendLine("<color=#ef4444><b>[!] UPSTREAM GATEWAY TIMED OUT. NO CARRIER ON WAN PORT.</b></color>");
            sb.AppendLine("<color=#94a3b8>[X] CONNECTION DROPPED. FIBER LINE DEAD.</color>");

            if (hackingTerminalText != null) hackingTerminalText.text = sb.ToString();
            if (hackingFinishContainer != null) hackingFinishContainer.SetActive(true);
        }
    }
}
