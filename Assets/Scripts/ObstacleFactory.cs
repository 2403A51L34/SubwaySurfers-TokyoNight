using UnityEngine;

/// Builds every obstacle from primitives. Pivot = ground centre of the lane.
public static class ObstacleFactory
{
    static readonly Color Snow = new Color(0.96f, 0.98f, 1f);
    static readonly Color Wood = new Color(0.55f, 0.35f, 0.18f);
    static readonly Color DarkWood = new Color(0.35f, 0.2f, 0.1f);
    static readonly Color Ice = new Color(0.55f, 0.85f, 1f);
    static readonly Color Red = new Color(0.85f, 0.1f, 0.15f);
    static readonly Color Green = new Color(0.08f, 0.5f, 0.22f);
    static readonly Color Gold = new Color(1f, 0.8f, 0.2f);
    static readonly Color Blue = new Color(0.2f, 0.4f, 0.85f);
    static readonly Color Coal = new Color(0.1f, 0.1f, 0.12f);
    static readonly Color Orange = new Color(1f, 0.5f, 0.1f);
    static readonly Color Silver = new Color(0.75f, 0.78f, 0.85f);
    static readonly Color Warm = new Color(1f, 0.85f, 0.45f);

    public static GameObject Create(ObstacleType type, Transform parent, Vector3 localPos)
    {
        GameObject root = new GameObject(type.ToString());
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        Obstacle ob = root.AddComponent<Obstacle>();
        ob.type = type;
        ob.kind = ObstacleKind.Full;
        switch (type)
        {
            case ObstacleType.Crate: BuildCrate(root); ob.kind = ObstacleKind.Low; break;
            case ObstacleType.Barrier: BuildBarrier(root); ob.kind = ObstacleKind.Low; break;
            case ObstacleType.IceBlock: BuildIceBlock(root); break;
            case ObstacleType.Snowman: BuildSnowman(root); break;
            case ObstacleType.GiftStack: BuildGiftStack(root); break;
            case ObstacleType.Train: BuildTrain(root); ob.moveSpeed = Random.Range(3f, 4f); break;
            case ObstacleType.Snowplow: BuildSnowplow(root); break;
            case ObstacleType.MarketStall: BuildStall(root); break;
            case ObstacleType.Banner: BuildBanner(root); ob.kind = ObstacleKind.High; break;
        }
        return root;
    }

    static void BuildCrate(GameObject r)
    {
        Transform t = r.transform;
        Prim.Box(t, new Vector3(0, 0.45f, 0), new Vector3(1.8f, 0.9f, 1.2f), Wood);
        Prim.Box(t, new Vector3(0, 0.45f, -0.61f), new Vector3(1.85f, 0.14f, 0.03f), DarkWood);
        Prim.Box(t, new Vector3(-0.8f, 0.45f, -0.61f), new Vector3(0.14f, 0.92f, 0.03f), DarkWood);
        Prim.Box(t, new Vector3(0.8f, 0.45f, -0.61f), new Vector3(0.14f, 0.92f, 0.03f), DarkWood);
        Prim.Box(t, new Vector3(0, 0.96f, 0), new Vector3(1.9f, 0.14f, 1.3f), Snow);
        Prim.AddBox(r, new Vector3(0, 0.5f, 0), new Vector3(1.8f, 1.0f, 1.2f));
    }

    static void BuildBarrier(GameObject r)
    {
        Transform t = r.transform;
        for (int i = 0; i < 5; i++)
            Prim.Box(t, new Vector3(-0.88f + i * 0.44f, 0.62f, 0), new Vector3(0.44f, 0.34f, 0.18f), i % 2 == 0 ? Red : Snow);
        Prim.Box(t, new Vector3(-0.9f, 0.3f, 0), new Vector3(0.14f, 0.6f, 0.14f), Ice, 0.3f);
        Prim.Box(t, new Vector3(0.9f, 0.3f, 0), new Vector3(0.14f, 0.6f, 0.14f), Ice, 0.3f);
        Prim.Box(t, new Vector3(0, 0.82f, 0), new Vector3(2.2f, 0.08f, 0.24f), Snow);
        Prim.AddBox(r, new Vector3(0, 0.45f, 0), new Vector3(2.2f, 0.9f, 0.4f));
    }

