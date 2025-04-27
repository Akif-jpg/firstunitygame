using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistBullet : MonoBehaviour
{
    private string damageUUID;
    void Start()
    {
        this.damageUUID = Guid.NewGuid().ToString();
    }

    void OnParticleCollision(GameObject other)
    {
        if(other.tag == "Player")
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            playerHealth.AddDamage(1f, this.damageUUID);
            playerHealth.RemoveDamage(this.damageUUID);
        }
    }
}
