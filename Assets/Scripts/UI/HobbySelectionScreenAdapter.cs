using UnityEngine;
using UnityEngine.UI;

namespace SundayLastBreath.UI
{
    [ExecuteAlways]
    public class HobbySelectionScreenAdapter : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private RectTransform containerRect;

        private void Awake()
        {
            UpdateLayout();
        }

        private void OnEnable()
        {
            UpdateLayout();
        }

        private void Start()
        {
            UpdateLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateLayout();
        }

        public void UpdateLayout()
        {
            if (containerRect == null && transform is RectTransform rt) containerRect = rt;
            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (gridLayout == null) gridLayout = GetComponentInChildren<GridLayoutGroup>(true);

            if (containerRect == null || gridLayout == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRt = canvas != null ? canvas.GetComponent<RectTransform>() : null;
            float canvasWidth = canvasRt != null ? canvasRt.rect.width : 1920f;
            float canvasHeight = canvasRt != null ? canvasRt.rect.height : 1080f;

            if (canvasWidth <= 0) canvasWidth = 1920f;
            if (canvasHeight <= 0) canvasHeight = 1080f;

            bool meetsRequirement = canvasHeight >= 900f && canvasWidth >= 1400f;

            if (meetsRequirement)
            {
                float targetContainerWidth = Mathf.Min(canvasWidth - 100f, 1540f);
                float targetContainerHeight = Mathf.Min(canvasHeight - 190f, 890f);
                containerRect.sizeDelta = new Vector2(targetContainerWidth, targetContainerHeight);
                containerRect.anchoredPosition = new Vector2(0f, -58f);

                // 5 columns
                float spacingX = 18f;
                float spacingY = 16f;
                float cardW = 280f;
                float cardH = 280f;

                float availableGridW = targetContainerWidth - 40f;
                if (availableGridW > 0)
                {
                    cardW = Mathf.Clamp((availableGridW - (4 * spacingX)) / 5f, 260f, 290f);
                }

                gridLayout.cellSize = new Vector2(cardW, cardH);
                gridLayout.spacing = new Vector2(spacingX, spacingY);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 5;
                gridLayout.padding = new RectOffset(20, 20, 10, 15);
                gridLayout.childAlignment = TextAnchor.UpperCenter;

                if (scrollRect != null)
                {
                    int count = gridLayout.transform.childCount;
                    // With 2-tab split for weaknesses (10 items = 2 rows of 5) and hobbies (15 items = 3 rows of 5),
                    // both fit completely in the 890px container without any scrolling!
                    if (count <= 15)
                    {
                        scrollRect.vertical = false;
                        scrollRect.horizontal = false;
                        scrollRect.verticalNormalizedPosition = 1f;
                    }
                    else
                    {
                        scrollRect.vertical = true;
                        scrollRect.horizontal = false;
                        scrollRect.scrollSensitivity = 35f;
                    }
                }
            }
            else
            {
                // Fallback for smaller screens
                float targetContainerWidth = Mathf.Max(canvasWidth - 60f, 320f);
                float targetContainerHeight = Mathf.Max(canvasHeight - 140f, 400f);
                containerRect.sizeDelta = new Vector2(targetContainerWidth, targetContainerHeight);
                containerRect.anchoredPosition = new Vector2(0f, -40f);

                int cols = canvasWidth >= 1200f ? 5 : (canvasWidth >= 950f ? 4 : (canvasWidth >= 700f ? 3 : 2));
                float cardW = Mathf.Clamp((targetContainerWidth - 40f - ((cols - 1) * 15f)) / cols, 220f, 280f);

                gridLayout.cellSize = new Vector2(cardW, 280f);
                gridLayout.spacing = new Vector2(15f, 15f);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = cols;
                gridLayout.padding = new RectOffset(15, 15, 10, 25);
                gridLayout.childAlignment = TextAnchor.UpperCenter;

                if (scrollRect != null)
                {
                    scrollRect.vertical = true;
                    scrollRect.horizontal = false;
                    scrollRect.scrollSensitivity = 35f;
                }
            }
        }
    }
}

