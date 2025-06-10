using UnityEngine;

public class BrushInteractor : MonoBehaviour
{
    public Camera mainCamera;
    public Material brushMat;
    public float brushRadius = 0.1f;
    public float brushStrength = 1.0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[Brush Interactor] Mouse button down at position ({Input.mousePosition.x}, {Input.mousePosition.y})");
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.GetComponent<Renderer>().sharedMaterial == brushMat)
                {
                    Vector2 uv = hit.textureCoord;
                    Debug.Log($"[BrushInteractor] Clicked on {hit.collider.name} at UV: {uv}");
                    brushMat.SetVector("_BrushPos", new Vector4(uv.x, uv.y, 0, 0));
                    brushMat.SetFloat("_BrushRadius", brushRadius);
                    brushMat.SetFloat("_BrushStrength", brushStrength);
                    brushMat.SetFloat("_EnableBrush", 1.0f);
                }
            }
        }
        else
        {
            // Disable the brush to avoid unwanted painting
            brushMat.SetFloat("_BrushStrength", 0.0f);
        }
    }
}
