using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Units
{
    public class PlayerColor : NetworkBehaviour
    {
        private static Dictionary<(Material, Color), Material> _playerColorMaterialCache =
            new Dictionary<(Material, Color), Material>();
        private Renderer _renderer;
        private Material _referenceMaterial;
        public NetworkVariable<Color> Color = new NetworkVariable<Color>();

        public void SetPlayerColor(Color color) => Color.Value = color;

        void Awake()
        {
            Color.OnValueChanged += SetPlayerColorInternal;
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer == null) return;
            _referenceMaterial = _renderer.sharedMaterial;
        }

        public override void OnDestroy()
        {
            Color.OnValueChanged -= SetPlayerColorInternal;
            base.OnDestroy();
        }
        
        private void SetPlayerColorInternal(Color oldColor, Color newColor)
        {
            if (_renderer == null) return;
            if (!_playerColorMaterialCache.ContainsKey((_referenceMaterial, newColor)))
            {
                var mat = new Material(_referenceMaterial);
                mat.color = newColor;
                mat.name = $"{_referenceMaterial.name} ({newColor})";
                _playerColorMaterialCache[(_referenceMaterial, newColor)] = mat;
            }
            _renderer.sharedMaterial = _playerColorMaterialCache[(_referenceMaterial, newColor)];
        }
    }
}