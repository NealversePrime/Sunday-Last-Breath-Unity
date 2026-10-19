using UnityEngine;
using UnityEngine.UI;

namespace SundayLastBreath.UI
{
    [ExecuteAlways]
    public class JobSelectionScreenAdapter : MonoBehaviour
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

            // Minimal requirement for 1-screen view (e.g. 1080p, 1440p, 4K)
            // Height >= 900 and Width >= 1400: displays all 6 jobs in a 3x2 grid without any scrolling
            bool meetsRequirement = canvasHeight >= 900f && canvasWidth >= 1400f;

            if (meetsRequirement)
            {
                // Fit ScrollView container below header & S.P.A.I.C. Legend Bar with comfortable top/bottom breathing room
                float targetContainerWidth = Mathf.Min(canvasWidth - 100f, 1540f);
                float targetContainerHeight = Mathf.Min(canvasHeight - 216f, 864f);
                containerRect.sizeDelta = new Vector2(targetContainerWidth, targetContainerHeight);
                containerRect.anchoredPosition = new Vector2(0f, -72f);

                // 3 columns, 2 rows
                float spacingX = 24f;
                float spacingY = 16f;
                float cardW = 470f;
                float cardH = 412f;

                // Adjust card width slightly if container is wider/narrower
                float availableGridW = targetContainerWidth - 30f;
                if (availableGridW > 0)
                {
                    cardW = Mathf.Clamp((availableGridW - (2 * spacingX)) / 3f, 440f, 480f);
                }

                gridLayout.cellSize = new Vector2(cardW, cardH);
                gridLayout.spacing = new Vector2(spacingX, spacingY);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 3;
                gridLayout.padding = new RectOffset(15, 15, 6, 10);
                gridLayout.childAlignment = TextAnchor.UpperCenter;

                if (scrollRect != null)
                {
                    scrollRect.vertical = false;
                    scrollRect.horizontal = false;
                    scrollRect.verticalNormalizedPosition = 1f;
                }
            }
            else
            {
                // Fallback for smaller screens / windowed mode: enable smooth vertical scrolling
                float targetContainerWidth = Mathf.Max(canvasWidth - 60f, 320f);
                float targetContainerHeight = Mathf.Max(canvasHeight - 140f, 400f);
                containerRect.sizeDelta = new Vector2(targetContainerWidth, targetContainerHeight);
                containerRect.anchoredPosition = new Vector2(0f, -40f);

                int cols = canvasWidth >= 1100f ? 3 : (canvasWidth >= 750f ? 2 : 1);
                float cardW = Mathf.Clamp((targetContainerWidth - 60f - ((cols - 1) * 20f)) / cols, 320f, 470f);

                gridLayout.cellSize = new Vector2(cardW, 440f);
                gridLayout.spacing = new Vector2(20f, 20f);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = cols;
                gridLayout.padding = new RectOffset(15, 15, 10, 30);
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

