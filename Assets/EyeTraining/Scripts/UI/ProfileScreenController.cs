using System;
using System.Collections.Generic;
using EyeTraining.Exercises.Landolt;
using EyeTraining.Profiles;
using EyeTraining.Save;
using EyeTraining.Sessions.History;
using EyeTraining.Sessions.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeTraining.UI
{
    public sealed class ProfileScreenController : MonoBehaviour
    {
        private const string PlaceholderRankName = "Pierwsze spojrzenie";
        private const float PlaceholderRankProgress = 0.15f;

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

        [Header("Ekran główny")]
        [SerializeField] private GameObject homeScreen;
        [SerializeField] private TMP_Text homeProfileName;
        [SerializeField] private TMP_Text homeRankName;
        [SerializeField] private RectTransform homeRankProgressFill;
        [SerializeField] private TMP_Text homeTrainingTitle;
        [SerializeField] private TMP_Text homeTrainingDuration;
        [SerializeField] private Button startTrainingButton;
        [SerializeField] private Button changeProfileButton;
        [SerializeField] private SessionRuntimeController sessionRuntimeController;

        private readonly List<GameObject> _generatedProfileTiles = new List<GameObject>();

        private IProfileRepository _profileRepository;
        private ProfileCategory? _selectedCategory;
        private TMP_Text _homeTrainingDay;
        private TMP_Text _homePlan;
        private GameObject _profilePreferencesPanel;
        private Button _profilePreferencesButton;
        private Button _darkBackgroundButton;
        private Button _lightBackgroundButton;
        private Button _preferencesBackButton;
        private TMP_Text _preferencesMessage;

        public UserProfile ActiveProfile { get; private set; }

        private void Awake()
        {
            _profileRepository = new JsonProfileRepository();
            CreateHomePlanPresentation();
            BindActions();
            if (sessionRuntimeController != null)
            {
                sessionRuntimeController.PreparedSessionChanged += RefreshHome;
            }
            ShowProfileSelection();
        }

        private void OnDestroy()
        {
            UnbindActions();
            UnbindPreferenceActions();
            if (sessionRuntimeController != null)
            {
                sessionRuntimeController.PreparedSessionChanged -= RefreshHome;
            }
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
            changeProfileButton.onClick.AddListener(ShowProfileSelection);
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
            changeProfileButton.onClick.RemoveListener(ShowProfileSelection);
        }

        private void ShowProfileSelection()
        {
            _profilePreferencesPanel?.SetActive(false);
            ActiveProfile = null;
            sessionRuntimeController?.ClearPreparedSession();
            addProfileScreen.SetActive(false);
            homeScreen.SetActive(false);
            profileSelectionScreen.SetActive(true);
            Button firstProfileTile = RefreshProfileTiles();
            Select(firstProfileTile != null ? firstProfileTile.gameObject : addProfileButton.gameObject);
        }

        private void ShowAddProfile()
        {
            ResetForm();
            profileSelectionScreen.SetActive(false);
            addProfileScreen.SetActive(true);
            Select(profileNameInput.gameObject);
        }

        private Button RefreshProfileTiles()
        {
            foreach (GameObject tile in _generatedProfileTiles)
            {
                Destroy(tile);
            }

            _generatedProfileTiles.Clear();

            Button firstProfileTile = null;
            IReadOnlyList<UserProfile> profiles = _profileRepository.GetAll();
            foreach (UserProfile profile in profiles)
            {
                Button tile = Instantiate(profileTileTemplate, profileTilesContainer);
                tile.name = $"Profile {profile.Id}";
                tile.GetComponentInChildren<TMP_Text>(true).text = profile.DisplayName;
                UserProfile selectedProfile = profile;
                tile.onClick.AddListener(() => ShowHome(selectedProfile));
                tile.transform.SetSiblingIndex(addProfileButton.transform.GetSiblingIndex());
                tile.gameObject.SetActive(true);
                _generatedProfileTiles.Add(tile.gameObject);
                firstProfileTile ??= tile;
            }

            return firstProfileTile;
        }

        private void ShowHome(UserProfile profile)
        {
            ActiveProfile = profile;
            _profilePreferencesPanel.SetActive(false);
            homeProfileName.text = profile.DisplayName;

            // Wartości demonstracyjne UI. Nie są częścią modelu profilu ani systemu XP.
            homeRankName.text = PlaceholderRankName;
            Vector2 progressMax = homeRankProgressFill.anchorMax;
            progressMax.x = PlaceholderRankProgress;
            homeRankProgressFill.anchorMax = progressMax;

            profileSelectionScreen.SetActive(false);
            addProfileScreen.SetActive(false);
            homeScreen.SetActive(true);
            RefreshHome();
            Select(startTrainingButton.gameObject);
        }

        private void ShowProfilePreferences()
        {
            if (ActiveProfile == null)
            {
                return;
            }

            _preferencesMessage.text = string.Empty;
            RefreshBackgroundModeSelection();
            _profilePreferencesPanel.SetActive(true);
            Select(ActiveProfile.LandoltBackgroundMode == LandoltBackgroundMode.Dark
                ? _darkBackgroundButton.gameObject
                : _lightBackgroundButton.gameObject);
        }

        private void HideProfilePreferences()
        {
            _profilePreferencesPanel.SetActive(false);
            Select(_profilePreferencesButton.gameObject);
        }

        private void SelectDarkBackground()
        {
            SaveLandoltBackgroundMode(LandoltBackgroundMode.Dark);
        }

        private void SelectLightBackground()
        {
            SaveLandoltBackgroundMode(LandoltBackgroundMode.Light);
        }

        private void SaveLandoltBackgroundMode(LandoltBackgroundMode mode)
        {
            if (ActiveProfile == null || ActiveProfile.LandoltBackgroundMode == mode)
            {
                RefreshBackgroundModeSelection();
                return;
            }

            UserProfile previousProfile = ActiveProfile;
            var updatedProfile = new UserProfile(
                previousProfile.Id,
                previousProfile.DisplayName,
                previousProfile.Category,
                mode);
            if (!_profileRepository.Save(updatedProfile))
            {
                ActiveProfile = previousProfile;
                _preferencesMessage.text = "Nie udało się zapisać ustawienia.";
                RefreshBackgroundModeSelection();
                Debug.LogError(
                    $"Nie udało się zapisać preferencji Landolta dla profilu "
                    + $"'{previousProfile.Id}'.");
                return;
            }

            ActiveProfile = updatedProfile;
            sessionRuntimeController?.UpdatePreparedProfile(updatedProfile);
            _preferencesMessage.text = string.Empty;
            RefreshBackgroundModeSelection();
        }

        private void RefreshBackgroundModeSelection()
        {
            bool dark = ActiveProfile?.LandoltBackgroundMode == LandoltBackgroundMode.Dark;
            SetPreferenceButtonState(_darkBackgroundButton, "Ciemne", dark);
            SetPreferenceButtonState(_lightBackgroundButton, "Jasne", !dark);
        }

        private static void SetPreferenceButtonState(
            Button button,
            string label,
            bool selected)
        {
            button.GetComponentInChildren<TMP_Text>().text = selected
                ? $"{label} (wybrane)"
                : label;
            button.GetComponent<Image>().color = selected
                ? new Color(0.24f, 0.42f, 0.53f, 1f)
                : new Color(0.18f, 0.25f, 0.34f, 1f);
        }

        public void RefreshHome()
        {
            if (ActiveProfile == null)
            {
                return;
            }

            if (sessionRuntimeController == null || !sessionRuntimeController.PrepareSession())
            {
                ShowPlanError();
                return;
            }

            TrainingHistorySnapshot snapshot = sessionRuntimeController.PendingSnapshot;
            int trainingDay = TrainingDayCalculator.Calculate(
                snapshot.State.TrainingStartDate,
                DateTimeOffset.Now);
            _homeTrainingDay.text = $"Dzień treningu {trainingDay}";
            homeTrainingTitle.text = "Dzisiejszy trening";

            double totalSeconds = sessionRuntimeController.CurrentPlan.EstimatedDuration.TotalSeconds;
            int estimatedMinutes = Math.Max(1, (int)Math.Ceiling(totalSeconds / 60d));
            homeTrainingDuration.text = $"około {estimatedMinutes} min";

            var lines = new List<string>(sessionRuntimeController.CurrentPlan.Exercises.Count);
            for (var index = 0; index < sessionRuntimeController.CurrentPlan.Exercises.Count; index++)
            {
                lines.Add(sessionRuntimeController.CurrentPlan.Exercises[index].Definition.DisplayName);
            }

            _homePlan.text = string.Join("\n", lines);
            startTrainingButton.interactable = true;
            Select(startTrainingButton.gameObject);
        }

        private void ShowPlanError()
        {
            _homeTrainingDay.text = "Dzień treningu —";
            homeTrainingTitle.text = "Dzisiejszy trening";
            homeTrainingDuration.text = string.Empty;
            _homePlan.text = "Nie udało się przygotować treningu.";
            startTrainingButton.interactable = false;
        }

        private void CreateHomePlanPresentation()
        {
            _homeTrainingDay = Instantiate(homeRankName, homeScreen.transform);
            _homeTrainingDay.name = "Training Day";
            _homeTrainingDay.fontSize = 24f;
            _homeTrainingDay.alignment = TextAlignmentOptions.Center;
            ConfigureRect(_homeTrainingDay.rectTransform, 0.18f, 0.76f, 0.82f, 0.80f);

            _homePlan = Instantiate(homeRankName, homeScreen.transform);
            _homePlan.name = "Training Plan";
            _homePlan.fontSize = 24f;
            _homePlan.lineSpacing = 8f;
            _homePlan.alignment = TextAlignmentOptions.TopLeft;
            _homePlan.textWrappingMode = TextWrappingModes.Normal;
            ConfigureRect(_homePlan.rectTransform, 0.20f, 0.30f, 0.80f, 0.50f);

            CreateProfilePreferencesPresentation();
        }

        private void CreateProfilePreferencesPresentation()
        {
            _profilePreferencesButton = Instantiate(changeProfileButton, homeScreen.transform);
            _profilePreferencesButton.name = "Profile Preferences";
            _profilePreferencesButton.GetComponentInChildren<TMP_Text>().text =
                "Ustawienia profilu";
            ConfigureButtonRect(
                _profilePreferencesButton.GetComponent<RectTransform>(),
                new Vector2(0.84f, 0.88f),
                new Vector2(230f, 58f));

            _profilePreferencesPanel = new GameObject(
                "Profile Preferences Panel",
                typeof(RectTransform),
                typeof(Image));
            _profilePreferencesPanel.transform.SetParent(homeScreen.transform, false);
            RectTransform panelRect =
                (RectTransform)_profilePreferencesPanel.transform;
            ConfigureRect(panelRect, 0.20f, 0.20f, 0.80f, 0.80f);
            _profilePreferencesPanel.GetComponent<Image>().color =
                new Color(0.035f, 0.055f, 0.085f, 0.98f);
            _profilePreferencesPanel.transform.SetAsLastSibling();

            TMP_Text title = Instantiate(homeRankName, _profilePreferencesPanel.transform);
            title.name = "Preferences Title";
            title.text = "Landolt C";
            title.fontSize = 42f;
            title.alignment = TextAlignmentOptions.Center;
            ConfigureRect(title.rectTransform, 0.15f, 0.72f, 0.85f, 0.88f);

            TMP_Text label = Instantiate(homeRankName, _profilePreferencesPanel.transform);
            label.name = "Background Mode Label";
            label.text = "Tło ćwiczenia";
            label.fontSize = 28f;
            label.alignment = TextAlignmentOptions.Center;
            ConfigureRect(label.rectTransform, 0.15f, 0.57f, 0.85f, 0.68f);

            _darkBackgroundButton = CreatePreferenceButton(
                "Dark Background",
                "Ciemne",
                new Vector2(0.36f, 0.45f));
            _lightBackgroundButton = CreatePreferenceButton(
                "Light Background",
                "Jasne",
                new Vector2(0.64f, 0.45f));
            _preferencesBackButton = CreatePreferenceButton(
                "Preferences Back",
                "Wróć",
                new Vector2(0.5f, 0.20f));

            _preferencesMessage = Instantiate(
                homeRankName,
                _profilePreferencesPanel.transform);
            _preferencesMessage.name = "Preferences Message";
            _preferencesMessage.text = string.Empty;
            _preferencesMessage.fontSize = 22f;
            _preferencesMessage.alignment = TextAlignmentOptions.Center;
            ConfigureRect(
                _preferencesMessage.rectTransform,
                0.10f,
                0.29f,
                0.90f,
                0.36f);

            _profilePreferencesButton.onClick.AddListener(ShowProfilePreferences);
            _darkBackgroundButton.onClick.AddListener(SelectDarkBackground);
            _lightBackgroundButton.onClick.AddListener(SelectLightBackground);
            _preferencesBackButton.onClick.AddListener(HideProfilePreferences);
            _profilePreferencesPanel.SetActive(false);
        }

        private Button CreatePreferenceButton(string name, string label, Vector2 anchor)
        {
            Button button = Instantiate(
                changeProfileButton,
                _profilePreferencesPanel.transform);
            button.name = name;
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            text.text = label;
            text.fontSize = 26f;
            ConfigureButtonRect(
                button.GetComponent<RectTransform>(),
                anchor,
                new Vector2(210f, 72f));
            return button;
        }

        private static void ConfigureButtonRect(
            RectTransform rectTransform,
            Vector2 anchor,
            Vector2 size)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;
        }

        private void UnbindPreferenceActions()
        {
            _profilePreferencesButton?.onClick.RemoveListener(ShowProfilePreferences);
            _darkBackgroundButton?.onClick.RemoveListener(SelectDarkBackground);
            _lightBackgroundButton?.onClick.RemoveListener(SelectLightBackground);
            _preferencesBackButton?.onClick.RemoveListener(HideProfilePreferences);
        }

        private static void ConfigureRect(
            RectTransform rectTransform,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            rectTransform.anchorMin = new Vector2(minX, minY);
            rectTransform.anchorMax = new Vector2(maxX, maxY);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
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
