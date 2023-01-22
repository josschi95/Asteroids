using System.Collections;
using UnityEngine;

public class PlayerMissile : MonoBehaviour, IPooledObject
{
    [SerializeField] private int damage = 5;
    private float timeToDespawn = 5;
    private bool isActive = false;

    private void Update()
    {
        WaitToDespawn();
    }

    private void WaitToDespawn()
    {
        if (!isActive) return;
        timeToDespawn -= Time.deltaTime;
        if (timeToDespawn <= 0) ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        var other = collision.gameObject.GetComponent<IDamageable>();
        if (other != null && !collision.gameObject.CompareTag("Player"))
        {
            other.OnDamage(damage);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        isActive = false;
        ObjectPooler.ReturnToPool_Static("playerMissile", gameObject);
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        timeToDespawn = 5;
    }
}
