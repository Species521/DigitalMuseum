using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class Museumloader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneAddress = "museum_scene_01";

    [Header("Optional Loading Screen")]
    [SerializeField] private GameObject loadingScreen;

    void Start()
    {
        StartCoroutine(LoadMuseum());
    }

    private IEnumerator LoadMuseum()
    {
        // Activate loading screen if assigned
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Wait one frame so UI renders before heavy loading begins
        yield return null;

        AsyncOperationHandle<SceneInstance> handle =
            Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Single);

        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
            Debug.LogError("Failed to load scene: " + sceneAddress);
    }
}