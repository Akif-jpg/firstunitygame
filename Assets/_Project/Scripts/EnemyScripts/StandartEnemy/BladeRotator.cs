using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGunRotator : MonoBehaviour
{
    public float rotationSpeed = 200f; // derece/saniye

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
