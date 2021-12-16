using System.Collections.Generic;
using Game;
using Unity.Netcode;
using UnityEngine;

public class Selection : MonoBehaviour
{
    public LayerMask FloorLayer;
    public SelectionBox SelectionBox;
    public HashSet<Selectable> Selected = new HashSet<Selectable>();
    private Camera _camera;

    void Awake()
    {
        _camera = Camera.main;
    }
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            var clickPoint = _camera.ScreenToViewportPoint(Input.mousePosition);
            var ray = _camera.ViewportPointToRay(clickPoint);
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, FloorLayer))
            {
                AttackMove(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!Input.GetKey(KeyCode.LeftShift)) ClearSelection();
            var clickPoint = _camera.ScreenToViewportPoint(Input.mousePosition);
            var ray = _camera.ViewportPointToRay(clickPoint);
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, FloorLayer))
            {
                SelectionBox.StartSelect(hit.point);
            }
        }
        if (Input.GetMouseButton(0))
        {
            var clickPoint = _camera.ScreenToViewportPoint(Input.mousePosition);
            var ray = _camera.ViewportPointToRay(clickPoint);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, FloorLayer))
            {
                SelectionBox.UpdateSelect(hit.point);
            }        
        }
        if (Input.GetMouseButtonUp(0))
        {
            SelectionBox.GetSelectablesInBox(Selected);
            SelectionBox.CompleteSelect();
            UpdateSelected();
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ClearSelection();
        }
    }

    void AttackMove(Vector3 targetPosition)
    {
        var netMgr = NetworkManager.Singleton;
        if (netMgr != null)
        {
            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
            player.AttackMove(Selected, targetPosition);
        }
        else
        {
            // Playing offline
            foreach (var selectable in Selected)
            {
                if (selectable.Unit.CanAttack) selectable.Unit.AttackMove(targetPosition);
            }
        }
    }

    void UpdateSelected()
    {
        foreach (var selectable in Selected)
        {
            selectable.Select();
        }
    }
    void ClearSelection()
    {
        foreach (var selectable in Selected)
        {
            selectable.Deselect();
        }
        Selected.Clear();
        SelectionBox.CompleteSelect();
    }

}
