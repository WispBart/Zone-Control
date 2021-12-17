using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class Player : NetworkBehaviour
    {
        public NetworkManager NetMgr => NetworkManager.Singleton;
        
        public NetworkVariable<Color> Color = new NetworkVariable<Color>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<int> PlayerNumber;

        public ServerPlayerCommands serverPlayerCommands;

        private const int FirstPlayerLayer = 6;

        public void SetupPlayer(int playerNumber, Color color, Vector3 position)
        {
            PlayerNumber.Value = playerNumber;
            Color.Value = color;
            Position.Value = position;
        }

        public List<string> AllPlayers => new List<string>
        {
            "Player1", "Player2", "Player3", "Player4", "Player5", "Player6", "Player7", "Player8"
        };

        public string GetPlayerLayerName()
        {
            return LayerMask.LayerToName(PlayerNumber.Value + FirstPlayerLayer); // oof I don't like this
        }

        public int GetPlayerLayerInt() => PlayerNumber.Value + FirstPlayerLayer;

        public LayerMask GetEnemyLayerMask()
        {
            var enemies = AllPlayers;
            enemies.Remove(GetPlayerLayerName());
            return LayerMask.GetMask(enemies.ToArray());
        }
        
        private bool _hasNetMgr;
        
        public override void OnNetworkSpawn()
        {
            serverPlayerCommands = GetComponent<ServerPlayerCommands>();
            if (IsOwner)
            {
                SpawnFactory();
            }
        }
        void Start()
        {
            if (!_hasNetMgr) SpawnFactory();
        }
        public void SpawnFactory()
        {
            serverPlayerCommands.SubmitSpawnFactoryServerRPC();
        }

        public void AttackMove(HashSet<Selectable> selectables, Vector3 target)
        {
            NetworkBehaviourReference[] unitIDs = selectables.Select(s => new NetworkBehaviourReference(s.Unit)).ToArray();
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
        public NetworkBehaviourReference[] Units;
        public Vector3 TargetPosition;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        { 
            // Length
            int length = 0;
            if (!serializer.IsReader)
            {
                length = Units.Length;
            }
            serializer.SerializeValue(ref length);
            // Array
            if (serializer.IsReader)
            {
                Units = new NetworkBehaviourReference[length];
            }
            for (int n = 0; n < length; ++n)
            {
                serializer.SerializeValue(ref Units[n]);
            }
            serializer.SerializeValue(ref TargetPosition);
        }
    }
}
