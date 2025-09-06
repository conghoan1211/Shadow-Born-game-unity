using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] public GameObject GameManagerGO;

    private void Start()
    {
        GameManagerGO.GetComponent<_GameController>().IsShowDialogNPCPanel(false);

    }

    private void OnMouseDown()
    {
        GameManagerGO.GetComponent<_GameController>().IsShowDialogNPCPanel(true);
    }
}

