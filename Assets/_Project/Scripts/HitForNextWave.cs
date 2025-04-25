using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitForNextWave : MonoBehaviour
{

    [SerializeField] private GameController gameController;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bullet")
        {
            this.gameController.SendSignalHitNextWave();
        }
    }
}
