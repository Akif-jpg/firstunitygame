using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitForMarketPlaceScript : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bullet")
        {
            this.gameController.SendSignalHitMarketplace();
        }
    }

}
