using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControl : MonoBehaviour
{
    public float panSpeed = 20f; // Speed of panning
    public float zoomSpeed = 2f; // Speed of zooming
    public float minZoom = 5f;   // Minimum zoom limit
    public float maxZoom = 20f;  // Maximum zoom limit
    public float scrollZoomSpeed = 5f; // Speed of scroll wheel zoom
    public float dragPanSpeed = 0.5f;  // Speed of mouse drag panning

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging = false;

    void Start()
    {
        cam = Camera.main; // Get the main camera
    }

    void Update()
    {
        HandlePanning();
        HandleMouseDragPan();
        HandleZooming();
        HandleScrollZoom();
    }

    // Handle camera panning with keyboard
    void HandlePanning()
    {
        float horizontal = 0f;
        float vertical = 0f;

        // Panning with arrow keys or WASD
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            vertical = 1f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            vertical = -1f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horizontal = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horizontal = 1f;

        Vector3 move = new Vector3(horizontal, vertical, 0f) * panSpeed * Time.deltaTime;
        transform.position += move;
    }

    // Handle mouse drag panning with middle or right mouse button
    void HandleMouseDragPan()
    {
        // Don't pan when over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Start drag on middle or right mouse button down
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        // End drag
        if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        // Continue dragging
        if (isDragging && (Input.GetMouseButton(2) || Input.GetMouseButton(1)))
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentPos;
            transform.position += difference;
        }
    }

    // Handle camera zooming with keyboard
    void HandleZooming()
    {
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus)) // '+' key or '=' key
        {
            cam.orthographicSize -= zoomSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Minus))
        {
            cam.orthographicSize += zoomSpeed * Time.deltaTime;
        }

        // Clamp the zoom values to prevent going beyond limits
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    // Handle mouse scroll wheel zoom
    void HandleScrollZoom()
    {
        // Don't zoom when over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            cam.orthographicSize -= scroll * scrollZoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}
