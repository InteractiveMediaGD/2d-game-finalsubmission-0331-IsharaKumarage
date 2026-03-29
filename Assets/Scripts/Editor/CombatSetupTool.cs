using UnityEngine;
using UnityEditor;
using System.IO;

public class CombatEffectsSetupTool : Editor
{
    [MenuItem("Tools/Setup Real War Combat")]
    public static void SetupCombat()
    {
        // 1. Create AudioManager.cs
        CreateAudioManager();

        // 2. Modify Scripts
        UpdatePlayerCombat();
        UpdateSwordHitDetector();
        UpdateEnemyAI();

        Debug.Log("Real War Combat Setup Complete! Creating AudioManager and Compiling...");
        AssetDatabase.Refresh();
    }

    private static void CreateAudioManager()
    {
        string dir = "Assets/Scripts/Managers";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string amPath = dir + "/AudioManager.cs";
        string amText = @"using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header(""Audio Source"")]
    public AudioSource sfxSource;

    [Header(""Sword Combat Sounds"")]
    public AudioClip swordSwingClip;
    public AudioClip swordHitClip;
    public AudioClip enemyDeathClip;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (sfxSource == null) { sfxSource = gameObject.AddComponent<AudioSource>(); sfxSource.playOnAwake = false; }
    }

    public void PlaySwordSwing()
    {
        if (swordSwingClip != null && sfxSource != null) { sfxSource.pitch = Random.Range(0.9f, 1.1f); sfxSource.PlayOneShot(swordSwingClip); }
    }

    public void PlaySwordHit()
    {
        if (swordHitClip != null && sfxSource != null) { sfxSource.pitch = Random.Range(0.85f, 1.15f); sfxSource.PlayOneShot(swordHitClip); }
    }

    public void PlayEnemyDeath()
    {
        if (enemyDeathClip != null && sfxSource != null) { sfxSource.pitch = Random.Range(0.9f, 1.1f); sfxSource.PlayOneShot(enemyDeathClip); }
    }
}";
        File.WriteAllText(amPath, amText);
    }

    private static void UpdatePlayerCombat()
    {
        string path = "Assets/Scripts/Player/PlayerCombat.cs";
        if (!File.Exists(path)) return;

        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        if (!text.Contains("PlaySwordSwing"))
        {
            text = text.Replace(
                "if (animator != null) animator.SetTrigger(\"Attack\");",
                "if (animator != null) animator.SetTrigger(\"Attack\");\n        AudioManager.Instance?.PlaySwordSwing();"
            );
            File.WriteAllText(path, text);
        }
    }

    private static void UpdateEnemyAI()
    {
        string path = "Assets/Scripts/Enemy/EnemyAI.cs";
        if (!File.Exists(path)) return;

        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        if (!text.Contains("hurtTimer"))
        {
            text = text.Replace(
                "private Vector3 initialScale;",
                "private Vector3 initialScale;\n    private float hurtTimer = 0f;"
            );

            text = text.Replace(
                "if (health != null && health.IsDead())\n        {\n            Die();\n            return;\n        }\n\n        if (player == null) return;",
                "if (health != null && health.IsDead())\n        {\n            Die();\n            return;\n        }\n\n        if (hurtTimer > 0)\n        {\n            hurtTimer -= Time.deltaTime;\n            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 5f), rb.velocity.y);\n            return;\n        }\n\n        if (player == null) return;"
            );

            text = text.Replace(
                "public void OnHurt()\n    {\n        currentState = EnemyState.Hurt;\n        if (animator != null) animator.SetTrigger(\"Hurt\");\n    }",
                "public void OnHurt()\n    {\n        currentState = EnemyState.Hurt;\n        hurtTimer = 0.4f;\n        if (animator != null) animator.SetTrigger(\"Hurt\");\n\n        if (player != null)\n        {\n            float dir = player.position.x > transform.position.x ? -1f : 1f;\n            rb.velocity = new Vector2(dir * 6f, rb.velocity.y);\n        }\n    }"
            );
            File.WriteAllText(path, text);
        }
    }

    private static void UpdateSwordHitDetector()
    {
        string path = "Assets/Scripts/Combat/SwordHitDetector.cs";
        if (!File.Exists(path)) return;

        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        if (!text.Contains("HitStop"))
        {
            text = text.Replace(
                "        // Creative Feature: Screen Shake on hitting enemy!\n        if (targetTag == \"Enemy\")\n        {\n            CameraShake.Instance?.Shake(0.1f, 0.05f);\n            // ✨ Spark particles at hit position\n            EffectsManager.Instance?.PlayHitSparks(other.transform.position);\n            // Award score on EVERY HIT (not just kills) so the fight feels active!\n            GameManager.Instance?.AddScore(10);\n        }\n\n        // Award BONUS score if we killed the enemy\n        if (wasKilled && targetTag == \"Enemy\")\n        {\n            // \U0001F4A5 Death explosion particles\n            EffectsManager.Instance?.PlayDeath(other.transform.position);\n            GameManager.Instance?.AddScore(10); // extra 10 for the finishing blow\n        }",
                "        // Creative Feature: Hit Stop, Screen Shake, and Sounds!\n        if (targetTag == \"Enemy\")\n        {\n            CameraShake.Instance?.Shake(0.1f, 0.05f);\n            EffectsManager.Instance?.PlayHitSparks(other.transform.position);\n            GameManager.Instance?.AddScore(10);\n            AudioManager.Instance?.PlaySwordHit();\n            StartCoroutine(HitStop(0.05f));\n        }\n\n        // Award BONUS score if we killed the enemy\n        if (wasKilled && targetTag == \"Enemy\")\n        {\n            EffectsManager.Instance?.PlayDeath(other.transform.position);\n            GameManager.Instance?.AddScore(10);\n            AudioManager.Instance?.PlayEnemyDeath();\n        }"
            );

            text = text.Replace(
                "    System.Collections.IEnumerator FlashRed(SpriteRenderer sr)",
                "    System.Collections.IEnumerator HitStop(float duration)\n    {\n        Time.timeScale = 0f;\n        yield return new WaitForSecondsRealtime(duration);\n        Time.timeScale = 1f;\n    }\n\n    System.Collections.IEnumerator FlashRed(SpriteRenderer sr)"
            );

            text = text.Replace(
                "yield return new WaitForSeconds(0.1f);",
                "yield return new WaitForSecondsRealtime(0.1f);"
            );
            File.WriteAllText(path, text);
        }
    }
}
