using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

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

    // Cursor line
    public LineRenderer cursorLine; // Assign the LineRenderer in the Inspector

    // Color tone management
    public GameObject[] plusSigns; // Assign the plus signs in the Inspector
    public Dictionary<string, List<Color>> colorTones = new Dictionary<string, List<Color>>()
    {
        { "Red", new List<Color> 
            { 
                new Color(0.5f, 0, 0), // Maroon (Darkest Red)
                new Color(0.7f, 0.1f, 0.1f), // Crimson
                new Color(0.8f, 0, 0), // Dark Red
                new Color(1, 0, 0), // Pure Red
                new Color(1, 0.2f, 0.2f), // Bright Red
                new Color(1, 0.4f, 0.4f), // Light Red
                new Color(1, 0.6f, 0.6f), // Pastel Red
                new Color(1, 0.7f, 0.7f), // Blush
                new Color(1, 0.8f, 0.8f), // Light Blush
                new Color(1, 0.9f, 0.9f) // Very Light Blush (Lightest Red)
            } 
        },
        { "Green", new List<Color> 
            { 
                new Color(0, 0.2f, 0), // Forest Green (Darkest Green)
                new Color(0, 0.4f, 0), // Emerald
                new Color(0, 0.5f, 0), // Dark Green
                new Color(0, 1, 0), // Pure Green
                new Color(0.2f, 0.8f, 0.2f), // Lime Green
                new Color(0.4f, 1, 0.4f), // Light Green
                new Color(0.6f, 1, 0.6f), // Mint
                new Color(0.7f, 1, 0.7f), // Light Mint
                new Color(0.8f, 1, 0.8f), // Pastel Green
                new Color(0.9f, 1, 0.9f) // Very Light Mint (Lightest Green)
            } 
        },
        { "Blue", new List<Color> 
            { 
                new Color(0, 0, 0.2f), // Navy (Darkest Blue)
                new Color(0, 0, 0.4f), // Dark Blue
                new Color(0, 0, 0.6f), // Medium Blue
                new Color(0, 0, 1), // Pure Blue
                new Color(0.2f, 0.2f, 1), // Sky Blue
                new Color(0.4f, 0.4f, 1), // Light Blue
                new Color(0.54f, 0.81f, 0.94f), // RGB(137, 207, 240) - Baby Blue
                new Color(0.6f, 0.85f, 0.95f), // Slightly Lighter Baby Blue
                new Color(0.7f, 0.9f, 0.98f), // Very Light Baby Blue
                new Color(0.8f, 0.95f, 1) // Lightest Baby Blue
            } 
        },
        { "Yellow", new List<Color> 
            { 
                new Color(0.5f, 0.5f, 0), // Mustard (Darkest Yellow)
                new Color(0.7f, 0.7f, 0), // Olive Yellow
                new Color(0.8f, 0.8f, 0), // Dark Yellow
                new Color(1, 1, 0), // Pure Yellow
                new Color(1, 1, 0.2f), // Bright Yellow
                new Color(1, 1, 0.4f), // Light Yellow
                new Color(1, 1, 0.6f), // Pastel Yellow
                new Color(1, 1, 0.7f), // Cream
                new Color(1, 1, 0.8f), // Light Cream
                new Color(1, 1, 0.9f) // Very Light Cream (Lightest Yellow)
            } 
        },
        { "Black", new List<Color> 
            { 
                new Color(0, 0, 0), // Pure Black (Darkest)
                new Color(0.1f, 0.1f, 0.1f), // Dark Gray
                new Color(0.2f, 0.2f, 0.2f), // Charcoal
                new Color(0.3f, 0.3f, 0.3f), // Slate
                new Color(0.4f, 0.4f, 0.4f), // Gray
                new Color(0.5f, 0.5f, 0.5f), // Medium Gray
                new Color(0.6f, 0.6f, 0.6f), // Light Gray
                new Color(0.7f, 0.7f, 0.7f), // Silver
                new Color(0.8f, 0.8f, 0.8f), // Platinum
                new Color(0.9f, 0.9f, 0.9f) // Off-White (Lightest Black)
            } 
        },
        { "White", new List<Color> 
            { 
                new Color(0.9f, 0.9f, 0.9f), // Off-White (Darkest White)
                new Color(0.95f, 0.95f, 0.95f), // Snow
                new Color(0.96f, 0.96f, 0.96f), // Light Snow
                new Color(0.97f, 0.97f, 0.97f), // Bright Snow
                new Color(0.98f, 0.98f, 0.98f), // Very Bright Snow
                new Color(0.99f, 0.99f, 0.99f), // Almost White
                new Color(1, 1, 1), // Pure White
                new Color(1, 1, 1), // Pure White (Duplicate for consistency)
                new Color(1, 1, 1), // Pure White (Duplicate for consistency)
                new Color(1, 1, 1) // Pure White (Lightest White)
            } 
        },
        { "Orange", new List<Color> 
            { 
                new Color(0.3f, 0.15f, 0), // Dark Brown (Darkest)
                new Color(0.4f, 0.2f, 0), // Deep Brown
                new Color(0.5f, 0.25f, 0), // Medium Brown
                new Color(0.6f, 0.3f, 0), // Light Brown
                new Color(0.7f, 0.35f, 0), // Tan
                new Color(0.8f, 0.4f, 0), // Dark Orange
                new Color(1, 0.5f, 0), // Pure Orange (255, 114, 0 in RGB)
                new Color(1, 0.6f, 0.2f), // Bright Orange
                new Color(1, 0.7f, 0.4f), // Light Orange
                new Color(1, 0.8f, 0.6f) // Pastel Orange (Lightest)
            } 
        },
        { "Purple", new List<Color> 
            { 
                new Color(0.3f, 0, 0.3f), // Darkest Purple
                new Color(0.4f, 0, 0.4f), // Dark Purple
                new Color(0.5f, 0, 0.5f), // Deep Purple
                new Color(1, 0, 1), // Pure Purple (255, 0, 255 in RGB)
                new Color(1, 0.2f, 1), // Bright Purple
                new Color(1, 0.4f, 1), // Light Purple
                new Color(1, 0.6f, 1), // Pastel Purple
                new Color(1, 0.8f, 1), // Lavender
                new Color(1, 0.9f, 1), // Light Lavender
                new Color(1, 1, 1) // White (Lightest Purple)
            } 
        }
    };
    private Dictionary<string, GameObject[]> colorToneSpheres = new Dictionary<string, GameObject[]>();
    private string currentToneGroup = ""; // Currently visible tone group
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

        // Initialize the cursor line
        if (cursorLine != null)
        {
            cursorLine.positionCount = 2;
            cursorLine.startWidth = 0.005f;
            cursorLine.endWidth = 0.005f;
            cursorLine.material = new Material(Shader.Find("Unlit/Color")) { color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.3f) }; // Brush color line with alpha
        }

        // Initialize color tone spheres
        InitializeColorTones();
    }

    void InitializeColorTones()
    {
        foreach (var plusSign in plusSigns)
        {
            string baseColor = plusSign.name.Replace("Plus", "");
            if (colorTones.ContainsKey(baseColor))
            {
                // Create and position the tone spheres
                List<GameObject> toneSpheres = new List<GameObject>();
                for (int i = 0; i < colorTones[baseColor].Count; i++)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = plusSign.transform.position + new Vector3(-i/3f, 0, -i/3f); // Position the spheres
                    sphere.transform.localScale = Vector3.one * 0.4f; // Scale down the spheres
                    sphere.GetComponent<Renderer>().material.color = colorTones[baseColor][i];
                    sphere.SetActive(false); // Hide initially
                    toneSpheres.Add(sphere);
                }
                colorToneSpheres[baseColor] = toneSpheres.ToArray();
            }
        }
    }

    void Update()
    {
        // Toggle drawing when the grab button is pressed
        if (grabAction.action.triggered)
        {
            isDrawing = !isDrawing; // Toggle the drawing state
            cursorLine.enabled = !isDrawing; // Show the cursor line when not drawing
            Debug.Log("Drawing toggled: " + isDrawing);
        }

        // Perform a raycast from the head (Main Camera)
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Update the cursor line
            if (cursorLine != null)
            {
                // Convert the hit point to local space relative to the camera
                Vector3 localHitPoint = cursorLine.transform.InverseTransformPoint(hit.point);

                // Update the LineRenderer positions
                cursorLine.SetPosition(0, Vector3.zero); // Start at the camera (local origin)
                cursorLine.SetPosition(1, localHitPoint); // End at the hit point
            }
            
            // Show and position the preview circle if not drawing
            if (previewCircle != null)
            {
                previewCircle.enabled = !isDrawing; // Show only when not drawing
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, hit.point, null, out localPoint);
                previewCircle.rectTransform.anchoredPosition = localPoint;
            }

            // Check if the user is looking at a plus sign
            bool isLookingAtPlusSign = false;
            foreach (var plusSign in plusSigns)
            {
                if (hit.collider.gameObject == plusSign)
                {
                    isLookingAtPlusSign = true;
                    gazeTimer += Time.deltaTime;

                    // Update progress indicator
                    if (progressIndicator != null)
                    {
                        progressIndicator.fillAmount = gazeTimer / colorSelectionTime;
                    }

                    if (gazeTimer >= colorSelectionTime)
                    {
                        string baseColor = plusSign.name.Replace("Plus", "");
                        ShowColorTones(baseColor); // Show additional color tones
                        gazeTimer = 0f; // Reset the timer
                    }
                    break;
                }
            }

            // Check if the user is looking at a color tone sphere
            bool isLookingAtColorTone = false;
            foreach (var toneGroup in colorToneSpheres)
            {
                foreach (var toneSphere in toneGroup.Value)
                {
                    if (hit.collider.gameObject == toneSphere)
                    {
                        isLookingAtColorTone = true;
                        gazeTimer += Time.deltaTime;

                        // Update progress indicator
                        if (progressIndicator != null && brushColor != toneSphere.GetComponent<Renderer>().material.color)
                        {
                            progressIndicator.color = toneSphere.GetComponent<Renderer>().material.color; // set progress indicator color to target color
                            progressIndicator.fillAmount = gazeTimer / colorSelectionTime;
                        }

                        if (gazeTimer >= colorSelectionTime)
                        {
                            brushColor = toneSphere.GetComponent<Renderer>().material.color;
                            cursorLine.material = new Material(Shader.Find("Unlit/Color")) { color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.3f) }; // Brush color line with alpha
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
                            HideColorTones(); // Hide the additional tones
                        }
                            break;
                    }
                }
                if (isLookingAtColorTone) break;
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
                        cursorLine.material = new Material(Shader.Find("Unlit/Color")) { color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.3f) }; // Brush color line with alpha
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
            if (!isLookingAtSphere && !isLookingAtBrushSizeControl && !isLookingAtClearCanvas && !isLookingAtPlusSign && !isLookingAtColorTone)
            {
                gazeTimer = 0f;
                brushSizeGazeTimer = 0f;
                if (progressIndicator != null)
                {
                    progressIndicator.fillAmount = 0f;
                    progressIndicator.color = brushColor; // Reset progress indicator color
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
            if (cursorLine != null)
            {
                // Extend the line to the max distance in local space
                cursorLine.SetPosition(0, Vector3.zero); // Start at the camera (local origin)
                cursorLine.SetPosition(1, Vector3.forward * raycastDistance); // End at max distance
            }
            
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

            //HideColorTones(); // Hide additional tones if not looking at anything
        }
    }

    void ShowColorTones(string baseColor)
    {
        if (colorToneSpheres.ContainsKey(baseColor))
        {
            HideColorTones(); // Hide any currently visible tones
            foreach (var toneSphere in colorToneSpheres[baseColor])
            {
                toneSphere.SetActive(true); // Show the tone spheres
            }
            currentToneGroup = baseColor; // Set the current tone group
        }
    }

    void HideColorTones()
    {
        if (!string.IsNullOrEmpty(currentToneGroup) && colorToneSpheres.ContainsKey(currentToneGroup))
        {
            foreach (var toneSphere in colorToneSpheres[currentToneGroup])
            {
                toneSphere.SetActive(false); // Hide the tone spheres
            }
            currentToneGroup = ""; // Clear the current tone group
        }
    }

    void UpdateBrushColor(Color color)
    {
        brushColor = color;
        cursorLine.material.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.3f); // Update cursor line color
        if (colorFeedbackUI != null) colorFeedbackUI.color = brushColor;
        if (colorFeedbackText != null) colorFeedbackText.text = "Selected Color: " + ColorUtility.ToHtmlStringRGB(brushColor);
        if (previewCircle != null) previewCircle.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.5f);
        if (selectedBrushSizeControl != null) selectedBrushSizeControl.GetComponent<Renderer>().material.color = brushColor;
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