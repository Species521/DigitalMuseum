using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class examroom_switch_script : MonoBehaviour
{
    [SerializeField] private string sceneAddress = "examRoom_nightWatch_01";
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
            yield break;
        }

        yield return null;
        yield return Resources.UnloadUnusedAssets();

        isLoading = false;
    }
}