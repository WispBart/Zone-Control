using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayConnectionManager
{
    NetworkManager _netMgr => NetworkManager.Singleton;

    private static RelayConnectionManager _relayConnectionManager = new RelayConnectionManager();
    public static RelayConnectionManager Get() => _relayConnectionManager;
    
    public async Task<string> StartServer(int maxConnections)
    {
        string joinCode = "";
        try
        {
            var alloc = await AllocateRelayServerAndGetJoinCode(maxConnections);
            var transport = _netMgr.GetComponent<UnityTransport>();
            transport.SetRelayServerData(alloc.ipv4address, alloc.port,
                alloc.allocationIdBytes, alloc.key, alloc.connectionData);
            _netMgr.StartHost();
            joinCode = alloc.joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to start relay server. Exception " + e.Message);
        }
        return joinCode;
    }

    public async Task JoinServer(string joinCode)
    {
        try
        {
            var alloc = await JoinRelayServerFromJoinCode(joinCode);
            var transport = _netMgr.GetComponent<UnityTransport>();
            transport.SetRelayServerData(alloc.ipv4address, alloc.port,
                alloc.allocationIdBytes, alloc.key, alloc.connectionData, alloc.hostConnectionData);
            _netMgr.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to join relay server. Exception " + e.Message);
        }
    }
    
    
    public static async Task<(string ipv4address, ushort port, byte[] allocationIdBytes, byte[] connectionData, byte[] key, string joinCode)> 
        AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        Allocation allocation;
        string createJoinCode;
        try
        {
            allocation = await Relay.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId}");

        try
        {
            createJoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        return (allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.Key, createJoinCode);
    }
    
    public static async Task<(string ipv4address, ushort port, byte[] allocationIdBytes, byte[] connectionData, byte[] hostConnectionData, byte[] key)> 
        JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"client: {allocation.AllocationId}");

        return (allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.HostConnectionData, allocation.Key);
    }
    
}
