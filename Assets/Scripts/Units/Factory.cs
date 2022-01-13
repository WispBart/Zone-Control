using Game;
using Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PlayerColor))]
public class Factory : NetworkBehaviour
{
    public Unit UnitPrefab;
    public EventObject SpawnTicker;
    public float SpawnUnitSize = 1f;
    public float SpawnCircleSize = 30f;
    public LayerMask Mask;
    private PlayerColor _playerColor;
    private ServerPlayerCommands _commands;

    private Player _player;
    private GameScene _game;
    
    void Awake()
    {
        _playerColor = GetComponent<PlayerColor>();
    }

    void Start()
    {
        _game = GameScene.GetSingleton();
    }

    void OnEnable()
    {
        if (IsServer) SpawnTicker.AddListener(OnSpawnTick);
    }

    void OnDisable()
    {
        if (IsServer) SpawnTicker.RemoveListener(OnSpawnTick);
    }
    
    void OnSpawnTick()
    {
        // Spawn unit for owning player.
        if (FindSpawnPointSpiral(transform.position, SpawnCircleSize, SpawnUnitSize, Mask, out Vector3 spawnPoint))
        {
            // spawn unit
            var po = Instantiate(UnitPrefab, spawnPoint, Quaternion.identity).GetComponent<NetworkObject>();
            var unit = po.GetComponent<Unit>();
            var colorComponent = po.GetComponent<PlayerColor>();
            colorComponent.SetPlayerColor(_playerColor.Color.Value);
            po.SpawnWithOwnership(OwnerClientId);
            unit.SetupOwnershipClientRPC(_player.GetPlayerLayerInt(), _player.GetEnemyLayerMask());
            _game.IncrementUnitCount();
        }
    }

    // Based on answer on: https://stackoverflow.com/questions/3706219/algorithm-for-iterating-over-an-outward-spiral-on-a-discrete-2d-grid-from-the-or
    bool FindSpawnPointSpiral(Vector3 center, float radius, float unitSize, LayerMask mask, out Vector3 spawnPoint)
    {
        // (di, dj) is a vector - direction in which we move right now
        int di = 1;
        int dj = 0;
        // length of current segment
        int segment_length = 1;

        // current position (i, j) and how much of current segment we passed
        int i = 0;
        int j = 0;
        int segment_passed = 0;
        while (true) 
        {
            // make a step, add 'direction' vector (di, dj) to current position (i, j)
            i += di;
            j += dj;
            ++segment_passed; 
            // Do thing
            Vector3 position = new Vector3(center.x + i * unitSize, center.y, center.z + j * unitSize);
            if (Vector3.Distance(position, center) > radius)
            {
                spawnPoint = center;
                return false;
            }
            if (!Physics.CheckSphere(position, unitSize, mask) && NavMesh.SamplePosition(position, out NavMeshHit hit, unitSize, NavMesh.AllAreas))
            {
                spawnPoint = hit.position;
                return true;
            }

            if (segment_passed == segment_length) 
            {
                // done with current segment
                segment_passed = 0;

                // 'rotate' directions
                int buffer = di;
                di = -dj;
                dj = buffer;

                // increase segment length if necessary
                if (dj == 0) {
                    ++segment_length;
                }
            }
        }
    }
    

    public void SetupFactory(Player player)
    {
        _player = player;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, SpawnCircleSize);
    }
}
