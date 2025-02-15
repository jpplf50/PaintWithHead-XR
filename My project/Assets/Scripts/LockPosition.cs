using UnityEngine;

public class LockPosition : MonoBehaviour
{
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        transform.position = initialPosition; // Lock the position
    }
}
