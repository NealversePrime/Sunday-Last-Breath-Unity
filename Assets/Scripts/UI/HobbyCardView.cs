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

        public void Setup(HobbyData data, Action onSelect)
        {
            if (titleText != null) titleText.text = data.title;
            if (descriptionText != null) descriptionText.text = data.description;
            if (statsPreviewText != null) statsPreviewText.text = data.statsPreview;

            // Weakness card styling (Red border and red stats text)
            if (data.isWeakness)
            {
                if (cardBorder != null) cardBorder.color = new Color(0.7f, 0.17f, 0.18f); // Red
                if (titleText != null) titleText.color = new Color(0.85f, 0.2f, 0.2f);
                if (statsPreviewText != null) statsPreviewText.color = new Color(0.85f, 0.2f, 0.2f);
            }
            else
            {
                if (cardBorder != null) cardBorder.color = new Color(0.83f, 0.33f, 0f); // Gold/Orange
                if (titleText != null) titleText.color = Color.white;
                if (statsPreviewText != null) statsPreviewText.color = new Color(0.36f, 0.55f, 0.71f);
            }

            // Load Sprite from Assets/Sprites/Traits/trait_{id}.png
            string spritePath = $"Assets/Sprites/Traits/trait_{data.id}.png";
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (s != null && iconImage != null)
            {
                iconImage.sprite = s;
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

