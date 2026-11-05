using UnityEngine;

/// A police officer and a dog that chase the player at the start of a run (like classic runner games),
/// then fall behind and disappear. If you crash, they catch up to you.
public class ChaserController : MonoBehaviour
{
    [Header("Chase settings")]
    public float chaseDuration = 25f;     // seconds they chase you. Try 5 for a short chase, 60 for one minute.
    public float policeDistance = 4.2f;   // how far behind the player the police run
    public float dogDistance = 2.8f;      // how far behind the player the dog runs
    public float retreatSpeed = 6f;       // how fast they fall back when the chase ends
    public float catchUpSpeed = 14f;      // how fast they run up to you on Game Over

    Transform police, dog;
    Transform pLegL, pLegR, pArmL, pArmR;
    Transform dLegFL, dLegFR, dLegBL, dLegBR, dTail;
    Renderer sirenRenderer;
    Material redLight, blueLight;

    float policeBehind, dogBehind, policeX, dogX;
    float lastRunTime;
    bool visible;
    GameState prevState = GameState.Menu;

    void Awake()
    {
        redLight = Prim.Mat(new Color(1f, 0.1f, 0.1f), 2.5f);
        blueLight = Prim.Mat(new Color(0.15f, 0.3f, 1f), 2.5f);
        BuildPolice();
        BuildDog();
        police.gameObject.SetActive(false);
        dog.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------ models
    void BuildPolice()
    {
        police = new GameObject("Police").transform;
        police.SetParent(transform, false);
        police.localScale = Vector3.one * 1.05f;
        Color navy = new Color(0.1f, 0.15f, 0.38f);
        Color skin = new Color(1f, 0.78f, 0.62f);
        Color black = new Color(0.08f, 0.08f, 0.1f);
        Color gold = new Color(1f, 0.8f, 0.2f);

        pLegL = Limb(police, new Vector3(-0.17f, 0.5f, 0), new Vector3(0.2f, 0.5f, 0.22f), new Vector3(0, -0.25f, 0), navy);
        pLegR = Limb(police, new Vector3(0.17f, 0.5f, 0), new Vector3(0.2f, 0.5f, 0.22f), new Vector3(0, -0.25f, 0), navy);
        pArmL = Limb(police, new Vector3(-0.4f, 1.2f, 0), new Vector3(0.15f, 0.5f, 0.15f), new Vector3(0, -0.23f, 0), navy);
        pArmR = Limb(police, new Vector3(0.4f, 1.2f, 0), new Vector3(0.15f, 0.5f, 0.15f), new Vector3(0, -0.23f, 0), navy);

        Prim.Make(PrimitiveType.Capsule, police, new Vector3(0, 0.88f, 0), new Vector3(0.6f, 0.4f, 0.42f), navy);
        Prim.Tube(police, new Vector3(0, 0.72f, 0), new Vector3(0.61f, 0.04f, 0.44f), black);
        // reflective vest stripes on the back
        Prim.Box(police, new Vector3(0, 0.95f, 0.22f), new Vector3(0.5f, 0.36f, 0.04f), new Color(1f, 0.9f, 0.2f), 0.4f);
        Prim.Box(police, new Vector3(0, 1.02f, 0.25f), new Vector3(0.5f, 0.06f, 0.03f), Color.white, 0.5f);
        Prim.Box(police, new Vector3(0, 0.86f, 0.25f), new Vector3(0.5f, 0.06f, 0.03f), Color.white, 0.5f);
        // head, cap, siren light
        Prim.Ball(police, new Vector3(0, 1.57f, 0), Vector3.one * 0.46f, skin);
        Prim.Tube(police, new Vector3(0, 1.73f, 0), new Vector3(0.5f, 0.1f, 0.5f), navy);
        Prim.Box(police, new Vector3(0, 1.66f, 0.25f), new Vector3(0.4f, 0.04f, 0.25f), black);
        Prim.Ball(police, new Vector3(0, 1.75f, 0.26f), Vector3.one * 0.09f, gold, 0.5f);
        GameObject siren = Prim.Ball(police, new Vector3(0, 1.92f, 0), Vector3.one * 0.16f, Color.white);
        sirenRenderer = siren.GetComponent<Renderer>();
        sirenRenderer.sharedMaterial = blueLight;
    }

    void BuildDog()
    {
        dog = new GameObject("Dog").transform;
        dog.SetParent(transform, false);
        Color fur = new Color(0.58f, 0.37f, 0.18f);
        Color dark = new Color(0.35f, 0.2f, 0.1f);

        Prim.Box(dog, new Vector3(0, 0.68f, 0), new Vector3(0.4f, 0.4f, 0.9f), fur);
        Prim.Box(dog, new Vector3(0, 0.85f, 0.55f), new Vector3(0.32f, 0.32f, 0.34f), fur);
        Prim.Box(dog, new Vector3(0, 0.79f, 0.78f), new Vector3(0.18f, 0.16f, 0.2f), dark);
        Prim.Ball(dog, new Vector3(0, 0.84f, 0.89f), Vector3.one * 0.08f, Color.black);
        Prim.Box(dog, new Vector3(0, 0.71f, 0.82f), new Vector3(0.07f, 0.05f, 0.12f), new Color(1f, 0.5f, 0.6f));
        Prim.Box(dog, new Vector3(-0.14f, 1.07f, 0.5f), new Vector3(0.08f, 0.2f, 0.1f), dark);
        Prim.Box(dog, new Vector3(0.14f, 1.07f, 0.5f), new Vector3(0.08f, 0.2f, 0.1f), dark);
        Prim.Box(dog, new Vector3(0, 0.8f, 0.4f), new Vector3(0.36f, 0.1f, 0.12f), new Color(0.85f, 0.1f, 0.15f));
        Prim.Ball(dog, new Vector3(0, 0.74f, 0.45f), Vector3.one * 0.07f, new Color(1f, 0.8f, 0.2f), 0.5f);

        dTail = Limb(dog, new Vector3(0, 0.8f, -0.45f), new Vector3(0.08f, 0.32f, 0.08f), new Vector3(0, 0.15f, -0.06f), fur);
        dLegFL = Limb(dog, new Vector3(-0.14f, 0.5f, 0.3f), new Vector3(0.12f, 0.5f, 0.12f), new Vector3(0, -0.25f, 0), dark);
        dLegFR = Limb(dog, new Vector3(0.14f, 0.5f, 0.3f), new Vector3(0.12f, 0.5f, 0.12f), new Vector3(0, -0.25f, 0), dark);
        dLegBL = Limb(dog, new Vector3(-0.14f, 0.5f, -0.3f), new Vector3(0.12f, 0.5f, 0.12f), new Vector3(0, -0.25f, 0), dark);
        dLegBR = Limb(dog, new Vector3(0.14f, 0.5f, -0.3f), new Vector3(0.12f, 0.5f, 0.12f), new Vector3(0, -0.25f, 0), dark);
    }

    Transform Limb(Transform parent, Vector3 pivotPos, Vector3 size, Vector3 offset, Color c)
    {
        Transform pivot = new GameObject("Limb").transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = pivotPos;
        Prim.Box(pivot, offset, size, c);
        return pivot;
    }

    // ------------------------------------------------------------ behaviour
    void SetVisible(bool v)
    {
        if (visible == v) return;
        visible = v;
        police.gameObject.SetActive(v);
        dog.gameObject.SetActive(v);
    }

    void StartChase(Transform pl)
    {
        SetVisible(chaseDuration > 0f);
        policeBehind = 1.5f;     // they start right behind you, then drop back
        dogBehind = 0.8f;
        policeX = pl.position.x + 0.5f;
        dogX = pl.position.x - 0.6f;
        police.position = new Vector3(policeX, 0f, pl.position.z - policeBehind);
        dog.position = new Vector3(dogX, 0f, pl.position.z - dogBehind);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;
        GameState s = gm.State;
        float rt = gm.RunTime;
        bool newRun = s == GameState.Playing && (prevState == GameState.Menu || rt < lastRunTime - 0.01f);
        prevState = s;
        lastRunTime = rt;

        if (s == GameState.Menu) { SetVisible(false); return; }
        Transform pl = gm.player.transform;
        if (newRun) StartChase(pl);
        if (!visible || s == GameState.Paused) return;

        float dt = Time.deltaTime;
        bool chasing = rt < chaseDuration;
        float targetP, targetD, speed;
        if (s == GameState.GameOver) { targetP = 1.6f; targetD = 0.9f; speed = catchUpSpeed; }
        else if (chasing) { targetP = policeDistance; targetD = dogDistance; speed = 3f; }
        else { targetP = 40f; targetD = 40f; speed = retreatSpeed; }

        policeBehind = Mathf.MoveTowards(policeBehind, targetP, speed * dt);
        dogBehind = Mathf.MoveTowards(dogBehind, targetD, speed * dt);

        if (s == GameState.Playing && !chasing && policeBehind > 25f && dogBehind > 25f)
        {
            SetVisible(false);
            return;
        }

        float follow = 1f - Mathf.Exp(-6f * dt);
        policeX = Mathf.Lerp(policeX, pl.position.x + 0.5f, follow);
        dogX = Mathf.Lerp(dogX, pl.position.x - 0.6f, follow);
        float py = Mathf.Lerp(police.position.y, pl.position.y, 10f * dt);
        float dy = Mathf.Lerp(dog.position.y, pl.position.y, 12f * dt);
        police.position = new Vector3(policeX, py, pl.position.z - policeBehind);
        dog.position = new Vector3(dogX, dy, pl.position.z - dogBehind);

        bool running = s == GameState.Playing || Mathf.Abs(policeBehind - targetP) > 0.05f;
        float t = Time.time * 13f;
        float sw = running ? Mathf.Sin(t) * 50f : 0f;
        pLegL.localRotation = Quaternion.Euler(sw, 0, 0);
        pLegR.localRotation = Quaternion.Euler(-sw, 0, 0);
        pArmL.localRotation = Quaternion.Euler(-sw, 0, 0);
        pArmR.localRotation = Quaternion.Euler(sw, 0, 0);
        float dsw = running ? Mathf.Sin(t * 1.15f) * 55f : 0f;
        dLegFL.localRotation = Quaternion.Euler(dsw, 0, 0);
        dLegBR.localRotation = Quaternion.Euler(dsw, 0, 0);
        dLegFR.localRotation = Quaternion.Euler(-dsw, 0, 0);
        dLegBL.localRotation = Quaternion.Euler(-dsw, 0, 0);
        dTail.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 14f) * 30f);

        bool blue = ((int)(Time.time * 6f)) % 2 == 0;
        sirenRenderer.sharedMaterial = blue ? blueLight : redLight;
    }
}
