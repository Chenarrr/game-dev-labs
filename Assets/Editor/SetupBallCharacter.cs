
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class SetupBallCharacter
{
    public static void Execute()
    {
        // ── 1. Import all sprites ─────────────────────────────────────────
        string[] spritePaths = {
            "Assets/Sprites/ball_idle.png",
            "Assets/Sprites/ball_blink1.png",
            "Assets/Sprites/ball_blink2.png",
            "Assets/Sprites/ball_blink3.png",
            "Assets/Sprites/ball_run1.png",
            "Assets/Sprites/ball_run2.png",
            "Assets/Sprites/ball_run3.png",
            "Assets/Sprites/ball_run4.png",
            "Assets/Sprites/ball_run5.png",
            "Assets/Sprites/ball_run6.png",
            "Assets/Sprites/ball_jump_anticipation.png",
            "Assets/Sprites/ball_jump_up.png",
            "Assets/Sprites/ball_jump_peak.png",
            "Assets/Sprites/ball_jump_fall.png",
            "Assets/Sprites/ball_jump_land.png",
        };

        foreach (var path in spritePaths)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { Debug.LogWarning("Missing: " + path); continue; }
            imp.textureType       = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.filterMode        = FilterMode.Bilinear;
            imp.mipmapEnabled     = false;
            imp.SaveAndReimport();
        }
        AssetDatabase.Refresh();

        // ── 2. Helper: load sprite ────────────────────────────────────────
        Sprite Spr(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        // ── 3. Create animation clips ─────────────────────────────────────
        string animDir = "Assets/Sprites/Animations";
        if (!Directory.Exists(animDir)) Directory.CreateDirectory(animDir);

        AnimationClip MakeClip(string name, string[] frames, float fps, bool loop = true)
        {
            var clip = new AnimationClip { frameRate = fps };
            var binding = new UnityEditor.Animations.AnimatorController(); // just for type ref
            var spriteBinding = new EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite"
            };
            var keyframes = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time  = i / fps,
                    value = Spr(frames[i])
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, $"{animDir}/{name}.anim");
            return clip;
        }

        var idleClip = MakeClip("BallIdle", new[]{ "Assets/Sprites/ball_idle.png" }, 30);
        var blinkClip = MakeClip("BallBlink", new[]{
            "Assets/Sprites/ball_idle.png",
            "Assets/Sprites/ball_idle.png",
            "Assets/Sprites/ball_idle.png",
            "Assets/Sprites/ball_blink1.png",
            "Assets/Sprites/ball_blink2.png",
            "Assets/Sprites/ball_blink1.png",
            "Assets/Sprites/ball_idle.png",
        }, 12, true);
        var runClip = MakeClip("BallRun", new[]{
            "Assets/Sprites/ball_run1.png",
            "Assets/Sprites/ball_run2.png",
            "Assets/Sprites/ball_run3.png",
            "Assets/Sprites/ball_run4.png",
            "Assets/Sprites/ball_run5.png",
            "Assets/Sprites/ball_run6.png",
        }, 12);
        var jumpClip = MakeClip("BallJump", new[]{
            "Assets/Sprites/ball_jump_anticipation.png",
            "Assets/Sprites/ball_jump_up.png",
            "Assets/Sprites/ball_jump_peak.png",
        }, 12, false);
        var fallClip = MakeClip("BallFall", new[]{
            "Assets/Sprites/ball_jump_fall.png",
        }, 12);
        var landClip = MakeClip("BallLand", new[]{
            "Assets/Sprites/ball_jump_land.png",
            "Assets/Sprites/ball_idle.png",
        }, 12, false);

        // ── 4. Create Animator Controller ─────────────────────────────────
        string ctrlPath = "Assets/Sprites/Animations/BallAnimator.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

        controller.AddParameter("Speed",    AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VelocityY",  AnimatorControllerParameterType.Float);

        var root = controller.layers[0].stateMachine;

        var sIdle  = root.AddState("Idle");  sIdle.motion  = idleClip;
        var sBlink = root.AddState("Blink"); sBlink.motion = blinkClip;
        var sRun   = root.AddState("Run");   sRun.motion   = runClip;
        var sJump  = root.AddState("Jump");  sJump.motion  = jumpClip;
        var sFall  = root.AddState("Fall");  sFall.motion  = fallClip;
        var sLand  = root.AddState("Land");  sLand.motion  = landClip;

        root.defaultState = sIdle;

        AnimatorStateTransition T(AnimatorState from, AnimatorState to, float duration = 0.05f)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = false;
            t.duration = duration;
            return t;
        }

        // Idle → Run
        var t1 = T(sIdle, sRun);
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        // Idle → Blink (auto from idle)
        var t2 = T(sIdle, sBlink, 0f);
        t2.hasExitTime = true; t2.exitTime = 1f;
        // Blink → Idle
        var t3 = T(sBlink, sIdle, 0f);
        t3.hasExitTime = true; t3.exitTime = 1f;

        // Run → Idle
        var t4 = T(sRun, sIdle);
        t4.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Any → Jump
        var tJump = root.AddAnyStateTransition(sJump);
        tJump.hasExitTime = false; tJump.duration = 0.05f;
        tJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VelocityY");
        tJump.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded"); // NOT grounded
        // but we want !IsGrounded — use IfNot
        tJump.conditions = new AnimatorCondition[]{
            new AnimatorCondition{ mode = AnimatorConditionMode.Greater, parameter = "VelocityY", threshold = 0.5f },
            new AnimatorCondition{ mode = AnimatorConditionMode.IfNot,   parameter = "IsGrounded", threshold = 0 }
        };

        // Jump → Fall
        var tFall = T(sJump, sFall, 0f);
        tFall.hasExitTime = true; tFall.exitTime = 1f;

        // Any → Fall (falling without jump)
        var tFall2 = root.AddAnyStateTransition(sFall);
        tFall2.hasExitTime = false; tFall2.duration = 0.05f;
        tFall2.conditions = new AnimatorCondition[]{
            new AnimatorCondition{ mode = AnimatorConditionMode.Less,  parameter = "VelocityY", threshold = -0.5f },
            new AnimatorCondition{ mode = AnimatorConditionMode.IfNot, parameter = "IsGrounded", threshold = 0 }
        };

        // Fall → Land
        var tLand = T(sFall, sLand);
        tLand.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        // Land → Idle
        var tLandIdle = T(sLand, sIdle, 0f);
        tLandIdle.hasExitTime = true; tLandIdle.exitTime = 1f;

        AssetDatabase.SaveAssets();

        // ── 5. Assign Animator to Player ──────────────────────────────────
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found!"); return; }

        var animator = player.GetComponent<Animator>();
        if (animator == null) animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        // Set idle sprite immediately
        var sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = Spr("Assets/Sprites/ball_idle.png");

        // Fix collider to circle
        var box = player.GetComponent<BoxCollider2D>();
        if (box != null) Object.DestroyImmediate(box);
        var circle = player.GetComponent<CircleCollider2D>();
        if (circle == null) player.AddComponent<CircleCollider2D>();

        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Ball character setup complete!");
    }
}
#endif
