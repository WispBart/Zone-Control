using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace HelloWorld
{
    public class TestScene : NetworkBehaviour
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
            root.Q<Button>("BtnClient").clicked += BtnClient;
            root.Q<Button>("BtnServer").clicked += BtnServer;
            root.Q<Button>("BtnHost").clicked += BtnHost;
            root.Q<Button>("BtnMove").clicked += BtnMove;
            _roleLabel = root.Q<Label>("LblRole");
            NetMgr.SceneManager.OnLoadEventCompleted += SceneManagerOnOnLoadEventCompleted;
        }

        private void SceneManagerOnOnLoadEventCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
        {
            if (!IsHost) return;

            int playerNr = 0;
            foreach (var clientID in clientscompleted)
            {
                var config = PlayerConfigs[playerNr];
                var po = Instantiate(PlayerObjectTemplate, config.StartPosition.position, config.StartPosition.rotation);
                po.GetComponent<Player>().SetPlayerColor(config.Color);
                po.SpawnAsPlayerObject(clientID);

                playerNr++;
            }
        }


        private void BtnMove()
        {
            NetMgr.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().MoveStep();
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
