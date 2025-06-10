using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BrushInteractor : MonoBehaviour
{
    public MeshRenderer targetMesh;
    public Camera mainCamera;

    [Header("RenderTexture Settings")]
    public RenderTexture deformationTex;

    [Header("Shaders & Materials")]
    public Shader drawShader;
    public Shader fadeShader;

    public float brushSize = 0.1f;
    public float brushStrength = 1.0f;
    [Range(0f, 1f)]
    public float fadeAmount = 0.95f; // quanto lentamente svanisce (1=non svanisce, 0=svanisce subito)

    public Material drawMat;
    private Material fadeMat;

    private static readonly int BrushPosID = Shader.PropertyToID("_BrushPos");
    private static readonly int BrushRadiusID = Shader.PropertyToID("_BrushRadius");
    private static readonly int BrushStrengthID = Shader.PropertyToID("_BrushStrength");
    private static readonly int FadeAmountID = Shader.PropertyToID("_FadeAmount");
    private static readonly int DeformationTexID = Shader.PropertyToID("_DeformationTex");

    void Start()
    {
        if (!deformationTex)
        {
            Debug.LogError("Deformation RenderTexture is not assigned!");
            enabled = false;
            return;
        }

        fadeMat = new Material(fadeShader);

        // Inizializza la RenderTexture a nero
        RenderTexture.active = deformationTex;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = null;

    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.Log($"[BrushInteractor] Drawing at mouse position (x: {Input.mousePosition.x}, y: {Input.mousePosition.y})");
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.GetComponent<MeshRenderer>() == targetMesh)
                {
                    Vector2 uv = hit.textureCoord;
                    Debug.Log($"[BrushInteractor] Clicked on {hit.collider.name} at UV: {uv}");
                    DrawAtUV(uv);
                }
            }
        }
        else
        {
            drawMat.SetFloat(BrushStrengthID, 0);
        }

        ApplyFade();
    }

    void DrawAtUV(Vector2 uv)
    {
        if (drawMat == null || deformationTex == null)
            return;

        drawMat.SetVector(BrushPosID, uv);
        drawMat.SetFloat(BrushRadiusID, brushSize);
        drawMat.SetFloat(BrushStrengthID, brushStrength);

        RenderTexture temp = RenderTexture.GetTemporary(deformationTex.width, deformationTex.height, 0, deformationTex.format);

        // Copy current deformation texture to temp
        Graphics.Blit(deformationTex, temp);

        // Draw brush stroke on temp
        Graphics.Blit(temp, deformationTex, drawMat);

        RenderTexture.ReleaseTemporary(temp);
    }

    void ApplyFade()
    {
        if (fadeMat == null || deformationTex == null)
            return;

        fadeMat.SetFloat(FadeAmountID, fadeAmount);

        RenderTexture temp = RenderTexture.GetTemporary(deformationTex.width, deformationTex.height, 0, deformationTex.format);

        Graphics.Blit(deformationTex, temp, fadeMat);

        Graphics.Blit(temp, deformationTex);

        RenderTexture.ReleaseTemporary(temp);
    }
}
