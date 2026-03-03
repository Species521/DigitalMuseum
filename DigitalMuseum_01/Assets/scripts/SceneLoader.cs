using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneAddress;

    [Header("Optional Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    // Assign a UI Image panel or full canvas root here

    private bool isLoading;

    public void LoadScene()
    {
        if (!isLoading)
            StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        isLoading = true;

        // Activate loading screen if assigned
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Small frame delay so UI actually renders before heavy load starts
        yield return null;

        var handle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Single);

        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
            Debug.LogError("Scene load failed: " + sceneAddress);

        yield return Resources.UnloadUnusedAssets();

        // Deactivate loading screen (in case object persists)
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        isLoading = false;
    }
}