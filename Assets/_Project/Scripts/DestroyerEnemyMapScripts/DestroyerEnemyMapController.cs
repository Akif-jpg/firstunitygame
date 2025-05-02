// DestroyerEnemyMapController.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerEnemyMapController : MonoBehaviour
{
    [SerializeField] private List<MapTrigger> mapTriggersList;

    private MapTriggerId playerLocatedTriggerId;
    public void SendSignalFrom(MapTriggerId signalSender, string tagName)
    {
        if (tagName == "Player")
        {
            this.playerLocatedTriggerId = signalSender;
        }
    }

    public Dictionary<MapTriggerId, MapTrigger> GetMapTriggersList()
    {
        var map = new Dictionary<MapTriggerId, MapTrigger>();
        foreach (var trigger in mapTriggersList)
        {
            if (trigger == null) continue;
            var id = trigger.GetMapTriggerId();
            if (!map.ContainsKey(id))
                map.Add(id, trigger);
            else
                Debug.LogWarning($"Duplicate MapTriggerId detected: {id}");
        }
        return map;
    }

    // 1. Yeni eklenen getter:
    public MapTriggerId GetPlayerLocatedTriggerId() => playerLocatedTriggerId;
}
