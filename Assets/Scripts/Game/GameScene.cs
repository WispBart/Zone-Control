using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Game
{
    public class GameScene : NetworkBehaviour
    {
        [Serializable] public class PlayerData
        {
            public Color Color;
            public Transform StartPosition;
        }
        
        public UIDocument GUI;
        public NetworkManager NetMgr => NetworkManager.Singleton;
        [FormerlySerializedAs("PlayerObject")] public NetworkObject PlayerObjectTemplate;
        public PlayerData[] PlayerConfigs;
        public Selection Selection;

        private Player _localPlayer;
        private Dictionary<int, Player> _guidToPlayerObject = new Dictionary<int, Player>();
        private Label _roleLabel;
        //private Player _localPlayer;
        private bool _hasNetmgr;
        
        void Awake()
        {
            var root = GUI.rootVisualElement;
            // root.Q<Button>("BtnClient").clicked += BtnClient;
            // root.Q<Button>("BtnServer").clicked += BtnServer;
            // root.Q<Button>("BtnHost").clicked += BtnHost;
            root.Q<Button>("BtnDisconnect").clicked += BtnDisconnect;
            _roleLabel = root.Q<Label>("LblRole");
            _hasNetmgr = NetMgr != null;
            if (_hasNetmgr)
            {
                NetMgr.SceneManager.OnLoadEventCompleted += SceneManagerOnOnLoadEventCompleted;
            }
        }
        

        private void BtnDisconnect()
        {
            Debug.Log("Disconnecting");
            NetMgr.DisconnectClient(NetMgr.LocalClientId);
            Destroy(NetMgr.gameObject);
            SceneManager.LoadScene(0);
        }

        private void Start()
        {
            // Debug startup without netmgr.
            if (!_hasNetmgr)
            {
                var config = PlayerConfigs[0];
                _localPlayer = Instantiate(PlayerObjectTemplate, config.StartPosition.position, Quaternion.identity).GetComponent<Player>();
                _localPlayer.SetupPlayer(1, config.Color, config.StartPosition.position);
            }
        }
        
        private void SceneManagerOnOnLoadEventCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
        {
            if (IsServer)
            {
                ServerInitialization(clientscompleted);
                LocalInitializationClientRPC();
            }
        }

        private void ServerInitialization(List<ulong> clientscompleted)
        {
            int playerNr = 1;
            foreach (var clientID in clientscompleted)
            {
                var config = PlayerConfigs[playerNr - 1];
                var spawnPos = config.StartPosition.position;
                var po = Instantiate(PlayerObjectTemplate, spawnPos, config.StartPosition.rotation);
                var player = po.GetComponent<Player>();
                player.SetupPlayer(playerNr, config.Color, config.StartPosition.position);
                po.SpawnAsPlayerObject(clientID);
                playerNr++;
            }
        }
        
        [ClientRpc]
        private void LocalInitializationClientRPC(ClientRpcParams rpcParams = default)
        {
            _localPlayer = NetMgr.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
            Selection.SelectionBox.SetUnitLayer(LayerMask.GetMask(_localPlayer.GetPlayerLayerName()));
        }

        private void BtnHost() => NetMgr.StartHost();
        private void BtnServer() => NetMgr.StartServer();
        private void BtnClient() => NetMgr.StartClient();

        // Update is called once per frame
        void Update()
        {
            if (!_hasNetmgr) return;
            var role = NetMgr.IsHost ? "Host" : NetMgr.IsServer ? "Server" : "Client";
            _roleLabel.text = role;
        }
        
    }
}
