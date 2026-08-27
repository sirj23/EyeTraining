using System;
using System.Collections;
using EyeTraining.Core;
using EyeTraining.Sessions.History;
using EyeTraining.Sessions.Progression.Peripheral;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EyeTraining.Exercises.Peripheral
{
    public sealed class EdgeSignalsController : MonoBehaviour
    {
        private enum ExercisePhase
        {
            Inactive,
            Intro,
            Countdown,
            Running,
            Completed
        }

        private const float FixationSizeViewportHeight = 0.027f;

        private static readonly Color BackgroundColor = new(0.025f, 0.04f, 0.065f, 1f);
        private static readonly Color ForegroundColor = new(0.78f, 0.84f, 0.90f, 1f);
        private static readonly Color StimulusColor = new(0.76f, 0.86f, 0.94f, 1f);

        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject trackingExerciseScreen;
        [SerializeField] private TMP_Text textTemplate;
        [SerializeField] private Button buttonTemplate;
        [SerializeField] private Button homeStartTrainingButton;

        [Header("Debug")]
        [SerializeField] private int debugEdgeSignalsSeed = 1;
        [SerializeField, Range(0, 5)] private int debugEdgeSignalsLevel;
        [SerializeField] private bool debugFixedDirection;
        [SerializeField] private PeripheralDirection debugDirection;

        private GameObject root;
        private TMP_Text titleText;
        private TMP_Text instructionText;
        private TMP_Text desktopHintText;
        private TMP_Text countdownText;
        private TMP_Text summaryText;
        private PeripheralMarkerGraphic fixationGraphic;
        private PeripheralMarkerGraphic stimulusGraphic;
        private Button introStartButton;
        private Button interruptButton;
        private Button continueButton;
        private PeripheralStimulusSequence sequence;
        private EdgeSignalsRound round;
        private Coroutine activeRoutine;
        private ExercisePhase phase;
        private string exerciseId;
        private bool sessionManaged;
        private EdgeSignalsLevelSettings settings;

        public int DebugEdgeSignalsSeed => debugEdgeSignalsSeed;
        public int DebugEdgeSignalsLevel => debugEdgeSignalsLevel;
        public EdgeSignalsExerciseResult LastResult { get; private set; }
        public event Action<EdgeSignalsExerciseResult> ResultReady;
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
        }

        private void Update()
        {
            if (phase != ExercisePhase.Running) return;

            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            bool keyboardResponse = (keyboard?.spaceKey.wasPressedThisFrame ?? false)
                || (keyboard?.enterKey.wasPressedThisFrame ?? false)
                || (keyboard?.numpadEnterKey.wasPressedThisFrame ?? false);
            bool gamepadResponse = gamepad?.buttonSouth.wasPressedThisFrame ?? false;
            bool mouseResponse = Mouse.current?.leftButton.wasPressedThisFrame ?? false;
            if (mouseResponse && EventSystem.current?.IsPointerOverGameObject() == true)
                mouseResponse = false;

            if (keyboardResponse || gamepadResponse || mouseResponse)
                round.TryRespond(Time.realtimeSinceStartupAsDouble);
        }

        public void Begin(SessionGuidanceMode guidanceMode, string currentExerciseId, int seed,
            EdgeSignalsLevelSettings levelSettings)
        {
            if (string.IsNullOrWhiteSpace(currentExerciseId))
                throw new ArgumentException("Exercise ID cannot be empty.", nameof(currentExerciseId));

            _ = guidanceMode;
            settings = levelSettings ?? throw new ArgumentNullException(nameof(levelSettings));
            exerciseId = currentExerciseId;
            sessionManaged = true;
            LastResult = null;
            StopActiveRoutine();
            sequence = PeripheralStimulusSequence.Create(
                seed,
                settings,
                debugFixedDirection,
                debugDirection);
            round = new EdgeSignalsRound(settings.TrialCount, settings.ResponseWindow);
            trackingExerciseScreen.SetActive(false);
            homeScreen.SetActive(false);
            root.SetActive(true);
            ShowIntro();
        }

        public void ExitToHome()
        {
            StopActiveRoutine();
            phase = ExercisePhase.Inactive;
            sessionManaged = false;
            root.SetActive(false);
            homeScreen.SetActive(true);
            Select(homeStartTrainingButton.gameObject);
        }

        private void StartCountdown()
        {
            if (phase != ExercisePhase.Intro || activeRoutine != null) return;
            introStartButton.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            desktopHintText.gameObject.SetActive(false);
            phase = ExercisePhase.Countdown;
            activeRoutine = StartCoroutine(RunExercise());
        }

        private IEnumerator RunExercise()
        {
            countdownText.gameObject.SetActive(true);
            for (var number = 3; number >= 1; number--)
            {
                countdownText.text = number.ToString();
                yield return new WaitForSecondsRealtime(EdgeSignalsLevelSettings.CountdownStepDuration);
            }

            countdownText.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            fixationGraphic.gameObject.SetActive(true);
            interruptButton.gameObject.SetActive(true);
            stimulusGraphic.gameObject.SetActive(false);
            phase = ExercisePhase.Running;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

            RectTransform canvasRect = (RectTransform)canvas.transform;
            float stimulusSize = canvasRect.rect.height * settings.StimulusSizeViewportHeight;
            float fixationSize = canvasRect.rect.height * FixationSizeViewportHeight;
            stimulusGraphic.rectTransform.sizeDelta = Vector2.one * stimulusSize;
            fixationGraphic.rectTransform.sizeDelta = Vector2.one * fixationSize;

            foreach (PeripheralTrial trial in sequence.Trials)
            {
                yield return new WaitForSecondsRealtime(trial.DelayBeforeStimulus);
                RectTransform stimulusRect = stimulusGraphic.rectTransform;
                stimulusRect.anchorMin = stimulusRect.anchorMax =
                    PeripheralLayout.GetViewportPosition(trial.Direction, settings);
                stimulusRect.anchoredPosition = Vector2.zero;
                double appearedAt = Time.realtimeSinceStartupAsDouble;
                round.BeginTrial(appearedAt);
                stimulusGraphic.gameObject.SetActive(true);

                bool stimulusHidden = false;
                while (Time.realtimeSinceStartupAsDouble - appearedAt < settings.ResponseWindow)
                {
                    if (!stimulusHidden
                        && Time.realtimeSinceStartupAsDouble - appearedAt >= settings.StimulusVisibleDuration)
                    {
                        stimulusGraphic.gameObject.SetActive(false);
                        stimulusHidden = true;
                    }
                    yield return null;
                }

                stimulusGraphic.gameObject.SetActive(false);
                round.CloseTrial(Time.realtimeSinceStartupAsDouble);
            }

            CompleteExercise();
            activeRoutine = null;
        }

        private void CompleteExercise()
        {
            phase = ExercisePhase.Completed;
            fixationGraphic.gameObject.SetActive(false);
            stimulusGraphic.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            LastResult = CreateResult(ExerciseCompletionStatus.Completed);
            summaryText.text = "Ćwiczenie zakończone\n\n"
                + $"Zauważone: {round.DetectedCount}/{settings.TrialCount}\n"
                + $"Pominięte: {round.MissedCount}";
            summaryText.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(true);
            Select(continueButton.gameObject);
            ResultReady?.Invoke(LastResult);
        }

        private void Interrupt()
        {
            if (phase != ExercisePhase.Running) return;
            StopActiveRoutine();
            round.Interrupt();
            LastResult = CreateResult(ExerciseCompletionStatus.Interrupted);
            ResultReady?.Invoke(LastResult);
            ExitToHome();
        }

        private EdgeSignalsExerciseResult CreateResult(ExerciseCompletionStatus status)
        {
            return new EdgeSignalsExerciseResult(
                exerciseId,
                status,
                round.CompletedTrialCount,
                round.DetectedCount,
                round.MissedCount,
                round.MeanReactionTimeSeconds);
        }

        private void Continue()
        {
            if (phase != ExercisePhase.Completed) return;
            if (sessionManaged)
            {
                phase = ExercisePhase.Inactive;
                root.SetActive(false);
                ContinueRequested?.Invoke();
            }
            else ExitToHome();
        }

        private void ShowIntro()
        {
            phase = ExercisePhase.Intro;
            titleText.gameObject.SetActive(true);
            instructionText.gameObject.SetActive(true);
            desktopHintText.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(false);
            summaryText.gameObject.SetActive(false);
            fixationGraphic.gameObject.SetActive(false);
            stimulusGraphic.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
            introStartButton.gameObject.SetActive(true);
            Select(introStartButton.gameObject);
        }

        private void StopActiveRoutine()
        {
            if (activeRoutine == null) return;
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        private void CreateUi()
        {
            root = new GameObject("Edge Signals Screen", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)root.transform);
            Image background = root.GetComponent<Image>();
            background.color = BackgroundColor;
            background.raycastTarget = false;

            titleText = CreateText("Title", "Sygnały na obrzeżach", 58f, new Vector2(0.12f, 0.63f), new Vector2(0.88f, 0.75f));
            instructionText = CreateText("Instruction", "Patrz cały czas na punkt pośrodku.\nGdy zauważysz sygnał na obrzeżu, naciśnij przycisk odpowiedzi.", 30f, new Vector2(0.13f, 0.43f), new Vector2(0.87f, 0.60f));
            desktopHintText = CreateText("Desktop Hint", "Spacja, Enter lub kliknięcie", 22f, new Vector2(0.25f, 0.36f), new Vector2(0.75f, 0.42f));
            countdownText = CreateText("Countdown", "3", 108f, new Vector2(0.40f, 0.39f), new Vector2(0.60f, 0.59f));
            summaryText = CreateText("Summary", string.Empty, 42f, new Vector2(0.22f, 0.40f), new Vector2(0.78f, 0.70f));

            fixationGraphic = CreateMarker("Fixation", true, new Vector2(0.5f, 0.5f));
            stimulusGraphic = CreateMarker("Peripheral Stimulus", false, new Vector2(0.5f, 0.5f));
            introStartButton = CreateButton("Intro Start", "START", new Vector2(0.5f, 0.29f), new Vector2(300f, 88f));
            interruptButton = CreateButton("Interrupt", "Przerwij", new Vector2(0.5f, 0.08f), new Vector2(250f, 68f));
            continueButton = CreateButton("Continue", "Dalej", new Vector2(0.5f, 0.29f), new Vector2(280f, 76f));
            introStartButton.onClick.AddListener(StartCountdown);
            interruptButton.onClick.AddListener(Interrupt);
            continueButton.onClick.AddListener(Continue);
        }

        private PeripheralMarkerGraphic CreateMarker(string name, bool fixation, Vector2 anchor)
        {
            var markerObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(PeripheralMarkerGraphic));
            markerObject.transform.SetParent(root.transform, false);
            var graphic = markerObject.GetComponent<PeripheralMarkerGraphic>();
            graphic.FixationMarker = fixation;
            graphic.color = fixation ? ForegroundColor : StimulusColor;
            graphic.raycastTarget = false;
            RectTransform rect = graphic.rectTransform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            return graphic;
        }

        private TMP_Text CreateText(string name, string value, float fontSize, Vector2 min, Vector2 max)
        {
            TMP_Text text = Instantiate(textTemplate, root.transform);
            text.name = name;
            text.text = value;
            text.fontSize = fontSize;
            text.color = ForegroundColor;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
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
            TMP_Text labelText = button.GetComponentInChildren<TMP_Text>();
            labelText.text = label;
            labelText.fontSize = 30f;
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            return button;
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
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
