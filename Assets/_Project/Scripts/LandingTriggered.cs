using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingTriggered : MonoBehaviour
{
    // For send signal to platform controller about player statue.
   [SerializeField] private PlatformController platformController;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by " + other.tag);
        if(other.tag == "Player")
        {
            this.platformController.PlayerTriggeredLandingArea();
            Debug.Log("Triggered by player");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            this.platformController.PlayerExitLandingArea();
        }
    }
}
