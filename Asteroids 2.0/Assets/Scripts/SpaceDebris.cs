using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceDebris : Asteroid
{
    [SerializeField] private int healthOverride = 20, pointsOverride = 5;

    public override void OnObjectSpawn()
    {
        base.OnObjectSpawn();
        health = healthOverride;
        points = pointsOverride;
    }
}
