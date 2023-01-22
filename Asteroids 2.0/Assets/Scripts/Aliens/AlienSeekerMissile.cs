using UnityEngine;

public class AlienSeekerMissile : MonoBehaviour, IDamageable, IPooledObject
{
    private PlayerController player;
    private Rigidbody2D rb;
    [SerializeField] private int damage = 1;
    [SerializeField] private float movementSpeed = 2f;
    private float rotationSpeed = 200f;
    private bool isActive;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameManager.instance.player;
    }

    private void FixedUpdate()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        Vector2 direction = ((Vector2)player.transform.position - rb.position).normalized;
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        rb.angularVelocity = -rotateAmount * rotationSpeed;
        rb.velocity = transform.up * movementSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnDamage(damage);
            ReturnToPool();
        }
    }

    public void OnDamage(int dmg)
    {
        if (isActive) ReturnToPool();
    }

    private void ReturnToPool()
    {
        isActive = false;
        AudioManager.PlayClip("alien_ship_destroyed_02");
        ObjectPooler.SpawnFromPool_Static("explosion_02", transform.position, Quaternion.identity);
        ObjectPooler.ReturnToPool_Static("alienSeekerMissile", gameObject);
    }

    public void OnObjectSpawn()
    {
        isActive = true;
    }
}