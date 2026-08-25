using System;
using System.Collections;
using System.Collections.Generic;
using EyeTraining.Core;
using EyeTraining.Sessions.History;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeTraining.Exercises.VisualSearch
{
    public sealed class ShapeSearchController : MonoBehaviour
    {
        private enum ExercisePhase
        {
            Inactive,
            Intro,
            Countdown,
            Running,
            Completed
        }

        private const float CountdownStepDuration = 0.7f;
        private const float IncorrectFeedbackDuration = 0.25f;
        private const float BoardShapeHeightFraction = 0.066f;
        private const float PatternHeightFraction = 0.088f;

        private static readonly Color BackgroundColor = new(0.025f, 0.04f, 0.065f, 1f);
        private static readonly Color NeutralShapeColor = new(0.66f, 0.72f, 0.79f, 1f);
        private static readonly Color SelectedShapeColor = new(0.63f, 0.82f, 0.95f, 1f);
        private static readonly Color IncorrectShapeColor = new(0.84f, 0.68f, 0.54f, 1f);
        private static readonly Color PanelColor = new(0.08f, 0.12f, 0.18f, 0.92f);

        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject trackingExerciseScreen;
        [SerializeField] private TMP_Text textTemplate;
        [SerializeField] private Button buttonTemplate;
        [SerializeField] private Button homeStartTrainingButton;

        [Header("Debug")]
        [SerializeField] private int debugShapeSearchSeed = 1;
        [SerializeField] private bool debugShowTargetDistribution;

        private readonly List<BoardItemView> boardItems = new();
        private GameObject root;
        private TMP_Text titleText;
        private TMP_Text instructionText;
        private TMP_Text countdownText;
        private TMP_Text foundText;
        private TMP_Text summaryText;
        private GameObject patternPanel;
        private ShapeSearchGraphic patternGraphic;
        private Button introStartButton;
        private Button interruptButton;
        private Button continueButton;
        private ShapeSearchRound round;
        private ShapeSearchProgress progress;
        private Coroutine activeRoutine;
        private ExercisePhase phase;
        private string exerciseId;
        private bool sessionManaged;

        public int DebugShapeSearchSeed => debugShapeSearchSeed;

        public ShapeSearchExerciseResult LastResult { get; private set; }

        public event Action<ShapeSearchExerciseResult> ResultReady;

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
            int seed)
        {
            if (string.IsNullOrWhiteSpace(currentExerciseId))
            {
                throw new ArgumentException("Exercise ID cannot be empty.", nameof(currentExerciseId));
            }

            _ = guidanceMode;
            exerciseId = currentExerciseId;
            sessionManaged = true;
            LastResult = null;
            StopActiveRoutine();

            RectTransform canvasRect = (RectTransform)canvas.transform;
            float aspectRatio = canvasRect.rect.width / canvasRect.rect.height;
            ShapeSearchLayout layout = ShapeSearchLayout.Create(seed, aspectRatio);
            round = ShapeSearchRound.Create(seed, layout);
            progress = new ShapeSearchProgress(round);
            ApplyRound(canvasRect.rect.height);

            if (debugShowTargetDistribution)
            {
                var targets = new List<string>();
                for (var index = 0; index < round.Items.Count; index++)
                {
                    if (round.Items[index].IsTarget)
                    {
                        targets.Add($"{index}@R{round.Items[index].Region}");
                    }
                }

                Debug.Log(
                    $"[ShapeSearch] Seed {seed}, target {round.TargetShape}, targets: "
                    + string.Join(", ", targets));
            }

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
            if (phase != ExercisePhase.Intro || activeRoutine != null)
            {
                return;
            }

            introStartButton.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            phase = ExercisePhase.Countdown;
            activeRoutine = StartCoroutine(RunCountdown());
        }

        private IEnumerator RunCountdown()
        {
            countdownText.gameObject.SetActive(true);
            for (var number = 3; number >= 1; number--)
            {
                countdownText.text = number.ToString();
                yield return new WaitForSecondsRealtime(CountdownStepDuration);
            }

            countdownText.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            SetRunningUiVisible(true);
            phase = ExercisePhase.Running;
            activeRoutine = null;
            Select(boardItems[0].Button.gameObject);
        }

        private void HandleSelection(int itemIndex)
        {
            if (phase != ExercisePhase.Running)
            {
                return;
            }

            ShapeSearchSelectionOutcome outcome = progress.Select(itemIndex);
            BoardItemView view = boardItems[itemIndex];
            switch (outcome)
            {
                case ShapeSearchSelectionOutcome.Correct:
                    MarkSelected(view);
                    UpdateFoundText();
                    break;
                case ShapeSearchSelectionOutcome.RoundCompleted:
                    MarkSelected(view);
                    UpdateFoundText();
                    CompleteExercise();
                    break;
                case ShapeSearchSelectionOutcome.Incorrect:
                    StartCoroutine(ShowIncorrectFeedback(view));
                    break;
                case ShapeSearchSelectionOutcome.AlreadySelected:
                    break;
            }
        }

        private IEnumerator ShowIncorrectFeedback(BoardItemView view)
        {
            view.Graphic.color = IncorrectShapeColor;
            yield return new WaitForSecondsRealtime(IncorrectFeedbackDuration);
            if (phase == ExercisePhase.Running && !progress.IsTargetSelected(view.Index))
            {
                view.Graphic.color = NeutralShapeColor;
            }
        }

        private void MarkSelected(BoardItemView view)
        {
            view.Graphic.color = SelectedShapeColor;
            view.Graphic.rectTransform.localScale = Vector3.one * 1.08f;
            view.Button.interactable = false;
        }

        private void CompleteExercise()
        {
            phase = ExercisePhase.Completed;
            SetBoardInteractable(false);
            SetRunningUiVisible(false);
            LastResult = new ShapeSearchExerciseResult(
                exerciseId,
                ExerciseCompletionStatus.Completed,
                progress.CorrectSelections,
                progress.ErrorCount,
                ShapeSearchRound.TargetCount);
            summaryText.text =
                "Ćwiczenie zakończone\n\n"
                + $"Znalezione: {progress.CorrectSelections}/{ShapeSearchRound.TargetCount}\n"
                + $"Błędy: {progress.ErrorCount}";
            summaryText.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(true);
            Select(continueButton.gameObject);
            ResultReady?.Invoke(LastResult);
        }

        private void Interrupt()
        {
            if (phase != ExercisePhase.Running)
            {
                return;
            }

            LastResult = new ShapeSearchExerciseResult(
                exerciseId,
                ExerciseCompletionStatus.Interrupted,
                progress.CorrectSelections,
                progress.ErrorCount,
                ShapeSearchRound.TargetCount);
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

        private void ApplyRound(float canvasHeight)
        {
            float itemSize = canvasHeight * BoardShapeHeightFraction;
            float patternSize = canvasHeight * PatternHeightFraction;
            patternGraphic.Shape = round.TargetShape;
            patternGraphic.rectTransform.sizeDelta = Vector2.one * patternSize;

            for (var index = 0; index < round.Items.Count; index++)
            {
                ShapeSearchRoundItem item = round.Items[index];
                BoardItemView view = boardItems[index];
                view.Graphic.Shape = item.Shape;
                view.Graphic.color = NeutralShapeColor;
                view.Graphic.rectTransform.localScale = Vector3.one;
                RectTransform rect = view.Graphic.rectTransform;
                rect.anchorMin = rect.anchorMax = item.ViewportPosition;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.one * itemSize;
                view.Button.interactable = true;
            }

            UpdateFoundText();
        }

        private void ShowIntro()
        {
            phase = ExercisePhase.Intro;
            titleText.gameObject.SetActive(true);
            instructionText.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(false);
            summaryText.gameObject.SetActive(false);
            introStartButton.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(false);
            SetRunningUiVisible(false);
            Select(introStartButton.gameObject);
        }

        private void SetRunningUiVisible(bool visible)
        {
            patternPanel.SetActive(visible);
            foundText.gameObject.SetActive(visible);
            interruptButton.gameObject.SetActive(visible);
            for (var index = 0; index < boardItems.Count; index++)
            {
                boardItems[index].Graphic.gameObject.SetActive(visible);
            }
        }

        private void SetBoardInteractable(bool interactable)
        {
            for (var index = 0; index < boardItems.Count; index++)
            {
                boardItems[index].Button.interactable = interactable;
            }
        }

        private void UpdateFoundText()
        {
            foundText.text = $"Znalezione: {progress.CorrectSelections}/{ShapeSearchRound.TargetCount}";
        }

        private void StopActiveRoutine()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }
        }

        private void CreateUi()
        {
            root = new GameObject("Shape Search Screen", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = BackgroundColor;

            titleText = CreateText(
                "Title", "Znajdź kształt", 58f,
                new Vector2(0.12f, 0.61f), new Vector2(0.88f, 0.73f));
            instructionText = CreateText(
                "Instruction",
                "Znajdź wszystkie kształty takie jak wzorzec.\nKlikaj znalezione obiekty.",
                30f,
                new Vector2(0.14f, 0.44f), new Vector2(0.86f, 0.58f));
            countdownText = CreateText(
                "Countdown", "3", 108f,
                new Vector2(0.40f, 0.39f), new Vector2(0.60f, 0.59f));
            foundText = CreateText(
                "Found", "Znalezione: 0/4", 28f,
                new Vector2(0.72f, 0.87f), new Vector2(0.94f, 0.94f));
            summaryText = CreateText(
                "Summary", string.Empty, 42f,
                new Vector2(0.22f, 0.40f), new Vector2(0.78f, 0.70f));

            CreatePatternPanel();
            for (var index = 0; index < ShapeSearchLayout.ItemCount; index++)
            {
                CreateBoardItem(index);
            }

            introStartButton = CreateButton(
                "Intro Start", "START", new Vector2(0.5f, 0.32f), new Vector2(300f, 88f));
            interruptButton = CreateButton(
                "Interrupt", "Przerwij", new Vector2(0.5f, 0.08f), new Vector2(250f, 68f));
            continueButton = CreateButton(
                "Continue", "Dalej", new Vector2(0.5f, 0.29f), new Vector2(280f, 76f));

            introStartButton.onClick.AddListener(StartCountdown);
            interruptButton.onClick.AddListener(Interrupt);
            continueButton.onClick.AddListener(Continue);
        }

        private void CreatePatternPanel()
        {
            patternPanel = new GameObject("Pattern Panel", typeof(RectTransform), typeof(Image));
            patternPanel.transform.SetParent(root.transform, false);
            RectTransform panelRect = (RectTransform)patternPanel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.83f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(300f, 112f);
            patternPanel.GetComponent<Image>().color = PanelColor;

            TMP_Text label = Instantiate(textTemplate, patternPanel.transform);
            label.name = "Pattern Label";
            label.text = "Znajdź:";
            label.fontSize = 30f;
            label.color = NeutralShapeColor;
            label.alignment = TextAlignmentOptions.Center;
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.55f, 1f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;

            var graphicObject = new GameObject(
                "Pattern Shape",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(ShapeSearchGraphic));
            graphicObject.transform.SetParent(patternPanel.transform, false);
            patternGraphic = graphicObject.GetComponent<ShapeSearchGraphic>();
            patternGraphic.color = SelectedShapeColor;
            patternGraphic.raycastTarget = false;
            RectTransform graphicRect = patternGraphic.rectTransform;
            graphicRect.anchorMin = graphicRect.anchorMax = new Vector2(0.76f, 0.5f);
            graphicRect.pivot = new Vector2(0.5f, 0.5f);
            graphicRect.anchoredPosition = Vector2.zero;
        }

        private void CreateBoardItem(int index)
        {
            var itemObject = new GameObject(
                $"Board Shape {index + 1}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(ShapeSearchGraphic),
                typeof(Button));
            itemObject.transform.SetParent(root.transform, false);
            ShapeSearchGraphic graphic = itemObject.GetComponent<ShapeSearchGraphic>();
            Button button = itemObject.GetComponent<Button>();
            graphic.raycastTarget = true;
            button.targetGraphic = graphic;
            int capturedIndex = index;
            button.onClick.AddListener(() => HandleSelection(capturedIndex));
            boardItems.Add(new BoardItemView(index, graphic, button));
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
            text.color = NeutralShapeColor;
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

        private sealed class BoardItemView
        {
            public BoardItemView(int index, ShapeSearchGraphic graphic, Button button)
            {
                Index = index;
                Graphic = graphic;
                Button = button;
            }

            public int Index { get; }

            public ShapeSearchGraphic Graphic { get; }

            public Button Button { get; }
        }
    }
}
