using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wheelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f; // Degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.right; // Default rotation around X axis
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the wheel around the specified axis at the given speed
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}