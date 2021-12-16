using System.Collections;
using System.Collections.Generic;
using Game;
using Units;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The players' manager objects on the server
// </summary>
[RequireComponent(typeof(Player))]
public class ServerPlayerCommands : NetworkBehaviour
{
    public Player Player { get; private set; }
    public Factory FactoryPrefab;
    public Unit CombatUnitPrefab;

    private Dictionary<ulong, Unit> _netObjectToUnit = new Dictionary<ulong, Unit>();

    void Awake()
    {
        if (!IsServer) enabled = false;
        Player = GetComponent<Player>();
    }
    
    
    
    [ServerRpc]
    public void SubmitSpawnFactoryServerRPC(ServerRpcParams rpcParams = default)
    {
        var client = rpcParams.Receive.SenderClientId;
        var po = Instantiate(FactoryPrefab, Player.Position.Value, Quaternion.identity).GetComponent<NetworkObject>();
        var colorComponent = po.GetComponent<PlayerColor>();
        colorComponent.SetPlayerColor(Player.Color.Value);
        po.SpawnWithOwnership(client);
    }
    
    [ServerRpc]
    public void DispatchAttackMoveServerRPC(AttackMoveCommand command, ServerRpcParams rpcParams = default)
    {
            
    }
    
    
}
