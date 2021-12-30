using Game;
using Unity.Netcode;
using UnityEngine;

public class DebugScene : MonoBehaviour
{
    public GameScene GameScene;
 
    void Start()
    {
        NetworkManager.Singleton.StartHost();
        GameScene.Debug_Init();
    }


}
