using System.Collections.Generic;
using System.Linq;
using Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Player : NetworkBehaviour
    {
        public NetworkManager NetMgr => NetworkManager.Singleton;
        
        public NetworkVariable<Color> Color = new NetworkVariable<Color>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public ServerPlayerCommands serverPlayerCommands;

        private bool _hasNetMgr;
        public void SetPlayerColor(Color color) => Color.Value = color;
        
        public override void OnNetworkSpawn()
        {
            serverPlayerCommands = GetComponent<ServerPlayerCommands>();
            if (IsOwner) SpawnFactory();
        }
        void Start()
        {
            if (!_hasNetMgr) SpawnFactory();
        }
        public void SpawnFactory()
        {
            serverPlayerCommands.SubmitSpawnFactoryServerRPC();
        }

        public void AttackMove(HashSet<Selectable> units, Vector3 target)
        {
            ulong[] unitIDs = units.Select(u => u.Unit.NetworkObjectId).ToArray();
            var attackMove = new AttackMoveCommand()
            {
                Units = unitIDs,
                TargetPosition = target,
            };
            serverPlayerCommands.DispatchAttackMoveServerRPC(attackMove);
        }

        void Awake()
        {
            _hasNetMgr = NetMgr != null;
        }




        
    }

    public struct AttackMoveCommand : INetworkSerializable
    {
        public ulong[] Units;
        public Vector3 TargetPosition;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // impl
        }
    }
}
