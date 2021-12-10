using System.Collections;
using System.Collections.Generic;
using HelloWorld;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerColor))]
public class Factory : NetworkBehaviour
{
    public Transform SpawnPoints;
    public Unit UnitPrefab;
    private PlayerColor _playerColor;
    
    void Awake()
    {
        _playerColor = GetComponent<PlayerColor>();
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner) SubmitStartSpawningServerRPC();
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
            var colorComponent = po.GetComponent<PlayerColor>();
            colorComponent.SetPlayerColor(_playerColor.Color.Value);
            po.SpawnWithOwnership(clientId);
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
