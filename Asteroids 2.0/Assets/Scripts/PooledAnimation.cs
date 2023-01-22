using UnityEngine;

public class PooledAnimation : MonoBehaviour, IPooledObject
{
    [SerializeField] private Animator anim;
    public void OnObjectSpawn()
    {
        anim.Play("Enter");
    }
}
