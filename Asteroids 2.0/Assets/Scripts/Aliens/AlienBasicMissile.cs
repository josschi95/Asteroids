using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienBasicMissile : MonoBehaviour, IDamageable, IPooledObject
{
    private Rigidbody2D rb;
    private TrailRenderer trail;
    [SerializeField] private int damage = 1;
    [SerializeField] private float movementSpeed = 30f;
    private float timeToDespawn = 7;
    private bool isActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponentInChildren<TrailRenderer>();
    }

    private void Update()
    {
        DespawnTimer();
    }

    private void DespawnTimer()
    {
        if (isActive)
        {
            timeToDespawn -= Time.deltaTime;
            if (timeToDespawn <= 0) ReturnToPool(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnDamage(damage);
            ReturnToPool(true);
        }
    }

    public void OnDamage(int dmg)
    {
        if (isActive) ReturnToPool(true);
    }

    private void ReturnToPool(bool explode)
    {
        isActive = false;
        if (explode)
        {
            AudioManager.PlayClip("alien_ship_destroyed_01");
            ObjectPooler.SpawnFromPool_Static("explosion_02", transform.position, Quaternion.identity);
        }
        ObjectPooler.ReturnToPool_Static("alienBasicMissile", gameObject);
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        trail.Clear();
        timeToDespawn = 7;
        rb.velocity = -transform.up * movementSpeed;
    }
}
