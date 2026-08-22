using System.Collections;
using EyeTraining.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace EyeTraining.Exercises
{
    public sealed class TrackingExerciseController : MonoBehaviour
    {
        private enum ExercisePhase
        {
            Inactive,
            Intro,
            Transition,
            Countdown,
            Running,
            Completed
        }

        private const float CycleCount = 1f;
        private const float TargetViewportHeight = 76f / 1080f;
        private const float TrainingBoundsLineWidthInViewportHeight = 0.0015f;
        private const float IntroTransitionDuration = 0.325f;
        private const float CountdownStepDuration = 0.7f;
        private const float IntroTitleFontSize = 62f;
        private const float IntroInstructionFontSize = 32f;

        private static readonly Vector2 IntroTitleAnchorMin = new(0.12f, 0.56f);
        private static readonly Vector2 IntroTitleAnchorMax = new(0.88f, 0.69f);
        private static readonly Vector2 IntroInstructionAnchorMin = new(0.16f, 0.40f);
        private static readonly Vector2 IntroInstructionAnchorMax = new(0.84f, 0.52f);
        private static readonly Color IntroTitleColor = new(0.96f, 0.98f, 1f, 1f);
        private static readonly Color IntroInstructionColor = new(0.85f, 0.89f, 0.94f, 1f);
        private static readonly Color RunningTitleColor = new(0.78f, 0.84f, 0.91f, 0.82f);
        private static readonly Color RunningInstructionColor = new(0.68f, 0.74f, 0.81f, 0.68f);

        private static readonly Color TrainingBoundsColor =
            new(0.55f, 0.62f, 0.70f, 0.22f);

        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject exerciseScreen;
        [SerializeField] private GameObject exerciseWorld;
        [SerializeField] private Camera exerciseCamera;
        [SerializeField] private Transform target;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private TrackingPathType pathType;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject completionMessage;
        [SerializeField] private Button interruptButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button startTrainingButton;

        [Header("Debug")]
        [SerializeField] private bool showTrainingBounds;

        private ITrackingPath trackingPath;
        private LineRenderer trainingBoundsRenderer;
        private TMP_Text countdownText;
        private Button introStartButton;
        private RectTransform titleRectTransform;
        private RectTransform instructionRectTransform;
        private Coroutine introSequence;
        private Vector2 runningTitleAnchorMin;
        private Vector2 runningTitleAnchorMax;
        private Vector2 runningInstructionAnchorMin;
        private Vector2 runningInstructionAnchorMax;
        private Vector2 targetExtentsInViewport;
        private float runningTitleFontSize;
        private float runningInstructionFontSize;
        private float exerciseDuration;
        private float remainingTime;
        private float lastBoundsOrthographicSize = -1f;
        private float lastBoundsTargetPlaneZ = float.NaN;
        private int lastBoundsPixelWidth = -1;
        private int lastBoundsPixelHeight = -1;
        private double movementStartTime;
        private ExercisePhase phase = ExercisePhase.Inactive;

        public SessionGuidanceMode GuidanceMode { get; private set; }

        private void Awake()
        {
            trackingPath = CreateTrackingPath();
            CreateTrainingBoundsRenderer();
            CreateIntroControls();
            CacheRunningPresentation();
            introStartButton.onClick.AddListener(StartIntroSequence);
            interruptButton.onClick.AddListener(Interrupt);
            nextButton.onClick.AddListener(ReturnHome);
            exerciseScreen.SetActive(false);
            exerciseWorld.SetActive(false);
        }

        private void OnDestroy()
        {
            introStartButton.onClick.RemoveListener(StartIntroSequence);
            interruptButton.onClick.RemoveListener(Interrupt);
            nextButton.onClick.RemoveListener(ReturnHome);
        }

        private void Update()
        {
            if (phase != ExercisePhase.Running)
            {
                return;
            }

            double elapsedTime = Time.timeAsDouble - movementStartTime;
            remainingTime = Mathf.Max(0f, exerciseDuration - (float)elapsedTime);
            UpdateTimer();

            if (elapsedTime >= exerciseDuration)
            {
                CompleteAtCycleEnd();
            }
        }

        private void LateUpdate()
        {
            if (phase == ExercisePhase.Running)
            {
                UpdateTargetPosition(Time.timeAsDouble - movementStartTime);
            }

            UpdateTrainingBounds();
        }

        public void Begin(SessionGuidanceMode guidanceMode)
        {
            GuidanceMode = guidanceMode;

            StopIntroSequence();

            exerciseScreen.SetActive(true);
            exerciseWorld.SetActive(true);
            completionMessage.SetActive(false);
            nextButton.gameObject.SetActive(false);
            introStartButton.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(false);
            timerText.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            UpdateTitle();
            ConfigureTargetScale();
            targetExtentsInViewport = GetTargetExtentsInViewport();
            float fullCycleLength = trackingPath.GetFullCycleLength(targetExtentsInViewport);
            exerciseDuration =
                fullCycleLength / TrackingMotionSettings.LinearSpeed * CycleCount;
            remainingTime = exerciseDuration;
            ResetTargetPosition();
            targetRenderer.enabled = false;
            ApplyIntroPresentation();
            phase = ExercisePhase.Intro;
            UpdateTimer();
            trainingBoundsRenderer.enabled = false;
            Select(introStartButton.gameObject);
        }

        private void StartIntroSequence()
        {
            if (phase != ExercisePhase.Intro)
            {
                return;
            }

            introStartButton.gameObject.SetActive(false);
            introSequence = StartCoroutine(PlayIntroSequence());
        }

        private IEnumerator PlayIntroSequence()
        {
            phase = ExercisePhase.Transition;
            float elapsedTime = 0f;

            while (elapsedTime < IntroTransitionDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsedTime / IntroTransitionDuration);
                float smoothProgress = progress * progress * (3f - 2f * progress);
                ApplyTransitionPresentation(smoothProgress);
                yield return null;
            }

            ApplyRunningPresentation();
            phase = ExercisePhase.Countdown;
            countdownText.gameObject.SetActive(true);

            for (int value = 3; value >= 1; value--)
            {
                countdownText.text = value.ToString();
                yield return new WaitForSecondsRealtime(CountdownStepDuration);
            }

            countdownText.gameObject.SetActive(false);
            introSequence = null;
            StartRunningExercise();
        }

        private void StartRunningExercise()
        {
            remainingTime = exerciseDuration;
            ResetTargetPosition();
            UpdateTimer();
            timerText.gameObject.SetActive(true);
            interruptButton.gameObject.SetActive(true);
            targetRenderer.enabled = true;
            movementStartTime = Time.timeAsDouble;
            phase = ExercisePhase.Running;
            UpdateTrainingBounds(true);
            Select(interruptButton.gameObject);
        }

        private void UpdateTargetPosition(double elapsedTime)
        {
            Vector2 viewportPosition = trackingPath.Evaluate(
                elapsedTime,
                targetExtentsInViewport);
            target.position = ViewportToTargetPlane(viewportPosition);
        }

        private ITrackingPath CreateTrackingPath()
        {
            return pathType switch
            {
                TrackingPathType.Vertical => new VerticalTrackingPath(),
                TrackingPathType.DiagonalUp => new DiagonalUpTrackingPath(),
                TrackingPathType.DiagonalDown => new DiagonalDownTrackingPath(),
                TrackingPathType.Circle => new CircleTrackingPath(),
                TrackingPathType.HorizontalEllipse => new HorizontalEllipseTrackingPath(),
                TrackingPathType.UpperSemicircle => new UpperSemicircleTrackingPath(),
                TrackingPathType.LowerSemicircle => new LowerSemicircleTrackingPath(),
                TrackingPathType.UpperHorizontalSemiEllipse => new UpperHorizontalSemiEllipseTrackingPath(),
                TrackingPathType.LowerHorizontalSemiEllipse => new LowerHorizontalSemiEllipseTrackingPath(),
                TrackingPathType.Square => new SquareTrackingPath(),
                TrackingPathType.HorizontalRectangle => new HorizontalRectangleTrackingPath(),
                TrackingPathType.Triangle => new TriangleTrackingPath(),
                TrackingPathType.Diamond => new DiamondTrackingPath(),
                TrackingPathType.HorizontalZigzag => new HorizontalZigzagTrackingPath(),
                TrackingPathType.VerticalZigzag => new VerticalZigzagTrackingPath(),
                TrackingPathType.HorizontalWave => new HorizontalWaveTrackingPath(),
                TrackingPathType.VerticalWave => new VerticalWaveTrackingPath(),
                TrackingPathType.FigureEight => new FigureEightTrackingPath(),
                TrackingPathType.Spiral => new SpiralTrackingPath(),
                TrackingPathType.UShape => new UShapeTrackingPath(),
                TrackingPathType.InvertedUShape => new InvertedUShapeTrackingPath(),
                _ => new HorizontalTrackingPath()
            };
        }

        private void UpdateTitle()
        {
            titleText.text = pathType switch
            {
                TrackingPathType.Vertical => "Śledzenie pionowe",
                TrackingPathType.DiagonalUp => "Śledzenie po przekątnej w górę",
                TrackingPathType.DiagonalDown => "Śledzenie po przekątnej w dół",
                TrackingPathType.Circle => "Śledzenie po okręgu",
                TrackingPathType.HorizontalEllipse => "Śledzenie po elipsie",
                TrackingPathType.UpperSemicircle => "Śledzenie po górnym półokręgu",
                TrackingPathType.LowerSemicircle => "Śledzenie po dolnym półokręgu",
                TrackingPathType.UpperHorizontalSemiEllipse => "Śledzenie po górnej półelipsie",
                TrackingPathType.LowerHorizontalSemiEllipse => "Śledzenie po dolnej półelipsie",
                TrackingPathType.Square => "Śledzenie po kwadracie",
                TrackingPathType.HorizontalRectangle => "Śledzenie po prostokącie",
                TrackingPathType.Triangle => "Śledzenie po trójkącie",
                TrackingPathType.Diamond => "Śledzenie po diamencie",
                TrackingPathType.HorizontalZigzag => "Śledzenie po zygzaku poziomym",
                TrackingPathType.VerticalZigzag => "Śledzenie po zygzaku pionowym",
                TrackingPathType.HorizontalWave => "Śledzenie po fali poziomej",
                TrackingPathType.VerticalWave => "Śledzenie po fali pionowej",
                TrackingPathType.FigureEight => "Śledzenie po ósemce",
                TrackingPathType.Spiral => "Śledzenie po spirali",
                TrackingPathType.UShape => "Śledzenie po literze U",
                TrackingPathType.InvertedUShape => "Śledzenie po odwróconej literze U",
                _ => "Śledzenie poziome"
            };
        }

        private void Complete()
        {
            phase = ExercisePhase.Completed;
            trainingBoundsRenderer.enabled = false;
            completionMessage.SetActive(true);
            interruptButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            Select(nextButton.gameObject);
        }

        private void CompleteAtCycleEnd()
        {
            remainingTime = 0f;
            UpdateTimer();

            float roundedCycleCount = Mathf.Round(CycleCount);
            double finalElapsedTime = Mathf.Approximately(CycleCount, roundedCycleCount)
                ? 0d
                : exerciseDuration;
            UpdateTargetPosition(finalElapsedTime);
            Complete();
        }

        private void Interrupt()
        {
            ReturnHome();
        }

        private void ReturnHome()
        {
            phase = ExercisePhase.Inactive;
            StopIntroSequence();
            trainingBoundsRenderer.enabled = false;
            targetRenderer.enabled = false;
            introStartButton.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);
            exerciseScreen.SetActive(false);
            exerciseWorld.SetActive(false);
            ApplyRunningPresentation();
            homeScreen.SetActive(true);
            Select(startTrainingButton.gameObject);
        }

        private void ResetTargetPosition()
        {
            Vector2 viewportPosition = trackingPath.Evaluate(
                0d,
                targetExtentsInViewport);
            target.position = ViewportToTargetPlane(viewportPosition);
        }

        private void ConfigureTargetScale()
        {
            float visibleWorldHeight = exerciseCamera.orthographicSize * 2f;
            float desiredDiameter = visibleWorldHeight * TargetViewportHeight;
            float spriteDiameter = targetRenderer.sprite.bounds.size.y;
            float uniformScale = desiredDiameter / spriteDiameter;
            target.localScale = Vector3.one * uniformScale;
        }

        private void CacheRunningPresentation()
        {
            titleRectTransform = titleText.rectTransform;
            instructionRectTransform = instructionText.rectTransform;
            runningTitleAnchorMin = titleRectTransform.anchorMin;
            runningTitleAnchorMax = titleRectTransform.anchorMax;
            runningInstructionAnchorMin = instructionRectTransform.anchorMin;
            runningInstructionAnchorMax = instructionRectTransform.anchorMax;
            runningTitleFontSize = titleText.fontSize;
            runningInstructionFontSize = instructionText.fontSize;
        }

        private void CreateIntroControls()
        {
            GameObject buttonObject = new(
                "Intro Start Button",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.layer = exerciseScreen.layer;
            buttonObject.transform.SetParent(exerciseScreen.transform, false);

            RectTransform buttonRectTransform = (RectTransform)buttonObject.transform;
            buttonRectTransform.anchorMin = new Vector2(0.4f, 0.25f);
            buttonRectTransform.anchorMax = new Vector2(0.6f, 0.36f);
            buttonRectTransform.anchoredPosition = Vector2.zero;
            buttonRectTransform.sizeDelta = Vector2.zero;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.12f, 0.36f, 0.62f, 1f);

            introStartButton = buttonObject.GetComponent<Button>();
            introStartButton.targetGraphic = buttonImage;
            ColorBlock buttonColors = introStartButton.colors;
            buttonColors.highlightedColor = new Color(0.34f, 0.52f, 0.715f, 1f);
            buttonColors.selectedColor = buttonColors.highlightedColor;
            buttonColors.fadeDuration = 0.1f;
            introStartButton.colors = buttonColors;

            GameObject labelObject = new(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            labelObject.layer = exerciseScreen.layer;
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRectTransform = (RectTransform)labelObject.transform;
            labelRectTransform.anchorMin = Vector2.zero;
            labelRectTransform.anchorMax = Vector2.one;
            labelRectTransform.anchoredPosition = Vector2.zero;
            labelRectTransform.sizeDelta = new Vector2(-36f, -20f);

            TMP_Text labelText = labelObject.GetComponent<TMP_Text>();
            labelText.font = titleText.font;
            labelText.fontSize = 36f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.raycastTarget = false;
            labelText.text = "START";

            GameObject countdownObject = new(
                "Countdown",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            countdownObject.layer = exerciseScreen.layer;
            countdownObject.transform.SetParent(exerciseScreen.transform, false);

            RectTransform countdownRectTransform = (RectTransform)countdownObject.transform;
            countdownRectTransform.anchorMin = new Vector2(0.38f, 0.36f);
            countdownRectTransform.anchorMax = new Vector2(0.62f, 0.64f);
            countdownRectTransform.anchoredPosition = Vector2.zero;
            countdownRectTransform.sizeDelta = Vector2.zero;

            countdownText = countdownObject.GetComponent<TMP_Text>();
            countdownText.font = titleText.font;
            countdownText.fontSize = 120f;
            countdownText.fontStyle = FontStyles.Bold;
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.color = IntroTitleColor;
            countdownText.raycastTarget = false;
            countdownText.text = "3";

            buttonObject.SetActive(false);
            countdownObject.SetActive(false);
        }

        private void ApplyIntroPresentation()
        {
            ApplyPresentation(
                IntroTitleAnchorMin,
                IntroTitleAnchorMax,
                IntroInstructionAnchorMin,
                IntroInstructionAnchorMax,
                IntroTitleFontSize,
                IntroInstructionFontSize,
                IntroTitleColor,
                IntroInstructionColor);
        }

        private void ApplyRunningPresentation()
        {
            ApplyPresentation(
                runningTitleAnchorMin,
                runningTitleAnchorMax,
                runningInstructionAnchorMin,
                runningInstructionAnchorMax,
                runningTitleFontSize,
                runningInstructionFontSize,
                RunningTitleColor,
                RunningInstructionColor);
        }

        private void ApplyTransitionPresentation(float progress)
        {
            ApplyPresentation(
                Vector2.Lerp(IntroTitleAnchorMin, runningTitleAnchorMin, progress),
                Vector2.Lerp(IntroTitleAnchorMax, runningTitleAnchorMax, progress),
                Vector2.Lerp(IntroInstructionAnchorMin, runningInstructionAnchorMin, progress),
                Vector2.Lerp(IntroInstructionAnchorMax, runningInstructionAnchorMax, progress),
                Mathf.Lerp(IntroTitleFontSize, runningTitleFontSize, progress),
                Mathf.Lerp(IntroInstructionFontSize, runningInstructionFontSize, progress),
                Color.Lerp(IntroTitleColor, RunningTitleColor, progress),
                Color.Lerp(IntroInstructionColor, RunningInstructionColor, progress));
        }

        private void ApplyPresentation(
            Vector2 titleAnchorMin,
            Vector2 titleAnchorMax,
            Vector2 instructionAnchorMin,
            Vector2 instructionAnchorMax,
            float titleFontSize,
            float instructionFontSize,
            Color titleColor,
            Color instructionColor)
        {
            titleRectTransform.anchorMin = titleAnchorMin;
            titleRectTransform.anchorMax = titleAnchorMax;
            instructionRectTransform.anchorMin = instructionAnchorMin;
            instructionRectTransform.anchorMax = instructionAnchorMax;
            titleText.fontSize = titleFontSize;
            instructionText.fontSize = instructionFontSize;
            titleText.color = titleColor;
            instructionText.color = instructionColor;
        }

        private void StopIntroSequence()
        {
            if (introSequence == null)
            {
                return;
            }

            StopCoroutine(introSequence);
            introSequence = null;
        }

        private Vector2 GetTargetExtentsInViewport()
        {
            Vector3 viewportMin = ViewportToTargetPlane(Vector2.zero);
            Vector3 viewportMax = ViewportToTargetPlane(Vector2.one);
            float visibleWorldWidth = viewportMax.x - viewportMin.x;
            float visibleWorldHeight = viewportMax.y - viewportMin.y;

            return new Vector2(
                targetRenderer.bounds.extents.x / visibleWorldWidth,
                targetRenderer.bounds.extents.y / visibleWorldHeight);
        }

        private Vector3 ViewportToTargetPlane(Vector2 viewportPosition)
        {
            float distanceFromCamera = target.position.z - exerciseCamera.transform.position.z;
            Vector3 position = exerciseCamera.ViewportToWorldPoint(
                new Vector3(viewportPosition.x, viewportPosition.y, distanceFromCamera));
            position.z = target.position.z;
            return position;
        }

        private void CreateTrainingBoundsRenderer()
        {
            GameObject boundsObject = new("Training Bounds (Debug)");
            boundsObject.transform.SetParent(exerciseWorld.transform, false);
            trainingBoundsRenderer = boundsObject.AddComponent<LineRenderer>();
            trainingBoundsRenderer.enabled = false;
            trainingBoundsRenderer.useWorldSpace = true;
            trainingBoundsRenderer.loop = true;
            trainingBoundsRenderer.positionCount = 4;
            trainingBoundsRenderer.startColor = TrainingBoundsColor;
            trainingBoundsRenderer.endColor = TrainingBoundsColor;
            trainingBoundsRenderer.numCapVertices = 0;
            trainingBoundsRenderer.numCornerVertices = 0;
            trainingBoundsRenderer.alignment = LineAlignment.View;
            trainingBoundsRenderer.textureMode = LineTextureMode.Stretch;
            trainingBoundsRenderer.shadowCastingMode = ShadowCastingMode.Off;
            trainingBoundsRenderer.receiveShadows = false;
            trainingBoundsRenderer.lightProbeUsage = LightProbeUsage.Off;
            trainingBoundsRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            trainingBoundsRenderer.sharedMaterial = targetRenderer.sharedMaterial;
            trainingBoundsRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            trainingBoundsRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
        }

        private void UpdateTrainingBounds(bool force = false)
        {
            bool shouldShow = showTrainingBounds && phase == ExercisePhase.Running;
            trainingBoundsRenderer.enabled = shouldShow;

            if (!shouldShow)
            {
                return;
            }

            float targetPlaneZ = target.position.z;
            bool dimensionsChanged =
                lastBoundsPixelWidth != exerciseCamera.pixelWidth ||
                lastBoundsPixelHeight != exerciseCamera.pixelHeight ||
                !Mathf.Approximately(lastBoundsOrthographicSize, exerciseCamera.orthographicSize) ||
                !Mathf.Approximately(lastBoundsTargetPlaneZ, targetPlaneZ);

            if (!force && !dimensionsChanged)
            {
                return;
            }

            trainingBoundsRenderer.SetPosition(
                0,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Left,
                    TrackingTrainingArea.Bottom)));
            trainingBoundsRenderer.SetPosition(
                1,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Left,
                    TrackingTrainingArea.Top)));
            trainingBoundsRenderer.SetPosition(
                2,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Right,
                    TrackingTrainingArea.Top)));
            trainingBoundsRenderer.SetPosition(
                3,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Right,
                    TrackingTrainingArea.Bottom)));

            float visibleWorldHeight = exerciseCamera.orthographicSize * 2f;
            float lineWidth = visibleWorldHeight * TrainingBoundsLineWidthInViewportHeight;
            trainingBoundsRenderer.startWidth = lineWidth;
            trainingBoundsRenderer.endWidth = lineWidth;
            lastBoundsPixelWidth = exerciseCamera.pixelWidth;
            lastBoundsPixelHeight = exerciseCamera.pixelHeight;
            lastBoundsOrthographicSize = exerciseCamera.orthographicSize;
            lastBoundsTargetPlaneZ = targetPlaneZ;
        }

        private void UpdateTimer()
        {
            timerText.text = $"{Mathf.CeilToInt(remainingTime)} s";
        }

        private static void Select(GameObject gameObject)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}
