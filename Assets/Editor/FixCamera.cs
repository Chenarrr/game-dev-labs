#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FixCamera
{
    public static void Execute()
    {
        // Fix camera
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null) { Debug.LogError("No Main Camera!"); return; }

        // Add CameraFollow if missing
        if (camGO.GetComponent<CameraFollow>() == null)
            camGO.AddComponent<CameraFollow>();

        // Set sky blue background
        var cam = camGO.GetComponent<Camera>();
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
            cam.clearFlags      = CameraClearFlags.SolidColor;
        }

        EditorUtility.SetDirty(camGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Camera fixed!");
    }
}
#endif
