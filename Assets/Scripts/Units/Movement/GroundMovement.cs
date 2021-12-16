using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class GroundMovement : Movement
{
    private NavMeshAgent _agent;
    
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void MoveTo(Vector3 position)
    {
        if (!_agent.SetDestination(position))
        {
            Debug.LogWarning("Agent could not find path", this);
        }
    }
    
}
