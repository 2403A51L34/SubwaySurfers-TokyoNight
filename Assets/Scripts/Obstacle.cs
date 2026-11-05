using UnityEngine;

public enum ObstacleType { Crate, Barrier, IceBlock, Snowman, GiftStack, Train, Snowplow, MarketStall, Banner }

// Low  = jump over it.  High = slide under it.  Full = change lanes.
public enum ObstacleKind { Low, High, Full }

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    public ObstacleKind kind = ObstacleKind.Full;
    public float moveSpeed = 0f;          // > 0 for moving trains (they drive toward the player)
    public float activateDistance = 30f;  // train starts moving when this close to the player

    void Update()
    {
        if (moveSpeed <= 0f) return;
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing || gm.player == null) return;
        if (transform.position.z - gm.player.transform.position.z < activateDistance)
            transform.position += Vector3.back * (moveSpeed * Time.deltaTime);
    }

    public void Smash() { gameObject.SetActive(false); }
}
