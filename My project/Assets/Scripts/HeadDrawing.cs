using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


public class HeadDrawing : MonoBehaviour
{
    // Networking variables
    private TcpListener tcpListener;
    private Thread tcpThread;
    private TcpClient connectedClient;
    private NetworkStream stream;
    public int port = 5005;

    // Sequence logic
    public bool isSequenceActive = false; // Flag to check if the sequence is active - send everything after each stroke
    List<Vector2> coordinates = new List<Vector2>();

    List<string> listOfCoordinates = new List<string>(); // List to store coordinates as strings

    public bool isSendingEnabled = true; // Flag to enable/disable drawing

    public GameObject scene; // Whole scene (buttons + canvas)

    private Vector2 smoothedLocalPoint; // Smoothed local point for drawing
    private bool toggleLerp = false; // Toggle for lerping the indicator position
    public float smoothingFactor = 0.1f; // Try 0.05 to 0.2 depending on responsiveness
    private bool firstPoint = true; // Flag to check if it's the first point
    public TextMeshProUGUI textToggleSmooth; // Assign the Text GameObject in the Inspector

    public TextMeshProUGUI textToggleSequence; // Assign the Text GameObject in the Inspector
    public TextMeshProUGUI textToggleSending; // Assign the Text GameObject in the Inspector
    public Canvas drawingCanvas; // Assign your Canvas in the Inspector
    public float raycastDistance = 10f;
    public float brushSize = 0.01f; // Current brush size
    public Color brushColor = Color.black;

    private Texture2D drawingTexture;
    private RectTransform canvasRect;
    private bool isDrawing = false; // Toggle for drawing

