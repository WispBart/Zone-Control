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
        private Label _roleLabel;
        //private Player _localPlayer;
        void Awake()
        {
            var root = GUI.rootVisualElement;
            // root.Q<Button>("BtnClient").clicked += BtnClient;
            // root.Q<Button>("BtnServer").clicked += BtnServer;
            // root.Q<Button>("BtnHost").clicked += BtnHost;
            root.Q<Button>("BtnDisconnect").clicked += BtnDisconnect;
            _roleLabel = root.Q<Label>("LblRole");
            NetMgr.SceneManager.OnLoadEventCompleted += SceneManagerOnOnLoadEventCompleted;
        }
        

        private void BtnDisconnect()
        {
            Debug.Log("Disconnecting");
            NetMgr.DisconnectClient(NetMgr.LocalClientId);
            Destroy(NetMgr.gameObject);
            SceneManager.LoadScene(0);
        }

        private void SceneManagerOnOnLoadEventCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
        {
            if (!IsHost) return;

            int playerNr = 0;
            foreach (var clientID in clientscompleted)
            {
                var config = PlayerConfigs[playerNr];
                var spawnPos = config.StartPosition.position;
                var po = Instantiate(PlayerObjectTemplate, spawnPos, config.StartPosition.rotation);
                var player = po.GetComponent<Player>();
                player.Position.Value = spawnPos;
                player.SetPlayerColor(config.Color);
                po.SpawnAsPlayerObject(clientID);
                playerNr++;
            }
        }

        private void BtnHost() => NetMgr.StartHost();
        private void BtnServer() => NetMgr.StartServer();
        private void BtnClient() => NetMgr.StartClient();

        // Update is called once per frame
        void Update()
        {
            var role = NetMgr.IsHost ? "Host" : NetMgr.IsServer ? "Server" : "Client";
            _roleLabel.text = role;
        }
        
    }
}