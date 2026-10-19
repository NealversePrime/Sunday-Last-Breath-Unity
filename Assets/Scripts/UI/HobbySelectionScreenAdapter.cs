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

        public void UpdateLayout(int explicitCardCount = -1)
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

            int activeCount = explicitCardCount;
            if (activeCount < 0)
            {
                activeCount = 0;
                for (int i = 0; i < gridLayout.transform.childCount; i++)
                {
                    if (gridLayout.transform.GetChild(i).gameObject.activeSelf)
                        activeCount++;
                }
            }

            bool meetsRequirement = canvasHeight >= 900f && canvasWidth >= 1400f;

            if (meetsRequirement)
            {
                float targetContainerWidth = Mathf.Min(canvasWidth - 100f, 1560f);
                float targetContainerHeight = Mathf.Min(canvasHeight - 230f, 840f);
                containerRect.sizeDelta = new Vector2(targetContainerWidth, targetContainerHeight);
                containerRect.anchoredPosition = new Vector2(0f, -82f);

                // Positive Hobbies (7 or 8 cards per tab): 4 columns x 2 rows (wider 345x310 cards, 0 empty slots in Tab 0!)
                // Weaknesses (10 cards per tab): 5 columns x 2 rows (284x305 cards, 0 empty slots!)
                bool useFourColumns = activeCount > 0 && activeCount <= 8;
                int cols = useFourColumns ? 4 : 5;
                float spacingX = useFourColumns ? 22f : 18f;
                float spacingY = 20f;
                float cardW = useFourColumns ? 345f : 284f;
                float cardH = useFourColumns ? 310f : 304f;

                float availableGridW = targetContainerWidth - 40f;
                if (availableGridW > 0)
                {
                    if (useFourColumns)
                        cardW = Mathf.Clamp((availableGridW - (3 * spacingX)) / 4f, 310f, 350f);
                    else
                        cardW = Mathf.Clamp((availableGridW - (4 * spacingX)) / 5f, 265f, 290f);
                }

                gridLayout.cellSize = new Vector2(cardW, cardH);
                gridLayout.spacing = new Vector2(spacingX, spacingY);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = cols;
                gridLayout.padding = new RectOffset(20, 20, 12, 16);
                gridLayout.childAlignment = TextAnchor.UpperCenter;

                if (scrollRect != null)
                {
                    if (activeCount <= 10)
                    {
                        scrollRect.vertical = false;
                        scrollRect.horizontal = false;
                        scrollRect.verticalNormalizedPosition = 1f;
                        if (scrollRect.verticalScrollbar != null)
                        {
                            scrollRect.verticalScrollbar.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        scrollRect.vertical = true;
                        scrollRect.horizontal = false;
                        scrollRect.scrollSensitivity = 35f;
                        if (scrollRect.verticalScrollbar != null)
                        {
                            scrollRect.verticalScrollbar.gameObject.SetActive(true);
                        }
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

                int cols = canvasWidth >= 1200f ? 4 : (canvasWidth >= 900f ? 3 : 2);
                float cardW = Mathf.Clamp((targetContainerWidth - 40f - ((cols - 1) * 15f)) / cols, 220f, 330f);

                gridLayout.cellSize = new Vector2(cardW, 300f);
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
                    if (scrollRect.verticalScrollbar != null)
                    {
                        scrollRect.verticalScrollbar.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
