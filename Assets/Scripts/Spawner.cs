using UnityEngine;

public class Spawner : MonoBehaviour
{

    [Header("Spawn Settings")]
    [Tooltip("생성할 몹 Prefab (Minion Prefab)")]
    public GameObject minionPrefab;

    [Tooltip("몬스터이 스폰될 기지 위치 (Base Transform)")]
    public Transform spawnPoint;

    [Tooltip("스폰 간격 (Spawn Interval) (초 단위)")]
    public float spawnInterval = 3f;

    [Tooltip("한 번에 최대 스폰할 수 있는 몹 개수 (Optional)")]
    public int maxSpawnCount = 10;

    private float timer = 0f;
    private int spawnedCount = 0;
    void Start()
    {
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // 스폰 개수 제한이 걸렸으면 더 이상 스폰하지 않음
        if (spawnedCount >= maxSpawnCount)
            return;

        // 타이머 누적
        timer += Time.deltaTime;

        // 스폰 타이밍 도달 시
        if (timer >= spawnInterval)
        {
            SpawnMinion();
            timer = 0f;     // 타이머 초기화
            spawnedCount++; // 생성된 몹 수 카운트
        }
    }
      /// <summary>
    /// 기지(spawnPoint) 위치에서 몹(minionPrefab)을 생성 (Instantiate)하는 함수
    /// </summary>
    private void SpawnMinion()
    {
        if (minionPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("EnemySpawner 오류: minionPrefab 또는 spawnPoint가 할당되지 않음!");
            return;
        }

        // Instantiate를 사용해 몹 생성 (Quaternion.identity: 기본 회전값)
        GameObject newEnemy = Instantiate(minionPrefab, spawnPoint.position, Quaternion.identity);

        // 생성된 몹을 계층 구조 아래에 정리(Optional)
        newEnemy.transform.parent = null; // 예: null로 하면 루트(최상위)에 생성
        // 또는: newEnemy.transform.parent = this.transform; 로 하면 스폰 시스템 아래에 생성

        // Debug.Log를 통해 확인
        Debug.Log($"[EnemySpawner] 몹 스폰됨: {newEnemy.name} at {spawnPoint.position}");
    }
}
