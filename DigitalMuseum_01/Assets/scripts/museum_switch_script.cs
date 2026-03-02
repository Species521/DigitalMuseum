using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class museum_switch_script : MonoBehaviour
{
    [SerializeField] private string sceneAddress = "museum_scene_01";
    private bool isLoading = false;

    // Tracks the currently-loaded Addressables scene (if any)
    private static AsyncOperationHandle<SceneInstance>? s_currentSceneHandle;

    public void LoadScene()
    {
        if (!isLoading)
            StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        isLoading = true;

        // 1) If we previously loaded an Addressables scene, unload it first.
        if (s_currentSceneHandle.HasValue)
        {
            var unloadHandle = Addressables.UnloadSceneAsync(s_currentSceneHandle.Value, true);
            yield return unloadHandle;

            // Release the unload handle (good hygiene)
            Addressables.Release(unloadHandle);

            // Release the old scene handle reference count
            Addressables.Release(s_currentSceneHandle.Value);
            s_currentSceneHandle = null;
        }

        // 2) Load the new scene
        var loadHandle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Single, true);
        yield return loadHandle;

        if (loadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[museum_switch_script] Scene load failed for address '{sceneAddress}'.");
            isLoading = false;
            yield break;
        }

        // Store as current so we can unload/release it next time we switch
        s_currentSceneHandle = loadHandle;

        // 3) Let Unity unload anything no longer referenced after the switch
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        isLoading = false;
    }
}