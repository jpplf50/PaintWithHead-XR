using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HeadDrawing : MonoBehaviour
{
    public Canvas drawingCanvas; // Assign your Canvas in the Inspector
    public float raycastDistance = 10f;
    public float brushSize = 0.01f; // Current brush size
    public Color brushColor = Color.black;

    private Texture2D drawingTexture;
    private RectTransform canvasRect;
    private bool isDrawing = false; // Toggle for drawing

    // Input action for the grab button
    public InputActionReference grabAction;

    // Preview circle
    public Image previewCircle; // Assign the PreviewCircle RawImage in the Inspector

    // Color selection
    public GameObject[] colorSpheres; // Assign your color spheres in the Inspector
    public float colorSelectionTime = 2f; // Time to look at a sphere to select its color
    private float gazeTimer = 0f;
    private Color targetColor;

    // Brush size selection
    public GameObject[] brushSizeControls; // Assign your brush size controls in the Inspector
    public float brushSizeSelectionTime = 2f; // Time to look at a control to select its size
    private float brushSizeGazeTimer = 0f;

    // Visual feedback for color selection
    public Image colorFeedbackUI; // Assign a UI Image to show the selected color
    public Text colorFeedbackText; // Assign a UI Text to show the selected color name

    // Progress indicator
    public Image progressIndicator; // Assign a UI Image (e.g., a radial fill) for the progress indicator

    private GameObject selectedBrushSizeControl; // Currently selected brush size control

    public GameObject[] ClearCanvas; // Assign the ClearCanvas GameObject in the Inspector

    void Start()
    {
        // Initialize the drawing texture
        canvasRect = drawingCanvas.GetComponent<RectTransform>();
        drawingTexture = new Texture2D((int)canvasRect.sizeDelta.x, (int)canvasRect.sizeDelta.y);
        drawingTexture.filterMode = FilterMode.Point;

        // Apply the texture to the Canvas
        var rawImage = drawingCanvas.GetComponent<RawImage>();
        rawImage.texture = drawingTexture;

        // Clear the texture to white
        ClearTexture();

        // Enable the grab action
        grabAction.action.Enable();

        // Disable the preview circle initially
        if (previewCircle != null)
        {
            previewCircle.enabled = false;
        }

        // Initialize visual feedback
        if (colorFeedbackUI != null)
        {
            colorFeedbackUI.color = brushColor; // Set initial color
        }

        if (colorFeedbackText != null)
        {
            colorFeedbackText.text = "Selected Color: " + ColorUtility.ToHtmlStringRGB(brushColor);
        }

        // Initialize progress indicator
        if (progressIndicator != null)
        {
            progressIndicator.fillAmount = 0f; // Start with empty progress
        }

        // Initialize the selected brush size control
        if (brushSizeControls.Length > 0)
        {
            selectedBrushSizeControl = brushSizeControls[1]; // Default to "MediumBrush"
            selectedBrushSizeControl.GetComponent<Renderer>().material.color = brushColor; // Set to brush color
        }
    }

    void Update()
    {
        // Toggle drawing when the grab button is pressed
        if (grabAction.action.triggered)
        {
            isDrawing = !isDrawing; // Toggle the drawing state
            Debug.Log("Drawing toggled: " + isDrawing);
        }

        // Perform a raycast from the head (Main Camera)
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Show and position the preview circle if not drawing
            if (previewCircle != null)
            {
                previewCircle.enabled = !isDrawing; // Show only when not drawing
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, hit.point, null, out localPoint);
                previewCircle.rectTransform.anchoredPosition = localPoint;
            }

            // Check if the user is looking at a color sphere
            bool isLookingAtSphere = false;
            foreach (var sphere in colorSpheres)
            {
                if (hit.collider.gameObject == sphere)
                {
                    isLookingAtSphere = true;
                    gazeTimer += Time.deltaTime;

                    // Update progress indicator
                    if (progressIndicator != null && brushColor != sphere.GetComponent<Renderer>().material.color)
                    {
                        progressIndicator.color = sphere.GetComponent<Renderer>().material.color; // set progress indicator color to target color
                        progressIndicator.fillAmount = gazeTimer / colorSelectionTime;
                    }

                    if (gazeTimer >= colorSelectionTime)
                    {
                        brushColor = sphere.GetComponent<Renderer>().material.color;
                        Debug.Log("Selected color: " + brushColor);

                        // Update visual feedback
                        if (colorFeedbackUI != null)
                        {
                            colorFeedbackUI.color = brushColor;
                        }

                        if (colorFeedbackText != null)
                        {
                            colorFeedbackText.text = "Selected Color: " + ColorUtility.ToHtmlStringRGB(brushColor);
                        }

                        // Update preview circle color
                        if (previewCircle != null)
                        {
                            previewCircle.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.5f); // Semi-transparent
                        }

                        // Update the selected brush size control color
                        if (selectedBrushSizeControl != null)
                        {
                            selectedBrushSizeControl.GetComponent<Renderer>().material.color = brushColor;
                        }

                        gazeTimer = 0f; // Reset the timer
                    }
                    break;
                }
            }

            // Check if the user is looking at a brush size control
            bool isLookingAtBrushSizeControl = false;
            foreach (var control in brushSizeControls)
            {
                if (hit.collider.gameObject == control)
                {
                    isLookingAtBrushSizeControl = true;
                    brushSizeGazeTimer += Time.deltaTime;

                    // Update progress indicator
                    if (progressIndicator != null && control != selectedBrushSizeControl)
                    {
                        progressIndicator.fillAmount = brushSizeGazeTimer / brushSizeSelectionTime;
                    }

                    if (brushSizeGazeTimer >= brushSizeSelectionTime)
                    {
                        // Update brush size based on the control
                        if (control.name == "SmallBrush")
                        {
                            brushSize = 0.005f;
                            previewCircle.transform.localScale = Vector3.one * 0.01f;
                        }
                        else if (control.name == "MediumBrush")
                        {
                            brushSize = 0.01f;
                            previewCircle.transform.localScale = Vector3.one * 0.02f;
                        }
                        else if (control.name == "LargeBrush")
                        {
                            brushSize = 0.02f;
                            previewCircle.transform.localScale = Vector3.one * 0.04f;
                        }

                        // Update the selected brush size control
                        if (selectedBrushSizeControl != null)
                        {
                            // Reset the color of the previously selected control
                            selectedBrushSizeControl.GetComponent<Renderer>().material.color = Color.white; // Default color
                        }

                        selectedBrushSizeControl = control; // Set the new selected control
                        selectedBrushSizeControl.GetComponent<Renderer>().material.color = brushColor; // Set to brush color

                        Debug.Log("Selected brush size: " + brushSize);
                        brushSizeGazeTimer = 0f; // Reset the timer
                    }
                    break;
                }
            }

            bool isLookingAtClearCanvas = false;
            foreach (var control in ClearCanvas)
            {
                if (hit.collider.gameObject == control)
                {
                    isLookingAtClearCanvas = true;
                    gazeTimer += Time.deltaTime;

                    // Update progress indicator
                    if (progressIndicator != null)
                    {
                        progressIndicator.fillAmount = gazeTimer / 4f;
                    }

                    if (gazeTimer >= 4f)
                    {
                        ClearTexture();
                        Debug.Log("Cleared the canvas");
                        gazeTimer = 0f; // Reset the timer
                    }
                    break;
                }
            }
            

            // Reset progress if not looking at a sphere or brush size control
            if (!isLookingAtSphere && !isLookingAtBrushSizeControl && !isLookingAtClearCanvas)
            {
                gazeTimer = 0f;
                brushSizeGazeTimer = 0f;
                if (progressIndicator != null)
                {
                    progressIndicator.fillAmount = 0f;
                }
            }

            // Draw on the texture if drawing is enabled and the raycast hits the Canvas
            if (hit.collider.gameObject == drawingCanvas.gameObject && isDrawing)
            {
                // Convert hit point to texture coordinates
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, hit.point, null, out localPoint);

                // Map the local point to texture coordinates
                int texX = (int)(localPoint.x + canvasRect.sizeDelta.x / 2);
                int texY = (int)(localPoint.y + canvasRect.sizeDelta.y / 2);

                // Draw on the texture
                DrawCircle(texX, texY, brushSize, brushColor);
                drawingTexture.Apply();
            }
        }
        else
        {
            // Hide the preview circle if the raycast doesn't hit anything
            if (previewCircle != null)
            {
                previewCircle.enabled = false;
            }

            // Reset progress if not looking at anything
            gazeTimer = 0f;
            brushSizeGazeTimer = 0f;
            if (progressIndicator != null)
            {
                progressIndicator.fillAmount = 0f;
            }
        }
    }

    void DrawCircle(int x, int y, float radius, Color color)
    {
        int radiusInt = (int)(radius * drawingTexture.width);
        for (int i = -radiusInt; i <= radiusInt; i++)
        {
            for (int j = -radiusInt; j <= radiusInt; j++)
            {
                if (i * i + j * j <= radiusInt * radiusInt)
                {
                    int texX = x + i;
                    int texY = y + j;

                    if (texX >= 0 && texX < drawingTexture.width && texY >= 0 && texY < drawingTexture.height)
                    {
                        drawingTexture.SetPixel(texX, texY, color);
                    }
                }
            }
        }
    }

    void ClearTexture()
    {
        Color[] clearPixels = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.white;
        }
        drawingTexture.SetPixels(clearPixels);
        drawingTexture.Apply();
    }
}