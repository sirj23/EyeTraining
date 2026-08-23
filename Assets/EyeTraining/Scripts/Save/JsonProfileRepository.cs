using System;
using System.Collections.Generic;
using System.IO;
using EyeTraining.Exercises.Landolt;
using EyeTraining.Profiles;
using UnityEngine;

namespace EyeTraining.Save
{
    public sealed class JsonProfileRepository : IProfileRepository
    {
        private const int CurrentFormatVersion = 2;
        private const int MinimumSupportedFormatVersion = 1;
        private const string FileName = "profiles.json";

        private readonly string _filePath;

        public JsonProfileRepository()
            : this(Path.Combine(Application.persistentDataPath, FileName))
        {
        }

        public JsonProfileRepository(string filePath)
        {
            _filePath = filePath;
        }

        public IReadOnlyList<UserProfile> GetAll()
        {
            return TryLoad(out List<UserProfile> profiles)
                ? profiles
                : Array.Empty<UserProfile>();
        }

        public bool Save(UserProfile profile)
        {
            if (!TryLoad(out List<UserProfile> profiles))
            {
                return false;
            }

            int existingIndex = profiles.FindIndex(savedProfile =>
                string.Equals(savedProfile.Id, profile.Id, StringComparison.Ordinal));
            if (existingIndex >= 0)
            {
                profiles[existingIndex] = profile;
            }
            else
            {
                profiles.Add(profile);
            }

            try
            {
                string directoryPath = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileData = new ProfileFileData
                {
                    version = CurrentFormatVersion,
                    profiles = new List<ProfileRecord>(profiles.Count)
                };

                foreach (UserProfile savedProfile in profiles)
                {
                    fileData.profiles.Add(ProfileRecord.FromProfile(savedProfile));
                }

                File.WriteAllText(_filePath, JsonUtility.ToJson(fileData, true));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Nie udało się zapisać profili w '{_filePath}': {exception.Message}");
                return false;
            }
        }

        private bool TryLoad(out List<UserProfile> profiles)
        {
            profiles = new List<UserProfile>();

            if (!File.Exists(_filePath))
            {
                return true;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                ProfileFileData fileData = JsonUtility.FromJson<ProfileFileData>(json);

                if (fileData == null || fileData.profiles == null)
                {
                    throw new InvalidDataException("Plik nie zawiera prawidłowej listy profili.");
                }

                if (fileData.version < MinimumSupportedFormatVersion
                    || fileData.version > CurrentFormatVersion)
                {
                    throw new InvalidDataException(
                        $"Nieobsługiwana wersja pliku profili: {fileData.version}.");
                }

                foreach (ProfileRecord record in fileData.profiles)
                {
                    if (record == null ||
                        string.IsNullOrWhiteSpace(record.id) ||
                        string.IsNullOrWhiteSpace(record.displayName))
                    {
                        throw new InvalidDataException("Profil nie zawiera prawidłowego identyfikatora lub nazwy.");
                    }

                    profiles.Add(record.ToProfile());
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Nie udało się odczytać profili z '{_filePath}': {exception.Message}");
                profiles.Clear();
                return false;
            }
        }

        [Serializable]
        private sealed class ProfileFileData
        {
            public int version = CurrentFormatVersion;
            public List<ProfileRecord> profiles = new List<ProfileRecord>();
        }

        [Serializable]
        private sealed class ProfileRecord
        {
            public string id;
            public string displayName;
            public string category;
            public string landoltBackgroundMode;

            public static ProfileRecord FromProfile(UserProfile profile)
            {
                return new ProfileRecord
                {
                    id = profile.Id,
                    displayName = profile.DisplayName,
                    category = profile.Category.ToString(),
                    landoltBackgroundMode = profile.LandoltBackgroundMode.ToString()
                };
            }

            public UserProfile ToProfile()
            {
                if (!Enum.TryParse(category, out ProfileCategory parsedCategory) ||
                    !Enum.IsDefined(typeof(ProfileCategory), parsedCategory))
                {
                    throw new InvalidDataException($"Nieznana kategoria profilu: '{category}'.");
                }

                LandoltBackgroundMode parsedBackgroundMode = LandoltBackgroundMode.Dark;
                if (!string.IsNullOrEmpty(landoltBackgroundMode)
                    && (!Enum.TryParse(landoltBackgroundMode, out parsedBackgroundMode)
                        || !Enum.IsDefined(
                            typeof(LandoltBackgroundMode),
                            parsedBackgroundMode)))
                {
                    throw new InvalidDataException(
                        $"Nieznany tryb tła Landolta: '{landoltBackgroundMode}'.");
                }

                return new UserProfile(
                    id,
                    displayName,
                    parsedCategory,
                    parsedBackgroundMode);
            }
        }
    }
}
