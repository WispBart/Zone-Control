using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour
{
    public LayerMask UnitLayer;

    private Camera _camera;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    public RectTransform ScreenspaceBox;
    public Image BoxDrawer;
    
    void Awake()
    {
        _camera = Camera.main;
        BoxDrawer.enabled = false;
    }

    public void SetUnitLayer(LayerMask unitLayer) => UnitLayer = unitLayer;
    
    public void StartSelect(Vector3 position)
    {
        _startPosition = position;
    }

    public void UpdateSelect(Vector3 wsMousePosition)
    {
        _endPosition = wsMousePosition;
        BoxDrawer.enabled = true;
        var start = _camera.WorldToViewportPoint(_startPosition);
        var end = _camera.WorldToViewportPoint(wsMousePosition);
        var min = new Vector2(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y));
        var max = new Vector2(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
        ScreenspaceBox.anchorMin = min;
        ScreenspaceBox.anchorMax = max;
        ScreenspaceBox.sizeDelta = Vector2.zero;
        ScreenspaceBox.anchoredPosition = Vector2.zero;
    }

    public void GetSelectablesInBox(HashSet<Selectable> selectables)
    {
        var center = (_startPosition + _endPosition) / 2;
        var halfExtents = Abs(_endPosition - center);
        Debug.DrawLine(center, center + new Vector3(halfExtents.x, 0, 0), Color.green, 2f);
        Debug.DrawLine(center, center - new Vector3(halfExtents.x, 0, 0), Color.red, 2f);
        Debug.DrawLine(center, center + new Vector3(0, 0, halfExtents.z), Color.magenta, 2f);
        Debug.DrawLine(center, center - new Vector3(0,0, halfExtents.z), Color.yellow, 2f);
        var colliders = Physics.OverlapBox(center, new Vector3(halfExtents.x, 3f, halfExtents.z), Quaternion.identity, UnitLayer);
        foreach (var overlappingCollider in colliders)
        {
            var selectable = overlappingCollider.GetComponent<Selectable>();
            if (selectable != null) selectables.Add(selectable);
        }
    }

    Vector3 Abs(Vector3 input) => new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));

    public void CompleteSelect()
    {
        BoxDrawer.enabled = false;
    }


}
