using UnityEngine;
using UnityEngine.UI;

public class SpeciesModelPreview : MonoBehaviour
{
    public RawImage previewImage;
    public Material agentMaterial;
    public float rotationSpeed = 45f;
    public int textureSize = 256;

    private RenderTexture renderTexture;
    private Camera previewCamera;
    private GameObject bodyObject;
    private GameObject eye1Object;
    private GameObject eye2Object;
    private MeshRenderer bodyRenderer;
    private MaterialPropertyBlock bodyProps;
    private float currentRotation;
    private bool hasAgent = false;

    private const int PREVIEW_LAYER = 31;

    private void Awake()
    {
        bodyProps = new MaterialPropertyBlock();
        SetupRenderTexture();
        SetupCamera();
        SetupAgentModel();

        if (previewImage != null)
            previewImage.texture = renderTexture;
    }

    private void SetupRenderTexture()
    {
        renderTexture = new RenderTexture(textureSize, textureSize, 16);
        renderTexture.antiAliasing = 4;
    }

    private void SetupCamera()
    {
        GameObject camObj = new GameObject("SpeciesPreviewCamera");
        camObj.transform.SetParent(transform);
        camObj.transform.localPosition = new Vector3(0, 0, -10);
        camObj.layer = PREVIEW_LAYER;

        previewCamera = camObj.AddComponent<Camera>();
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = 2.5f;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        previewCamera.cullingMask = 1 << PREVIEW_LAYER;
        previewCamera.targetTexture = renderTexture;
        previewCamera.depth = -100;
    }

    private void SetupAgentModel()
    {
        Mesh circleMesh = CircleMeshGenerator.GenerateCircleMesh(16);

        // Body
        bodyObject = CreateMeshObject("PreviewBody", circleMesh, Vector3.zero, 1f);
        bodyRenderer = bodyObject.GetComponent<MeshRenderer>();

        // Eyes
        eye1Object = CreateMeshObject("PreviewEye1", circleMesh, Vector3.zero, 0.25f);
        eye2Object = CreateMeshObject("PreviewEye2", circleMesh, Vector3.zero, 0.25f);

        SetEyeColour(eye1Object, Color.black);
        SetEyeColour(eye2Object, Color.black);
    }

    private GameObject CreateMeshObject(string name, Mesh mesh, Vector3 position, float scale)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = position;
        obj.transform.localScale = Vector3.one * scale;
        obj.layer = PREVIEW_LAYER;

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.material = agentMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return obj;
    }

    private void SetEyeColour(GameObject eye, Color colour)
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetColor("_Color", colour);
        eye.GetComponent<MeshRenderer>().SetPropertyBlock(props);
    }

    public void SetAgent(Color colour, float size)
    {
        hasAgent = true;

        float displaySize = Mathf.Clamp(size, 0.5f, 3f);
        bodyObject.transform.localScale = Vector3.one * displaySize;

        bodyProps.SetColor("_Color", colour);
        bodyRenderer.SetPropertyBlock(bodyProps);

        bodyObject.SetActive(true);
        eye1Object.SetActive(true);
        eye2Object.SetActive(true);

        UpdateEyePositions();
    }

    public void Clear()
    {
        hasAgent = false;
        bodyObject.SetActive(false);
        eye1Object.SetActive(false);
        eye2Object.SetActive(false);
    }

    private void Update()
    {
        if (!hasAgent) return;

        currentRotation += rotationSpeed * Time.deltaTime;
        UpdateEyePositions();
    }

    private void UpdateEyePositions()
    {
        float size = bodyObject.transform.localScale.x;

        for (int i = 0; i < 2; i++)
        {
            GameObject eye = (i == 0) ? eye1Object : eye2Object;
            float angleOffset = (i == 0) ? 30f : -30f;
            float angle = currentRotation + angleOffset;
            angle = Mathf.Repeat(angle, 360);

            // Eyes only visible when facing forward (same as AgentRenderer)
            if (angle < 180 && angle > 0)
            {
                eye.SetActive(false);
                continue;
            }

            eye.SetActive(true);
            float distanceFromCenter = size;
            float eyeX = distanceFromCenter * Mathf.Cos(Mathf.Deg2Rad * angle);
            float eyeY = distanceFromCenter * Mathf.Sin(Mathf.Deg2Rad * angle) / (size * 2);

            eye.transform.localPosition = new Vector3(eyeX, eyeY, -0.1f);
        }
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
            renderTexture.Release();
    }
}
