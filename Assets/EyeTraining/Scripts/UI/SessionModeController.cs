using EyeTraining.Core;
using EyeTraining.Sessions.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeTraining.UI
{
    public sealed class SessionModeController : MonoBehaviour
    {
        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject sessionModeScreen;
        [SerializeField] private Button startTrainingButton;
        [SerializeField] private Button voiceModeButton;
        [SerializeField] private Button textModeButton;
        [SerializeField] private Button backButton;
        [SerializeField] private PreparationController preparationController;
        [SerializeField] private SessionRuntimeController sessionRuntimeController;

        public SessionGuidanceMode? SelectedMode { get; private set; }

        private void Awake()
        {
            startTrainingButton.onClick.AddListener(ShowSessionMode);
            voiceModeButton.onClick.AddListener(SelectVoiceMode);
            textModeButton.onClick.AddListener(SelectTextMode);
            backButton.onClick.AddListener(ReturnHome);
            sessionModeScreen.SetActive(false);
        }

        private void OnDestroy()
        {
            startTrainingButton.onClick.RemoveListener(ShowSessionMode);
            voiceModeButton.onClick.RemoveListener(SelectVoiceMode);
            textModeButton.onClick.RemoveListener(SelectTextMode);
            backButton.onClick.RemoveListener(ReturnHome);
        }

        private void ShowSessionMode()
        {
            if (sessionRuntimeController != null && !sessionRuntimeController.PrepareSession())
            {
                return;
            }

            SelectedMode = null;
            homeScreen.SetActive(false);
            sessionModeScreen.SetActive(true);
            Select(voiceModeButton.gameObject);
        }

        private void SelectVoiceMode()
        {
            SelectedMode = SessionGuidanceMode.Voice;
            StartSelectedMode();
        }

        private void SelectTextMode()
        {
            SelectedMode = SessionGuidanceMode.Text;
            StartSelectedMode();
        }

        private void StartSelectedMode()
        {
            if (sessionRuntimeController != null && sessionRuntimeController.HasPreparedSession)
            {
                sessionRuntimeController.StartPreparedSession(SelectedMode.Value);
                return;
            }

            preparationController.Begin(SelectedMode.Value);
        }

        private void ReturnHome()
        {
            sessionRuntimeController?.AbortSession();
            sessionModeScreen.SetActive(false);
            homeScreen.SetActive(true);
            Select(startTrainingButton.gameObject);
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
