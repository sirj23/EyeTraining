using System;
using System.Collections;
using System.Collections.Generic;
using EyeTraining.Core;
using EyeTraining.Sessions.History;
using EyeTraining.Sessions.Progression.Saccades;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeTraining.Exercises.Saccades
{
    public sealed class NumberJourneyController : MonoBehaviour
    {
        private enum ExercisePhase
        {
            Inactive,
            Intro,
            Countdown,
            Demonstration,
            Pause,
            Anticipation,
            Completed
        }

        private const float ActiveScale = 1.12f;

        private static readonly Color BackgroundColor = new(0.025f, 0.04f, 0.065f, 1f);
        private static readonly Color NeutralNumberColor = new(0.60f, 0.68f, 0.76f, 1f);
        private static readonly Color ActiveNumberColor = new(0.94f, 0.97f, 1f, 1f);

        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject trackingExerciseScreen;
        [SerializeField] private TMP_Text textTemplate;
        [SerializeField] private Button buttonTemplate;
        [SerializeField] private Button homeStartTrainingButton;

        [Header("Debug")]
        [SerializeField] private int debugSequenceSeed = 1;
        [Range(0, 5)]
        [SerializeField] private int debugNumberJourneyLevel;
        [SerializeField] private bool debugShowSequence;

        private readonly List<TMP_Text> numberTexts = new();

        private GameObject root;
        private Image background;
        private TMP_Text titleText;
        private TMP_Text instructionText;
        private TMP_Text countdownText;
        private TMP_Text phaseText;
        private TMP_Text summaryText;
        private Button introStartButton;
        private Button interruptButton;
        private Button continueButton;
        private NumberJourneyLayout layout;
        private NumberJourneySequence sequence;
        private NumberJourneyLevelSettings settings;
        private Coroutine activeSequence;
        private ExercisePhase phase;
        private string exerciseId;
        private bool sessionManaged;

        public SaccadesExerciseResult LastResult { get; private set; }

        public int DebugSequenceSeed => debugSequenceSeed;

        public int DebugNumberJourneyLevel => debugNumberJourneyLevel;

        public event Action<SaccadesExerciseResult> ResultReady;

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

        public void Begin(
            SessionGuidanceMode guidanceMode,
            string currentExerciseId,
            int sequenceSeed,
            NumberJourneyLevelSettings levelSettings)
        {
            if (string.IsNullOrWhiteSpace(currentExerciseId))
            {
                throw new ArgumentException(
                    "Exercise ID cannot be empty.",
                    nameof(currentExerciseId));
            }

            settings = levelSettings ?? throw new ArgumentNullException(nameof(levelSettings));

            _ = guidanceMode;
            exerciseId = currentExerciseId;
            sessionManaged = true;
            LastResult = null;
            StopSequence();

            RectTransform canvasRect = (RectTransform)canvas.transform;
            float aspectRatio = canvasRect.rect.width / canvasRect.rect.height;
            layout = NumberJourneyLayout.Create(
                sequenceSeed,
                aspectRatio,
                settings.NumberCount);
            sequence = NumberJourneySequence.Create(
                sequenceSeed,
                layout,
                aspectRatio,
                settings.SequenceLength,
                settings.PreferredMinimumJump);
            ApplyLayout();

            if (debugShowSequence)
            {
                Debug.Log("[NumberJourney] Sequence: "
                    + string.Join(" -> ", sequence.Numbers));
            }

            trackingExerciseScreen.SetActive(false);
            homeScreen.SetActive(false);
            root.SetActive(true);
            SetIntroVisible(true);
            SetNumbersVisible(false);
            SetSummaryVisible(false);
            phaseText.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(false);
            phase = ExercisePhase.Intro;
            Select(introStartButton.gameObject);
        }

        private void StartCountdown()
        {
            if (phase != ExercisePhase.Intro || activeSequence != null)
            {
                return;
            }

            introStartButton.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            phase = ExercisePhase.Countdown;
            activeSequence = StartCoroutine(RunExercise());
        }

        private IEnumerator RunExercise()
        {
            countdownText.gameObject.SetActive(true);
            for (var number = 3; number >= 1; number--)
            {
                countdownText.text = number.ToString();
                yield return new WaitForSecondsRealtime(
                    NumberJourneyLevelSettings.CountdownStepDuration);
            }

            countdownText.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            SetNumbersVisible(true);
            interruptButton.gameObject.SetActive(true);

            phase = ExercisePhase.Demonstration;
            yield return PlaySequence();

            phase = ExercisePhase.Pause;
            phaseText.text = "Teraz wyprzedzaj wzrokiem.";
            phaseText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(
                NumberJourneyLevelSettings.BetweenPhasesDuration);
            phaseText.gameObject.SetActive(false);

            phase = ExercisePhase.Anticipation;
            yield return PlaySequence();

            CompleteExercise();
            activeSequence = null;
        }

        private IEnumerator PlaySequence()
        {
            for (var index = 0; index < sequence.Numbers.Count; index++)
            {
                TMP_Text numberText = numberTexts[sequence.Numbers[index] - 1];
                SetNumberActive(numberText, true);
                yield return new WaitForSecondsRealtime(settings.ActiveDuration);
                SetNumberActive(numberText, false);
                if (index < sequence.Numbers.Count - 1)
                {
                    yield return new WaitForSecondsRealtime(settings.GapDuration);
                }
            }
        }

        private void CompleteExercise()
        {
            phase = ExercisePhase.Completed;
            LastResult = new SaccadesExerciseResult(
                exerciseId,
                ExerciseCompletionStatus.Completed);
            SetNumbersVisible(false);
            interruptButton.gameObject.SetActive(false);
            summaryText.text = "Ćwiczenie zakończone";
            SetSummaryVisible(true);
            Select(continueButton.gameObject);
            ResultReady?.Invoke(LastResult);
        }

        private void Interrupt()
        {
            if (phase != ExercisePhase.Demonstration
                && phase != ExercisePhase.Pause
                && phase != ExercisePhase.Anticipation)
            {
                return;
            }

            StopSequence();
            LastResult = new SaccadesExerciseResult(
                exerciseId,
                ExerciseCompletionStatus.Interrupted);
            ResultReady?.Invoke(LastResult);
            ExitToHome();
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
            ResetNumberAppearance();
            phase = ExercisePhase.Inactive;
            sessionManaged = false;
            root.SetActive(false);
            homeScreen.SetActive(true);
            Select(homeStartTrainingButton.gameObject);
        }

        private void ApplyLayout()
        {
            for (var index = 0; index < layout.Items.Count; index++)
            {
                NumberJourneyLayoutItem item = layout.Items[index];
                TMP_Text text = numberTexts[index];
                RectTransform rect = text.rectTransform;
                rect.anchorMin = rect.anchorMax = item.ViewportPosition;
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.Euler(0f, 0f, item.RotationDegrees);
                SetNumberActive(text, false);
            }

            for (var index = layout.Items.Count; index < numberTexts.Count; index++)
            {
                numberTexts[index].gameObject.SetActive(false);
            }
        }

        private static void SetNumberActive(TMP_Text text, bool active)
        {
            text.color = active ? ActiveNumberColor : NeutralNumberColor;
            text.rectTransform.localScale = active
                ? Vector3.one * ActiveScale
                : Vector3.one;
        }

        private void ResetNumberAppearance()
        {
            for (var index = 0; index < numberTexts.Count; index++)
            {
                SetNumberActive(numberTexts[index], false);
            }
        }

        private void SetIntroVisible(bool visible)
        {
            titleText.gameObject.SetActive(visible);
            instructionText.gameObject.SetActive(visible);
            introStartButton.gameObject.SetActive(visible);
            countdownText.gameObject.SetActive(false);
        }

        private void SetNumbersVisible(bool visible)
        {
            for (var index = 0; index < numberTexts.Count; index++)
            {
                numberTexts[index].gameObject.SetActive(
                    visible && index < settings.NumberCount);
            }
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
            root = new GameObject(
                "Number Journey Screen",
                typeof(RectTransform),
                typeof(Image));
            root.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = (RectTransform)root.transform;
            Stretch(rootRect);
            background = root.GetComponent<Image>();
            background.color = BackgroundColor;

            titleText = CreateText(
                "Title",
                "Wędrówka wśród liczb",
                58f,
                new Vector2(0.12f, 0.61f),
                new Vector2(0.88f, 0.73f));
            instructionText = CreateText(
                "Instruction",
                "Zapamiętaj kolejność, a potem spróbuj wyprzedzać wzrokiem kolejne liczby.",
                30f,
                new Vector2(0.14f, 0.47f),
                new Vector2(0.86f, 0.58f));
            countdownText = CreateText(
                "Countdown",
                "3",
                108f,
                new Vector2(0.40f, 0.39f),
                new Vector2(0.60f, 0.59f));
            phaseText = CreateText(
                "Phase Message",
                string.Empty,
                28f,
                new Vector2(0.30f, 0.14f),
                new Vector2(0.70f, 0.21f));
            summaryText = CreateText(
                "Summary",
                string.Empty,
                44f,
                new Vector2(0.22f, 0.44f),
                new Vector2(0.78f, 0.64f));

            for (var number = 1; number <= NumberJourneyLayout.MaximumNumberCount; number++)
            {
                TMP_Text numberText = CreateText(
                    $"Number {number}",
                    number.ToString(),
                    76f,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f));
                numberText.textWrappingMode = TextWrappingModes.NoWrap;
                numberText.overflowMode = TextOverflowModes.Overflow;
                numberText.rectTransform.sizeDelta = new Vector2(160f, 100f);
                numberTexts.Add(numberText);
            }

            introStartButton = CreateButton(
                "Intro Start",
                "START",
                new Vector2(0.5f, 0.34f),
                new Vector2(300f, 88f));
            interruptButton = CreateButton(
                "Interrupt",
                "Przerwij",
                new Vector2(0.5f, 0.08f),
                new Vector2(250f, 68f));
            continueButton = CreateButton(
                "Continue",
                "Dalej",
                new Vector2(0.5f, 0.31f),
                new Vector2(280f, 76f));

            introStartButton.onClick.AddListener(StartCountdown);
            interruptButton.onClick.AddListener(Interrupt);
            continueButton.onClick.AddListener(Continue);
        }

        private TMP_Text CreateText(
            string name,
            string value,
            float fontSize,
            Vector2 min,
            Vector2 max)
        {
            TMP_Text text = Instantiate(textTemplate, root.transform);
            text.name = name;
            text.text = value;
            text.fontSize = fontSize;
            text.color = ActiveNumberColor;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            return text;
        }

        private Button CreateButton(
            string name,
            string label,
            Vector2 anchor,
            Vector2 size)
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
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}
