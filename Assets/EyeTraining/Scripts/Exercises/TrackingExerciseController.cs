using EyeTraining.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace EyeTraining.Exercises
{
    public sealed class TrackingExerciseController : MonoBehaviour
    {
        private const float CycleCount = 1f;
        private const float TargetViewportHeight = 76f / 1080f;
        private const float TrainingBoundsLineWidthInViewportHeight = 0.0015f;

        private static readonly Color TrainingBoundsColor =
            new(0.55f, 0.62f, 0.70f, 0.22f);

        [SerializeField] private GameObject homeScreen;
        [SerializeField] private GameObject exerciseScreen;
        [SerializeField] private GameObject exerciseWorld;
        [SerializeField] private Camera exerciseCamera;
        [SerializeField] private Transform target;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private TrackingPathType pathType;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject completionMessage;
        [SerializeField] private Button interruptButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button startTrainingButton;

        [Header("Debug")]
        [SerializeField] private bool showTrainingBounds;

        private ITrackingPath trackingPath;
        private LineRenderer trainingBoundsRenderer;
        private Vector2 targetExtentsInViewport;
        private float exerciseDuration;
        private float remainingTime;
        private float lastBoundsOrthographicSize = -1f;
        private float lastBoundsTargetPlaneZ = float.NaN;
        private int lastBoundsPixelWidth = -1;
        private int lastBoundsPixelHeight = -1;
        private double movementStartTime;
        private bool isRunning;

        public SessionGuidanceMode GuidanceMode { get; private set; }

        private void Awake()
        {
            trackingPath = CreateTrackingPath();
            CreateTrainingBoundsRenderer();
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

            double elapsedTime = Time.timeAsDouble - movementStartTime;
            remainingTime = Mathf.Max(0f, exerciseDuration - (float)elapsedTime);
            UpdateTimer();

            if (elapsedTime >= exerciseDuration)
            {
                CompleteAtCycleEnd();
            }
        }

        private void LateUpdate()
        {
            if (isRunning)
            {
                UpdateTargetPosition(Time.timeAsDouble - movementStartTime);
            }

            UpdateTrainingBounds();
        }

        public void Begin(SessionGuidanceMode guidanceMode)
        {
            GuidanceMode = guidanceMode;

            exerciseScreen.SetActive(true);
            exerciseWorld.SetActive(true);
            completionMessage.SetActive(false);
            nextButton.gameObject.SetActive(false);
            interruptButton.gameObject.SetActive(true);
            UpdateTitle();
            ConfigureTargetScale();
            targetExtentsInViewport = GetTargetExtentsInViewport();
            float fullCycleLength = trackingPath.GetFullCycleLength(targetExtentsInViewport);
            exerciseDuration =
                fullCycleLength / TrackingMotionSettings.LinearSpeed * CycleCount;
            remainingTime = exerciseDuration;
            ResetTargetPosition();
            movementStartTime = Time.timeAsDouble;
            isRunning = true;
            UpdateTimer();
            UpdateTrainingBounds(true);
            Select(interruptButton.gameObject);
        }

        private void UpdateTargetPosition(double elapsedTime)
        {
            Vector2 viewportPosition = trackingPath.Evaluate(
                elapsedTime,
                targetExtentsInViewport);
            target.position = ViewportToTargetPlane(viewportPosition);
        }

        private ITrackingPath CreateTrackingPath()
        {
            return pathType switch
            {
                TrackingPathType.Vertical => new VerticalTrackingPath(),
                TrackingPathType.DiagonalUp => new DiagonalUpTrackingPath(),
                TrackingPathType.DiagonalDown => new DiagonalDownTrackingPath(),
                TrackingPathType.Circle => new CircleTrackingPath(),
                TrackingPathType.HorizontalEllipse => new HorizontalEllipseTrackingPath(),
                TrackingPathType.UpperSemicircle => new UpperSemicircleTrackingPath(),
                TrackingPathType.LowerSemicircle => new LowerSemicircleTrackingPath(),
                TrackingPathType.UpperHorizontalSemiEllipse => new UpperHorizontalSemiEllipseTrackingPath(),
                TrackingPathType.LowerHorizontalSemiEllipse => new LowerHorizontalSemiEllipseTrackingPath(),
                TrackingPathType.Square => new SquareTrackingPath(),
                TrackingPathType.HorizontalRectangle => new HorizontalRectangleTrackingPath(),
                TrackingPathType.Triangle => new TriangleTrackingPath(),
                TrackingPathType.Diamond => new DiamondTrackingPath(),
                TrackingPathType.HorizontalZigzag => new HorizontalZigzagTrackingPath(),
                TrackingPathType.VerticalZigzag => new VerticalZigzagTrackingPath(),
                TrackingPathType.HorizontalWave => new HorizontalWaveTrackingPath(),
                TrackingPathType.VerticalWave => new VerticalWaveTrackingPath(),
                TrackingPathType.FigureEight => new FigureEightTrackingPath(),
                TrackingPathType.Spiral => new SpiralTrackingPath(),
                TrackingPathType.UShape => new UShapeTrackingPath(),
                TrackingPathType.InvertedUShape => new InvertedUShapeTrackingPath(),
                _ => new HorizontalTrackingPath()
            };
        }

        private void UpdateTitle()
        {
            titleText.text = pathType switch
            {
                TrackingPathType.Vertical => "Śledzenie pionowe",
                TrackingPathType.DiagonalUp => "Śledzenie po przekątnej w górę",
                TrackingPathType.DiagonalDown => "Śledzenie po przekątnej w dół",
                TrackingPathType.Circle => "Śledzenie po okręgu",
                TrackingPathType.HorizontalEllipse => "Śledzenie po elipsie",
                TrackingPathType.UpperSemicircle => "Śledzenie po górnym półokręgu",
                TrackingPathType.LowerSemicircle => "Śledzenie po dolnym półokręgu",
                TrackingPathType.UpperHorizontalSemiEllipse => "Śledzenie po górnej półelipsie",
                TrackingPathType.LowerHorizontalSemiEllipse => "Śledzenie po dolnej półelipsie",
                TrackingPathType.Square => "Śledzenie po kwadracie",
                TrackingPathType.HorizontalRectangle => "Śledzenie po prostokącie",
                TrackingPathType.Triangle => "Śledzenie po trójkącie",
                TrackingPathType.Diamond => "Śledzenie po diamencie",
                TrackingPathType.HorizontalZigzag => "Śledzenie po zygzaku poziomym",
                TrackingPathType.VerticalZigzag => "Śledzenie po zygzaku pionowym",
                TrackingPathType.HorizontalWave => "Śledzenie po fali poziomej",
                TrackingPathType.VerticalWave => "Śledzenie po fali pionowej",
                TrackingPathType.FigureEight => "Śledzenie po ósemce",
                TrackingPathType.Spiral => "Śledzenie po spirali",
                TrackingPathType.UShape => "Śledzenie po literze U",
                TrackingPathType.InvertedUShape => "Śledzenie po odwróconej literze U",
                _ => "Śledzenie poziome"
            };
        }

        private void Complete()
        {
            isRunning = false;
            trainingBoundsRenderer.enabled = false;
            completionMessage.SetActive(true);
            interruptButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            Select(nextButton.gameObject);
        }

        private void CompleteAtCycleEnd()
        {
            remainingTime = 0f;
            UpdateTimer();

            float roundedCycleCount = Mathf.Round(CycleCount);
            double finalElapsedTime = Mathf.Approximately(CycleCount, roundedCycleCount)
                ? 0d
                : exerciseDuration;
            UpdateTargetPosition(finalElapsedTime);
            Complete();
        }

        private void Interrupt()
        {
            isRunning = false;
            ReturnHome();
        }

        private void ReturnHome()
        {
            isRunning = false;
            trainingBoundsRenderer.enabled = false;
            exerciseScreen.SetActive(false);
            exerciseWorld.SetActive(false);
            homeScreen.SetActive(true);
            Select(startTrainingButton.gameObject);
        }

        private void ResetTargetPosition()
        {
            Vector2 viewportPosition = trackingPath.Evaluate(
                0d,
                targetExtentsInViewport);
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

        private void CreateTrainingBoundsRenderer()
        {
            GameObject boundsObject = new("Training Bounds (Debug)");
            boundsObject.transform.SetParent(exerciseWorld.transform, false);
            trainingBoundsRenderer = boundsObject.AddComponent<LineRenderer>();
            trainingBoundsRenderer.enabled = false;
            trainingBoundsRenderer.useWorldSpace = true;
            trainingBoundsRenderer.loop = true;
            trainingBoundsRenderer.positionCount = 4;
            trainingBoundsRenderer.startColor = TrainingBoundsColor;
            trainingBoundsRenderer.endColor = TrainingBoundsColor;
            trainingBoundsRenderer.numCapVertices = 0;
            trainingBoundsRenderer.numCornerVertices = 0;
            trainingBoundsRenderer.alignment = LineAlignment.View;
            trainingBoundsRenderer.textureMode = LineTextureMode.Stretch;
            trainingBoundsRenderer.shadowCastingMode = ShadowCastingMode.Off;
            trainingBoundsRenderer.receiveShadows = false;
            trainingBoundsRenderer.lightProbeUsage = LightProbeUsage.Off;
            trainingBoundsRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            trainingBoundsRenderer.sharedMaterial = targetRenderer.sharedMaterial;
            trainingBoundsRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            trainingBoundsRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
        }

        private void UpdateTrainingBounds(bool force = false)
        {
            bool shouldShow = showTrainingBounds && isRunning;
            trainingBoundsRenderer.enabled = shouldShow;

            if (!shouldShow)
            {
                return;
            }

            float targetPlaneZ = target.position.z;
            bool dimensionsChanged =
                lastBoundsPixelWidth != exerciseCamera.pixelWidth ||
                lastBoundsPixelHeight != exerciseCamera.pixelHeight ||
                !Mathf.Approximately(lastBoundsOrthographicSize, exerciseCamera.orthographicSize) ||
                !Mathf.Approximately(lastBoundsTargetPlaneZ, targetPlaneZ);

            if (!force && !dimensionsChanged)
            {
                return;
            }

            trainingBoundsRenderer.SetPosition(
                0,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Left,
                    TrackingTrainingArea.Bottom)));
            trainingBoundsRenderer.SetPosition(
                1,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Left,
                    TrackingTrainingArea.Top)));
            trainingBoundsRenderer.SetPosition(
                2,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Right,
                    TrackingTrainingArea.Top)));
            trainingBoundsRenderer.SetPosition(
                3,
                ViewportToTargetPlane(new Vector2(
                    TrackingTrainingArea.Right,
                    TrackingTrainingArea.Bottom)));

            float visibleWorldHeight = exerciseCamera.orthographicSize * 2f;
            float lineWidth = visibleWorldHeight * TrainingBoundsLineWidthInViewportHeight;
            trainingBoundsRenderer.startWidth = lineWidth;
            trainingBoundsRenderer.endWidth = lineWidth;
            lastBoundsPixelWidth = exerciseCamera.pixelWidth;
            lastBoundsPixelHeight = exerciseCamera.pixelHeight;
            lastBoundsOrthographicSize = exerciseCamera.orthographicSize;
            lastBoundsTargetPlaneZ = targetPlaneZ;
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
