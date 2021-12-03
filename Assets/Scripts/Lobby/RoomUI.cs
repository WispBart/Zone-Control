using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class RoomUI : MonoBehaviour
{
    public VisualTreeAsset PlayerEntryTemplate;
    public string GameScene;
    
    private Lobby _currentLobby;
    private bool _amHost;
    private UIDocument _ui;
    private Label _roomName;
    private ListView _playerContainer;
    private List<Player> _playersInRoom = new List<Player>();
    private Player _localPlayer;
    
    public const string ReadyKey = "IsReady";
    public const string JoinCodeKey = "JoinCode";

    public SimpleLobby MainUI { get; set; }
    private RelayConnectionManager _gameConnection = RelayConnectionManager.Get();
    private NetworkSceneManager _sceneMgr => NetworkManager.Singleton.SceneManager;


    IEnumerator HeartBeat()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5f);
            if (_currentLobby != null)
            {
                Debug.Log("Heartbeat");
                try
                {
                    Lobbies.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    _currentLobby = null;
                    MainUI.ReturnFromLobby();
                }
                UpdateLobbyData();
            }
        }
    }

    async void UpdateLobbyData()
    {
        _currentLobby = await Lobbies.Instance.GetLobbyAsync(_currentLobby.Id);
        if (_currentLobby == null)
        {
            Debug.LogWarning("Lobby no longer exists, returning to main menu");
            MainUI.ReturnFromLobby();
        }
        else
        {
            Debug.Log($"Updated lobby {_currentLobby.Name} ({_currentLobby.Id})");
            UpdatePlayerList();
        }
    }

    void Awake()
    {
        _ui = GetComponent<UIDocument>();
        var root = _ui.rootVisualElement;
        root.Q<Button>("BtnLeave").clicked += BtnLeave;
        root.Q<Button>("BtnStart").clicked += BtnStart;
        _roomName = root.Q<Label>("RoomName");
        _playerContainer = root.Q<ListView>();
        _playerContainer.selectionType = SelectionType.None;
        _playerContainer.makeItem = () => PlayerEntryTemplate.CloneTree().contentContainer;
        _playerContainer.bindItem = (ve, idx) =>
        {
            ve.Q<Label>("PlayerName").text = _playersInRoom[idx].Data[SimpleLobby.PlayerNamePrefKey].Value;
            ve.Q<Label>("Ready").text = _playersInRoom[idx].Data[ReadyKey].Value;
        };
        _playerContainer.itemsSource = _playersInRoom;
    }

    void Start()
    {
        StartCoroutine(HeartBeat());
    }

    private void BtnStart()
    {
        _sceneMgr.LoadScene(GameScene, LoadSceneMode.Single);
    }
    
    private async void BtnLeave() 
    {
        try
        {
            bool isHost = _currentLobby.HostId == _localPlayer.Id;
            await Lobbies.Instance.RemovePlayerAsync(_currentLobby.Id, _localPlayer.Id);
            if (isHost) await Lobbies.Instance.DeleteLobbyAsync(_currentLobby.Id);
            _currentLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        finally
        {
            MainUI.ReturnFromLobby();
        }
    }
    
    public async Task CreateLobby(string name, Player host, int maxPlayers, bool isPrivate)
    {
        _localPlayer = host;
        _localPlayer.Data[ReadyKey] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "No");
        try
        {
            var joinCode = await _gameConnection.StartServer(maxPlayers);
            var data = new Dictionary<string, DataObject>();
            data[JoinCodeKey] = new DataObject(DataObject.VisibilityOptions.Member, joinCode);
            _currentLobby = await Lobbies.Instance.CreateLobbyAsync(
                lobbyName: name,
                maxPlayers: maxPlayers,
                options: new CreateLobbyOptions()
                {
                    Data = data,
                    IsPrivate = isPrivate,
                    Player = host
                });
            Debug.Log($"Created new lobby {_currentLobby.Name} ({_currentLobby.Id})");
            SetupUI();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async Task JoinLobby(Lobby lobby, Player player)
    {
        _localPlayer = player;
        _localPlayer.Data[ReadyKey] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "No");
        try
        {
            _currentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions()
            {
                Player = player
            });
            var joinCode = _currentLobby.Data[JoinCodeKey].Value;
            await _gameConnection.JoinServer(joinCode);
            SetupUI();
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void SetUIVisible(bool value) => _ui.rootVisualElement.style.visibility =
        value ? Visibility.Visible : Visibility.Hidden;

    void SetupUI()
    {
        SetUIVisible(true);
        _roomName.text = _currentLobby.Name;
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        _playersInRoom = _currentLobby.Players;
        Debug.Log($"Players in room: {_playersInRoom.Count}");
        _playerContainer.itemsSource = _playersInRoom;
        _playerContainer.Rebuild();
    }
    
    private async Task UpdatePlayerInfo()
    {
        try 
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            //Ensure you sign-in before calling Authentication Instance
            //See IAuthenticationService interface
            string playerId = AuthenticationService.Instance.PlayerId;

            Lobby lobby = await Lobbies.Instance.UpdatePlayerAsync("lobbyId", playerId, options);
            //...
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
}
