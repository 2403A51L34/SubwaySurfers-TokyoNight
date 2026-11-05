using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Transform snowAnchor;
    public Vector3 offset = new Vector3(0f, 4.6f, -8f);
    public float xFollow = 0.65f;
    public float smooth = 7f;
    public float lookAhead = 10f;
    public float baseFov = 60f;
    public float maxFovBonus = 12f;

    Camera cam;
    float shakeTime, shakeStrength;

    void Awake() { cam = GetComponent<Camera>(); }

    Vector3 Desired()
    {
        return new Vector3(target.position.x * xFollow, offset.y + target.position.y * 0.4f, target.position.z + offset.z);
    }

    Quaternion LookRot()
    {
        Vector3 lookAt = new Vector3(target.position.x * 0.85f, 1.2f, target.position.z + lookAhead);
        return Quaternion.LookRotation(lookAt - transform.position);
    }

    public void Snap()
    {
        if (target == null) return;
        transform.position = Desired();
        transform.rotation = LookRot();
        MoveSnow();
    }

    public void Shake(float duration, float strength) { shakeTime = duration; shakeStrength = strength; }

    void LateUpdate()
    {
        if (target == null) return;
        float k = 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime);
        Vector3 desired = Desired();
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, desired.x, k);
        pos.y = Mathf.Lerp(pos.y, desired.y, k);
        pos.z = desired.z;                       // no lag on the forward axis
        if (shakeTime > 0f)
        {
            shakeTime -= Time.unscaledDeltaTime;
            pos += Random.insideUnitSphere * shakeStrength;
        }
        transform.position = pos;
        transform.rotation = Quaternion.Slerp(transform.rotation, LookRot(), k);

        if (cam != null)
        {
            GameManager gm = GameManager.Instance;
            float speedT = (gm != null && gm.State == GameState.Playing) ? Mathf.InverseLerp(10f, 28f, gm.CurrentSpeed) : 0f;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFov + speedT * maxFovBonus, 3f * Time.unscaledDeltaTime);
        }
        MoveSnow();
    }

    void MoveSnow()
    {
        if (snowAnchor != null)
            snowAnchor.position = new Vector3(transform.position.x, 14f, transform.position.z + 22f);
    }
}
