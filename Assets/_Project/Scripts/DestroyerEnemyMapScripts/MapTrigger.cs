using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapTrigger : MonoBehaviour
{
    [SerializeField] private DestroyerEnemyMapController destroyerEnemyMapController;
    [SerializeField] private MapTriggerId id;

    void OnTriggerEnter(Collider other)
    {
        destroyerEnemyMapController.SendSignalFrom(this.id, other.tag);
    }

    public MapTriggerId GetMapTriggerId()
    {
        return this.id;
    }
}
