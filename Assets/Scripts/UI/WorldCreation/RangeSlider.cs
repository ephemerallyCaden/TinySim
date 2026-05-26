using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RangeSlider : MonoBehaviour
{
    [Header("Handles")]
    public RectTransform minHandle;
    public RectTransform maxHandle;

    [Header("Fill")]
    public RectTransform fillArea;  // Stretches between the two handles

    [Header("Track")]
    public RectTransform track;     // The background bar

    [Header("Labels")]
    public TMP_Text minLabel;
    public TMP_Text maxLabel;

    [Header("Range Settings")]
    public float rangeMin = 0f;
    public float rangeMax = 1f;
    public bool wholeNumbers = false;

    [Header("Current Values")]
    public float currentMin;
    public float currentMax;

    public event System.Action<float, float> OnValueChanged;

    private bool draggingMin = false;
    private bool draggingMax = false;

    private void Start()
    {
        StartCoroutine(InitAfterLayout());
    }

    private System.Collections.IEnumerator InitAfterLayout()
    {
        // Wait one frame for Unity to calculate layout/rect sizes
        yield return null;
        UpdateVisuals();
        UpdateLabels();
    }

    private void Update()
    {
        if (draggingMin || draggingMax)
        {
            HandleDrag();

            // Release drag if mouse button goes up anywhere
            if (Input.GetMouseButtonUp(0))
            {
                draggingMin = false;
                draggingMax = false;
            }
        }
    }

    public void SetValues(float min, float max)
    {
        currentMin = Mathf.Clamp(min, rangeMin, rangeMax);
        currentMax = Mathf.Clamp(max, rangeMin, rangeMax);
        if (currentMin > currentMax) currentMin = currentMax;
        UpdateVisuals();
        UpdateLabels();
    }

    public void OnMinHandleDown() => draggingMin = true;
    public void OnMaxHandleDown() => draggingMax = true;

    public void OnHandleUp()
    {
        draggingMin = false;
        draggingMax = false;
    }

    private void HandleDrag()
    {
        if (track == null) return;

        Vector2 localPoint;
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            track, Input.mousePosition, cam, out localPoint))
            return;

        // Convert local point to normalised 0-1 using the track's rect (pivot-independent)
        Rect trackRect = track.rect;
        float normalised = Mathf.Clamp01((localPoint.x - trackRect.xMin) / trackRect.width);
        float value = Mathf.Lerp(rangeMin, rangeMax, normalised);

        if (wholeNumbers)
            value = Mathf.Round(value);

        if (draggingMin)
        {
            currentMin = Mathf.Clamp(value, rangeMin, currentMax);
        }
        else if (draggingMax)
        {
            currentMax = Mathf.Clamp(value, currentMin, rangeMax);
        }

        UpdateVisuals();
        UpdateLabels();
        OnValueChanged?.Invoke(currentMin, currentMax);
    }

    private float handleWidth = -1f;

    private void UpdateVisuals()
    {
        if (track == null) return;
        float trackWidth = track.rect.width;
        if (trackWidth <= 0) return;

        float minNorm = Mathf.InverseLerp(rangeMin, rangeMax, currentMin);
        float maxNorm = Mathf.InverseLerp(rangeMin, rangeMax, currentMax);

        // Cache handle width on first valid call
        if (handleWidth < 0 && minHandle != null)
            handleWidth = minHandle.rect.width;

        float halfHandle = handleWidth > 0 ? handleWidth * 0.5f : 0f;

        // Position handles using pixel offset from left edge of track.
        // Handles must be children of Track with anchor (0, 0.5)-(1, 0.5) stretch horizontal,
        // then we use offsetMin/Max to pin them at a point.
        if (minHandle != null)
        {
            float px = minNorm * trackWidth;
            minHandle.anchorMin = new Vector2(0f, 0.5f);
            minHandle.anchorMax = new Vector2(0f, 0.5f);
            minHandle.anchoredPosition = new Vector2(px, 0f);
        }

        if (maxHandle != null)
        {
            float px = maxNorm * trackWidth;
            maxHandle.anchorMin = new Vector2(0f, 0.5f);
            maxHandle.anchorMax = new Vector2(0f, 0.5f);
            maxHandle.anchoredPosition = new Vector2(px, 0f);
        }

        // Stretch fill — use pixel offsets from edges to match handle centres exactly
        if (fillArea != null)
        {
            fillArea.anchorMin = new Vector2(0f, 0f);
            fillArea.anchorMax = new Vector2(1f, 1f);
            float leftPx = minNorm * trackWidth;
            float rightPx = trackWidth - (maxNorm * trackWidth);
            fillArea.offsetMin = new Vector2(leftPx, 0f);
            fillArea.offsetMax = new Vector2(-rightPx, 0f);
        }
    }

    private void UpdateLabels()
    {
        string format = wholeNumbers ? "F0" : "F2";
        if (minLabel != null) minLabel.text = currentMin.ToString(format);
        if (maxLabel != null) maxLabel.text = currentMax.ToString(format);
    }
}
