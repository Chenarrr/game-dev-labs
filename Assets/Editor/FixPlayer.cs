#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class FixPlayer
{
    public static void Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found!"); return; }

        // Fix SpriteRenderer - assign sprite and reset color to white
        var sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ball_idle.png");
            if (sprite != null) sr.sprite = sprite;
            else Debug.LogWarning("ball_idle.png not found as sprite!");
            sr.color = Color.white;
        }

        // Add Animator if missing
        var anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Sprites/Animations/BallAnimator.controller");
        if (controller != null) anim.runtimeAnimatorController = controller;
        else Debug.LogWarning("BallAnimator.controller not found!");

        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Player fixed!");
    }
}
#endif
