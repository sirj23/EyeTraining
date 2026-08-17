using EyeTraining.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeTraining.Exercises
{
    public sealed class HorizontalTrackingController : MonoBehaviour
    {
        private const float ExerciseDurationSeconds = 15f;
        private const float TargetViewportHeight = 76f / 1080f;

        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject exerciseScreen;
        [SerializeField] private GameObject exerciseWorld;
        [SerializeField] private Camera exerciseCamera;
        [SerializeField] private Transform target;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject completionMessage;
        [SerializeField] private Button interruptButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button startTrainingButton;

        private readonly ITrackingPath trackingPath = new HorizontalTrackingPath();
        private float remainingTime;
        private double movementStartTime;
        private bool isRunning;

        public SessionGuidanceMode GuidanceMode { get; private set; }

        private void Awake()
        {
            interruptButton.onClick.AddListener(Interrupt);
            nextButton.onClick.AddListener(ReturnHome);
            exerciseScreen.SetActive(false);
            exerciseWorld.SetActive(false);
        }

        private void OnDestroy()
        {
            interruptButton.onClick.RemoveListener(Interrupt);
            nextButton.onClick.RemoveListener(ReturnHome);
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
            UpdateTimer();

            if (remainingTime <= 0f)
            {
                Complete();
            }
        }

        private void LateUpdate()
        {
            if (isRunning)
            {
                UpdateTargetPosition();
            }
        }

        public void Begin(SessionGuidanceMode guidanceMode)
        {
            GuidanceMode = guidanceMode;
            remainingTime = ExerciseDurationSeconds;
            isRunning = true;

            exerciseScreen.SetActive(true);
            exerciseWorld.SetActive(true);
            completionMessage.SetActive(false);
            nextButton.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(true);
            ConfigureTargetScale();
            ResetTargetPosition();
            movementStartTime = Time.timeAsDouble;
            UpdateTimer();
            Select(interruptButton.gameObject);
        }

        private void UpdateTargetPosition()
        {
            double elapsedTime = Time.timeAsDouble - movementStartTime;
            Vector2 viewportPosition = trackingPath.Evaluate(
                elapsedTime,
                GetTargetExtentsInViewport());
            target.position = ViewportToTargetPlane(viewportPosition);
        }

        private void Complete()
        {
            isRunning = false;
            completionMessage.SetActive(true);
            interruptButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            Select(nextButton.gameObject);
        }

        private void Interrupt()
        {
            isRunning = false;
            ReturnHome();
        }

        private void ReturnHome()
        {
            isRunning = false;
            exerciseScreen.SetActive(false);
            exerciseWorld.SetActive(false);
            homeScreen.SetActive(true);
            Select(startTrainingButton.gameObject);
        }

        private void ResetTargetPosition()
        {
            Vector2 viewportPosition = trackingPath.Evaluate(
                0d,
                GetTargetExtentsInViewport());
            target.position = ViewportToTargetPlane(viewportPosition);
        }

        private void ConfigureTargetScale()
        {
            float visibleWorldHeight = exerciseCamera.orthographicSize * 2f;
            float desiredDiameter = visibleWorldHeight * TargetViewportHeight;
            float spriteDiameter = targetRenderer.sprite.bounds.size.y;
            float uniformScale = desiredDiameter / spriteDiameter;
            target.localScale = Vector3.one * uniformScale;
        }

        private Vector2 GetTargetExtentsInViewport()
        {
            Vector3 viewportMin = ViewportToTargetPlane(Vector2.zero);
            Vector3 viewportMax = ViewportToTargetPlane(Vector2.one);
            float visibleWorldWidth = viewportMax.x - viewportMin.x;
            float visibleWorldHeight = viewportMax.y - viewportMin.y;

            return new Vector2(
                targetRenderer.bounds.extents.x / visibleWorldWidth,
                targetRenderer.bounds.extents.y / visibleWorldHeight);
        }

        private Vector3 ViewportToTargetPlane(Vector2 viewportPosition)
        {
            float distanceFromCamera = target.position.z - exerciseCamera.transform.position.z;
            Vector3 position = exerciseCamera.ViewportToWorldPoint(
                new Vector3(viewportPosition.x, viewportPosition.y, distanceFromCamera));
            position.z = target.position.z;
            return position;
        }

        private void UpdateTimer()
        {
            timerText.text = $"{Mathf.CeilToInt(remainingTime)} s";
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
