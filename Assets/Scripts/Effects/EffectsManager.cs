using UnityEngine;

/// <summary>
/// Manages all runtime particle effects — sword sparks, death explosions, projectile trails.
/// Attach to a persistent GameObject (e.g. GameManager or its own Effects object).
/// </summary>
public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    [Header("Particle Prefabs (auto-generated if null)")]
    public ParticleSystem hitSparkPrefab;
    public ParticleSystem deathExplosionPrefab;
    public ParticleSystem projectileHitPrefab;
    public ParticleSystem footDustPrefab;
    public ParticleSystem healPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        // Auto-generate particle systems if not assigned
        if (hitSparkPrefab       == null) hitSparkPrefab       = CreateSparks("HitSparks",       Color.yellow, 0.3f, 20);
        if (deathExplosionPrefab == null) deathExplosionPrefab = CreateSparks("DeathExplosion",  Color.red,    0.8f, 60);
        if (projectileHitPrefab  == null) projectileHitPrefab  = CreateSparks("ProjectileHit",   Color.cyan,   0.2f, 15);
        if (footDustPrefab       == null) footDustPrefab       = CreateDust ("FootDust",         Color.grey);
        if (healPrefab           == null) healPrefab           = CreateSparks("HealEffect",      Color.green,  0.5f, 30);
    }

    // ── Public API ─────────────────────────────────────────────────────────
    public void PlayHitSparks(Vector3 pos)       => Play(hitSparkPrefab,       pos);
    public void PlayDeath(Vector3 pos)           => Play(deathExplosionPrefab, pos);
    public void PlayProjectileHit(Vector3 pos)   => Play(projectileHitPrefab,  pos);
    public void PlayHeal(Vector3 pos)            => Play(healPrefab,           pos);

    void Play(ParticleSystem prefab, Vector3 pos)
    {
        if (prefab == null) return;
        var ps = Instantiate(prefab, pos, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 0.1f);
    }

    // ── Particle Factory ───────────────────────────────────────────────────
    ParticleSystem CreateSparks(string name, Color color, float duration, int count)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor       = new ParticleSystem.MinMaxGradient(color, Color.white);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize        = new ParticleSystem.MinMaxCurve(0.03f, 0.12f);
        main.startLifetime    = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.duration         = duration;
        main.loop             = false;
        main.maxParticles     = count;
        main.gravityModifier  = 0.5f;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.1f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;

        // Color over lifetime: fade out
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        go.SetActive(false);
        return ps;
    }

    ParticleSystem CreateDust(string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor     = new ParticleSystem.MinMaxGradient(color);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
        main.loop           = true;
        main.gravityModifier = -0.1f;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(0.3f, 0.01f, 0.01f);

        go.SetActive(false);
        return ps;
    }
}
