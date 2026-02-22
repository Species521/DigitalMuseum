using UnityEngine;
using UnityEngine.AddressableAssets;

public class Museumloader : MonoBehaviour
{
    public void Start()
    {
        // This tells Unity to go find the Addressable scene 
        // without "hard-linking" it to this loader scene.
        Addressables.LoadSceneAsync("Assets/Scenes/museum_scene_02_noLights.unity");
    }
}