    public float indicatorFollowSpeed = 0.1f; // Speed at which the indicator follows
    private Vector3 indicatorTargetPosition; // Target position for the indicator

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
        PrintLocalIPAddress(); // Print the local IP address
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
            //cursorLine.material = new Material(Shader.Find("Unlit/Color")) { color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.3f) }; // Brush color line with alpha
            Shader unlitShader = Shader.Find("Unlit/Color");
            if (unlitShader != null)
            {
                cursorLine.material = new Material(unlitShader) { color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.3f) };
            }
            else
            {
                Debug.LogError("Unlit/Color shader not found! Using fallback shader.");
                cursorLine.material = new Material(Shader.Find("Standard")) { color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.3f) };
            }
        }

        // Initialize color tone spheres
        InitializeColorTones();

        Debug.Log("Starting TCP thread...");
        tcpThread = new Thread(StartServer);
        tcpThread.IsBackground = true;
        tcpThread.Start();

        
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
                    sphere.transform.SetParent(plusSign.transform, worldPositionStays: true);
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
        if (grabAction.action.triggered || Input.GetKeyDown(KeyCode.D))
        {
            isDrawing = !isDrawing; // Toggle the drawing state
            cursorLine.enabled = !isDrawing; // Show the cursor line when not drawing
            Debug.Log("Drawing toggled: " + isDrawing);
            StringBuilder messageBuilder = new StringBuilder();
            foreach (var coord in coordinates)
            {
                messageBuilder.Append($"{coord[0]},{coord[1]} ");
            }
            messageBuilder.Append("\n"); // End the message with a newline
            listOfCoordinates.Add(messageBuilder.ToString()); // Add the coordinates to the list
            if (isDrawing)
                firstPoint = true; //make sure first point is not smothed
            if (!isDrawing && isSequenceActive)
            {
                SendCoordinatesSequenced(coordinates); // Send the coordinates when drawing is stopped
            }
            
            else
            {
                coordinates.Clear(); // Clear the coordinates when starting a new drawing
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (string coordList in listOfCoordinates)
            {
                Debug.Log("Sending coordinates from list: " + coordList);
                SendString(coordList); // Send all coordinates in the list
            }
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            isSendingEnabled = !isSendingEnabled; // Toggle sending state
            Debug.Log("Sending toggled: " + isSendingEnabled);
            if (isSendingEnabled)
                textToggleSending.text = "Sending Mode: On";
            else
                textToggleSending.text = "Sending Mode: Off";
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            isSequenceActive = !isSequenceActive; // Change the sequence mode
            Debug.Log("Toggled sequence mode: " + isSequenceActive);
            if (isSequenceActive)
                textToggleSequence.text = "Sequence Mode: On";
            else
                textToggleSequence.text = "Sequence Mode: Off";
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearTexture();
            listOfCoordinates.Clear(); // Clear the list of coordinates
            Debug.Log("Canvas cleared");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Transform cam = Camera.main.transform;

            // Step 1: Place the scene 2.3 meters in front of the camera
            Vector3 forward = cam.forward;
            forward.y = 0f; // Keep it level
            forward.Normalize();

            scene.transform.position = cam.position + forward * 2.3f;

            // Step 2: Make the scene look at the player
            scene.transform.LookAt(new Vector3(cam.position.x, scene.transform.position.y, cam.position.z));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveCanvasAsImage();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            toggleLerp = !toggleLerp;
            Debug.Log("Lerp toggled: " + toggleLerp);
            if(toggleLerp)
                textToggleSmooth.text = "Smoothness is On";
            else
                textToggleSmooth.text = "Smoothness is Off";
            // Update the text based on the toggle state
            
        }

        // Perform a raycast from the head (Main Camera)
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Set the target position for the progress indicator
            indicatorTargetPosition = Vector3.Lerp(transform.position, hit.point, 0.5f); // Adjust the lerp factor as needed

            // Smoothly move the progress indicator toward the target position
            if (progressIndicator != null)
            {
                progressIndicator.transform.LookAt(Camera.main.transform); // Look at the camera
                progressIndicator.rectTransform.position = Vector3.Lerp(
                    progressIndicator.rectTransform.position,
                    indicatorTargetPosition,
                    indicatorFollowSpeed
                );
            }
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
                        listOfCoordinates.Clear(); // Clear the list of coordinates
                        Debug.Log("Cleared the canvas");
                        gazeTimer = 0f; // Reset the timer
                    }
                    break;
                }
            }

            bool isLookingAtSaveCanvas = false;
            if (hit.collider.CompareTag("SaveButton"))
            {
                isLookingAtSaveCanvas = true;
                gazeTimer += Time.deltaTime;

                // Update progress indicator
                if (progressIndicator != null)
                {
                    progressIndicator.fillAmount = gazeTimer / 2f;
                }

                if (gazeTimer >= 2f)
                {
                    SaveCanvasAsImage();
                    Debug.Log("Saved the canvas as an image");
                    gazeTimer = 0f; // Reset the timer
                }
            }

            bool isLookingAtForward = false;
            if (hit.collider.CompareTag("Forward"))
            {
                isLookingAtForward = true;
                gazeTimer += Time.deltaTime;

                // Update progress indicator
                if (progressIndicator != null)
                {
                    progressIndicator.fillAmount = gazeTimer / 2f;
                }

                if (gazeTimer >= 2f)
                {
                    scene.transform.position += Camera.main.transform.forward * 0.2f; // Move the scene forward
                    Debug.Log("Forward action triggered");
                    gazeTimer = 0f; // Reset the timer
                }
            }

            bool isLookingAtBackward = false;
            if (hit.collider.CompareTag("Backward"))
            {
                isLookingAtBackward = true;
                gazeTimer += Time.deltaTime;

                // Update progress indicator
                if (progressIndicator != null)
                {
                    progressIndicator.fillAmount = gazeTimer / 2f;
                }

                if (gazeTimer >= 2f)
                {
                    scene.transform.position -= Camera.main.transform.forward * 0.2f; // Move the scene backward
                    Debug.Log("Backward action triggered");
                    gazeTimer = 0f; // Reset the timer
                }
            }
            

            // Reset progress if not looking at a sphere or brush size control
            if (!isLookingAtSphere && !isLookingAtBrushSizeControl && !isLookingAtClearCanvas && !isLookingAtPlusSign && !isLookingAtColorTone && !isLookingAtSaveCanvas && !isLookingAtForward && !isLookingAtBackward)
            {
                gazeTimer = 0f;
                brushSizeGazeTimer = 0f;
                if (progressIndicator != null)
                {
                    progressIndicator.fillAmount = 0f; // Reset progress indicator
                    progressIndicator.color = brushColor; // Reset progress indicator color
                }
            }

            // Draw on the texture if drawing is enabled and the raycast hits the Canvas
            if (hit.collider.gameObject == drawingCanvas.gameObject && isDrawing)
            {
                // Convert hit point to texture coordinates
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, hit.point, null, out localPoint);

                // Apply smoothing if enabled
                if (toggleLerp)
                {
                    if(firstPoint)
                        smoothedLocalPoint = localPoint; // Initialize smoothedLocalPoint to the first point
                    else
                        smoothedLocalPoint = Vector2.Lerp(smoothedLocalPoint, localPoint, smoothingFactor);
                }
                else
                {
                    smoothedLocalPoint = localPoint;
                }

                // Don't smooth the first point
                if (firstPoint)
                {
                    int texX = (int)(localPoint.x + canvasRect.sizeDelta.x / 2);
                    int texY = (int)(localPoint.y + canvasRect.sizeDelta.y / 2);
                    firstPoint = false; // Set to false after the first point

                    // Draw on the texture
                    DrawCircle(texX, texY, brushSize, brushColor);
                    drawingTexture.Apply();
                    Vector2 temp = new Vector2(texX, texY);
                    coordinates.Add(temp); // Add the coordinates to the list
                    if (!isSequenceActive && isSendingEnabled)
                        SendCoordinates(texX, texY);
                    
                }
                else
                {
                    // Map the local point to texture coordinates
                    int texX = (int)(smoothedLocalPoint.x + canvasRect.sizeDelta.x / 2);
                    int texY = (int)(smoothedLocalPoint.y + canvasRect.sizeDelta.y / 2);
                    // Draw on the texture
                    DrawCircle(texX, texY, brushSize, brushColor);
                    drawingTexture.Apply();
                    Vector2 temp = new Vector2(texX, texY);
                    coordinates.Add(temp); // Add the coordinates to the list
                    if (!isSequenceActive && isSendingEnabled)
                        SendCoordinates(texX, texY);
                }
                
                
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
    
    private void SendCoordinates(int x, int y)
    {
        lock (this)
        {
            if (stream != null && stream.CanWrite)
            {
                try
                {
                    string message = $"{x},{y}\n";
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error sending data: " + ex.Message);
                    stream = null;
                    connectedClient = null;
                }
            }
        }
    }

    void SendCoordinatesSequenced(List<Vector2> coordinates)
    {
        lock (this)
        {
            if (stream != null && stream.CanWrite)
            {
                try
                {
                    StringBuilder messageBuilder = new StringBuilder();
                    foreach (var coord in coordinates)
                    {
                        messageBuilder.Append($"{coord[0]},{coord[1]} ");
                    }
                    messageBuilder.Append("\n"); // End the message with a newline
                    byte[] data = Encoding.ASCII.GetBytes(messageBuilder.ToString());
                    stream.Write(data, 0, data.Length);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error sending sequenced data: " + ex.Message);
                    stream = null;
                    connectedClient = null;
                }
            }
        }
    }

    private void SendString(string message)
    {
        lock (this)
        {
            if (stream != null && stream.CanWrite)
            {
                try
                {
                    if (!message.EndsWith("\n"))
                        message += "\n"; // Ensure newline termination if needed

                    byte[] data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error sending string data: " + ex.Message);
                    stream = null;
                    connectedClient = null;
                }
            }
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

    public void SaveCanvasAsImage()
    {
        // Create a Texture2D from the drawing texture
        Texture2D texture = new Texture2D(drawingTexture.width, drawingTexture.height, TextureFormat.RGB24, false);
        texture.SetPixels(drawingTexture.GetPixels());
        texture.Apply();

        texture = FlipTextureVertically(texture); // Flip the texture vertically

        // Encode the texture to a PNG file
        byte[] bytes = texture.EncodeToPNG();
        Destroy(texture); // Free up memory

        // Define the file path
        string filePath = Application.persistentDataPath + "/CanvasArtwork.png";
        int fileNumber = 1;
        while (File.Exists(filePath))
        {
            filePath = Application.persistentDataPath + "/CanvasArtwork_" + fileNumber + ".png";
            fileNumber++;
        }

        // Save the file
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Canvas saved to: " + filePath);
    }

    // Helper method to flip the texture vertically
    private Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        int width = original.width;
        int height = original.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flipped.SetPixel(width - x - 1, y, original.GetPixel(x, y));
            }
        }

        flipped.Apply();
        return flipped;
    }
    void PrintLocalIPAddress()
    {
        string localIP = "Not available";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        Debug.Log("Local IP Address: " + localIP);
    }

    void StartServer()
    {
        try
        {
            IPAddress ip = IPAddress.Any;
            tcpListener = new TcpListener(ip, port);
            tcpListener.Start();
            Debug.Log("TCP Server started on port " + port);

            while (true)
            {
                Debug.Log("Waiting for connection...");
                connectedClient = tcpListener.AcceptTcpClient();
                stream = connectedClient.GetStream();
                Debug.Log("Client connected: " + connectedClient.Client.RemoteEndPoint);

                // Echo loop
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string received = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Debug.Log("Received: " + received);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("TCP Server error: " + e.Message);
        }
    }


    void OnApplicationQuit()
    {
        tcpListener?.Stop();
        stream?.Close();
        connectedClient?.Close();
        if (tcpThread != null && tcpThread.IsAlive)
            tcpThread.Abort();
    }

}