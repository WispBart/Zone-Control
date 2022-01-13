using Game;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameUI : MonoBehaviour
{
    private VisualElement _root;
    private GameScene _game;
    private Label _unitCount;
    // Start is called before the first frame update
    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _unitCount = _root.Q<Label>("UnitCount");
        _game = GameScene.GetSingleton();
        _game.UnitCount.OnValueChanged += OnUnitCounterUpdate;

    }

    void OnDestroy()
    {
        _game.UnitCount.OnValueChanged -= OnUnitCounterUpdate;
    }
    
    private void OnUnitCounterUpdate(int previousValue, int newValue)
    {
        _unitCount.text = newValue.ToString();
    }
    
}
