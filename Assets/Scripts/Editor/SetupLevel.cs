using UnityEngine;
using UnityEditor;

public class SetupLevel
{
    public static void Execute()
    {
        // 1. Restore camera background to original light blue
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f, 1f);
            EditorUtility.SetDirty(cam);
        }

        // 2. Re-enable CameraFollow
        EnableComponent("Main Camera", "CameraFollow");

        // 3. Re-enable all runtime scripts
        var allMBs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in allMBs)
        {
            string typeName = mb.GetType().Name;
            if (typeName == "ParallaxBackground" ||
                typeName == "CloudSpawner" ||
                typeName == "WorldGenerator" ||
                typeName == "ObstacleSpawner")
            {
                mb.enabled = true;
                EditorUtility.SetDirty(mb.gameObject);
            }
        }

        // 4. Restore background "ground" to original green
        var bgGround = GameObject.Find("ground");
        if (bgGround != null)
        {
            var sr = bgGround.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.266f, 0.925f, 0.349f, 1f);
                sr.sortingOrder = 0;
            }
            EditorUtility.SetDirty(bgGround);
        }

        // 5. Restore "Ground" platform to original
        var mainGround = GameObject.Find("Ground");
        if (mainGround != null)
        {
            mainGround.transform.position = new Vector3(0f, -4f, 0f);
            mainGround.transform.localScale = new Vector3(22f, 1f, 1f);
            var sr = mainGround.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.18f, 0.65f, 0.18f, 1f);
                sr.sortingOrder = 0;
            }
            EditorUtility.SetDirty(mainGround);
        }

        // 6. Restore Player to original
        var player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(-4f, -3.1f, 0f);
            player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red;
                sr.sortingOrder = 0;
            }
            EditorUtility.SetDirty(player);
        }

        // 7. Delete all extra platforms I created
        string[] toDelete = { "Ground_TopCeiling", "Ground_TopRight", "Ground_BottomRight", "Ground_BottomStrip" };
        foreach (var name in toDelete)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Reverted to original state!");
    }

    static void EnableComponent(string objectPath, string componentName)
    {
        var obj = GameObject.Find(objectPath);
        if (obj != null)
        {
            var components = obj.GetComponents<MonoBehaviour>();
            foreach (var c in components)
            {
                if (c.GetType().Name == componentName)
                {
                    c.enabled = true;
                    EditorUtility.SetDirty(obj);
                    break;
                }
            }
        }
    }
}
