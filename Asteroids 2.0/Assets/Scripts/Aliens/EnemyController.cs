using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable, IPooledObject
{
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private SpriteFlash spriteFlash;
    private Collider2D coll;

    [SerializeField] private AlienShipType shipType;
    [SerializeField] private Sprite destroyedSprite;
    private Sprite defaultSprite;
    [SerializeField] private Transform muzzle;

    //Movement
    private float movementSpeed = 1;
    private float rotationSpeed = 2.5f;

    private float retreatDistance; //minimum preferred distance to player
    private float stoppingDistance; //maximum preferred distance from player

    //Projectiles
    private string projectileTag;
    private float firingCooldown;
    private float timeToNextShot;

    //Health and Points
    private int maxHealth = 25;
    private int currentHealth;
    private int points;

    //Audio
    private string hit01 = "asteroid_hit_01", hit02 = "asteroid_hit_02"; //clips to play when the ship is hit
    private string destroyed01 = "alien_ship_destroyed_01", destroyed02 = "alien_ship_destroyed_02";
    
    private string poolTag;
    private bool isActive = false;

    private void Awake()
    {
        player = GameManager.instance.player.transform;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteFlash = new SpriteFlash(spriteRenderer);
        defaultSprite = spriteRenderer.sprite;
        coll = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!isActive) return;

        MoveShip();
        FacePlayer();
        HandleFireRate();
    }

    //Use this to set stats, projectile type, and behavior type for each ship
    private void GetShipStats()
    {
        switch (shipType)
        {
            //Grunt
            case AlienShipType.Cruiser:
                poolTag = "cruiser";
                maxHealth = 20;
                points = 10;

                movementSpeed = 1.5f;
                retreatDistance = 2;
                stoppingDistance = 3;

                projectileTag = "alienBasicMissile";
                firingCooldown = 1;
                break;

            //Kamikaze
            case AlienShipType.Screecher:
                poolTag = "screecher";
                maxHealth = 15;
                points = 10;

                movementSpeed = 2;
                retreatDistance = 0;
                stoppingDistance = 0;

                projectileTag = "alienBasicMissile";
                firingCooldown = 4;
                break;

            //Seeker Missile Launcher
            case AlienShipType.Hunter:
                poolTag = "hunter";
                maxHealth = 25;
                points = 15;

                movementSpeed = 1;
                retreatDistance = 3;
                stoppingDistance = 6;

                projectileTag = "alienSeekerMissile";
                firingCooldown = 5;
                break;

            //Boss
            case AlienShipType.MotherShip:
                maxHealth = 150;
                points = 100;

                firingCooldown = 1;

                throw new System.Exception("Not Implemented Yet");
        }

    }

    #region - Movement -
    private void MoveShip()
    {
        //if distance between ship and player is greater than stopping distance, move towards player
        if (Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, movementSpeed * Time.deltaTime);
        }
        //else if the distance between ship and player is less than the retreat distance, move away from the player
        else if (Vector2.Distance(transform.position, player.position) < retreatDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, -movementSpeed * Time.deltaTime);
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotationSpeed);
    }
    #endregion

    #region - Weaponry -
    private void HandleFireRate()
    {
        timeToNextShot -= Time.deltaTime;

        if (timeToNextShot <= 0)
        {
            OnFireProjectile();
        }
    }

    private void OnFireProjectile()
    {
        var go = ObjectPooler.SpawnFromPool_Static(projectileTag, muzzle.position, muzzle.rotation);
        timeToNextShot = firingCooldown;
    }
    #endregion

    #region - Damage/Health -
    public void OnDamage(int dmg)
    {
        if (!isActive) return;

        currentHealth -= dmg;
        StartCoroutine(spriteFlash.Flash());

        string clip = hit01;
        if (Random.value > 0.5) clip = hit02;
        AudioManager.PlayClip(clip);

        if (currentHealth <= 0)
            OnShipDestroyed();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnDamage(1);
            OnShipDestroyed();
        }
    }

    private void OnShipDestroyed()
    {
        isActive = false;
        coll.enabled = false;
        //Swap Sprite
        spriteRenderer.sprite = destroyedSprite;
        //Grant points
        GameManager.OnPlayerPointsGained(points);

        //Play ship destroyed SFX
        string clip = destroyed01;
        if (Random.value > 0.5) clip = destroyed02;
        AudioManager.PlayClip(clip);

        //Explosion Anim
        ObjectPooler.SpawnFromPool_Static("explosion_02", transform.position, Quaternion.identity);

        StartCoroutine(ReturnToPoolDelay());
    }
    #endregion

    #region - Object Pooler -
    public void OnObjectSpawn()
    {
        isActive = true;
        coll.enabled = true;
        GetShipStats();

        spriteRenderer.sprite = defaultSprite;
        currentHealth = maxHealth;
        timeToNextShot = 1;
    }

    private void ReturnToPool()
    {
        ObjectPooler.ReturnToPool_Static(poolTag, gameObject);
    }

    private IEnumerator ReturnToPoolDelay()
    {
        yield return new WaitForSeconds(3);
        ReturnToPool();
    }
    #endregion
}

public enum AlienShipType { Cruiser, Screecher, Hunter, MotherShip }