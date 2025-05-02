using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockerDamageArea : MonoBehaviour
{
    private string damageUUID;
    void Start()
    {
        this.damageUUID = Guid.NewGuid().ToString();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            playerHealth.AddDamage(10f, this.damageUUID);
            playerHealth.RemoveDamage(this.damageUUID);
        }
    }

}
