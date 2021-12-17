using System.Collections;
using System.Collections.Generic;
using Game;
using Units;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerColor))]
public class Factory : NetworkBehaviour
{
    public Transform SpawnPoints;
    public Unit UnitPrefab;
    private PlayerColor _playerColor;
    private ServerPlayerCommands _commands;

    private Player _player;
    
    void Awake()
    {
        _playerColor = GetComponent<PlayerColor>();
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // _player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()
            //     .GetComponent<Player>();
            SubmitStartSpawningServerRPC();
        }
    }

    public void SetupFactory(Player player)
    {
        _player = player;
    }
    
    [ServerRpc]
    private void SubmitStartSpawningServerRPC(ServerRpcParams rpcParams = default)
    {
        var client = rpcParams.Receive.SenderClientId;
        StartCoroutine(SpawnUnits(client));
    }

    public IEnumerator SpawnUnits(ulong clientId)
    {
        for (int i = 0; i < SpawnPoints.childCount; i++)
        {
            var spawnPoint = SpawnPoints.GetChild(i);
            Vector3 position = spawnPoint.position;
            Quaternion rotation = spawnPoint.rotation;
            // spawn unit
            var po = Instantiate(UnitPrefab, position, rotation).GetComponent<NetworkObject>();
            var unit = po.GetComponent<Unit>();
            var colorComponent = po.GetComponent<PlayerColor>();
            colorComponent.SetPlayerColor(_playerColor.Color.Value);
            po.SpawnWithOwnership(clientId);
            unit.SetupOwnershipClientRPC(_player.GetPlayerLayerInt(), _player.GetEnemyLayerMask());
            yield return new WaitForSeconds(3f);
        }
    }


    void OnDrawGizmos()
    {
        if (SpawnPoints == null) return;
        for (int i = 0; i < SpawnPoints.childCount; i++)
        {
            var spawnPoint = SpawnPoints.GetChild(i);
            Gizmos.DrawSphere(spawnPoint.position, 0.3f);
        }
    }
    

}
