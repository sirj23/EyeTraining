using System.Collections.Generic;
using EyeTraining.Profiles;
using EyeTraining.Save;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeTraining.UI
{
    public sealed class ProfileScreenController : MonoBehaviour
    {
        [Header("Ekran wyboru profilu")]
        [SerializeField] private GameObject profileSelectionScreen;
        [SerializeField] private Transform profileTilesContainer;
        [SerializeField] private Button profileTileTemplate;
        [SerializeField] private Button addProfileButton;

        [Header("Ekran dodawania profilu")]
        [SerializeField] private GameObject addProfileScreen;
        [SerializeField] private TMP_InputField profileNameInput;
        [SerializeField] private TMP_Text categoryPrompt;
        [SerializeField] private TMP_Text validationMessage;
        [SerializeField] private Button childButton;
        [SerializeField] private Button teenButton;
        [SerializeField] private Button adultButton;
        [SerializeField] private Button seniorButton;
        [SerializeField] private Button createProfileButton;
        [SerializeField] private Button backButton;

        private readonly List<GameObject> _generatedProfileTiles = new List<GameObject>();

        private IProfileRepository _profileRepository;
        private ProfileCategory? _selectedCategory;

        private void Awake()
        {
            _profileRepository = new JsonProfileRepository();
            BindActions();
            ShowProfileSelection();
        }

        private void OnDestroy()
        {
            UnbindActions();
        }

        private void BindActions()
        {
            addProfileButton.onClick.AddListener(ShowAddProfile);
            childButton.onClick.AddListener(SelectChild);
            teenButton.onClick.AddListener(SelectTeen);
            adultButton.onClick.AddListener(SelectAdult);
            seniorButton.onClick.AddListener(SelectSenior);
            createProfileButton.onClick.AddListener(CreateProfile);
            backButton.onClick.AddListener(ShowProfileSelection);
        }

        private void UnbindActions()
        {
            addProfileButton.onClick.RemoveListener(ShowAddProfile);
            childButton.onClick.RemoveListener(SelectChild);
            teenButton.onClick.RemoveListener(SelectTeen);
            adultButton.onClick.RemoveListener(SelectAdult);
            seniorButton.onClick.RemoveListener(SelectSenior);
            createProfileButton.onClick.RemoveListener(CreateProfile);
            backButton.onClick.RemoveListener(ShowProfileSelection);
        }

        private void ShowProfileSelection()
        {
            addProfileScreen.SetActive(false);
            profileSelectionScreen.SetActive(true);
            RefreshProfileTiles();
            Select(addProfileButton.gameObject);
        }

        private void ShowAddProfile()
        {
            ResetForm();
            profileSelectionScreen.SetActive(false);
            addProfileScreen.SetActive(true);
            Select(profileNameInput.gameObject);
        }

        private void RefreshProfileTiles()
        {
            foreach (GameObject tile in _generatedProfileTiles)
            {
                Destroy(tile);
            }

            _generatedProfileTiles.Clear();

            IReadOnlyList<UserProfile> profiles = _profileRepository.GetAll();
            foreach (UserProfile profile in profiles)
            {
                Button tile = Instantiate(profileTileTemplate, profileTilesContainer);
                tile.name = $"Profile {profile.Id}";
                tile.GetComponentInChildren<TMP_Text>(true).text = profile.DisplayName;
                tile.transform.SetSiblingIndex(addProfileButton.transform.GetSiblingIndex());
                tile.gameObject.SetActive(true);
                _generatedProfileTiles.Add(tile.gameObject);
            }
        }

        private void CreateProfile()
        {
            string displayName = profileNameInput.text.Trim();

            if (string.IsNullOrEmpty(displayName))
            {
                ShowValidationMessage("Wpisz nazwę profilu.");
                Select(profileNameInput.gameObject);
                return;
            }

            if (!_selectedCategory.HasValue)
            {
                ShowValidationMessage("Wybierz kategorię profilu.");
                Select(childButton.gameObject);
                return;
            }

            var profile = new UserProfile(displayName, _selectedCategory.Value);
            if (!_profileRepository.Save(profile))
            {
                ShowValidationMessage("Nie udało się zapisać profilu.");
                return;
            }

            ShowProfileSelection();
        }

        private void SelectChild()
        {
            SelectCategory(ProfileCategory.Child, "Dziecko");
        }

        private void SelectTeen()
        {
            SelectCategory(ProfileCategory.Teen, "Nastolatek");
        }

        private void SelectAdult()
        {
            SelectCategory(ProfileCategory.Adult, "Dorosły");
        }

        private void SelectSenior()
        {
            SelectCategory(ProfileCategory.Senior, "Senior");
        }

        private void SelectCategory(ProfileCategory category, string displayName)
        {
            _selectedCategory = category;
            categoryPrompt.text = $"Wybrana kategoria: {displayName}";
            validationMessage.text = string.Empty;
        }

        private void ResetForm()
        {
            profileNameInput.text = string.Empty;
            _selectedCategory = null;
            categoryPrompt.text = "Wybierz kategorię";
            validationMessage.text = string.Empty;
        }

        private void ShowValidationMessage(string message)
        {
            validationMessage.text = message;
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
