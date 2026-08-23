using System;
using System.Collections;
using EyeTraining.Core;
using EyeTraining.Sessions.History;
using EyeTraining.Sessions.Progression.Tracking;
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
            Feedback,
            Completed
        }

        private const float StandaloneCycleCount = 1f;
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

        [Header("Path")]
        [SerializeField] private TrackingPathVisibility pathVisibility;

        [Header("Debug")]
        [SerializeField] private bool showTrainingBounds;

        private ITrackingPath trackingPath;
        private TrackingPathRenderer trackingPathRenderer;
        private LineRenderer trainingBoundsRenderer;
        private TMP_Text countdownText;
        private Button introStartButton;
        private GameObject feedbackPanel;
        private Button easyFeedbackButton;
        private Button comfortableFeedbackButton;
        private Button difficultFeedbackButton;
        private Button skipFeedbackButton;
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
        private float activeCycleCount = StandaloneCycleCount;
        private float activeSpeedMultiplier = 1f;
        private TrackingPathVisibility activePathVisibility;
        private string activeDisplayName;
        private bool sessionManaged;
        private float remainingTime;
        private float lastBoundsOrthographicSize = -1f;
        private float lastBoundsTargetPlaneZ = float.NaN;
        private int lastBoundsPixelWidth = -1;
        private int lastBoundsPixelHeight = -1;
        private double movementStartTime;
        private ExercisePhase phase = ExercisePhase.Inactive;

        public SessionGuidanceMode GuidanceMode { get; private set; }

        public TrackingExerciseResult LastResult { get; private set; }

        public event Action<TrackingExerciseResult> ResultReady;

        public event Action ContinueRequested;

        private void Awake()
        {
            trackingPath = CreateTrackingPath();
            CreateTrainingBoundsRenderer();
            trackingPathRenderer = new TrackingPathRenderer(
                exerciseWorld.transform,
                exerciseCamera,
                targetRenderer.sharedMaterial,
                targetRenderer.sortingLayerID,
                targetRenderer.sortingOrder - 1);
            CreateIntroControls();
            CreateFeedbackControls();
            CacheRunningPresentation();
            introStartButton.onClick.AddListener(StartIntroSequence);
            easyFeedbackButton.onClick.AddListener(SubmitEasyFeedback);
            comfortableFeedbackButton.onClick.AddListener(SubmitComfortableFeedback);
            difficultFeedbackButton.onClick.AddListener(SubmitDifficultFeedback);
            skipFeedbackButton.onClick.AddListener(SkipFeedback);
            interruptButton.onClick.AddListener(Interrupt);
            nextButton.onClick.AddListener(ReturnHome);
            exerciseScreen.SetActive(false);
            exerciseWorld.SetActive(false);
        }

        private void OnDestroy()
        {
            introStartButton.onClick.RemoveListener(StartIntroSequence);
            easyFeedbackButton.onClick.RemoveListener(SubmitEasyFeedback);
            comfortableFeedbackButton.onClick.RemoveListener(SubmitComfortableFeedback);
            difficultFeedbackButton.onClick.RemoveListener(SubmitDifficultFeedback);
            skipFeedbackButton.onClick.RemoveListener(SkipFeedback);
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
                trackingPathRenderer.UpdateIfNeeded();
            }

            UpdateTrainingBounds();
        }

        public void Begin(SessionGuidanceMode guidanceMode)
        {
            trackingPath = CreateTrackingPath();
            activeDisplayName = null;
            activeCycleCount = StandaloneCycleCount;
            activeSpeedMultiplier = 1f;
            sessionManaged = false;
            BeginInternal(guidanceMode, pathVisibility);
        }

        public void Begin(
            SessionGuidanceMode guidanceMode,
            ITrackingPath path,
            string displayName,
            TrackingPathVisibility visibility,
            float cycleCount,
            float speedMultiplier)
        {
            trackingPath = path ?? throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            }

            if (cycleCount <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cycleCount));
            }

            if (speedMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speedMultiplier));
            }

            activeDisplayName = displayName;
            activeCycleCount = cycleCount;
            activeSpeedMultiplier = speedMultiplier;
            sessionManaged = true;
            BeginInternal(guidanceMode, visibility);
        }

        private void BeginInternal(
            SessionGuidanceMode guidanceMode,
            TrackingPathVisibility visibility)
        {
            activePathVisibility = visibility;
            GuidanceMode = guidanceMode;
            LastResult = null;

            StopIntroSequence();

            exerciseScreen.SetActive(true);
            exerciseWorld.SetActive(true);
            completionMessage.SetActive(false);
            SetCompletionMessage("Ćwiczenie zakończone");
            nextButton.gameObject.SetActive(false);
            introStartButton.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(false);
            feedbackPanel.SetActive(false);
            timerText.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            UpdateTitle();
            ConfigureTargetScale();
            targetExtentsInViewport = GetTargetExtentsInViewport();
            float fullCycleLength = trackingPath.GetFullCycleLength(targetExtentsInViewport);
            exerciseDuration =
                fullCycleLength
                / (TrackingMotionSettings.LinearSpeed * activeSpeedMultiplier)
                * activeCycleCount;
            remainingTime = exerciseDuration;
            ResetTargetPosition();
            targetRenderer.enabled = false;
            trackingPathRenderer.Hide();
            titleText.gameObject.SetActive(true);
            instructionText.gameObject.SetActive(true);
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
            trackingPathRenderer.Show(
                trackingPath,
                targetExtentsInViewport,
                activePathVisibility,
                target.position.z);
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
            if (!string.IsNullOrEmpty(activeDisplayName))
            {
                titleText.text = activeDisplayName;
                return;
            }

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

        private void EnterFeedback()
        {
            phase = ExercisePhase.Feedback;
            trainingBoundsRenderer.enabled = false;
            trackingPathRenderer.Hide();
            targetRenderer.enabled = false;
            timerText.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            completionMessage.SetActive(false);
            nextButton.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            feedbackPanel.SetActive(true);
            Select(comfortableFeedbackButton.gameObject);
        }

        private void Complete(ExerciseFeedback feedback)
        {
            phase = ExercisePhase.Completed;
            LastResult = new TrackingExerciseResult(
                ExerciseCompletionStatus.Completed,
                feedback);
            trainingBoundsRenderer.enabled = false;
            trackingPathRenderer.Hide();
            feedbackPanel.SetActive(false);
            completionMessage.SetActive(true);
            interruptButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            Select(nextButton.gameObject);
            ResultReady?.Invoke(LastResult);
        }

        private void CompleteAtCycleEnd()
        {
            remainingTime = 0f;
            UpdateTimer();

            float roundedCycleCount = Mathf.Round(activeCycleCount);
            double finalElapsedTime = Mathf.Approximately(activeCycleCount, roundedCycleCount)
                ? 0d
                : exerciseDuration;
            UpdateTargetPosition(finalElapsedTime);
            EnterFeedback();
        }

        private void Interrupt()
        {
            if (phase != ExercisePhase.Running)
            {
                return;
            }

            TrackingExerciseResult result = new TrackingExerciseResult(
                ExerciseCompletionStatus.Interrupted,
                ExerciseFeedback.None);
            LastResult = result;
            ResultReady?.Invoke(result);
            ReturnHome();
        }

        private void SubmitEasyFeedback()
        {
            SubmitFeedback(ExerciseFeedback.Easy);
        }

        private void SubmitComfortableFeedback()
        {
            SubmitFeedback(ExerciseFeedback.Comfortable);
        }

        private void SubmitDifficultFeedback()
        {
            SubmitFeedback(ExerciseFeedback.Difficult);
        }

        private void SkipFeedback()
        {
            SubmitFeedback(ExerciseFeedback.None);
        }

        private void SubmitFeedback(ExerciseFeedback feedback)
        {
            if (phase != ExercisePhase.Feedback)
            {
                return;
            }

            Complete(feedback);
        }

        private void ReturnHome()
        {
            if (sessionManaged && phase == ExercisePhase.Completed)
            {
                ContinueRequested?.Invoke();
                return;
            }

            ExitToHome();
        }

        public void ExitToHome()
        {
            phase = ExercisePhase.Inactive;
            sessionManaged = false;
            StopIntroSequence();
            trainingBoundsRenderer.enabled = false;
            trackingPathRenderer.Hide();
            targetRenderer.enabled = false;
            introStartButton.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);
            feedbackPanel.SetActive(false);
            titleText.gameObject.SetActive(true);
            instructionText.gameObject.SetActive(true);
            exerciseScreen.SetActive(false);
            exerciseWorld.SetActive(false);
            ApplyRunningPresentation();
            homeScreen.SetActive(true);
            Select(startTrainingButton.gameObject);
        }

        public void ShowSessionCompleted()
        {
            sessionManaged = false;
            phase = ExercisePhase.Completed;
            trackingPathRenderer.Hide();
            trainingBoundsRenderer.enabled = false;
            targetRenderer.enabled = false;
            feedbackPanel.SetActive(false);
            timerText.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            exerciseScreen.SetActive(true);
            exerciseWorld.SetActive(false);
            SetCompletionMessage("Trening zakończony");
            completionMessage.SetActive(true);
            nextButton.gameObject.SetActive(true);
            Select(nextButton.gameObject);
        }

        public void ShowSessionError(string message)
        {
            sessionManaged = false;
            phase = ExercisePhase.Completed;
            trackingPathRenderer.Hide();
            trainingBoundsRenderer.enabled = false;
            targetRenderer.enabled = false;
            feedbackPanel.SetActive(false);
            timerText.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            exerciseScreen.SetActive(true);
            exerciseWorld.SetActive(false);
            SetCompletionMessage(message);
            completionMessage.SetActive(true);
            nextButton.gameObject.SetActive(true);
            Select(nextButton.gameObject);
        }

        private void SetCompletionMessage(string message)
        {
            TMP_Text messageText = completionMessage.GetComponentInChildren<TMP_Text>(true);
            if (messageText != null)
            {
                messageText.text = message;
            }
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

        private void CreateFeedbackControls()
        {
            feedbackPanel = new GameObject("Feedback Panel", typeof(RectTransform));
            feedbackPanel.layer = exerciseScreen.layer;
            feedbackPanel.transform.SetParent(exerciseScreen.transform, false);

            RectTransform panelRectTransform = (RectTransform)feedbackPanel.transform;
            panelRectTransform.anchorMin = Vector2.zero;
            panelRectTransform.anchorMax = Vector2.one;
            panelRectTransform.anchoredPosition = Vector2.zero;
            panelRectTransform.sizeDelta = Vector2.zero;

            GameObject headingObject = new(
                "Heading",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            headingObject.layer = exerciseScreen.layer;
            headingObject.transform.SetParent(feedbackPanel.transform, false);

            RectTransform headingRectTransform = (RectTransform)headingObject.transform;
            headingRectTransform.anchorMin = new Vector2(0.28f, 0.59f);
            headingRectTransform.anchorMax = new Vector2(0.72f, 0.70f);
            headingRectTransform.anchoredPosition = Vector2.zero;
            headingRectTransform.sizeDelta = Vector2.zero;

            TMP_Text headingText = headingObject.GetComponent<TMP_Text>();
            headingText.font = titleText.font;
            headingText.fontSize = 52f;
            headingText.fontStyle = FontStyles.Bold;
            headingText.alignment = TextAlignmentOptions.Center;
            headingText.color = IntroTitleColor;
            headingText.raycastTarget = false;
            headingText.text = "Jak było?";

            easyFeedbackButton = CreateFeedbackButton(
                "Easy Button",
                "Za łatwe",
                new Vector2(0.16f, 0.38f),
                new Vector2(0.38f, 0.50f),
                30f,
                false);
            comfortableFeedbackButton = CreateFeedbackButton(
                "Comfortable Button",
                "W sam raz",
                new Vector2(0.39f, 0.38f),
                new Vector2(0.61f, 0.50f),
                30f,
                false);
            difficultFeedbackButton = CreateFeedbackButton(
                "Difficult Button",
                "Trudne",
                new Vector2(0.62f, 0.38f),
                new Vector2(0.84f, 0.50f),
                30f,
                false);
            skipFeedbackButton = CreateFeedbackButton(
                "Skip Button",
                "Pomiń ocenę",
                new Vector2(0.41f, 0.23f),
                new Vector2(0.59f, 0.31f),
                24f,
                true);

            ConfigureFeedbackNavigation();
            feedbackPanel.SetActive(false);
        }

        private Button CreateFeedbackButton(
            string objectName,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            float fontSize,
            bool secondary)
        {
            GameObject buttonObject = new(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.layer = exerciseScreen.layer;
            buttonObject.transform.SetParent(feedbackPanel.transform, false);

            RectTransform buttonRectTransform = (RectTransform)buttonObject.transform;
            buttonRectTransform.anchorMin = anchorMin;
            buttonRectTransform.anchorMax = anchorMax;
            buttonRectTransform.anchoredPosition = Vector2.zero;
            buttonRectTransform.sizeDelta = Vector2.zero;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = secondary
                ? new Color(0.13f, 0.18f, 0.25f, 0.92f)
                : new Color(0.16f, 0.25f, 0.36f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            ColorBlock colors = button.colors;
            colors.highlightedColor = secondary
                ? new Color(0.25f, 0.32f, 0.41f, 1f)
                : new Color(0.31f, 0.43f, 0.56f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.22f, 0.34f, 0.47f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

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
            labelRectTransform.sizeDelta = new Vector2(-28f, -14f);

            TMP_Text labelText = labelObject.GetComponent<TMP_Text>();
            labelText.font = titleText.font;
            labelText.fontSize = fontSize;
            labelText.fontStyle = secondary ? FontStyles.Normal : FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = secondary
                ? new Color(0.75f, 0.80f, 0.86f, 1f)
                : new Color(0.94f, 0.96f, 0.99f, 1f);
            labelText.raycastTarget = false;
            labelText.text = label;

            return button;
        }

        private void ConfigureFeedbackNavigation()
        {
            easyFeedbackButton.navigation = CreateNavigation(
                null,
                comfortableFeedbackButton,
                null,
                skipFeedbackButton);
            comfortableFeedbackButton.navigation = CreateNavigation(
                easyFeedbackButton,
                difficultFeedbackButton,
                null,
                skipFeedbackButton);
            difficultFeedbackButton.navigation = CreateNavigation(
                comfortableFeedbackButton,
                null,
                null,
                skipFeedbackButton);
            skipFeedbackButton.navigation = CreateNavigation(
                easyFeedbackButton,
                difficultFeedbackButton,
                comfortableFeedbackButton,
                null);
        }

        private static Navigation CreateNavigation(
            Selectable left,
            Selectable right,
            Selectable up,
            Selectable down)
        {
            return new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = left,
                selectOnRight = right,
                selectOnUp = up,
                selectOnDown = down
            };
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
            trainingBoundsRenderer.sortingOrder = targetRenderer.sortingOrder - 2;
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