    static void BuildIceBlock(GameObject r)
    {
        Transform t = r.transform;
        Prim.Box(t, new Vector3(0, 0.9f, 0), new Vector3(1.8f, 1.8f, 1.4f), Ice, 0.25f);
        Prim.Box(t, new Vector3(0, 0.9f, 0), new Vector3(1.5f, 1.5f, 1.45f), new Color(0.8f, 0.95f, 1f), 0.3f);
        Prim.Box(t, new Vector3(0, 1.86f, 0), new Vector3(1.8f, 0.12f, 1.4f), Snow);
        Prim.AddBox(r, new Vector3(0, 0.95f, 0), new Vector3(1.8f, 1.9f, 1.4f));
    }

    // Also used by the track generator as roadside decoration (without a collider).
    public static GameObject SnowmanModel(Transform parent)
    {
        GameObject h = new GameObject("SnowmanModel");
        h.transform.SetParent(parent, false);
        Transform t = h.transform;
        Prim.Ball(t, new Vector3(0, 0.6f, 0), Vector3.one * 1.2f, Snow);
        Prim.Ball(t, new Vector3(0, 1.5f, 0), Vector3.one * 0.9f, Snow);
        Prim.Ball(t, new Vector3(0, 2.15f, 0), Vector3.one * 0.68f, Snow);
        Prim.Tube(t, new Vector3(0, 1.88f, 0), new Vector3(0.55f, 0.05f, 0.55f), Red);
        Prim.Tube(t, new Vector3(0, 2.45f, 0), new Vector3(0.62f, 0.025f, 0.62f), Coal);
        Prim.Tube(t, new Vector3(0, 2.68f, 0), new Vector3(0.38f, 0.2f, 0.38f), Coal);
        Prim.Ball(t, new Vector3(-0.12f, 2.25f, -0.3f), Vector3.one * 0.08f, Coal);
        Prim.Ball(t, new Vector3(0.12f, 2.25f, -0.3f), Vector3.one * 0.08f, Coal);
        Prim.Ball(t, new Vector3(0, 1.7f, -0.42f), Vector3.one * 0.1f, Coal);
        Prim.Ball(t, new Vector3(0, 1.45f, -0.44f), Vector3.one * 0.1f, Coal);
        Prim.Ball(t, new Vector3(0, 1.2f, -0.42f), Vector3.one * 0.1f, Coal);
        GameObject nose = Prim.MakeCone(t, new Vector3(0, 2.15f, -0.32f), new Vector3(0.12f, 0.4f, 0.12f), Orange);
        nose.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        return h;
    }

    static void BuildSnowman(GameObject r)
    {
        SnowmanModel(r.transform);
        Prim.AddBox(r, new Vector3(0, 1.45f, 0), new Vector3(1.2f, 2.9f, 1.2f));
    }

