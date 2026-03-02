using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneAddress;
    private bool isLoading;

    public void LoadScene()
    {
        if (!isLoading)
            StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        isLoading = true;

        var handle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Single);
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
            Debug.LogError("Scene load failed: " + sceneAddress);

        yield return Resources.UnloadUnusedAssets();

        isLoading = false;
    }
}