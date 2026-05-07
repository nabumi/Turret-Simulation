using UnityEngine;
using UnityEngine.Pool;


[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private Transform lookAtCenter;

    [Header("스폰 셋팅")]
    [SerializeField] [Min(1)] private int initialSpawnCount = 4;
    [SerializeField] [Min(1)] private int maxAliveCount = 8;
    [SerializeField] [Min(0.1f)] private float spawnIntervalSeconds = 1f;

    [Header("에너미 셋팅")]
    [SerializeField][Min(0.1f)] private float enemyMoveSpeed = 45f;
    [SerializeField][Min(0.1f)] private float enemyLifeTimeSeconds = 10f;

    private ObjectPool<GameObject> pool;
    private float nextSpawnTimeSeconds;

    private void Awake()
    {
        pool = new ObjectPool<GameObject>(
            OnCreateEnemy,            
            OnGetEnemy,
            OnReleaseEnemy,
            OnDestroyEnemy,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 20
            );
    }
    private void Start()
    {
        for (int i = 0; i < initialSpawnCount; i++)
        {
            TrySpawnOneEnemy();
        }
    }
    private void Update()
    {
        if (Time.time < nextSpawnTimeSeconds) return;

        if (pool.CountActive >= maxAliveCount) return;

        if (TrySpawnOneEnemy())
        {
            nextSpawnTimeSeconds = Time.time + spawnIntervalSeconds;
        }
    }

    private bool TrySpawnOneEnemy()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return false;

        pool.Get();
        return true;
    }

    private GameObject OnCreateEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, enemyRoot);
        return enemy;
    }

    private void OnGetEnemy(GameObject enemy)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        enemy.transform.position = spawnPoint.position;

        if (lookAtCenter != null)
        {
            Vector3 toCenter = lookAtCenter.position - spawnPoint.position;
            toCenter.y = 0;
            if (toCenter.sqrMagnitude > 1e-8f)
            {
                enemy.transform.rotation = Quaternion.LookRotation(toCenter.normalized, Vector3.up);
            }
        }

        var mover = enemy.GetComponent<TargetMover>();
        if (mover != null)
        {
            mover.Initialize(enemyMoveSpeed, enemyLifeTimeSeconds, (obj) => pool.Release(obj));
        }
        enemy.SetActive(true);
    }

    private void OnReleaseEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
    }
    private void OnDestroyEnemy(GameObject enemy)
    {
     Destroy(enemy);   
    }
}
