using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using EyeTraining.Sessions.History;
using EyeTraining.Exercises.Landolt;
using UnityEngine;

namespace EyeTraining.Save
{
    public sealed class JsonTrainingHistoryRepository : ITrainingHistoryRepository
    {
        private const int CurrentFormatVersion = 2;
        private const int MinimumSupportedFormatVersion = 1;
        private const string FileName = "training-history.json";

        private readonly string _filePath;

        public JsonTrainingHistoryRepository()
            : this(Path.Combine(Application.persistentDataPath, FileName))
        {
        }

        public JsonTrainingHistoryRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));
            }

            _filePath = filePath;
        }

        public bool TryLoad(string profileId, out TrainingHistorySnapshot snapshot)
        {
            ValidateProfileId(profileId);

            if (!TryLoadFile(out TrainingHistoryFileData fileData))
            {
                snapshot = null;
                return false;
            }

            try
            {
                TrainingProfileRecord selectedRecord = null;
                var profileIds = new HashSet<string>(StringComparer.Ordinal);
                for (var index = 0; index < fileData.profiles.Count; index++)
                {
                    TrainingProfileRecord record = fileData.profiles[index];
                    if (record == null || string.IsNullOrWhiteSpace(record.profileId))
                    {
                        throw new InvalidDataException("Training profile has no valid profile id.");
                    }

                    if (!profileIds.Add(record.profileId))
                    {
                        throw new InvalidDataException(
                            $"Duplicate training profile '{record.profileId}'.");
                    }

                    if (string.Equals(record.profileId, profileId, StringComparison.Ordinal))
                    {
                        selectedRecord = record;
                    }
                }

                snapshot = selectedRecord == null
                    ? TrainingHistorySnapshot.CreateNotStarted(profileId)
                    : ToDomain(selectedRecord, fileData.version);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Nie udało się odczytać historii treningu profilu '{profileId}' "
                    + $"z '{_filePath}': {exception.Message}");
                snapshot = TrainingHistorySnapshot.CreateNotStarted(profileId);
                return false;
            }
        }

        public bool Save(TrainingHistorySnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (!TryLoadAll(out Dictionary<string, TrainingHistorySnapshot> snapshots))
            {
                return false;
            }

            snapshots[snapshot.State.ProfileId] = snapshot;
            return TryWriteAll(snapshots);
        }

        public bool DeleteForProfile(string profileId)
        {
            ValidateProfileId(profileId);

            if (!TryLoadAll(out Dictionary<string, TrainingHistorySnapshot> snapshots))
            {
                return false;
            }

            if (!snapshots.Remove(profileId))
            {
                return true;
            }

            return TryWriteAll(snapshots);
        }

        private bool TryLoadAll(out Dictionary<string, TrainingHistorySnapshot> snapshots)
        {
            snapshots = new Dictionary<string, TrainingHistorySnapshot>(StringComparer.Ordinal);

            if (!TryLoadFile(out TrainingHistoryFileData fileData))
            {
                return false;
            }

            try
            {
                for (var index = 0; index < fileData.profiles.Count; index++)
                {
                    TrainingHistorySnapshot snapshot = ToDomain(
                        fileData.profiles[index],
                        fileData.version);
                    if (!snapshots.TryAdd(snapshot.State.ProfileId, snapshot))
                    {
                        throw new InvalidDataException(
                            $"Duplicate training profile '{snapshot.State.ProfileId}'.");
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Nie udało się odczytać historii treningu z '{_filePath}': "
                    + exception.Message);
                snapshots.Clear();
                return false;
            }
        }

        private bool TryLoadFile(out TrainingHistoryFileData fileData)
        {
            fileData = new TrainingHistoryFileData
            {
                version = CurrentFormatVersion,
                profiles = new List<TrainingProfileRecord>()
            };

            if (!File.Exists(_filePath))
            {
                return true;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                fileData = JsonUtility.FromJson<TrainingHistoryFileData>(json);
                if (fileData == null)
                {
                    throw new InvalidDataException("Training history file is not valid JSON data.");
                }

                if (fileData.version < MinimumSupportedFormatVersion
                    || fileData.version > CurrentFormatVersion)
                {
                    throw new NotSupportedException(
                        $"Unsupported training history version {fileData.version}; "
                        + $"supported range is {MinimumSupportedFormatVersion}–{CurrentFormatVersion}.");
                }

                if (fileData.profiles == null)
                {
                    throw new InvalidDataException("Training history file has no profiles list.");
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Nie udało się odczytać historii treningu z '{_filePath}': "
                    + exception.Message);
                return false;
            }
        }

        private bool TryWriteAll(
            IReadOnlyDictionary<string, TrainingHistorySnapshot> snapshots)
        {
            string temporaryPath = _filePath + ".tmp";

            try
            {
                string directoryPath = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var profileIds = new List<string>(snapshots.Keys);
                profileIds.Sort(StringComparer.Ordinal);

                var fileData = new TrainingHistoryFileData
                {
                    version = CurrentFormatVersion,
                    profiles = new List<TrainingProfileRecord>(profileIds.Count)
                };

                for (var index = 0; index < profileIds.Count; index++)
                {
                    fileData.profiles.Add(ToRecord(snapshots[profileIds[index]]));
                }

                File.WriteAllText(temporaryPath, JsonUtility.ToJson(fileData, true));

                if (File.Exists(_filePath))
                {
                    File.Replace(temporaryPath, _filePath, null);
                }
                else
                {
                    File.Move(temporaryPath, _filePath);
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Nie udało się zapisać historii treningu w '{_filePath}': "
                    + exception.Message);
                return false;
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch (Exception)
                    {
                        // The original file is still intact; stale temp data can be retried later.
                    }
                }
            }
        }

        private static TrainingHistorySnapshot ToDomain(
            TrainingProfileRecord record,
            int fileVersion)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.profileId))
            {
                throw new InvalidDataException("Training profile has no valid profile id.");
            }

            if (record.trainingState == null || record.exerciseHistory == null)
            {
                throw new InvalidDataException(
                    $"Training profile '{record.profileId}' has incomplete data.");
            }

            DateTimeOffset? trainingStartDate = ParseOptionalDate(
                record.trainingState.trainingStartDate,
                "trainingStartDate");
            DateTimeOffset? lastCompletedSessionDate = ParseOptionalDate(
                record.trainingState.lastCompletedSessionDate,
                "lastCompletedSessionDate");
            var state = new TrainingProfileState(
                record.profileId,
                trainingStartDate,
                record.trainingState.completedSessionCount,
                lastCompletedSessionDate);
            var entries = new List<ExerciseHistoryEntry>(record.exerciseHistory.Count);

            for (var index = 0; index < record.exerciseHistory.Count; index++)
            {
                entries.Add(ToDomain(
                    record.profileId,
                    record.exerciseHistory[index],
                    fileVersion));
            }

            return new TrainingHistorySnapshot(state, entries);
        }

        private static ExerciseHistoryEntry ToDomain(
            string profileId,
            ExerciseHistoryRecord record,
            int fileVersion)
        {
            if (record == null)
            {
                throw new InvalidDataException("Exercise history contains a null entry.");
            }

            if (!Enum.TryParse(
                    record.completionStatus,
                    out ExerciseCompletionStatus completionStatus)
                || !Enum.IsDefined(typeof(ExerciseCompletionStatus), completionStatus))
            {
                throw new InvalidDataException(
                    $"Unknown completion status '{record.completionStatus}'.");
            }

            if (!Enum.TryParse(record.feedback, out ExerciseFeedback feedback)
                || !Enum.IsDefined(typeof(ExerciseFeedback), feedback))
            {
                throw new InvalidDataException($"Unknown exercise feedback '{record.feedback}'.");
            }

            IExerciseHistoryDetails details = fileVersion >= 2
                ? ToLandoltDetails(record)
                : null;
            return new ExerciseHistoryEntry(
                profileId,
                record.exerciseId,
                record.completedSessionNumber,
                record.hasAppliedLevel ? record.appliedLevel : (int?)null,
                completionStatus,
                feedback,
                ParseRequiredDate(record.completedAt, "completedAt"),
                details);
        }

        private static TrainingProfileRecord ToRecord(TrainingHistorySnapshot snapshot)
        {
            var record = new TrainingProfileRecord
            {
                profileId = snapshot.State.ProfileId,
                trainingState = new TrainingStateRecord
                {
                    trainingStartDate = FormatDate(snapshot.State.TrainingStartDate),
                    completedSessionCount = snapshot.State.CompletedSessionCount,
                    lastCompletedSessionDate = FormatDate(
                        snapshot.State.LastCompletedSessionDate)
                },
                exerciseHistory = new List<ExerciseHistoryRecord>(snapshot.Entries.Count)
            };

            for (var index = 0; index < snapshot.Entries.Count; index++)
            {
                ExerciseHistoryEntry entry = snapshot.Entries[index];
                var historyRecord = new ExerciseHistoryRecord
                {
                    exerciseId = entry.ExerciseId,
                    completedSessionNumber = entry.CompletedSessionNumber,
                    hasAppliedLevel = entry.AppliedLevel.HasValue,
                    appliedLevel = entry.AppliedLevel.GetValueOrDefault(),
                    completionStatus = entry.CompletionStatus.ToString(),
                    feedback = entry.Feedback.ToString(),
                    completedAt = entry.CompletedAt.ToString("O", CultureInfo.InvariantCulture),
                    hasLandoltDetails = entry.Details is LandoltExerciseHistoryDetails,
                    landolt = ToLandoltRecord(entry.Details)
                };
                record.exerciseHistory.Add(historyRecord);
            }

            return record;
        }

        private static IExerciseHistoryDetails ToLandoltDetails(ExerciseHistoryRecord historyRecord)
        {
            LandoltHistoryRecord record = historyRecord.landolt;
            bool draftV2Payload = record != null
                && (!string.IsNullOrEmpty(record.backgroundMode)
                    || !string.IsNullOrEmpty(record.directionMode));

            if (!historyRecord.hasLandoltDetails && !draftV2Payload)
            {
                return null;
            }

            if (record == null)
            {
                throw new InvalidDataException(
                    "Landolt history is marked as present but has no payload.");
            }

            if (!Enum.TryParse(record.backgroundMode, out LandoltBackgroundMode backgroundMode)
                || !Enum.IsDefined(typeof(LandoltBackgroundMode), backgroundMode)
                || !Enum.TryParse(record.directionMode, out LandoltDirectionMode directionMode)
                || !Enum.IsDefined(typeof(LandoltDirectionMode), directionMode))
            {
                throw new InvalidDataException("Landolt history contains an unknown mode.");
            }

            return new LandoltExerciseHistoryDetails(
                record.correctAnswers,
                record.errorCount,
                record.exposureCount,
                record.highestLevel,
                record.finalLevel,
                backgroundMode,
                directionMode);
        }

        private static LandoltHistoryRecord ToLandoltRecord(IExerciseHistoryDetails details)
        {
            if (details == null)
            {
                return null;
            }

            if (details is not LandoltExerciseHistoryDetails landolt)
            {
                throw new InvalidDataException(
                    $"Unsupported exercise history details type '{details.GetType().Name}'.");
            }

            return new LandoltHistoryRecord
            {
                correctAnswers = landolt.CorrectAnswers,
                errorCount = landolt.ErrorCount,
                exposureCount = landolt.ExposureCount,
                highestLevel = landolt.HighestLevel,
                finalLevel = landolt.FinalLevel,
                backgroundMode = landolt.BackgroundMode.ToString(),
                directionMode = landolt.DirectionMode.ToString()
            };
        }

        private static DateTimeOffset ParseRequiredDate(string value, string fieldName)
        {
            if (!DateTimeOffset.TryParseExact(
                    value,
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTimeOffset parsed))
            {
                throw new InvalidDataException($"Field '{fieldName}' is not a valid ISO-8601 timestamp.");
            }

            return parsed;
        }

        private static DateTimeOffset? ParseOptionalDate(string value, string fieldName)
        {
            return string.IsNullOrEmpty(value)
                ? (DateTimeOffset?)null
                : ParseRequiredDate(value, fieldName);
        }

        private static string FormatDate(DateTimeOffset? value)
        {
            return value.HasValue
                ? value.Value.ToString("O", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static void ValidateProfileId(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentException("Profile id cannot be empty.", nameof(profileId));
            }
        }
    }
}
