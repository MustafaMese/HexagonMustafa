using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCanvas : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void Open()
    {
        panel.SetActive(true);
    }

    // Button method
    public void TryAgain()
    {
        LoadManager.Instance.Load();
    }
}
