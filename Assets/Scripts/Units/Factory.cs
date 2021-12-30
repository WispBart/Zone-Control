using System.Collections;
using System.Collections.Generic;
using Game;
using Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PlayerColor))]
public class Factory : NetworkBehaviour
{
    public Unit UnitPrefab;
    public EventObject SpawnTicker;
    private PlayerColor _playerColor;
    private ServerPlayerCommands _commands;

    private Player _player;
    
    void Awake()
    {
        _playerColor = GetComponent<PlayerColor>();
    }

    void OnEnable()
    {
        if (IsServer) SpawnTicker.AddListener(OnSpawnTick);
    }

    void OnDisable()
    {
        if (IsServer) SpawnTicker.RemoveListener(OnSpawnTick);
    }
    

    void OnSpawnTick()
    {
        // Spawn unit for owning player.
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            Vector3 position = hit.position;
            // spawn unit
            var po = Instantiate(UnitPrefab, position, Quaternion.identity).GetComponent<NetworkObject>();
            var unit = po.GetComponent<Unit>();
            var colorComponent = po.GetComponent<PlayerColor>();
            colorComponent.SetPlayerColor(_playerColor.Color.Value);
            po.SpawnWithOwnership(OwnerClientId);
            unit.SetupOwnershipClientRPC(_player.GetPlayerLayerInt(), _player.GetEnemyLayerMask());
        }

    }
    

    public void SetupFactory(Player player)
    {
        _player = player;
    }
    
}
