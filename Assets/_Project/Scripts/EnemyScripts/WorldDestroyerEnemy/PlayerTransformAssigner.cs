using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTransformAssigner : MonoBehaviour
{
    [SerializeField] private RocketSpawner rocketSpawner1;
    [SerializeField] private RocketSpawner rocketSpawner2;
    [SerializeField] private UpperBodyFollowPlayer upperBodyFollowPlayer;

    public void SetPlayerTransform(Transform playerTransform)
    {
        this.rocketSpawner1.SetPlayerTransform(playerTransform);
        this.rocketSpawner2.SetPlayerTransform(playerTransform);
        this.upperBodyFollowPlayer.SetPlayerTransform(playerTransform);
    }
}
