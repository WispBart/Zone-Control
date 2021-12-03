using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class Player : NetworkBehaviour
    {
        public NetworkManager NetMgr => NetworkManager.Singleton;
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>();
        private Renderer _renderer;

        public void SetPlayerColor(Color color) => PlayerColor.Value = color;
        
        void SetPlayerColorInternal(Color oldColor, Color newColor)
        {
            _renderer.material.color = newColor;
        }

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            Position.OnValueChanged += UpdatePosition;
            PlayerColor.OnValueChanged += SetPlayerColorInternal;
        }

        void OnDestroy()
        {
            Position.OnValueChanged -= UpdatePosition;
        }

        private void UpdatePosition(Vector3 previousvalue, Vector3 newvalue) => transform.position = newvalue;

        public override void OnNetworkSpawn()
        {
            if (IsOwner) MoveStep();
        }

        public void MoveStep()
        {
            if (NetMgr.IsServer)
            {
                Position.Value = transform.position + transform.forward;
            }
            else
            {
                SubmitMoveStepServerRPC();
            }
        }


        [ServerRpc]
        private void SubmitMoveStepServerRPC(ServerRpcParams rpcParams = default)
        {
            Position.Value = transform.position + transform.forward;
        }
    }
}
