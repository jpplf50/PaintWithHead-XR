using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorToneManager : MonoBehaviour
{
    public HeadDrawing headDrawingScript; // Access Head Drawing Script
    public float raycastDistance = 10f;

    // Dictionary to store color tones for each base color
    public Dictionary<string, List<Color>> colorTones = new Dictionary<string, List<Color>>()
    {
        { "Red", new List<Color> { new Color(1, 0, 0), new Color(0.8f, 0, 0), new Color(1, 0.5f, 0.5f) } },
        { "Green", new List<Color> { new Color(0, 1, 0.125f), new Color(0, 0.8f, 0.1f), new Color(0.5f, 1, 0.5f) } },
        { "Blue", new List<Color> { new Color(0, 0.58f, 1), new Color(0, 0.4f, 0.8f), new Color(0.5f, 0.7f, 1) } },
        { "Yellow", new List<Color> { new Color(1, 0.88f, 0), new Color(1, 0.8f, 0.2f), new Color(1, 1, 0.5f) } },
        { "Black", new List<Color> { new Color(0, 0, 0), new Color(0.1f, 0.1f, 0.1f), new Color(0.2f, 0.2f, 0.2f) } },
        { "White", new List<Color> { new Color(1, 1, 1), new Color(0.9f, 0.9f, 0.9f), new Color(0.8f, 0.8f, 0.8f) } }
    };

    // Reference to the additional color tone spheres
    public GameObject[] redTones;
    public GameObject[] greenTones;
    public GameObject[] blueTones;
    public GameObject[] yellowTones;
    public GameObject[] blackTones;
    public GameObject[] whiteTones;

    void Start()
    {
        // Hide all additional color tones initially
        HideAllTones();
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, raycastDistance))
    {
        // Check if the user is looking at a plus sign
        if (hit.collider.CompareTag("PlusSign"))
        {
            string baseColor = hit.collider.name.Replace("Plus", ""); // Extract base color name
            ShowTones(baseColor); // Show additional color tones
        }

        // Check if the user is looking at a color tone sphere
        if (hit.collider.CompareTag("ColorTone"))
        {
            Color selectedColor = hit.collider.GetComponent<Renderer>().material.color;
            UpdateBrushColor(selectedColor); // Update the brush color
            HideAllTones(); // Hide the additional tones after selection
        }
    }
    else
    {
        // Hide the additional tones if the user is not looking at anything
        HideAllTones();
    }
}

    // Method to show additional color tones for a specific base color
    public void ShowTones(string baseColor)
    {
        HideAllTones(); // Hide all tones first

        switch (baseColor)
        {
            case "Red":
                SetTonesActive(redTones, true);
                break;
            case "Green":
                SetTonesActive(greenTones, true);
                break;
            case "Blue":
                SetTonesActive(blueTones, true);
                break;
            case "Yellow":
                SetTonesActive(yellowTones, true);
                break;
            case "Black":
                SetTonesActive(blackTones, true);
                break;
            case "White":
                SetTonesActive(whiteTones, true);
                break;
        }
    }

    // Method to hide all additional color tones
    public void HideAllTones()
    {
        SetTonesActive(redTones, false);
        SetTonesActive(greenTones, false);
        SetTonesActive(blueTones, false);
        SetTonesActive(yellowTones, false);
        SetTonesActive(blackTones, false);
        SetTonesActive(whiteTones, false);
    }

    // Helper method to set the active state of a tone array
    private void SetTonesActive(GameObject[] tones, bool isActive)
    {
        foreach (var tone in tones)
        {
            tone.SetActive(isActive);
        }
    }

    // Method to update the brush color
    private void UpdateBrushColor(Color color)
    {
        if (headDrawingScript != null)
        {
            headDrawingScript.brushColor = color; // Update the brush color
        }
    }
}