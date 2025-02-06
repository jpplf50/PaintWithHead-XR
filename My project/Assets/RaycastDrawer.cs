using UnityEngine;

public class RaycastDrawer : MonoBehaviour
{
    public Camera mainCamera;  // Assign XR Camera (Main Camera) in Inspector
    public RenderTexture renderTexture;  // Assign the Render Texture manually
    public Collider canvasCollider;  // Assign the Canvas Collider manually
    public Color drawColor = Color.red;
    public float brushSize = 5f;
    
    private Texture2D texture;

    void Start()
    {
        if (renderTexture == null)
        {
            Debug.LogError("Render Texture not assigned!");
            return;
        }

        texture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // Replace with OpenXR Input later
        {
            Debug.Log("Pressed button");
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider == canvasCollider) // Check if we hit the Canvas
                {
                    Debug.Log("Hit the canvas!");
                    Draw(hit.textureCoord);
                }
            }
        }
    }

    void Draw(Vector2 uv)
    {
        int x = (int)(uv.x * texture.width);
        int y = (int)(uv.y * texture.height);

        for (int i = -((int)brushSize / 2); i < (int)brushSize / 2; i++)
        {
            for (int j = -((int)brushSize / 2); j < (int)brushSize / 2; j++)
            {
                texture.SetPixel(x + i, y + j, drawColor);
            }
        }

        texture.Apply();
        RenderTexture.active = renderTexture;
        Graphics.Blit(texture, renderTexture);
    }
}
