using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTexture : MonoBehaviour
{
    void Start()
    {
        Texture2D texture = new Texture2D(2048, 2048);
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
    }
}
