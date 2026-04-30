#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GridGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Tile Grid")]
    public static void ShowWindow()
    {
        int rows = 16;
        int cols = 16;
        float quadSize = 1.0f; // Adjust this to match your desired world scale

        // 1. Create or find the Root object
        GameObject root = GameObject.Find("TileGridRoot");
        if (root != null) DestroyImmediate(root);
        root = new GameObject("TileGridRoot");

        // 2. Loop through and create quads
        for (int r = 1; r <= rows; r++)
        {
            for (int c = 1; c <= cols; c++)
            {
                // Create the Quad
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

                // Name it exactly as the TileManager expects: Quad_01_01
                quad.name = $"Quad_{r:D2}_{c:D2}";

                // Set Parent
                quad.transform.SetParent(root.transform);

                // Position it (Layout: Row 1 is Top, Col 1 is Left)
                // We subtract from Y because Row 1 (top) is higher than Row 16 (bottom)
                float posX = (c - 1) * quadSize;
                float posY = -(r - 1) * quadSize;
                quad.transform.localPosition = new Vector3(posX, posY, 0);

                // Add the Tile script if it's not there
                if (quad.GetComponent<Tile>() == null)
                    quad.AddComponent<Tile>();
            }
        }

        // Center the root roughly
        root.transform.position = Vector3.zero;

        Debug.Log($"Successfully generated {rows * cols} tiles!");
    }
}
#endif