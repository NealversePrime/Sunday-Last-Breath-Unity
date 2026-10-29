using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Smoothly plays sequential frame animations on a UI Image or RawImage.
    /// Supports looping, custom framerates, and dynamic folder loading.
    /// </summary>
    public class FrameAnimationPlayer : MonoBehaviour
    {
        [Header("Target Components")]
        [SerializeField] private Image targetImage;
        [SerializeField] private RawImage targetRawImage;

        [Header("Animation Settings")]
        [SerializeField] private float framesPerSecond = 12f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnAwake = true;

        [Header("Frames")]
        [SerializeField] private List<Sprite> frames = new List<Sprite>();

        private int currentFrameIndex = 0;
        private float frameTimer = 0f;
        private bool isPlaying = false;

        public bool IsPlaying => isPlaying;
        public int FrameCount => frames.Count;

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }
            if (targetRawImage == null)
            {
                targetRawImage = GetComponent<RawImage>();
            }

            if (playOnAwake && frames.Count > 0)
            {
                Play();
            }
        }

        public void SetFrames(List<Sprite> newFrames, float fps = 12f, bool shouldLoop = true)
        {
            frames = newFrames ?? new List<Sprite>();
            framesPerSecond = fps;
            loop = shouldLoop;
            currentFrameIndex = 0;
            frameTimer = 0f;
            ApplyCurrentFrame();
        }

        public void Play()
        {
            if (frames == null || frames.Count == 0) return;
            isPlaying = true;
            currentFrameIndex = 0;
            frameTimer = 0f;
            ApplyCurrentFrame();
        }

        public void Stop()
        {
            isPlaying = false;
        }

        public void Pause()
        {
            isPlaying = false;
        }

        public void Resume()
        {
            if (frames.Count > 0) isPlaying = true;
        }

        private void Update()
        {
            if (!isPlaying || frames == null || frames.Count <= 1) return;

            frameTimer += Time.deltaTime;
            float frameInterval = 1f / Mathf.Max(framesPerSecond, 0.1f);

            if (frameTimer >= frameInterval)
            {
                frameTimer -= frameInterval;
                currentFrameIndex++;

                if (currentFrameIndex >= frames.Count)
                {
                    if (loop)
                    {
                        currentFrameIndex = 0;
                    }
                    else
                    {
                        currentFrameIndex = frames.Count - 1;
                        isPlaying = false;
                    }
                }

                ApplyCurrentFrame();
            }
        }

        private void ApplyCurrentFrame()
        {
            if (frames == null || frames.Count == 0 || currentFrameIndex < 0 || currentFrameIndex >= frames.Count)
                return;

            Sprite currentSprite = frames[currentFrameIndex];
            if (currentSprite != null)
            {
                if (targetImage != null)
                {
                    targetImage.sprite = currentSprite;
                }
                else if (targetRawImage != null)
                {
                    targetRawImage.texture = currentSprite.texture;
                }
            }
        }
    }
}

