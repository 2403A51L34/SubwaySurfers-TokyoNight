using UnityEngine;

/// THE ONLY SCRIPT YOU ATTACH BY HAND. It builds the lighting, player, track, camera, snow, audio and UI at runtime.
public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
        Application.targetFrameRate = 60;

        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject cg = new GameObject("Main Camera");
            cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
            cg.AddComponent<AudioListener>();
        }

        SetupEnvironment(cam);

        GameObject systems = new GameObject("GameSystems");
        systems.AddComponent<AudioManager>();
        GameManager gm = systems.AddComponent<GameManager>();
        UIManager ui = systems.AddComponent<UIManager>();

        TrackGenerator track = new GameObject("Track").AddComponent<TrackGenerator>();
        PlayerController player = new GameObject("Player").AddComponent<PlayerController>();
        ChaserController chasers = new GameObject("Chasers").AddComponent<ChaserController>();

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        follow.snowAnchor = CreateSnow();

        gm.player = player;
        gm.track = track;
        gm.cameraFollow = follow;

        ui.Build();
        gm.ShowMenu();
    }

    void SetupEnvironment(Camera cam)
    {
        Color sky = new Color(0.66f, 0.82f, 0.96f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = sky;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 500f;
        cam.fieldOfView = 60f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = sky;
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 260f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.68f, 0.8f);

        GameObject old = GameObject.Find("Directional Light");
        if (old != null) old.SetActive(false);
        GameObject sunGo = new GameObject("Winter Sun");
        Light sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.96f, 0.88f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
        sunGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        RenderSettings.sun = sun;
        QualitySettings.shadowDistance = 60f;
    }

    Transform CreateSnow()
    {
        GameObject go = new GameObject("Snowfall");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = 5f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
        main.startColor = new Color(1f, 1f, 1f, 0.9f);
        main.maxParticles = 3000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 450f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(60f, 1f, 70f);

        ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        vel.y = new ParticleSystem.MinMaxCurve(-4f, -2.5f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.sharedMaterial = SnowMaterial();

        go.transform.position = new Vector3(0f, 14f, 22f);
        ps.Play();
        return go.transform;
    }

    Material SnowMaterial()
    {
        Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 16f;
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        Shader sh = Shader.Find("Sprites/Default");
        Material m = sh != null ? new Material(sh) : new Material(Prim.Mat(Color.white, 1f));
        m.mainTexture = tex;
        return m;
    }
}
