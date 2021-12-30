using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Simple event")]
public class EventObject : ScriptableObject
{

    public UnityEvent Event;

    public void Invoke() => Event.Invoke();
    public void AddListener(UnityAction action) => Event.AddListener(action);
    public void RemoveListener(UnityAction action) => Event.RemoveListener(action);
}
