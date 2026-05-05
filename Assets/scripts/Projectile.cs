using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float projectileSpeed = 12f;
    public float projectileLifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, projectileLifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
    }
}