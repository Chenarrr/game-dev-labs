#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FixCamera
{
    public static void Execute()
    {
        var cam = GameObject.Find("Main Camera");
        if (cam == null) { Debug.LogError("No Main Camera!"); return; }

        if (cam.GetComponent<CameraFollow>() == null)
            cam.AddComponent<CameraFollow>();

        EditorUtility.SetDirty(cam);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("CameraFollow added to Main Camera!");
    }
}
#endif
