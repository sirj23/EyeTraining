using System;
using EyeTraining.Core;
using EyeTraining.Exercises;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EyeTraining.UI
{
    public sealed class PreparationController : MonoBehaviour
    {
        private static readonly string[] Steps =
        {
            "Usiądź wygodnie i rozluźnij ramiona.",
            "Wykonaj 5 spokojnych, pełnych mrugnięć.",
            "Na chwilę zamknij oczy i rozluźnij twarz.",
            "Spójrz przez moment na odległy punkt, a potem wróć wzrokiem na środek ekranu."
        };

        [SerializeField] private GameObject sessionModeScreen;
        [SerializeField] private GameObject preparationScreen;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text stepIndicatorText;
        [SerializeField] private TMP_Text nextButtonLabel;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button defaultModeButton;
        [FormerlySerializedAs("horizontalTrackingController")]
        [SerializeField] private TrackingExerciseController trackingExerciseController;

        private int currentStepIndex;

        public SessionGuidanceMode GuidanceMode { get; private set; }

        public event Action Completed;

        public event Action ReturnedToModeSelection;

        private void Awake()
        {
            nextButton.onClick.AddListener(GoNext);
            backButton.onClick.AddListener(GoBack);
            preparationScreen.SetActive(false);
        }

        private void OnDestroy()
        {
            nextButton.onClick.RemoveListener(GoNext);
            backButton.onClick.RemoveListener(GoBack);
        }

        public void Begin(SessionGuidanceMode guidanceMode)
        {
            GuidanceMode = guidanceMode;
            currentStepIndex = 0;
            RefreshStep();
            sessionModeScreen.SetActive(false);
            preparationScreen.SetActive(true);
            Select(nextButton.gameObject);
        }

        private void GoNext()
        {
            if (currentStepIndex < Steps.Length - 1)
            {
                currentStepIndex++;
                RefreshStep();
                return;
            }

            preparationScreen.SetActive(false);
            if (Completed != null)
            {
                Completed.Invoke();
            }
            else
            {
                trackingExerciseController.Begin(GuidanceMode);
            }
        }

        private void GoBack()
        {
            if (currentStepIndex > 0)
            {
                currentStepIndex--;
                RefreshStep();
                return;
            }

            preparationScreen.SetActive(false);
            sessionModeScreen.SetActive(true);
            ReturnedToModeSelection?.Invoke();
            Select(defaultModeButton.gameObject);
        }

        private void RefreshStep()
        {
            instructionText.text = Steps[currentStepIndex];
            stepIndicatorText.text = $"{currentStepIndex + 1} z {Steps.Length}";
            nextButtonLabel.text = currentStepIndex == Steps.Length - 1 ? "Rozpocznij" : "Dalej";
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
