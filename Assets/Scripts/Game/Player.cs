using Units;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class Player : NetworkBehaviour
    {
        public NetworkManager NetMgr => NetworkManager.Singleton;

        public Factory FactoryPrefab;

        public NetworkVariable<Color> Color = new NetworkVariable<Color>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        public void SetPlayerColor(Color color) => Color.Value = color;
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner) SpawnFactory();
        }

        public void SpawnFactory()
        {
            SubmitSpawnFactoryServerRPC();

            // if (NetMgr.IsServer)
            // {
            //     Debug.Log("Are we ever the server?");
            // }
            // else
            // {
            // }
        }


        [ServerRpc]
        private void SubmitSpawnFactoryServerRPC(ServerRpcParams rpcParams = default)
        {
            var client = rpcParams.Receive.SenderClientId;
            var po = Instantiate(FactoryPrefab, Position.Value, Quaternion.identity).GetComponent<NetworkObject>();
            var colorComponent = po.GetComponent<PlayerColor>();
            colorComponent.SetPlayerColor(Color.Value);
            po.SpawnWithOwnership(client);
        }
    }
}
