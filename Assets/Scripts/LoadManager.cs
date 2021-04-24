using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    private static LoadManager instance = null;

    public static LoadManager Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    private void Awake()
    {
        instance = this;
    }

    public void Load()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
