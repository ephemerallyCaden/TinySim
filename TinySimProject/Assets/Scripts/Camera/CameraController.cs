using UnityEngine;
using System;

public class CameraControl : MonoBehaviour
{
    public float panSpeed = 20f; // Speed of panning
    public float zoomSpeed = 2f; // Speed of zooming
    public float minZoom = 5f;   // Minimum zoom limit
    public float maxZoom = 20f;  // Maximum zoom limit

    private Camera cam;

    void Start()
    {
        cam = Camera.main; // Get the main camera
    }

    void Update()
    {
        HandlePanning();
        HandleZooming();
    }

    // Handle camera panning
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

    // Handle camera zooming
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
}