    static void BuildGiftStack(GameObject r)
    {
        Transform t = r.transform;
        Prim.Box(t, new Vector3(0, 0.55f, 0), new Vector3(1.7f, 1.1f, 1.5f), Red);
        Prim.Box(t, new Vector3(0, 0.55f, 0), new Vector3(0.24f, 1.14f, 1.55f), Gold, 0.3f);
        Prim.Box(t, new Vector3(0, 0.55f, 0), new Vector3(1.75f, 1.14f, 0.24f), Gold, 0.3f);
        Prim.Box(t, new Vector3(0, 1.55f, 0), new Vector3(1.3f, 0.9f, 1.2f), Green);
        Prim.Box(t, new Vector3(0, 1.55f, 0), new Vector3(0.2f, 0.94f, 1.25f), Snow);
        Prim.Box(t, new Vector3(0, 1.55f, 0), new Vector3(1.35f, 0.94f, 0.2f), Snow);
        Prim.Box(t, new Vector3(0, 2.35f, 0), new Vector3(0.9f, 0.7f, 0.9f), Blue);
        Prim.Box(t, new Vector3(0, 2.35f, 0), new Vector3(0.16f, 0.74f, 0.95f), Gold, 0.3f);
        Prim.Ball(t, new Vector3(0, 2.8f, 0), Vector3.one * 0.35f, Gold, 0.4f);
        Prim.AddBox(r, new Vector3(0, 1.4f, 0), new Vector3(1.8f, 2.8f, 1.5f));
    }

    static void BuildTrain(GameObject r)
    {
        Transform t = r.transform;
        Prim.Box(t, new Vector3(0, 0.15f, 0), new Vector3(1.9f, 0.3f, 8.8f), Coal);
        Prim.Box(t, new Vector3(0, 1.5f, 0), new Vector3(2f, 2.6f, 9f), Red);
        Prim.Box(t, new Vector3(0, 1.0f, 0), new Vector3(2.02f, 0.35f, 9.02f), Green);
        Prim.Box(t, new Vector3(0, 2.9f, 0), new Vector3(2.1f, 0.2f, 9.1f), Snow);
        for (int i = 0; i < 4; i++)
        {
            float z = -3.3f + i * 2.2f;
            Prim.Box(t, new Vector3(-1.01f, 1.9f, z), new Vector3(0.05f, 0.7f, 1.0f), Warm, 1.2f);
            Prim.Box(t, new Vector3(1.01f, 1.9f, z), new Vector3(0.05f, 0.7f, 1.0f), Warm, 1.2f);
        }
        Prim.Box(t, new Vector3(0, 1.9f, -4.52f), new Vector3(1.4f, 0.8f, 0.05f), new Color(0.6f, 0.85f, 1f), 0.5f);
        Prim.Ball(t, new Vector3(-0.6f, 0.9f, -4.55f), Vector3.one * 0.3f, Warm, 2f);
        Prim.Ball(t, new Vector3(0.6f, 0.9f, -4.55f), Vector3.one * 0.3f, Warm, 2f);
        Prim.Ball(t, new Vector3(0, 1.2f, -4.56f), new Vector3(0.8f, 0.8f, 0.12f), Green);
        Prim.Box(t, new Vector3(0, 1.7f, -4.62f), new Vector3(0.3f, 0.2f, 0.12f), Red);
        Prim.AddBox(r, new Vector3(0, 1.5f, 0), new Vector3(2f, 3f, 9f));
    }

    static void BuildSnowplow(GameObject r)
    {
        Transform t = r.transform;
        Prim.Box(t, new Vector3(0, 0.9f, 0.2f), new Vector3(1.9f, 1.0f, 3f), Orange);
        Prim.Box(t, new Vector3(0, 1.9f, 0.7f), new Vector3(1.5f, 1.0f, 1.4f), Orange);
        Prim.Box(t, new Vector3(0, 2.0f, 0.0f), new Vector3(1.3f, 0.6f, 0.05f), new Color(0.6f, 0.85f, 1f), 0.4f);
        GameObject blade = Prim.Box(t, new Vector3(0, 0.8f, -1.7f), new Vector3(2.2f, 1.1f, 0.25f), Silver);
        blade.transform.localEulerAngles = new Vector3(-15f, 0f, 0f);
        Prim.Box(t, new Vector3(0, 1.45f, -1.75f), new Vector3(2.2f, 0.12f, 0.35f), Snow);
        Prim.Ball(t, new Vector3(0, 2.55f, 0.7f), Vector3.one * 0.3f, Gold, 2.5f);
        for (int i = 0; i < 4; i++)
        {
            float x = (i % 2 == 0) ? -1.0f : 1.0f;
            float z = (i < 2) ? -0.8f : 1.2f;
            GameObject w = Prim.Tube(t, new Vector3(x, 0.4f, z), new Vector3(0.8f, 0.15f, 0.8f), Coal);
            w.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        }
        Prim.AddBox(r, new Vector3(0, 1.2f, -0.1f), new Vector3(2.2f, 2.4f, 3.8f));
    }

