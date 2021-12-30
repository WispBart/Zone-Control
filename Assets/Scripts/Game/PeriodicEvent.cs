using UnityEngine;
using UnityEngine.Events;

public class PeriodicEvent : MonoBehaviour
{

    public float Period = 5;

    private float _inversePeriod;
    private float _elapsed;
    private int _completedTicks;
    private bool _isStarted;

    public EventObject TickEvent;


    public void StartTimer() => _isStarted = true;

    void Start()
    {
        _inversePeriod = 1 / Period;
    }
    
    void Update()
    {
        if (_isStarted)
        {
            if (Mathf.FloorToInt(_elapsed * _inversePeriod) > _completedTicks)
            {
                _completedTicks++;
                TickEvent.Invoke();
            }
            _elapsed += Time.deltaTime;
        }
    }
}
