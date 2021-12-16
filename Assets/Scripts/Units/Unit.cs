using System.Collections;
using System.Collections.Generic;
using Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Unit : NetworkBehaviour
{
    public bool CanMove;
    public bool CanAttack;
    public int OwnedLayer;
    public LayerMask EnemiesLayerMask;
    public GameObject PhysicsGameObject;
    public Movement Movement;
    public override void OnNetworkSpawn()
    {
    }

    void OnEnable()
    {
        // Set layer if we're playing locally
        if (NetworkManager.Singleton == null) SetupOwnership(OwnedLayer, EnemiesLayerMask);
    }

    // Call on server and set client RPCs
    public void SetupOwnership(int playerLayer, LayerMask enemyMask)
    {
        OwnedLayer = playerLayer;
        EnemiesLayerMask = enemyMask;
        PhysicsGameObject.layer = OwnedLayer;
    }

    public void Move(Vector3 position)
    {
        
    }

    public void Stop()
    {
        
    }

    public void AttackMove(Vector3 position)
    {
        if (CanMove) Movement.MoveTo(position);
    }
}
