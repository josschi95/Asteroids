using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    private float timeToNextSpawn;
    [SerializeField] private float minTimeBetweenNewAsteroid = 0.5f;
    [SerializeField] private float maxTimeBetweenNewAsteroid = 2f;
    [SerializeField] private bool runTimer = true, debrisOnly;

    private string[] asteroidTags = { "asteroidSmall_0", "asteroidMed_0", "asteroidLarge_0", "_placeHolder_" };
    private string[] debrisTags = { "planet_01", "planet_02", "planet_03", "planet_04", "sat_01", "sat_02", "rocket" };

    private void Start()
    {
        runTimer = true;
        //Set callbacks to stop running the timer when invasion starts
        InvasionManager.instance.onInvasionWarning += delegate { runTimer = false; };
        InvasionManager.instance.onInvasionEnd += delegate { runTimer = true; };
    }

    private void Update()
    {
        RunTimer();
    }

    private void RunTimer()
    {
        if (runTimer)
        {
            timeToNextSpawn -= Time.deltaTime;
            if (timeToNextSpawn <= 0) SpawnAsteroid();
        }
    }

    private void SpawnAsteroid()
    {
        var sizeChance = Random.value;
        int asteroidSize = 0; //35% chance of small asteroid
        if (sizeChance >= 0.95f) asteroidSize = 3; //5% chance of space debris
        else if (sizeChance >= 0.70f) asteroidSize = 2; //25% chance of large asteroid
        else if (sizeChance >= 0.35f) asteroidSize = 1; //35% chance of medium asteroid

        int versionToSpawn = Random.Range(1, 5);
        Vector3 spawnPosition = GameManager.instance.GetSpawnPosition();

        //Don't spawn an asteroid directly next to the player
        if (Vector2.Distance(spawnPosition, GameManager.instance.player.transform.position) < 2.5f) return;

        string newTag = asteroidTags[asteroidSize];
        newTag += versionToSpawn;

        //spawn special debris
        if (asteroidSize == 3 || debrisOnly)
        {
            int index = Random.Range(0, debrisTags.Length - 1);
            newTag = debrisTags[index];
        }

        //spawn an asteroid and point it in the direction of the player
        var newAsteroid = ObjectPooler.SpawnFromPool_Static(newTag, spawnPosition, Quaternion.identity);
        Vector3 directionToPlayer = (GameManager.instance.player.transform.position - spawnPosition).normalized;
        newAsteroid.GetComponent<Asteroid>().SetValues(directionToPlayer, newTag);

        //Reset Timer
        timeToNextSpawn = Random.Range(minTimeBetweenNewAsteroid, maxTimeBetweenNewAsteroid);
    }
}
