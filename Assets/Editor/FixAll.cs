#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FixAll
{
    public static void Execute()
    {
        // Force reimport idle sprite as Sprite type
        string idlePath = "Assets/Sprites/ball_idle.png";
        var imp = AssetImporter.GetAtPath(idlePath) as TextureImporter;
        if (imp != null)
        {
            imp.textureType         = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.mipmapEnabled       = false;
            imp.filterMode          = FilterMode.Bilinear;
            imp.SaveAndReimport();
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        // Assign sprite + reset color
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found!"); return; }

        var sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(idlePath);
            if (sprite != null) { sr.sprite = sprite; Debug.Log("Sprite assigned: " + sprite.name); }
            else Debug.LogError("Still null — check Sprites folder has ball_idle.png");
            sr.color = Color.white;
        }

        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Done!");
    }
}
#endif
