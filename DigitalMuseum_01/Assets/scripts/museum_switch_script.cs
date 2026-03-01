using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class museum_switch_script : MonoBehaviour
{
    [SerializeField] private string sceneAddress = "museum_scene_01";
    private bool isLoading = false;

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
        {
            Debug.LogError("Scene load failed.");
            isLoading = false;
            yield break;
        }

        yield return Resources.UnloadUnusedAssets();

        isLoading = false;
    }
}