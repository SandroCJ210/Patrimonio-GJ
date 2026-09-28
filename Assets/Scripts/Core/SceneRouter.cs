using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRouter : MonoBehaviour
{
    public void Go(string sceneName, Action onLoaded = null)
    {
        SceneManager.LoadScene(sceneName);
        onLoaded?.Invoke();
    }
}