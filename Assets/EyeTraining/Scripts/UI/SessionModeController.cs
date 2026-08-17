using EyeTraining.Core;
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
            SelectedMode = null;
            homeScreen.SetActive(false);
            sessionModeScreen.SetActive(true);
            Select(voiceModeButton.gameObject);
        }

        private void SelectVoiceMode()
        {
            SelectedMode = SessionGuidanceMode.Voice;
            Debug.Log("Wybrano tryb prowadzenia sesji: Voice.");
        }

        private void SelectTextMode()
        {
            SelectedMode = SessionGuidanceMode.Text;
            Debug.Log("Wybrano tryb prowadzenia sesji: Text.");
        }

        private void ReturnHome()
        {
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
