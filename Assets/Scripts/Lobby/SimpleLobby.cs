using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SimpleLobby : MonoBehaviour
{

    public const string PlayerNamePrefKey = "PlayerName";
    public const string RoomNamePrefKey = "LastUsedRoomName";

    public int MaxPlayers = 8;
    public bool Private = false;

    public VisualTreeAsset RoomEntryTemplate;
    public RoomUI RoomUI;
    
    private UIDocument _ui;
    private VisualElement _root;
    private GroupBox _controls;
    private TextField _roomName;
    private TextField _playerName;
    private ListView _lobbyContainer;

    private List<Lobby> _availableLobbies = new List<Lobby>();
    private Player _localPlayer;
    private PlayerDataObject _localPlayerName;
    
    async public void BtnCreateRoom() => await CreateLobby(_roomName.value); 
    async public void BtnRefresh() => await GetLobbies();


    void Awake()
    {
        _ui = GetComponent<UIDocument>();
        _root = _ui.rootVisualElement;
        _root.Q<Button>("BtnCreateRoom").clicked += BtnCreateRoom;
        _root.Q<Button>("BtnRefresh").clicked += BtnRefresh;
        _controls = _root.Q<GroupBox>("ConnectedControls");
        _roomName = _root.Q<TextField>("RoomName");
        _roomName.value = PlayerPrefs.GetString(RoomNamePrefKey, "");
        _roomName.RegisterValueChangedCallback((evt) => PlayerPrefs.SetString(RoomNamePrefKey, evt.newValue));
        _playerName = _root.Q<TextField>("PlayerName");
        _playerName.RegisterValueChangedCallback(PlayerNameChange);
        _playerName.value = PlayerPrefs.GetString(PlayerNamePrefKey, "player");
        _lobbyContainer = _root.Q<ListView>("LobbyList");
        _lobbyContainer.selectionType = SelectionType.Single;
        _lobbyContainer.makeItem = () => RoomEntryTemplate.CloneTree().contentContainer;
        _lobbyContainer.bindItem = BindItem;
        _lobbyContainer.onItemsChosen += LobbyContainerItemChosen;
        _lobbyContainer.itemsSource = _availableLobbies;
        _controls.SetEnabled(false);
        RoomUI.MainUI = this;

    }

    public void ReturnFromLobby()
    {
        RoomUI.SetUIVisible(false);
        SetUIVisible(true);
        BtnRefresh();
    }
    
    public void SetUIVisible(bool value) => _ui.rootVisualElement.style.visibility =
        value ? Visibility.Visible : Visibility.Hidden;
    
    private void LobbyContainerItemChosen(IEnumerable<object> items)
    {
        var lobby = items.First() as Lobby;
        if (lobby == null)
        {
            Debug.LogError("Chosen lobby does not exist");
            return;
        }
        JoinLobby(lobby);
    }

    private async Task JoinLobby(Lobby lobby)
    {
        await RoomUI.JoinLobby(lobby, _localPlayer);
        SetUIVisible(false);
    }

    private void PlayerNameChange(ChangeEvent<string> evt)
    {
        PlayerPrefs.SetString(PlayerNamePrefKey, evt.newValue);
        _localPlayerName = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, evt.newValue);
        if (_localPlayer != null)
        {
            _localPlayer.Data[PlayerNamePrefKey] = _localPlayerName;
        }
    }

    private void BindItem(VisualElement ve, int idx)
    {
        var lobby = _availableLobbies[idx];
        ve.Q<Label>("RoomName").text = lobby.Name;
        ve.Q<Label>("UserCount").text = lobby.Players.Count.ToString();
        ve.Q<Label>("MaxUserCount").text = lobby.MaxPlayers.ToString();
    }

    async void Start()
    {
        //HeartbeatLobby();
        try
        {
            await SetupLobby();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }


    private async Task SetupLobby()
    {
        RoomUI.SetUIVisible(false);
        await UnityServices.InitializeAsync();
        _localPlayer = await GetPlayerFromAnonymousLoginAsync(_localPlayerName);
        _controls.SetEnabled(true);
        await GetLobbies();
    }

    private async Task GetLobbies()
    {
        var response = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions()
        {
            Count = 20, // Override default number of results to return
        });
        _availableLobbies = response.Results;
        Debug.Log($"Lobbies found: {_availableLobbies.Count}");
        _lobbyContainer.itemsSource = _availableLobbies;
        _lobbyContainer.Rebuild();
    }

    private async Task CreateLobby(string name)
    {
        await RoomUI.CreateLobby(name, _localPlayer, MaxPlayers, Private);
        SetUIVisible(false);
    }
    
    
    static async Task<Player> GetPlayerFromAnonymousLoginAsync(PlayerDataObject playerName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log($"Trying to log in a player ...");

            // Use Unity Authentication to log in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                throw new InvalidOperationException("Player was not signed in successfully; unable to continue without a logged in player");
            }
        }

        Debug.Log("Player signed in as " + AuthenticationService.Instance.PlayerId);

        // Player objects have Get-only properties, so you need to initialize the data bag here if you want to use it
        var data = new Dictionary<string, PlayerDataObject>();
        data[PlayerNamePrefKey] = playerName;
        return new Player(AuthenticationService.Instance.PlayerId, null, data);
    }
}
