using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public int row;
    [HideInInspector] public int col;

    // We removed the 'loaded' bool because we are now swapping 
    // between Low and High res constantly.

    public void ApplyTexture(Texture2D tex)
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.material != null)
        {
            // This covers Standard, Legacy, and most Unlit shaders
            mr.material.mainTexture = tex;

            // This covers URP (Universal Render Pipeline) shaders
            if (mr.material.HasProperty("_BaseMap"))
            {
                mr.material.SetTexture("_BaseMap", tex);
            }
        }
    }
}