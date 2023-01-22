using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour, IDamageable, IPooledObject
{
    private Rigidbody2D rb;
    private SpriteFlash spriteFlash;
    [SerializeField] private AsteroidSize size;
    [SerializeField] private float movementSpeed = 1;
    private string hit1 = "asteroid_hit_01", hit2 = "asteroid_hit_02"; //clips to play when the asteroid is hit

    protected int health; //Damage required to destroy
    protected int points; //Points gained upon being destroyed

    private bool isActive = false; //used to prevent onTriggerEnter being called when the gameObject is inactive
    private string poolTag; //The tag used to call/return the gameObject from the object pooler

    private float secondsToDespawn = 25; //delay before returning the asteroid to the pool
    private Coroutine despawnCoroutine; //Automatically return the asteroid to the pool after 60 seconds

    private void Awake()
    {
        spriteFlash = new SpriteFlash(GetComponent<SpriteRenderer>());
        rb = GetComponent<Rigidbody2D>();
    }

    //This method is called when the prefab is spawned from the objectPooler
    public virtual void OnObjectSpawn()
    {
        switch (size)
        {
            case AsteroidSize.Small:
                //poolTag = "asteroidSmall_0";
                health = 5;
                points = 1;
                break;
            case AsteroidSize.Medium:
                //poolTag = "asteroidMed_0";
                health = 10;
                points = 2;
                break;
            case AsteroidSize.Large:
                //poolTag = "asteroidLarge_0";
                health = 25;
                points = 3;
                break;
        }
        isActive = true;
        if (spriteFlash == null)
            spriteFlash = new SpriteFlash(GetComponent<SpriteRenderer>());
        spriteFlash.OnStart();

        if (despawnCoroutine != null) StopCoroutine(despawnCoroutine);
        despawnCoroutine = StartCoroutine(TimeToDespawn());
    }

    //Set the direction of movement for the asteroid
    public void SetValues(Vector3 direction, string newTag)
    {
        rb.velocity = direction * movementSpeed;
        rb.rotation = Random.Range(0f, 360f);
        poolTag = newTag;
    }

    //Detect collisions with the player
    //Unsure yet if asteroids will collide with enemy ships as well
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnDamage(1 + (int)size);
            OnPlayerCollision();
        }
    }

    //The asteroid collided with the player, Destroy/break it
    private void OnPlayerCollision()
    {
        OnDeath();
    }

    //The asteroid is hit by a missile
    public void OnDamage(int dmg)
    {
        if (!isActive) return;

        health -= dmg;

        string clip = hit1;
        if (Random.value > 0.5) clip = hit2;
        AudioManager.PlayClip(clip);

        if (health <= 0)
        {
            GameManager.OnPlayerPointsGained(points);
            OnDeath();
        }
        else StartCoroutine(spriteFlash.Flash());
    }

    //The asteroid's health was reduced to 0
    public void OnDeath()
    {
        if (size != AsteroidSize.Small)
        {
            SpawnMoreAsteroids();
        }
        ObjectPooler.SpawnFromPool_Static("explosion_01", transform.position, Quaternion.identity);
        ReturnToPool();
    }

    //Deactivate the asteroid and return it to the object pooler
    private void ReturnToPool()
    {
        if (despawnCoroutine != null) StopCoroutine(despawnCoroutine);

        isActive = false;
        rb.velocity = Vector2.zero;
        ObjectPooler.ReturnToPool_Static(poolTag, gameObject);
    }

    //Spawn smaller asteroids when a medium/large asteroid is destroyed
    private void SpawnMoreAsteroids()
    {
        for (int i = 0; i < 3; i++)
        {
            int num = Random.Range(1, 5);
            string newTag = "asteroidSmall_0";
            if (size == AsteroidSize.Large) newTag = "asteroidMed_0";

            newTag += num;
            GameObject newAsteroid = ObjectPooler.SpawnFromPool_Static(newTag, transform.position, Quaternion.identity);

            var x = Mathf.Cos(2 * Mathf.PI * i / 3);
            var y = Mathf.Sin(2 * Mathf.PI * i / 3);
            var direction = new Vector3(x, y, 0);
            newAsteroid.GetComponent<Asteroid>().SetValues(direction, newTag);
        }
    }

    private IEnumerator TimeToDespawn()
    {
        yield return new WaitForSeconds(secondsToDespawn);
        ReturnToPool();
    }
}

public enum AsteroidSize { Small, Medium, Large }
