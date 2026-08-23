using System;
using System.Collections;
using EyeTraining.Core;
using EyeTraining.Sessions.History;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EyeTraining.Exercises.Landolt
{
    public sealed class LandoltExerciseController : MonoBehaviour
    {
        private enum ExercisePhase
        {
            Inactive,
            Intro,
            Countdown,
            Running,
            AnswerFeedback,
            Completed
        }

        private const float BaseDiameterViewportHeight = 0.11f;
        private const float CountdownStepDuration = 0.7f;
        private const float AnswerFeedbackDuration = 0.25f;

        private static readonly Color DarkBackground = new(0.025f, 0.04f, 0.065f, 1f);
        private static readonly Color DarkSymbol = new(0.93f, 0.96f, 1f, 1f);
        private static readonly Color LightBackground = new(0.94f, 0.95f, 0.96f, 1f);
        private static readonly Color LightSymbol = new(0.06f, 0.08f, 0.11f, 1f);
        private static readonly Color NeutralButton = new(0.18f, 0.25f, 0.34f, 1f);
        private static readonly Color CorrectAccent = new(0.38f, 0.55f, 0.62f, 1f);
        private static readonly Color IncorrectAccent = new(0.45f, 0.43f, 0.48f, 1f);

        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject trackingExerciseScreen;
        [SerializeField] private TMP_Text textTemplate;
        [SerializeField] private Button buttonTemplate;
        [SerializeField] private Button homeStartTrainingButton;

        [Header("Debug")]
        [Tooltip("Gdy włączone, ignoruje preferencję aktywnego profilu.")]
        [SerializeField] private bool debugOverrideBackgroundMode;
        [SerializeField] private LandoltBackgroundMode debugBackgroundMode =
            LandoltBackgroundMode.Dark;
        [Range(0, LandoltLevelPlan.MaximumLevel)]
        [SerializeField] private int debugStartLevel;
        [SerializeField] private bool debugFixedDirection;
        [SerializeField] private LandoltDirection debugDirection;

        private GameObject root;
        private Image background;
        private TMP_Text titleText;
        private TMP_Text instructionText;
        private TMP_Text countdownText;
        private TMP_Text errorsText;
        private TMP_Text summaryText;
        private LandoltRingGraphic ring;
        private Button introStartButton;
        private Button interruptButton;
        private Button continueButton;
        private Button upButton;
        private Button downButton;
        private Button leftButton;
        private Button rightButton;
        private LandoltRound round;
        private Coroutine activeSequence;
        private ExercisePhase phase;
        private bool sessionManaged;
        private int deterministicSeed;
        private LandoltBackgroundMode activeBackgroundMode = LandoltBackgroundMode.Dark;

        public LandoltExerciseResult LastResult { get; private set; }
        public event Action<LandoltExerciseResult> ResultReady;
        public event Action ContinueRequested;

        private void Awake()
        {
            CreateUi();
            root.SetActive(false);
        }

        private void OnDestroy()
        {
            introStartButton.onClick.RemoveListener(StartCountdown);
            interruptButton.onClick.RemoveListener(Interrupt);
            continueButton.onClick.RemoveListener(Continue);
            upButton.onClick.RemoveAllListeners();
            downButton.onClick.RemoveAllListeners();
            leftButton.onClick.RemoveAllListeners();
            rightButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            if (phase != ExercisePhase.Running)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            if ((keyboard?.upArrowKey.wasPressedThisFrame ?? false)
                || (gamepad?.dpad.up.wasPressedThisFrame ?? false))
            {
                SubmitAnswer(LandoltDirection.Up);
            }
            else if ((keyboard?.downArrowKey.wasPressedThisFrame ?? false)
                || (gamepad?.dpad.down.wasPressedThisFrame ?? false))
            {
                SubmitAnswer(LandoltDirection.Down);
            }
            else if ((keyboard?.leftArrowKey.wasPressedThisFrame ?? false)
                || (gamepad?.dpad.left.wasPressedThisFrame ?? false))
            {
                SubmitAnswer(LandoltDirection.Left);
            }
            else if ((keyboard?.rightArrowKey.wasPressedThisFrame ?? false)
                || (gamepad?.dpad.right.wasPressedThisFrame ?? false))
            {
                SubmitAnswer(LandoltDirection.Right);
            }
        }

        private void LateUpdate()
        {
            if (phase == ExercisePhase.Running || phase == ExercisePhase.AnswerFeedback)
            {
                UpdateRingSize();
            }
        }

        public void Begin(
            SessionGuidanceMode guidanceMode,
            int sessionNumber,
            LandoltBackgroundMode preferredBackgroundMode)
        {
            if (sessionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionNumber));
            }

            if (!Enum.IsDefined(typeof(LandoltBackgroundMode), preferredBackgroundMode))
            {
                throw new ArgumentOutOfRangeException(nameof(preferredBackgroundMode));
            }

            _ = guidanceMode;
            sessionManaged = true;
            deterministicSeed = sessionNumber;
            activeBackgroundMode = debugOverrideBackgroundMode
                ? debugBackgroundMode
                : preferredBackgroundMode;
            LastResult = null;
            StopSequence();
            trackingExerciseScreen.SetActive(false);
            homeScreen.SetActive(false);
            root.SetActive(true);
            ApplyTheme();
            SetIntroVisible(true);
            SetRunningVisible(false);
            SetSummaryVisible(false);
            phase = ExercisePhase.Intro;
            Select(introStartButton.gameObject);
        }

        private void StartCountdown()
        {
            if (phase != ExercisePhase.Intro)
            {
                return;
            }

            introStartButton.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            phase = ExercisePhase.Countdown;
            activeSequence = StartCoroutine(CountdownSequence());
        }

        private IEnumerator CountdownSequence()
        {
            countdownText.gameObject.SetActive(true);
            for (var number = 3; number >= 1; number--)
            {
                countdownText.text = number.ToString();
                yield return new WaitForSecondsRealtime(CountdownStepDuration);
            }

            countdownText.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            round = new LandoltRound(debugStartLevel, deterministicSeed);
            phase = ExercisePhase.Running;
            SetRunningVisible(true);
            ShowCurrentExposure();
            activeSequence = null;
        }

        private void SubmitAnswer(LandoltDirection answer)
        {
            if (phase != ExercisePhase.Running)
            {
                return;
            }

            LandoltDirection evaluatedAnswer = answer;
            if (debugFixedDirection)
            {
                evaluatedAnswer = answer == debugDirection
                    ? round.CurrentDirection
                    : GetDifferentDirection(round.CurrentDirection);
            }

            bool isCorrect = round.SubmitAnswer(evaluatedAnswer);
            errorsText.text = $"Błędy: {round.ErrorCount}/{LandoltRound.MaximumErrors}";
            phase = ExercisePhase.AnswerFeedback;
            activeSequence = StartCoroutine(AnswerFeedbackSequence(answer, isCorrect));
        }

        private IEnumerator AnswerFeedbackSequence(LandoltDirection answer, bool isCorrect)
        {
            Button selectedButton = GetButton(answer);
            Image image = selectedButton.GetComponent<Image>();
            image.color = isCorrect ? CorrectAccent : IncorrectAccent;
            yield return new WaitForSecondsRealtime(AnswerFeedbackDuration);
            image.color = NeutralButton;

            if (round.IsFinished)
            {
                CompleteRound();
            }
            else
            {
                phase = ExercisePhase.Running;
                ShowCurrentExposure();
            }

            activeSequence = null;
        }

        private void ShowCurrentExposure()
        {
            LandoltDirection direction = debugFixedDirection
                ? debugDirection
                : round.CurrentDirection;
            ring.SetDirection(direction);
            UpdateRingSize();
            Select(GetButton(direction).gameObject);
        }

        private void CompleteRound()
        {
            phase = ExercisePhase.Completed;
            LastResult = CreateResult(ExerciseCompletionStatus.Completed);
            SetRunningVisible(false);
            summaryText.text = "Landolt C zakończony\n\n"
                + $"Poprawne: {round.CorrectAnswers} / {round.ExposureCount}\n"
                + $"Błędy: {round.ErrorCount}\n"
                + $"Najwyższy poziom: {round.HighestLevel}";
            SetSummaryVisible(true);
            Select(continueButton.gameObject);
            ResultReady?.Invoke(LastResult);
        }

        private void Interrupt()
        {
            if (phase != ExercisePhase.Running && phase != ExercisePhase.AnswerFeedback)
            {
                return;
            }

            StopSequence();
            LastResult = CreateResult(ExerciseCompletionStatus.Interrupted);
            ResultReady?.Invoke(LastResult);
            ExitToHome();
        }

        private LandoltExerciseResult CreateResult(ExerciseCompletionStatus status)
        {
            return new LandoltExerciseResult(
                status,
                round?.CorrectAnswers ?? 0,
                round?.ErrorCount ?? 0,
                round?.ExposureCount ?? 0,
                round?.HighestLevel ?? debugStartLevel,
                round?.CurrentLevel ?? debugStartLevel,
                activeBackgroundMode,
                LandoltDirectionMode.FourDirections);
        }

        private void Continue()
        {
            if (phase != ExercisePhase.Completed)
            {
                return;
            }

            if (sessionManaged)
            {
                phase = ExercisePhase.Inactive;
                root.SetActive(false);
                ContinueRequested?.Invoke();
            }
            else
            {
                ExitToHome();
            }
        }

        public void ExitToHome()
        {
            StopSequence();
            phase = ExercisePhase.Inactive;
            sessionManaged = false;
            root.SetActive(false);
            homeScreen.SetActive(true);
            Select(homeStartTrainingButton.gameObject);
        }

        private void ApplyTheme()
        {
            bool dark = activeBackgroundMode == LandoltBackgroundMode.Dark;
            background.color = dark ? DarkBackground : LightBackground;
            Color foreground = dark ? DarkSymbol : LightSymbol;
            ring.color = foreground;
            titleText.color = foreground;
            instructionText.color = foreground;
            countdownText.color = foreground;
            errorsText.color = foreground;
            summaryText.color = foreground;
        }

        private void UpdateRingSize()
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            float size = canvasRect.rect.height * BaseDiameterViewportHeight
                * LandoltLevelPlan.GetDiameterMultiplier(round.CurrentLevel);
            ring.rectTransform.sizeDelta = new Vector2(size, size);
        }

        private void SetIntroVisible(bool visible)
        {
            titleText.gameObject.SetActive(visible);
            instructionText.gameObject.SetActive(visible);
            introStartButton.gameObject.SetActive(visible);
            countdownText.gameObject.SetActive(false);
        }

        private void SetRunningVisible(bool visible)
        {
            ring.gameObject.SetActive(visible);
            errorsText.gameObject.SetActive(visible);
            interruptButton.gameObject.SetActive(visible);
            upButton.gameObject.SetActive(visible);
            downButton.gameObject.SetActive(visible);
            leftButton.gameObject.SetActive(visible);
            rightButton.gameObject.SetActive(visible);
        }

        private void SetSummaryVisible(bool visible)
        {
            summaryText.gameObject.SetActive(visible);
            continueButton.gameObject.SetActive(visible);
        }

        private void StopSequence()
        {
            if (activeSequence != null)
            {
                StopCoroutine(activeSequence);
                activeSequence = null;
            }
        }

        private void CreateUi()
        {
            root = new GameObject("Landolt Exercise Screen", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = (RectTransform)root.transform;
            Stretch(rootRect);
            background = root.GetComponent<Image>();

            titleText = CreateText("Title", "Landolt C", 62f, new Vector2(0.15f, 0.60f), new Vector2(0.85f, 0.72f));
            instructionText = CreateText("Instruction", "Wskaż kierunek przerwy w pierścieniu.", 32f, new Vector2(0.15f, 0.48f), new Vector2(0.85f, 0.58f));
            countdownText = CreateText("Countdown", "3", 108f, new Vector2(0.40f, 0.40f), new Vector2(0.60f, 0.60f));
            errorsText = CreateText("Errors", "Błędy: 0/3", 30f, new Vector2(0.72f, 0.86f), new Vector2(0.94f, 0.94f));
            summaryText = CreateText("Summary", string.Empty, 42f, new Vector2(0.22f, 0.42f), new Vector2(0.78f, 0.72f));

            var ringObject = new GameObject(
                "Landolt Ring",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(LandoltRingGraphic));
            ringObject.transform.SetParent(root.transform, false);
            ring = ringObject.GetComponent<LandoltRingGraphic>();
            ring.raycastTarget = false;
            RectTransform ringRect = ring.rectTransform;
            ringRect.anchorMin = ringRect.anchorMax = new Vector2(0.5f, 0.58f);
            ringRect.pivot = new Vector2(0.5f, 0.5f);
            ringRect.anchoredPosition = Vector2.zero;
            ringRect.localScale = Vector3.one;

            introStartButton = CreateButton("Intro Start", "START", new Vector2(0.5f, 0.34f), new Vector2(300f, 88f));
            interruptButton = CreateButton("Interrupt", "Przerwij", new Vector2(0.5f, 0.11f), new Vector2(250f, 68f));
            continueButton = CreateButton("Continue", "Dalej", new Vector2(0.5f, 0.29f), new Vector2(280f, 76f));
            upButton = CreateButton("Up", "↑", new Vector2(0.5f, 0.40f), new Vector2(112f, 76f));
            downButton = CreateButton("Down", "↓", new Vector2(0.5f, 0.24f), new Vector2(112f, 76f));
            leftButton = CreateButton("Left", "←", new Vector2(0.43f, 0.32f), new Vector2(112f, 76f));
            rightButton = CreateButton("Right", "→", new Vector2(0.57f, 0.32f), new Vector2(112f, 76f));

            introStartButton.onClick.AddListener(StartCountdown);
            interruptButton.onClick.AddListener(Interrupt);
            continueButton.onClick.AddListener(Continue);
            upButton.onClick.AddListener(() => SubmitAnswer(LandoltDirection.Up));
            downButton.onClick.AddListener(() => SubmitAnswer(LandoltDirection.Down));
            leftButton.onClick.AddListener(() => SubmitAnswer(LandoltDirection.Left));
            rightButton.onClick.AddListener(() => SubmitAnswer(LandoltDirection.Right));
        }

        private TMP_Text CreateText(string name, string value, float fontSize, Vector2 min, Vector2 max)
        {
            TMP_Text text = Instantiate(textTemplate, root.transform);
            text.name = name;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            return text;
        }

        private Button CreateButton(string name, string label, Vector2 anchor, Vector2 size)
        {
            Button button = Instantiate(buttonTemplate, root.transform);
            button.name = name;
            button.GetComponent<Image>().color = NeutralButton;
            TMP_Text labelText = button.GetComponentInChildren<TMP_Text>();
            labelText.text = label;
            labelText.fontSize = label.Length == 1 ? 48f : 30f;
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            return button;
        }

        private Button GetButton(LandoltDirection direction)
        {
            return direction switch
            {
                LandoltDirection.Up => upButton,
                LandoltDirection.Down => downButton,
                LandoltDirection.Left => leftButton,
                _ => rightButton
            };
        }

        private static LandoltDirection GetDifferentDirection(LandoltDirection direction)
        {
            return direction == LandoltDirection.Up
                ? LandoltDirection.Down
                : LandoltDirection.Up;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
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
