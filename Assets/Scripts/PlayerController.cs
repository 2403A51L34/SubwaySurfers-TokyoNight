using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 16f;
    public float jumpVelocity = 9.5f;
    public float gravity = -30f;
    public float slideDuration = 0.8f;
    public float swipeThreshold = 60f;   // pixels

    public bool IsDead { get; private set; }

    int lane = 1;                 // 0 = left, 1 = middle, 2 = right
    float verticalVelocity;
    bool grounded = true;
    bool sliding;
    bool slideQueued;
    float slideTimer;
    float invulnerableUntil;
    Vector2 swipeStart;
    bool swipeActive;

    BoxCollider box;
    Transform model, legL, legR, armL, armR, bubble;
    Renderer bubbleRenderer;
    Material shieldMat, boostMat;

    void Awake()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        SetSlideCollider(false);
        BuildModel();
        BuildBubble();
    }

    // ---------------------------------------------------------------- model
    void BuildModel()
    {
        model = new GameObject("Model").transform;
        model.SetParent(transform, false);
        Color coat = new Color(0.85f, 0.1f, 0.15f);
        Color pants = new Color(0.15f, 0.2f, 0.4f);
        Color skin = new Color(1f, 0.8f, 0.65f);
        Color green = new Color(0.08f, 0.55f, 0.25f);
        Color gold = new Color(1f, 0.8f, 0.2f);

        legL = MakeLimb("LegL", new Vector3(-0.17f, 0.5f, 0), new Vector3(0.2f, 0.5f, 0.22f), new Vector3(0, -0.25f, 0), pants);
        legR = MakeLimb("LegR", new Vector3(0.17f, 0.5f, 0), new Vector3(0.2f, 0.5f, 0.22f), new Vector3(0, -0.25f, 0), pants);
        armL = MakeLimb("ArmL", new Vector3(-0.38f, 1.15f, 0), new Vector3(0.14f, 0.45f, 0.14f), new Vector3(0, -0.22f, 0), coat);
        armR = MakeLimb("ArmR", new Vector3(0.38f, 1.15f, 0), new Vector3(0.14f, 0.45f, 0.14f), new Vector3(0, -0.22f, 0), coat);

        Prim.Make(PrimitiveType.Capsule, model, new Vector3(0, 0.85f, 0), new Vector3(0.55f, 0.38f, 0.4f), coat);
        Prim.Tube(model, new Vector3(0, 0.7f, 0), new Vector3(0.56f, 0.04f, 0.42f), new Color(0.1f, 0.1f, 0.12f));
        Prim.Tube(model, new Vector3(0, 1.22f, 0), new Vector3(0.5f, 0.07f, 0.4f), green);
        Prim.Box(model, new Vector3(0.12f, 1.05f, 0.2f), new Vector3(0.14f, 0.4f, 0.06f), green);
        Prim.Ball(model, new Vector3(0, 1.5f, 0), Vector3.one * 0.44f, skin);
        Prim.Ball(model, new Vector3(0, 1.66f, 0), new Vector3(0.46f, 0.3f, 0.46f), coat);
        Prim.Tube(model, new Vector3(0, 1.56f, 0), new Vector3(0.48f, 0.05f, 0.48f), Color.white);
        Prim.Ball(model, new Vector3(0, 1.86f, 0.1f), Vector3.one * 0.15f, Color.white, 0.2f);
        // gift sack on the back
        Prim.Box(model, new Vector3(0, 0.95f, 0.28f), new Vector3(0.35f, 0.4f, 0.2f), green);
        Prim.Box(model, new Vector3(0, 0.95f, 0.28f), new Vector3(0.37f, 0.06f, 0.22f), gold, 0.3f);
    }

    Transform MakeLimb(string name, Vector3 pivotPos, Vector3 size, Vector3 offset, Color c)
    {
        Transform pivot = new GameObject(name).transform;
        pivot.SetParent(model, false);
        pivot.localPosition = pivotPos;
        Prim.Box(pivot, offset, size, c);
        return pivot;
    }

    void BuildBubble()
    {
        GameObject b = Prim.Ball(transform, new Vector3(0, 0.9f, 0), new Vector3(2.2f, 2.4f, 2.2f), Color.white);
        bubble = b.transform;
        bubbleRenderer = b.GetComponent<Renderer>();
        shieldMat = Prim.TransparentMat(new Color(0.5f, 0.85f, 1f, 0.28f));
        boostMat = Prim.TransparentMat(new Color(1f, 0.6f, 0.2f, 0.28f));
        b.SetActive(false);
    }

    // ---------------------------------------------------------------- reset
    public void ResetPlayer()
    {
        transform.position = Vector3.zero;
        lane = 1; verticalVelocity = 0f; grounded = true;
        sliding = false; slideQueued = false; slideTimer = 0f;
        invulnerableUntil = 0f; IsDead = false;
        SetSlideCollider(false);
        model.localScale = Vector3.one;
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;
    }

    // ---------------------------------------------------------------- update
    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        bool playing = gm.State == GameState.Playing;
        if (playing)
        {
            ReadInput();
            Move(gm.CurrentSpeed);
        }
        Animate(playing);
        UpdateBubble(gm);
    }

    void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) ChangeLane(1);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Jump();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Slide();

        // Swipe (mouse drag on PC, finger on phone - touches are simulated as mouse by Unity)
        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
            swipeActive = true;
        }
        else if (Input.GetMouseButton(0) && swipeActive)
        {
            Vector2 d = (Vector2)Input.mousePosition - swipeStart;
            if (d.magnitude >= swipeThreshold)
            {
                swipeActive = false;
                if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) ChangeLane(d.x > 0f ? 1 : -1);
                else if (d.y > 0f) Jump();
                else Slide();
            }
        }
        if (Input.GetMouseButtonUp(0)) swipeActive = false;
    }

    void Move(float speed)
    {
        float dt = Time.deltaTime;
        Vector3 p = transform.position;
        p.z += speed * dt;
        p.x = Mathf.MoveTowards(p.x, (lane - 1) * laneDistance, laneChangeSpeed * dt);

        if (!grounded)
        {
            verticalVelocity += gravity * dt;
            p.y += verticalVelocity * dt;
            if (p.y <= 0f)
            {
                p.y = 0f;
                verticalVelocity = 0f;
                grounded = true;
                if (slideQueued) { slideQueued = false; StartSlide(); }
            }
        }
        transform.position = p;

        if (sliding)
        {
            slideTimer -= dt;
            if (slideTimer <= 0f) EndSlide();
        }
    }

    void ChangeLane(int dir)
    {
        int newLane = Mathf.Clamp(lane + dir, 0, 2);
        if (newLane == lane) return;
        lane = newLane;
        Sound(Sfx.Slide);
    }

    void Jump()
    {
        if (!grounded) return;
        if (sliding) EndSlide();
        verticalVelocity = jumpVelocity;
        grounded = false;
        Sound(Sfx.Jump);
    }

    void Slide()
    {
        if (grounded) { if (!sliding) StartSlide(); }
        else { verticalVelocity = -22f; slideQueued = true; }   // fast-fall, then slide on landing
    }

    void StartSlide()
    {
        sliding = true;
        slideTimer = slideDuration;
        SetSlideCollider(true);
        Sound(Sfx.Slide);
    }

    void EndSlide()
    {
        sliding = false;
        SetSlideCollider(false);
    }

    void SetSlideCollider(bool slide)
    {
        box.center = slide ? new Vector3(0, 0.4f, 0) : new Vector3(0, 0.9f, 0);
        box.size = slide ? new Vector3(0.8f, 0.8f, 0.6f) : new Vector3(0.8f, 1.7f, 0.6f);
    }

    // ---------------------------------------------------------------- visuals
    void Animate(bool running)
    {
        float dt = Time.deltaTime;
        if (IsDead)
        {
            model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(-80f, 0f, 0f), 10f * Time.unscaledDeltaTime);
            return;
        }
        bool legsMove = running && grounded && !sliding;
        float t = Time.time * 12f;
        float swing = legsMove ? Mathf.Sin(t) * 45f : 0f;
        legL.localRotation = Quaternion.Euler(swing, 0, 0);
        legR.localRotation = Quaternion.Euler(-swing, 0, 0);
        armL.localRotation = Quaternion.Euler(-swing, 0, 0);
        armR.localRotation = Quaternion.Euler(swing, 0, 0);

        float bob = legsMove ? Mathf.Abs(Mathf.Sin(t)) * 0.08f : 0f;
        model.localPosition = new Vector3(0, bob, 0);

        Vector3 s = model.localScale;
        s.y = Mathf.Lerp(s.y, sliding ? 0.5f : 1f, 20f * dt);
        model.localScale = s;

        float roll = running ? Mathf.Clamp((transform.position.x - (lane - 1) * laneDistance) * 10f, -25f, 25f) : 0f;
        model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(0, 0, roll), 15f * dt);
    }

    void UpdateBubble(GameManager gm)
    {
        bool show = gm.State == GameState.Playing && (gm.ShieldActive || gm.BoostActive) && bubbleRenderer != null;
        if (bubble.gameObject.activeSelf != show) bubble.gameObject.SetActive(show);
        if (show) bubbleRenderer.sharedMaterial = gm.BoostActive ? boostMat : shieldMat;
    }

    // ---------------------------------------------------------------- collisions
    void OnTriggerEnter(Collider other) { HandleContact(other); }
    void OnTriggerStay(Collider other) { HandleContact(other); }

    void HandleContact(Collider other)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing || IsDead) return;

        Collectible c = other.GetComponentInParent<Collectible>();
        if (c != null) { c.Collect(); return; }

        Obstacle o = other.GetComponentInParent<Obstacle>();
        if (o == null) return;
        if (Time.time < invulnerableUntil) return;

        if (gm.BoostActive) { o.Smash(); Sound(Sfx.Hit); return; }         // boost = smash through
        if (gm.ShieldActive)                                                // shield absorbs one hit
        {
            gm.ConsumeShield();
            o.Smash();
            invulnerableUntil = Time.time + 0.8f;
            return;
        }
        IsDead = true;
        gm.GameOver();
    }

    static void Sound(Sfx s)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(s);
    }
}
