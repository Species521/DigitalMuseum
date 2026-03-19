using UnityEngine;

public class Optimizations_script : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        DontDestroyOnLoad(gameObject);
    }
}