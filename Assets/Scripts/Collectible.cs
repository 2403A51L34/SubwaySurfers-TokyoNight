using UnityEngine;

public enum CollectibleType { Coin, Present, Snowflake, Magnet, Shield, Boost }

public class Collectible : MonoBehaviour
{
    public CollectibleType type;
    bool collected;

    static readonly Color Gold = new Color(1f, 0.8f, 0.15f);
    static readonly Color Red = new Color(0.85f, 0.1f, 0.15f);
    static readonly Color Green = new Color(0.08f, 0.5f, 0.22f);
    static readonly Color IceGlow = new Color(0.55f, 0.9f, 1f);

    public static GameObject Create(CollectibleType type, Transform parent, Vector3 localPos)
    {
        GameObject root = new GameObject(type.ToString());
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        Collectible c = root.AddComponent<Collectible>();
        c.type = type;
        SphereCollider sc = root.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = type == CollectibleType.Coin ? 0.55f : 0.8f;
        Transform t = root.transform;
        switch (type)
        {
            case CollectibleType.Coin:
            {
                GameObject coin = Prim.Tube(t, Vector3.zero, new Vector3(0.6f, 0.06f, 0.6f), Gold, 0.6f);
                coin.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                GameObject inner = Prim.Tube(t, Vector3.zero, new Vector3(0.4f, 0.07f, 0.4f), new Color(1f, 0.65f, 0.1f), 0.4f);
                inner.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                break;
            }
            case CollectibleType.Present:
                Prim.Box(t, Vector3.zero, Vector3.one * 0.6f, Red, 0.2f);
                Prim.Box(t, Vector3.zero, new Vector3(0.12f, 0.62f, 0.62f), Gold, 0.5f);
                Prim.Box(t, Vector3.zero, new Vector3(0.62f, 0.62f, 0.12f), Gold, 0.5f);
                Prim.Ball(t, new Vector3(0, 0.38f, 0), Vector3.one * 0.22f, Gold, 0.6f);
                break;
            case CollectibleType.Snowflake:
                for (int i = 0; i < 3; i++)
                {
                    GameObject arm = Prim.Box(t, Vector3.zero, new Vector3(0.08f, 0.9f, 0.08f), IceGlow, 1.4f);
                    arm.transform.localEulerAngles = new Vector3(0f, 0f, i * 60f);
                }
                Prim.Ball(t, Vector3.zero, Vector3.one * 0.2f, Color.white, 1.5f);
                break;
            case CollectibleType.Magnet:
                Prim.Box(t, new Vector3(-0.25f, 0.2f, 0), new Vector3(0.16f, 0.5f, 0.16f), Red, 0.3f);
                Prim.Box(t, new Vector3(0.25f, 0.2f, 0), new Vector3(0.16f, 0.5f, 0.16f), Red, 0.3f);
                Prim.Box(t, new Vector3(0, -0.08f, 0), new Vector3(0.66f, 0.16f, 0.16f), Red, 0.3f);
                Prim.Box(t, new Vector3(-0.25f, 0.5f, 0), new Vector3(0.17f, 0.14f, 0.17f), new Color(0.85f, 0.88f, 0.95f), 0.5f);
                Prim.Box(t, new Vector3(0.25f, 0.5f, 0), new Vector3(0.17f, 0.14f, 0.17f), new Color(0.85f, 0.88f, 0.95f), 0.5f);
                break;
            case CollectibleType.Shield:
                Prim.Ball(t, Vector3.zero, Vector3.one * 0.75f, IceGlow, 1.2f);
                Prim.Ball(t, Vector3.zero, Vector3.one * 0.4f, Color.white, 1.5f);
                break;
            case CollectibleType.Boost:
                Prim.Ball(t, Vector3.zero, Vector3.one * 0.7f, new Color(1f, 0.5f, 0.1f), 1.3f);
                GameObject bolt = Prim.Box(t, new Vector3(0, 0, -0.2f), new Vector3(0.15f, 0.7f, 0.15f), Color.white, 1.5f);
                bolt.transform.localEulerAngles = new Vector3(0f, 0f, 30f);
                break;
        }
        return root;
    }

    void Update()
    {
        if (collected) return;
        GameManager gm = GameManager.Instance;
        if (gm == null) return;
        if (gm.State == GameState.Playing || gm.State == GameState.Menu)
            transform.Rotate(0f, 180f * Time.deltaTime, 0f, Space.World);

        bool magnetizable = type == CollectibleType.Coin || type == CollectibleType.Present || type == CollectibleType.Snowflake;
        if (magnetizable && gm.State == GameState.Playing && gm.MagnetActive && gm.player != null)
        {
            Vector3 target = gm.player.transform.position + Vector3.up * 0.9f;
            if ((target - transform.position).sqrMagnitude < 12f * 12f)
                transform.position = Vector3.MoveTowards(transform.position, target, (gm.CurrentSpeed + 20f) * Time.deltaTime);
        }
    }

    public void Collect()
    {
        if (collected) return;
        collected = true;
        GameManager.Instance.OnCollect(type);
        gameObject.SetActive(false);
    }
}