    static void BuildStall(GameObject r)
    {
        Transform t = r.transform;
        Prim.Box(t, new Vector3(0, 0.5f, 0), new Vector3(2.2f, 1.0f, 1.2f), Wood);
        Prim.Box(t, new Vector3(0, 1.02f, 0), new Vector3(2.3f, 0.08f, 1.3f), Snow);
        Prim.Ball(t, new Vector3(-0.6f, 1.2f, -0.2f), Vector3.one * 0.3f, Red);
        Prim.Ball(t, new Vector3(-0.2f, 1.2f, 0.1f), Vector3.one * 0.3f, Red);
        Prim.Box(t, new Vector3(0.4f, 1.25f, -0.1f), Vector3.one * 0.4f, Green);
        Prim.Box(t, new Vector3(0.8f, 1.2f, 0.2f), Vector3.one * 0.3f, Gold, 0.3f);
        float[] px = { -1.0f, 1.0f };
        float[] pz = { -0.6f, 0.6f };
        for (int a = 0; a < 2; a++)
            for (int b = 0; b < 2; b++)
                Prim.Tube(t, new Vector3(px[a], 1.8f, pz[b]), new Vector3(0.1f, 0.8f, 0.1f), DarkWood);
        for (int i = 0; i < 4; i++)
            Prim.Box(t, new Vector3(-0.825f + i * 0.55f, 2.65f, 0), new Vector3(0.55f, 0.12f, 1.8f), i % 2 == 0 ? Red : Snow);
        Prim.Box(t, new Vector3(0, 2.78f, 0), new Vector3(2.2f, 0.12f, 1.7f), Snow);
        for (int i = 0; i < 6; i++)
            Prim.Ball(t, new Vector3(-1.0f + i * 0.4f, 2.5f, -0.92f), Vector3.one * 0.14f, i % 2 == 0 ? Warm : Red, 2f);
        Prim.AddBox(r, new Vector3(0, 1.45f, 0), new Vector3(2.3f, 2.9f, 1.8f));
    }

    static void BuildBanner(GameObject r)
    {
        Transform t = r.transform;
        Prim.Tube(t, new Vector3(-1.1f, 1.5f, 0), new Vector3(0.12f, 1.5f, 0.12f), Silver);
        Prim.Tube(t, new Vector3(1.1f, 1.5f, 0), new Vector3(0.12f, 1.5f, 0.12f), Silver);
        Prim.Box(t, new Vector3(0, 1.9f, 0), new Vector3(2.3f, 1.0f, 0.15f), Red, 0.15f);
        Prim.Box(t, new Vector3(0, 1.9f, -0.09f), new Vector3(1.7f, 0.22f, 0.04f), Snow, 0.3f);
        Prim.Box(t, new Vector3(0, 1.45f, 0), new Vector3(2.3f, 0.1f, 0.17f), Green);
        Prim.Box(t, new Vector3(0, 2.4f, 0), new Vector3(2.3f, 0.1f, 0.17f), Green);
        Prim.Ball(t, new Vector3(-1.1f, 3.05f, 0), Vector3.one * 0.25f, Gold, 2f);
        Prim.Ball(t, new Vector3(1.1f, 3.05f, 0), Vector3.one * 0.25f, Gold, 2f);
        Prim.AddBox(r, new Vector3(0, 1.9f, 0), new Vector3(2.2f, 1.0f, 0.5f));
    }
}
