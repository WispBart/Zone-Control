using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public GameObject SelectionGraphic;

    void Start() => SelectionGraphic.SetActive(false);
}
