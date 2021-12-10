using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class PlayerColor : NetworkBehaviour
    {
        private Renderer _renderer;
        public NetworkVariable<Color> Color = new NetworkVariable<Color>();

        public void SetPlayerColor(Color color) => Color.Value = color;

        void Awake()
        {
            Color.OnValueChanged += SetPlayerColorInternal;
            _renderer = GetComponentInChildren<Renderer>();
        }

        void OnDestroy()
        {
            Color.OnValueChanged -= SetPlayerColorInternal;
        }
        
        private void SetPlayerColorInternal(Color oldColor, Color newColor)
        {
            if (_renderer != null) _renderer.material.color = newColor;
        }
    }
}