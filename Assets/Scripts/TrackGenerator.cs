using System.Collections.Generic;
using UnityEngine;

/// Endless procedural track: builds 30-unit segments ahead of the player and removes them behind.
public class TrackGenerator : MonoBehaviour
{
    [Header("Track layout")]
    public float segmentLength = 30f;
    public int segmentsAhead = 9;
    public float laneDistance = 2.5f;
    public int safeSegments = 3;      // empty segments at the start of each run

    static readonly Color SnowField = new Color(0.93f, 0.96f, 1f);
    static readonly Color Road = new Color(0.80f, 0.86f, 0.95f);
    static readonly Color Bed = new Color(0.66f, 0.72f, 0.82f);
    static readonly Color Sleeper = new Color(0.5f, 0.4f, 0.33f);
    static readonly Color Silver = new Color(0.78f, 0.8f, 0.88f);
    static readonly Color Snow = new Color(0.96f, 0.98f, 1f);
    static readonly Color Warm = new Color(1f, 0.85f, 0.45f);
    static readonly Color Red = new Color(0.85f, 0.1f, 0.15f);
    static readonly Color Green = new Color(0.08f, 0.5f, 0.22f);
    static readonly Color Gold = new Color(1f, 0.8f, 0.2f);
    static readonly Color Wood = new Color(0.45f, 0.3f, 0.16f);
    static readonly Color[] BuildingColors =
    {
        new Color(0.75f, 0.55f, 0.5f), new Color(0.55f, 0.62f, 0.78f), new Color(0.82f, 0.72f, 0.55f),
        new Color(0.5f, 0.62f, 0.56f), new Color(0.7f, 0.5f, 0.62f)
    };
    static readonly Color[] LightColors =
    {
        new Color(1f, 0.2f, 0.2f), new Color(1f, 0.9f, 0.3f), new Color(0.3f, 0.6f, 1f), new Color(1f, 0.4f, 0.9f), new Color(0.4f, 1f, 0.5f)
    };

    readonly List<GameObject> segments = new List<GameObject>();
    float nextZ;
    int spawnedCount;
    int lastFreeLane = 1;

