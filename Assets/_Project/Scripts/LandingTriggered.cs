using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingTriggered : MonoBehaviour
{
    // For send signal to platform controller about player statue.
   [SerializeField] private PlatformController platformController;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            this.platformController.PlayerTriggeredLandingArea();
            Debug.Log("Player triggered to landing area");
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
