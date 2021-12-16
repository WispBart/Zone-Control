using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public GameObject SelectionGraphic;
    public Unit Unit;
    public bool IsSelected { get; private set; }
    void Start() => SelectionGraphic.SetActive(false);

    public void Select()
    {
        SelectionGraphic.SetActive(true);
        IsSelected = true;
    }

    public void Deselect()
    {
        SelectionGraphic.SetActive(false);
        IsSelected = false;
    }
}