    public void ResetTrack()
    {
        foreach (GameObject s in segments)
        {
            if (s == null) continue;
            s.SetActive(false);
            Destroy(s);
        }
        segments.Clear();
        nextZ = -segmentLength;
        spawnedCount = 0;
        lastFreeLane = 1;
        for (int i = 0; i < segmentsAhead + 1; i++) SpawnSegment();
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;
        float pz = gm.player.transform.position.z;

        if (nextZ < pz + segmentsAhead * segmentLength) SpawnSegment();   // at most one per frame

        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i] == null) { segments.RemoveAt(i); continue; }
            if (segments[i].transform.position.z + segmentLength < pz - 25f)
            {
                Destroy(segments[i]);
                segments.RemoveAt(i);
            }
        }
    }

    float LaneX(int lane) { return (lane - 1) * laneDistance; }

    void SpawnSegment()
    {
        GameObject seg = new GameObject("Segment_" + spawnedCount);
        seg.transform.SetParent(transform, false);
        seg.transform.position = new Vector3(0f, 0f, nextZ);
        BuildGround(seg.transform);
        BuildScenery(seg.transform);
        if (spawnedCount >= safeSegments) BuildGameplay(seg.transform);
        segments.Add(seg);
        nextZ += segmentLength;
        spawnedCount++;
    }

    // ------------------------------------------------------------ ground
    void BuildGround(Transform t)
    {
        float L = segmentLength;
        Prim.Box(t, new Vector3(0, -0.6f, L / 2f), new Vector3(300f, 1f, L), SnowField);
        Prim.Box(t, new Vector3(0, -0.5f, L / 2f), new Vector3(8.6f, 1f, L), Road);
        for (int lane = 0; lane < 3; lane++)
        {
            float x = LaneX(lane);
            Prim.Box(t, new Vector3(x, 0.015f, L / 2f), new Vector3(1.9f, 0.03f, L), Bed);
            Prim.Box(t, new Vector3(x - 0.55f, 0.1f, L / 2f), new Vector3(0.1f, 0.12f, L), Silver, 0.1f);
            Prim.Box(t, new Vector3(x + 0.55f, 0.1f, L / 2f), new Vector3(0.1f, 0.12f, L), Silver, 0.1f);
            for (float z = 1f; z < L; z += 3f)
                Prim.Box(t, new Vector3(x, 0.06f, z), new Vector3(1.6f, 0.06f, 0.3f), Sleeper);
        }
        for (int si = 0; si < 2; si++)
        {
            int side = si == 0 ? -1 : 1;
            Prim.Box(t, new Vector3(side * 6.3f, -0.2f, L / 2f), new Vector3(4f, 1f, L), Snow);
            Prim.Box(t, new Vector3(side * 4.4f, 0.1f, L / 2f), new Vector3(0.25f, 0.4f, L), Bed);
        }
    }

    // ------------------------------------------------------------ scenery
    void BuildScenery(Transform t)
    {
        for (int si = 0; si < 2; si++)
        {
            int side = si == 0 ? -1 : 1;
            BuildBuilding(t, side, 7.5f);
            BuildBuilding(t, side, 22.5f);
            BuildLamp(t, side, 3f);
            BuildLamp(t, side, 18f);
            BuildTree(t, new Vector3(side * 6.6f, 0.3f, 11f), 1f);
            if (Random.value < 0.6f) BuildTree(t, new Vector3(side * 6.6f, 0.3f, 26f), 0.9f);
            if (Random.value < 0.5f) BuildCandyCane(t, new Vector3(side * 4.8f, 0.3f, Random.value < 0.5f ? 7f : 23f));
            if (Random.value < 0.4f) BuildGiftPile(t, new Vector3(side * 7.8f, 0.3f, 17f));
            if (Random.value < 0.25f)
            {
                GameObject sm = ObstacleFactory.SnowmanModel(t);
                sm.transform.localPosition = new Vector3(side * 7.8f, 0.3f, 4f);
                sm.transform.localScale = Vector3.one * 0.8f;
            }
            BuildMountain(t, side, Random.Range(0f, segmentLength));
        }
    }

    void BuildBuilding(Transform t, int side, float z)
    {
        float w = 7f, depth = 11f;
        float h = Random.Range(8f, 19f);
        float xc = side * (12.5f + Random.Range(0f, 1.5f));
        Color col = BuildingColors[Random.Range(0, BuildingColors.Length)];
        Prim.Box(t, new Vector3(xc, h / 2f - 0.1f, z), new Vector3(w, h, depth), col);
        Prim.Box(t, new Vector3(xc, h - 0.1f + 0.25f, z), new Vector3(w + 0.5f, 0.5f, depth + 0.5f), Snow);

        float faceX = xc - side * (w / 2f) - side * 0.04f;
        int floors = Mathf.Min(4, Mathf.FloorToInt((h - 3f) / 3.2f));
        for (int f = 0; f < floors; f++)
        {
            float y = 2.7f + f * 3.2f;
            for (int k = -1; k <= 1; k += 2)
            {
                bool lit = Random.value < 0.7f;
                Color wc = lit ? Warm : new Color(0.2f, 0.25f, 0.4f);
                Prim.Box(t, new Vector3(faceX, y, z + k * 2.8f), new Vector3(0.1f, 1.3f, 1.6f), wc, lit ? 1.1f : 0f);
            }
        }
        if (Random.value < 0.5f)
        {
            Prim.Box(t, new Vector3(faceX, 1.1f, z), new Vector3(0.1f, 1.2f, 4.5f), Red, 0.2f);
            Prim.Box(t, new Vector3(faceX - side * 0.01f, 1.1f, z), new Vector3(0.11f, 0.16f, 4.0f), Snow, 0.3f);
            for (int i = 0; i < 6; i++)
                Prim.Ball(t, new Vector3(faceX - side * 0.1f, 1.85f, z - 2.5f + i), Vector3.one * 0.18f, LightColors[i % LightColors.Length], 2f);
        }
    }

    void BuildLamp(Transform t, int side, float z)
    {
        float x = side * 4.8f;
        Prim.Tube(t, new Vector3(x, 2.7f, z), new Vector3(0.12f, 2.4f, 0.12f), Silver);
        for (int i = 0; i < 3; i++)
            Prim.Tube(t, new Vector3(x, 1.0f + i * 0.8f, z), new Vector3(0.16f, 0.1f, 0.16f), Red);
        Prim.Box(t, new Vector3(x - side * 0.5f, 5.1f, z), new Vector3(1.1f, 0.08f, 0.1f), Silver);
        Prim.Ball(t, new Vector3(x - side * 1.0f, 4.95f, z), Vector3.one * 0.4f, Warm, 2.5f);
        GameObject halo = Prim.Ball(t, new Vector3(x - side * 1.0f, 4.95f, z), Vector3.one * 1.5f, Warm);
        halo.GetComponent<Renderer>().sharedMaterial = Prim.TransparentMat(new Color(1f, 0.9f, 0.5f, 0.2f));
    }

    void BuildTree(Transform parent, Vector3 pos, float s)
    {
        GameObject g = new GameObject("XmasTree");
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = Vector3.one * s;
        Transform t = g.transform;
        Color leaf = new Color(0.05f, 0.42f, 0.2f);
        Prim.Tube(t, new Vector3(0, 0.4f, 0), new Vector3(0.35f, 0.4f, 0.35f), Wood);
        float[] baseY = { 0.6f, 1.7f, 2.8f };
        float[] height = { 2.2f, 2.0f, 1.8f };
        float[] radius = { 1.6f, 1.25f, 0.9f };
        for (int i = 0; i < 3; i++)
            Prim.MakeCone(t, new Vector3(0, baseY[i], 0), new Vector3(radius[i] * 2f, height[i], radius[i] * 2f), leaf);
        Prim.Ball(t, new Vector3(0, 4.7f, 0), Vector3.one * 0.45f, Gold, 2f);
        for (int i = 0; i < 10; i++)
        {
            int layer = i % 3;
            float f = Random.Range(0.15f, 0.7f);
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float r = radius[layer] * (1f - f) * 1.03f;
            Vector3 p = new Vector3(Mathf.Cos(ang) * r, baseY[layer] + f * height[layer], Mathf.Sin(ang) * r);
            Prim.Ball(t, p, Vector3.one * 0.2f, LightColors[Random.Range(0, LightColors.Length)], 2.2f);
        }
    }

    void BuildCandyCane(Transform parent, Vector3 pos)
    {
        GameObject g = new GameObject("CandyCane");
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = Vector3.one * 1.6f;
        Transform t = g.transform;
        Prim.Tube(t, new Vector3(0, 0.7f, 0), new Vector3(0.12f, 0.7f, 0.12f), Color.white);
        for (int i = 0; i < 4; i++)
            Prim.Tube(t, new Vector3(0, 0.25f + i * 0.35f, 0), new Vector3(0.135f, 0.09f, 0.135f), Red);
        GameObject crook = Prim.Tube(t, new Vector3(0.14f, 1.42f, 0), new Vector3(0.12f, 0.16f, 0.12f), Red);
        crook.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        Prim.Ball(t, new Vector3(0.3f, 1.34f, 0), Vector3.one * 0.14f, Red);
    }

    void BuildGiftPile(Transform parent, Vector3 pos)
    {
        GameObject g = new GameObject("GiftPile");
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        Transform t = g.transform;
        Prim.Box(t, new Vector3(0, 0.4f, 0), new Vector3(0.8f, 0.8f, 0.8f), Red);
        Prim.Box(t, new Vector3(0, 0.4f, 0), new Vector3(0.14f, 0.82f, 0.82f), Gold, 0.3f);
        Prim.Box(t, new Vector3(0.8f, 0.3f, 0.2f), new Vector3(0.6f, 0.6f, 0.6f), Green);
        Prim.Box(t, new Vector3(0.8f, 0.3f, 0.2f), new Vector3(0.12f, 0.62f, 0.62f), Snow);
        Prim.Box(t, new Vector3(0.1f, 1.0f, 0), new Vector3(0.5f, 0.4f, 0.5f), new Color(0.2f, 0.4f, 0.85f));
        Prim.Box(t, new Vector3(0.1f, 1.0f, 0), new Vector3(0.1f, 0.42f, 0.52f), Gold, 0.3f);
    }

    void BuildMountain(Transform t, int side, float z)
    {
        float w = Random.Range(50f, 90f);
        float h = Random.Range(30f, 55f);
        float x = side * Random.Range(70f, 110f);
        Prim.MakeCone(t, new Vector3(x, -0.5f, z), new Vector3(w, h, w), new Color(0.55f, 0.66f, 0.82f));
        float capFrac = 0.45f;
        Prim.MakeCone(t, new Vector3(x, -0.5f + h * (1f - capFrac), z), new Vector3(w * capFrac, h * capFrac, w * capFrac), Color.white);
    }

    // ------------------------------------------------------------ gameplay
    void BuildGameplay(Transform seg)
    {
        GameManager gm = GameManager.Instance;
        float d = gm != null ? gm.Difficulty : 0f;
        float[] rowZ = { 5f, 15f, 25f };
        for (int i = 0; i < rowZ.Length; i++) BuildRow(seg, rowZ[i], d);
    }

    void BuildRow(Transform seg, float z, float d)
    {
        // The "free" lane is always passable. It moves at most one lane per row, so the player can always reach it.
        int free = Mathf.Clamp(lastFreeLane + Random.Range(-1, 2), 0, 2);
        bool calm = Random.value < (0.25f - 0.15f * d);
        float pBlock = 0.35f + 0.45f * d;

        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == free) continue;
            bool blocked = !calm && Random.value < pBlock;
            if (blocked)
            {
                ObstacleType type = PickBlock(d, lane == lastFreeLane);
                ObstacleFactory.Create(type, seg, new Vector3(LaneX(lane), 0f, z));
            }
            else if (Random.value < 0.25f)
            {
                CoinLine(seg, lane, z, false);
            }
        }

        // Free lane content
        if (!calm && Random.value < 0.15f + 0.15f * d)
        {
            float r = Random.value;
            ObstacleType t = r < 0.4f ? ObstacleType.Crate : (r < 0.7f ? ObstacleType.Barrier : ObstacleType.Banner);
            ObstacleFactory.Create(t, seg, new Vector3(LaneX(free), 0f, z));
            if (t != ObstacleType.Banner) CoinArc(seg, free, z);
        }
        else
        {
            float c = Random.value;
            bool pickup = true;
            if (c < 0.07f)
            {
                CollectibleType p = (CollectibleType)Random.Range((int)CollectibleType.Magnet, (int)CollectibleType.Boost + 1);
                Collectible.Create(p, seg, new Vector3(LaneX(free), 1.1f, z));
            }
            else if (c < 0.17f) Collectible.Create(CollectibleType.Present, seg, new Vector3(LaneX(free), 1.1f, z));
            else if (c < 0.27f) Collectible.Create(CollectibleType.Snowflake, seg, new Vector3(LaneX(free), 1.1f, z));
            else pickup = false;
            CoinLine(seg, free, z, pickup);
        }
        lastFreeLane = free;
    }

    ObstacleType PickBlock(float d, bool avoidTrain)
    {
        List<ObstacleType> pool = new List<ObstacleType>
        {
            ObstacleType.Snowman, ObstacleType.IceBlock, ObstacleType.GiftStack, ObstacleType.MarketStall
        };
                pool.Add(ObstacleType.Snowplow);
        if (!avoidTrain) { pool.Add(ObstacleType.Train); pool.Add(ObstacleType.Train); pool.Add(ObstacleType.Train); }
        return pool[Random.Range(0, pool.Count)];
    }

    void CoinLine(Transform seg, int lane, float z, bool skipCenter)
    {
        for (int i = -2; i <= 2; i++)
        {
            if (skipCenter && i == 0) continue;
            Collectible.Create(CollectibleType.Coin, seg, new Vector3(LaneX(lane), 1.0f, z + i * 2f));
        }
    }

    void CoinArc(Transform seg, int lane, float z)
    {
        float[] ys = { 1.0f, 1.8f, 2.3f, 1.8f, 1.0f };
        for (int i = 0; i < 5; i++)
            Collectible.Create(CollectibleType.Coin, seg, new Vector3(LaneX(lane), ys[i], z + (i - 2) * 2f));
    }
}
