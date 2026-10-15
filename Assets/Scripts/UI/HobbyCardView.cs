using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    public class HobbyCardView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsPreviewText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image cardBorder;
        [SerializeField] private Image topAccentBar;
        [SerializeField] private Image statsBg;
        [SerializeField] private TextMeshProUGUI buttonText;

        public void Setup(HobbyData data, Action onSelect)
        {
            if (titleText != null) titleText.text = data.title;
            if (descriptionText != null) descriptionText.text = data.description;
            if (statsPreviewText != null) statsPreviewText.text = data.statsPreview;

            // Weakness card styling (Red border, red accents, red stats text)
            if (data.isWeakness)
            {
                if (cardBorder != null) cardBorder.color = new Color(0.35f, 0.12f, 0.12f, 1f);
                if (topAccentBar != null) topAccentBar.color = new Color(0.85f, 0.18f, 0.18f, 1f);
                if (titleText != null) titleText.color = new Color(0.95f, 0.32f, 0.32f);
                if (statsPreviewText != null) statsPreviewText.color = new Color(0.95f, 0.35f, 0.35f);
                if (statsBg != null) statsBg.color = new Color(0.25f, 0.05f, 0.05f, 0.6f);

                if (selectButton != null)
                {
                    ColorBlock cb = selectButton.colors;
                    cb.normalColor = new Color(0.12f, 0.05f, 0.05f, 0.98f);
                    cb.highlightedColor = new Color(0.6f, 0.15f, 0.15f, 1f);
                    cb.pressedColor = new Color(0.4f, 0.08f, 0.08f, 1f);
                    cb.selectedColor = cb.normalColor;
                    selectButton.colors = cb;
                }
                if (buttonText != null) buttonText.text = "ACCEPT FLAW";
            }
            else
            {
                if (cardBorder != null) cardBorder.color = new Color(0.18f, 0.18f, 0.18f, 1f);
                if (topAccentBar != null) topAccentBar.color = new Color(0.83f, 0.33f, 0f, 1f); // Gold/Orange
                if (titleText != null) titleText.color = Color.white;
                if (statsPreviewText != null) statsPreviewText.color = new Color(0.36f, 0.70f, 1.0f); // Bright blue/cyan
                if (statsBg != null) statsBg.color = new Color(0.04f, 0.08f, 0.14f, 0.6f);

                if (selectButton != null)
                {
                    ColorBlock cb = selectButton.colors;
                    cb.normalColor = new Color(0.09f, 0.09f, 0.09f, 0.98f);
                    cb.highlightedColor = new Color(0.70f, 0.30f, 0.05f, 1f);
                    cb.pressedColor = new Color(0.50f, 0.20f, 0.02f, 1f);
                    cb.selectedColor = cb.normalColor;
                    selectButton.colors = cb;
                }
                if (buttonText != null) buttonText.text = "SELECT";
            }

            // Load Sprite from Assets/Sprites/Traits/trait_{id}.png
            string spritePath = $"Assets/Sprites/Traits/trait_{data.id}.png";
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (s != null && iconImage != null)
            {
                iconImage.sprite = s;
                iconImage.color = data.isWeakness 
                    ? new Color(1f, 0.85f, 0.85f, 0.95f) 
                    : new Color(0.92f, 0.92f, 0.92f, 0.95f);
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
#endif

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => onSelect?.Invoke());
            }
        }
    }
}
