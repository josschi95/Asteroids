using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvasionManager : MonoBehaviour
{
    public static InvasionManager instance;
    private void Awake()
    {
        instance = this;
    }

    public delegate void OnAlienInvasionCallback();
    public OnAlienInvasionCallback onInvasionWarning;
    public OnAlienInvasionCallback onInvasionStart;
    public OnAlienInvasionCallback onInvasionEnd;

    [SerializeField] private float secondsBetweenInvasions = 90;
    [SerializeField] private float invasionDuration = 30;
    [Space]
    [SerializeField] private float minTimeBetweenNewShip = 5f;
    [SerializeField] private float maxTimeBetweenNewShip = 7.5f;

    private float timeToNextInvasion; //time in seconds until next invasion starts
    private float timeLeftInInvasion; //time in seconds until end of invasion
    private float timeToNextShipSpawn; //time in seconds between spawning new ships
    private bool invasionActive; //if there is an ongoing invasion
    private bool warningIssued; //if the warning has been issued to the player via UIManager

    private string[] shipTags = { "cruiser", "screecher", "hunter" };

    private void Start()
    {
        invasionActive = false;
        invasionDuration = 30;
        timeToNextInvasion = secondsBetweenInvasions;
    }

    private void Update()
    {
        AlienInvasionTimer();
    }

    private void AlienInvasionTimer()
    {
        if (invasionActive)
        {
            timeLeftInInvasion -= Time.deltaTime;
            timeToNextShipSpawn -= Time.deltaTime;

            if (timeToNextShipSpawn <= 0)
            {
                SpawnAlienShip();
                SpawnAlienShip();

                //Reset Timer
                timeToNextShipSpawn = Random.Range(minTimeBetweenNewShip, maxTimeBetweenNewShip);
            }
            if (timeLeftInInvasion <= 0) OnInvasionEnd();
        }
        else
        {
            timeToNextInvasion -= Time.deltaTime;

            if (timeToNextInvasion <= 10 && !warningIssued)
            {
                warningIssued = true;
                onInvasionWarning?.Invoke();
            }

            if (timeToNextInvasion <= 0) OnInvasionStart();
        }
    }

    private void OnInvasionStart()
    {
        invasionActive = true;
        warningIssued = false;

        //Reset timer
        timeLeftInInvasion = invasionDuration;
        onInvasionStart?.Invoke();
    }

    private void OnInvasionEnd()
    {
        invasionActive = false;

        //Increase next invasion duration by 10 seconds
        invasionDuration += 10;

        //Reset timer
        timeToNextInvasion = secondsBetweenInvasions;
        
        onInvasionEnd?.Invoke();
    }

    private void SpawnAlienShip()
    {
        var shipTypeChance = Random.value;
        int shipIndex = 0; //Default cruiser, 50% chance
        if (shipTypeChance > 0.8) shipIndex = 2; //20% chance hunter
        else if (shipTypeChance > 0.5f) shipIndex = 1; //30% chance screecher

        Vector3 spawnPosition = GameManager.instance.GetSpawnPosition();
        //Don't spawn a ship directly next to the player
        if (Vector2.Distance(spawnPosition, GameManager.instance.player.transform.position) < 2.5f) return;

        //var angle = Vector2.Angle(spawnPosition, GameManager.instance.player.transform.position);
        //var rot = Quaternion.Euler(0, 0, -angle);
        ObjectPooler.SpawnFromPool_Static(shipTags[shipIndex], spawnPosition, Quaternion.identity);
    }
}
