using UnityEngine;
using UnityEngine.UI;

namespace EyeTraining.Exercises.Landolt
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LandoltRingGraphic : RawImage
    {
        private const int TextureSize = 512;
        private const float GapAngleDegrees = 48f;
        private const float InnerRadiusRatio = 0.62f;
        private const float OuterRadius = 0.48f;

        private static Texture2D sharedRingTexture;

        public LandoltDirection Direction { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            EnsureTexture();
        }

        protected override void OnEnable()
        {
            EnsureTexture();
            base.OnEnable();
        }

        public void SetDirection(LandoltDirection direction)
        {
            Direction = direction;
            rectTransform.localEulerAngles = new Vector3(0f, 0f, GetRotation(direction));
            SetVerticesDirty();
        }

        private void EnsureTexture()
        {
            if (sharedRingTexture == null)
            {
                sharedRingTexture = CreateRingTexture();
            }

            texture = sharedRingTexture;
            uvRect = new Rect(0f, 0f, 1f, 1f);
            SetMaterialDirty();
            SetVerticesDirty();
        }

        private static Texture2D CreateRingTexture()
        {
            var texture = new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = "Runtime Landolt Ring",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            float innerRadius = OuterRadius * InnerRadiusRatio;
            float halfGapRadians = GapAngleDegrees * 0.5f * Mathf.Deg2Rad;
            var rawPixels = new byte[TextureSize * TextureSize * 4];

            for (var y = 0; y < TextureSize; y++)
            {
                float normalizedY = (y + 0.5f) / TextureSize - 0.5f;
                for (var x = 0; x < TextureSize; x++)
                {
                    float normalizedX = (x + 0.5f) / TextureSize - 0.5f;
                    float radius = Mathf.Sqrt(
                        normalizedX * normalizedX + normalizedY * normalizedY);
                    float angle = Mathf.Atan2(normalizedY, normalizedX);

                    bool isVisible = radius >= innerRadius
                        && radius <= OuterRadius
                        && Mathf.Abs(angle) >= halfGapRadians;
                    int pixelOffset = (y * TextureSize + x) * 4;
                    rawPixels[pixelOffset] = 255;
                    rawPixels[pixelOffset + 1] = 255;
                    rawPixels[pixelOffset + 2] = 255;
                    rawPixels[pixelOffset + 3] = isVisible ? (byte)255 : (byte)0;
                }
            }

            texture.LoadRawTextureData(rawPixels);
            texture.Apply(false, true);
            return texture;
        }

        private static float GetRotation(LandoltDirection direction)
        {
            return direction switch
            {
                LandoltDirection.Right => 0f,
                LandoltDirection.Up => 90f,
                LandoltDirection.Left => 180f,
                _ => 270f
            };
        }
    }
}
