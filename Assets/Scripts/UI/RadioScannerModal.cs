using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Radio Transceiver / Police Scanner Modal.
    /// Manages encrypted VHF frequency scanning, waveform oscilloscope animations,
    /// and synchronized police broadcast readouts.
    /// </summary>
    public class RadioScannerModal : MonoBehaviour
    {
        [Header("Stage Reference")]
        [SerializeField] private GameStageScreen stage;

        [Header("Audio Components")]
        [SerializeField] private AudioSource radioSource;
        [SerializeField] private AudioClip policeRadioClip;

        [Header("UI Text Displays")]
        [SerializeField] private TextMeshProUGUI radioText;
        [SerializeField] private TextMeshProUGUI radioSignalMeterText;
        [SerializeField] private TextMeshProUGUI radioFreqHeaderText;
        [SerializeField] private TextMeshProUGUI radioTranscriptText;
        [SerializeField] private GameObject radioOutroObj;

        private Coroutine radioRoutine;
        private Coroutine radioMeterRoutine;
        private bool isRadioTransmitting = false;

        public bool IsTransmitting => isRadioTransmitting;

        public void Initialize(GameStageScreen stageController)
        {
            stage = stageController;
            if (radioSource == null)
            {
                radioSource = GetComponent<AudioSource>();
                if (radioSource == null && stage != null) radioSource = stage.RadioSource;
                if (radioSource == null)
                {
                    radioSource = gameObject.AddComponent<AudioSource>();
                    radioSource.playOnAwake = false;
                }
            }
            if (policeRadioClip == null && stage != null)
            {
                policeRadioClip = stage.PoliceRadioClip;
            }

            Transform radioPanel = transform.Find("RadioPanel") ?? transform;
            if (radioFreqHeaderText == null)
            {
                var fh = radioPanel.Find("FreqHeader");
                if (fh != null) radioFreqHeaderText = fh.GetComponent<TextMeshProUGUI>();
            }
            if (radioSignalMeterText == null)
            {
                var sm = radioPanel.Find("SignalMeterBar");
                if (sm != null) radioSignalMeterText = sm.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (radioTranscriptText == null)
            {
                var tr = radioPanel.Find("Transcript");
                if (tr != null) radioTranscriptText = tr.GetComponent<TextMeshProUGUI>();
            }
            if (radioOutroObj == null)
            {
                var outro = radioPanel.Find("Outro");
                if (outro != null) radioOutroObj = outro.gameObject;
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

        private void PlayScreenShake()
        {
            if (stage != null) stage.PlayScreenShake();
        }

        public void Open()
        {
            PlayClickSFX();
            var player = stage != null ? stage.CurrentPlayer : null;
            bool isPolice = player != null && string.Equals(player.job, "Police Officer", StringComparison.OrdinalIgnoreCase);
            if (stage != null && stage.Clock != null) stage.Clock.AddTime(isPolice ? 3 : 5);
            if (stage != null) stage.RadioVisited = true;

            gameObject.SetActive(true);

            if (radioSource != null && policeRadioClip != null)
            {
                radioSource.clip = policeRadioClip;
                radioSource.volume = 0.9f;
                radioSource.Play();
            }

            if (radioRoutine != null) StopCoroutine(radioRoutine);
            radioRoutine = StartCoroutine(RadioTransmissionRoutine());

            if (radioMeterRoutine != null) StopCoroutine(radioMeterRoutine);
            radioMeterRoutine = StartCoroutine(RadioWaveformMeterRoutine());
        }

        public void Close()
        {
            PlayClickSFX();
            isRadioTransmitting = false;

            if (radioRoutine != null) StopCoroutine(radioRoutine);
            if (radioMeterRoutine != null) StopCoroutine(radioMeterRoutine);
            if (radioSource != null) radioSource.Stop();

            gameObject.SetActive(false);

            if (stage != null)
            {
                if (stage.Lore != null)
                {
                    stage.Lore.AddLore(
                        "Police Radio Intercept (Midnight)",
                        "Police units at 5th Street lost their barricade and reported suspects ignoring chest shots. Dispatch suspended standard rules of engagement, ordered officers to aim for the head, and warned them to avoid all physical contact.",
                        stage.Clock != null ? stage.Clock.GetTimeString() : "23:45"
                    );
                }

                if (stage.CurrentPlayer != null)
                {
                    stage.CurrentPlayer.SetFlag("listened_radio_407", true);
                }

                stage.SetSituationNarration("<color=#EF4444>[POLICE FREQUENCY LOGGED]</color> Over the scanner, an officer on 5th Street yells that chest shots aren't stopping whoever's rushing the barricade. Dispatch tells all units to aim for the head before the channel drops to dead air.");
            }
        }

        private IEnumerator RadioWaveformMeterRoutine()
        {
            string[] wavePatterns = new string[]
            {
                "|| ||||||| |||| ||||||||| ||| ||||||",
                "||||| ||| |||||||| || ||||||| ||||||",
                "||| ||||||||| |||| ||||| |||||||| ||",
                "||||||| || |||||| ||||||||| |||| |||",
                "|||| |||||||| ||||||| || ||||| |||||"
            };

            int tick = 0;
            while (isRadioTransmitting && gameObject.activeSelf)
            {
                if (radioFreqHeaderText != null)
                {
                    string recDot = (tick % 2 == 0)
                        ? "<color=#ef4444>[LIVE RX]</color>"
                        : "<color=#7f1d1d>[LIVE RX]</color>";
                    radioFreqHeaderText.text = $"FREQ: 154.790 MHz // SEATTLE PD DISPATCH [CH-1]          {recDot}";
                }

                if (radioSignalMeterText != null)
                {
                    int dbm = -54 - UnityEngine.Random.Range(0, 12);
                    string wave = wavePatterns[tick % wavePatterns.Length];
                    radioSignalMeterText.text = $"<color=#34d399><b>RF SIGNAL:</b></color> <color=#10b981>[ {wave} ]</color>  <color=#94a3b8>{dbm} dBm // VHF ENCRYPTED</color>";
                }

                tick++;
                yield return new WaitForSecondsRealtime(0.18f);
            }

            if (radioFreqHeaderText != null)
            {
                radioFreqHeaderText.text = "FREQ: 154.790 MHz // SEATTLE PD DISPATCH [CH-1]          <color=#ef4444>[X] CARRIER LOST</color>";
            }

            if (radioSignalMeterText != null)
            {
                radioSignalMeterText.text = "<color=#ef4444><b>RF SIGNAL:</b> [ ------------------------------------ ]  NO CARRIER // DEAD AIR</color>";
            }
        }

        private float GetRadioSyncTime(float startRealtime, float maxObservedTime)
        {
            if (radioSource != null && radioSource.isPlaying && radioSource.time > 0.001f)
            {
                return Mathf.Max(maxObservedTime, radioSource.time);
            }
            return Mathf.Max(maxObservedTime, Time.realtimeSinceStartup - startRealtime);
        }

        private IEnumerator WaitUntilRadioTime(float targetTime, float startRealtime, System.Action<float> onUpdateTime, System.Func<float> getMaxTime)
        {
            while (isRadioTransmitting)
            {
                float t = GetRadioSyncTime(startRealtime, getMaxTime());
                onUpdateTime(t);
                if (t >= targetTime) yield break;
                yield return null;
            }
        }

        private IEnumerator TypeRichTextSegmentSynced(
            System.Text.StringBuilder sb,
            string segment,
            float startSec,
            float endSec,
            float startRealtime,
            System.Action<float> onUpdateTime,
            System.Func<float> getMaxTime)
        {
            yield return WaitUntilRadioTime(startSec, startRealtime, onUpdateTime, getMaxTime);
            if (!isRadioTransmitting) yield break;

            string prefix = sb.ToString();
            var visibleEnds = new System.Collections.Generic.List<int>(segment.Length);

            for (int i = 0; i < segment.Length; i++)
            {
                if (segment[i] == '<')
                {
                    int closeIdx = segment.IndexOf('>', i);
                    if (closeIdx >= 0)
                    {
                        i = closeIdx;
                        continue;
                    }
                }
                visibleEnds.Add(i + 1);
            }

            float duration = Mathf.Max(0.05f, endSec - startSec);
            while (isRadioTransmitting)
            {
                float t = GetRadioSyncTime(startRealtime, getMaxTime());
                onUpdateTime(t);
                float progress = Mathf.Clamp01((t - startSec) / duration);
                int count = Mathf.CeilToInt(progress * visibleEnds.Count);

                if (radioTranscriptText != null)
                {
                    string typed = count <= 0 ? "" : segment.Substring(0, visibleEnds[count - 1]);
                    radioTranscriptText.text = prefix + typed;
                }

                if (progress >= 1f) break;
                yield return null;
            }

            sb.Append(segment);
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();
        }

        private IEnumerator RadioTransmissionRoutine()
        {
            isRadioTransmitting = true;
            float startRealtime = Time.realtimeSinceStartup;
            float maxObservedTime = 0f;

            if (radioOutroObj != null)
            {
                radioOutroObj.SetActive(false);
            }

            PlayBeepSFX();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<color=#94A3B8>[STATIC HISS]</color>\n");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            // Audio cues in police_radio.mp3 (36.13s):
            // 1.55s–9.05s: Unit 4-Charlie first burst
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            yield return TypeRichTextSegmentSynced(
                sb,
                "<i>\"Dispatch, 4-Charlie! We've lost the 5th Street barricade! They are not stopping! I repeat, shooting them in the chest does nothing!\"</i>\n\n",
                1.55f, 9.05f, startRealtime, t => maxObservedTime = t, () => maxObservedTime
            );
            if (!isRadioTransmitting) yield break;

            // 9.85s: Heavy slam against cruiser + shout
            yield return WaitUntilRadioTime(9.85f, startRealtime, t => maxObservedTime = t, () => maxObservedTime);
            if (!isRadioTransmitting) yield break;

            PlayScreenShake();
            sb.AppendLine("<color=#EF4444><b>[HEAVY STRUGGLE / THUD]</b></color>\n");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            // 10.05s–11.65s: "Get off the damn car!" -> 13.25s–18.10s: "Dispatch, standard protocol is useless!..."
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            yield return TypeRichTextSegmentSynced(
                sb,
                "<i>\"Get off the damn car! ",
                10.05f, 11.65f, startRealtime, t => maxObservedTime = t, () => maxObservedTime
            );
            if (!isRadioTransmitting) yield break;

            yield return TypeRichTextSegmentSynced(
                sb,
                "Dispatch, standard protocol is useless! Requesting authorization for unrestricted lethal force!\"</i>\n\n",
                13.25f, 18.10f, startRealtime, t => maxObservedTime = t, () => maxObservedTime
            );
            if (!isRadioTransmitting) yield break;

            // 18.95s: Radio Squelch bursts
            yield return WaitUntilRadioTime(18.95f, startRealtime, t => maxObservedTime = t, () => maxObservedTime);
            if (!isRadioTransmitting) yield break;

            sb.AppendLine("<color=#F59E0B><b>[RADIO SQUELCH]</b></color>\n");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            // 22.15s–35.55s: Dispatch authorization
            sb.AppendLine("<b><color=#F59E0B>DISPATCH:</color></b>");
            yield return TypeRichTextSegmentSynced(
                sb,
                "<i>\"4-Charlie, unrestricted lethal force is authorized. All units, listen carefully. Standard rules of engagement are suspended. Body shots will not stop them. You must destroy the brain. <color=#EF4444>Aim for the head.</color> I repeat... <color=#EF4444>aim for the head.</color> Avoid all physical contact.\"</i>\n\n",
                22.15f, 35.55f, startRealtime, t => maxObservedTime = t, () => maxObservedTime
            );
            if (!isRadioTransmitting) yield break;

            // 35.75s: Signal Cuts Out
            yield return WaitUntilRadioTime(35.75f, startRealtime, t => maxObservedTime = t, () => maxObservedTime);
            if (!isRadioTransmitting) yield break;

            isRadioTransmitting = false;
            PlayBeepSFX();
            sb.AppendLine("<color=#EF4444><b>[SIGNAL CUTS OUT]</b></color>");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            if (radioOutroObj != null)
            {
                radioOutroObj.SetActive(true);
            }
        }

        /// <summary>
        /// Fast-forwards the radio transmission to full transcript if the player presses Space or clicks.
        /// </summary>
        public void FastForwardRadio()
        {
            if (!isRadioTransmitting) return;

            isRadioTransmitting = false;
            if (radioRoutine != null) StopCoroutine(radioRoutine);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<color=#94A3B8>[STATIC HISS]</color>\n");
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            sb.AppendLine("<i>\"Dispatch, 4-Charlie! We've lost the 5th Street barricade! They are not stopping! I repeat, shooting them in the chest does nothing!\"</i>\n");
            sb.AppendLine("<color=#EF4444><b>[HEAVY STRUGGLE / THUD]</b></color>\n");
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            sb.AppendLine("<i>\"Get off the damn car! Dispatch, standard protocol is useless! Requesting authorization for unrestricted lethal force!\"</i>\n");
            sb.AppendLine("<color=#F59E0B><b>[RADIO SQUELCH]</b></color>\n");
            sb.AppendLine("<b><color=#F59E0B>DISPATCH:</color></b>");
            sb.AppendLine("<i>\"4-Charlie, unrestricted lethal force is authorized. All units, listen carefully. Standard rules of engagement are suspended. Body shots will not stop them. You must destroy the brain. <color=#EF4444>Aim for the head.</color> I repeat... <color=#EF4444>aim for the head.</color> Avoid all physical contact.\"</i>\n");
            sb.AppendLine("<color=#EF4444><b>[SIGNAL CUTS OUT]</b></color>");

            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();
            if (radioOutroObj != null)
            {
                radioOutroObj.SetActive(true);
            }
        }
    }
}
