using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyUnitData
{
    public GameObject prefab; // BaseUnit이 붙은 적 유닛 프리팹만 할당
}

public class EnemyUnitSpawnManager : MonoBehaviour
{
    [Header("적 유닛 데이터 목록")]
    [Tooltip("적 유닛 프리팹을 등록하세요.")]
    public List<EnemyUnitData> enemyUnitDataList = new List<EnemyUnitData>();

    [Header("적 유닛 스폰 위치(보통 적 성 위치)")]
    public Transform enemyBaseSpawnPoint;

    private float[] lastSpawnTimes;

    private float autoSpawnTimer = 0f;
    public float autoSpawnInterval = 5f; // 자동 소환 간격(초)

    private void Awake()
    {
        if (enemyUnitDataList != null && enemyUnitDataList.Count > 0)
        {
            lastSpawnTimes = new float[enemyUnitDataList.Count];
            for (int i = 0; i < lastSpawnTimes.Length; i++)
                lastSpawnTimes[i] = -Mathf.Infinity;
        }
    }

    public void SpawnEnemyUnitByIndex(int index)
    {
        Debug.Log($"SpawnEnemyUnitByIndex 호출됨: {index}");
        if (enemyUnitDataList == null || index < 0 || index >= enemyUnitDataList.Count)
            return;

        GameObject prefab = enemyUnitDataList[index].prefab;
        if (prefab == null)
            return;

        BaseUnit unitInfo = prefab.GetComponent<BaseUnit>();
        if (unitInfo == null)
            return;

        // 쿨타임 체크
        float nextAvailableTime = lastSpawnTimes[index] + unitInfo.spawnCooldown;
        if (Time.time < nextAvailableTime)
            return;

        // 적 자원 체크
        if (!GameManager.Instance.TrySpendEnemyResources(unitInfo.cost))
        {
            Debug.Log("적 자원 부족");
            return;
        }

        // 스폰
        GameObject newEnemy = Instantiate(prefab, enemyBaseSpawnPoint.position, Quaternion.identity);
        newEnemy.name = prefab.name;

        // 레이어 및 공격 대상 설정
        newEnemy.layer = LayerMask.NameToLayer("EnemyUnit");
        BaseUnit baseUnitComp = newEnemy.GetComponent<BaseUnit>();
        if (baseUnitComp != null)
        {
            baseUnitComp.enemyLayerMask = LayerMask.GetMask("PlayerUnit");
            baseUnitComp.enemyBaseTag = "PlayerBase";
        }

        lastSpawnTimes[index] = Time.time;
    }

    // 예: 테스트용으로 Update에서 1번만 호출
    void Update()
    {
        // 자동 소환
        autoSpawnTimer += Time.deltaTime;
        if (autoSpawnTimer >= autoSpawnInterval)
        {
            autoSpawnTimer = 0f;
            SpawnRandomEnemyUnit();
        }

        // 테스트용 수동 소환
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnEnemyUnitByIndex(0);
        }
    }

    // 예시: 확률 가중치 배열
    float[] spawnWeights = { 0.6f, 0.3f, 0.1f }; // 약한/중간/강한 유닛

    int GetRandomIndexByWeight(float[] weights)
    {
        float total = 0;
        foreach (var w in weights) total += w;
        float rand = Random.Range(0, total);
        float sum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (rand < sum) return i;
        }
        return weights.Length - 1;
    }

    // 사용 예시
    void SpawnRandomEnemyUnit()
    {
        int index = GetRandomIndexByWeight(spawnWeights);
        SpawnEnemyUnitByIndex(index);
    }
}
