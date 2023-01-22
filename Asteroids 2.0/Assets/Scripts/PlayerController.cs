using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public delegate void OnHealthChangeCallback();
    public OnHealthChangeCallback onHealthChange;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private SpriteFlash spriteFlash;
    [SerializeField] private Transform muzzleLeft, muzzleRight;
    private GameObject thrusters;
    private bool lastFireFromLeftMuzzle; //Alternaate between the two muzzles

    private bool canAct = true;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float rotationSpeed = 2.5f;
    private bool moveForward;
    private float rotationInput;

    private int maxHealth = 5;
    public int currentHealth { get; private set; }
    [SerializeField] private float damageCooldown = 0.5f;
    private float timeToNextDamage;

    //Weaponry
    private float fireRate = 0.25f;
    private float projectileSpeed = 5;
    private float lastFire;
    private string shooting_01 = "shooting_01", shooting_02 = "shooting_02";
    private string hit01 = "player_hit_01", hit02 = "player_hit_02";

    [SerializeField] private Sprite destroyedShip;
    [SerializeField] private GameObject[] shipPieces;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        spriteFlash = new SpriteFlash(spriteRenderer);

        thrusters = GetComponentInChildren<Animator>().gameObject;
        thrusters.SetActive(false);

        GameManager.instance.onPause += delegate
        {
            canAct = !GameManager.instance.gamePaused;
        };
    }

    private void Update()
    {
        PlayerInput();
        timeToNextDamage -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && currentHealth > 0)
        {
            GameManager.instance.PauseGame();
        }

        //Player is dead, don't accept input
        if (!canAct) return;

        moveForward = Input.GetKey(KeyCode.W);

        if (Input.GetKeyDown(KeyCode.W))
        {
            AudioManager.PlayClip("thruster");
        }

        rotationInput = Input.GetAxisRaw("Horizontal");

        //Fire Projectile
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            FireProjectile();
        }
    }

    private void MovePlayer()
    {
        if (moveForward)
        {
            rb.AddForce(transform.up * movementSpeed, ForceMode2D.Force);
        }
        thrusters.SetActive(moveForward);

        rb.rotation += -rotationInput * rotationSpeed;
    }

    private void FireProjectile()
    {
        if (Time.time > lastFire + fireRate)
        {
            Transform muzzle = muzzleLeft;
            string clip = shooting_01;
            if (lastFireFromLeftMuzzle)
            {
                muzzle = muzzleRight;
                clip = shooting_02;
            }

            var projectile = ObjectPooler.SpawnFromPool_Static("playerMissile", muzzle.position, muzzle.rotation);
            projectile.GetComponent<Rigidbody2D>().velocity = transform.up * projectileSpeed;
            AudioManager.PlayClip(clip);

            lastFire = Time.time;
            lastFireFromLeftMuzzle = !lastFireFromLeftMuzzle;
        }
    }

    public void OnDamage(int dmg)
    {
        if (timeToNextDamage > 0 || currentHealth <= 0) return;

        currentHealth -= dmg;
        StartCoroutine(spriteFlash.Flash());
        timeToNextDamage = damageCooldown;

        string clip = hit01;
        if (Random.value > 0.5) clip = hit02;
        AudioManager.PlayClip(clip);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath();
        }

        onHealthChange?.Invoke();
    }

    private void OnDeath()
    {
        canAct = false;
        moveForward = false;
        rotationInput = 0;
        thrusters.SetActive(false);

        AudioManager.PlayClip("destroyed");
        var explosion = ObjectPooler.SpawnFromPool_Static("explosion_01", transform.position, Quaternion.identity);
        explosion.transform.SetParent(transform);

        GetComponent<BoxCollider2D>().enabled = false;

        spriteRenderer.sprite = destroyedShip;
        //Spawn destroyed ship pieces
        for (int i = 0; i < shipPieces.Length; i++)
        {
            var go = Instantiate(shipPieces[i], transform.position, Quaternion.identity);

            var x = Mathf.Cos(2 * Mathf.PI * i / shipPieces.Length);
            var y = Mathf.Sin(2 * Mathf.PI * i / shipPieces.Length);
            var direction = new Vector3(x, y, 0);
            go.GetComponent<Rigidbody2D>().AddForce(direction * 5);
        }

        GameManager.instance.onGameOver?.Invoke();
    }
}
