using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameSystem : MonoBehaviour
{
    public GameObject gameUI;
    

    private void OnCollisionEnter(Collision collision)
    {
        gameUI.SetActive(true);
    }
}
