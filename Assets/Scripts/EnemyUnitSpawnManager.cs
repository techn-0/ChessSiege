using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyUnitData
{
    public GameObject prefab; // BaseUnit이 붙은 적 유닛 프리팹만 할당 (프리팹 내부에 attackClip 등이 설정되어 있음)
    // 공격 사운드는 프리팹 Inspector에 설정한 값을 그대로 사용합니다.
    // public AudioClip attackClip; // 불필요하므로 제거합니다.
}

public class EnemyUnitSpawnManager : MonoBehaviour
{
    [Header("적 유닛 데이터 목록")]
    [Tooltip("적 유닛 프리팹을 등록하세요.")]
    public List<EnemyUnitData> enemyUnitDataList = new List<EnemyUnitData>();

    [Header("적 유닛 스폰 위치 (보통 적 성 위치)")]
    public Transform enemyBaseSpawnPoint;

    private float[] lastSpawnTimes;
    private float autoSpawnTimer = 0f;
    public float autoSpawnInterval = 5f; // 자동 소환 간격(초)

    // 확률 가중치 배열 (약한/중간/강한 유닛)
    float[] spawnWeights = { 0.6f, 0.3f, 0.1f };

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

        // (1) 쿨타임 체크
        float nextAvailableTime = lastSpawnTimes[index] + unitInfo.spawnCooldown;
        if (Time.time < nextAvailableTime)
        {
            float remain = nextAvailableTime - Time.time;
            Debug.Log($"[EnemyUnitSpawnManager] {unitInfo.minionName} 은(는) 아직 쿨타임 중입니다. 남은 시간: {remain:F2}초");
            return;
        }

        // (2) 적 자원 체크 (골드만 사용)
        bool canSpend = GameManager.Instance.TrySpendEnemyResources(unitInfo.costGold, 0, 0);
        if (!canSpend)
        {
            Debug.Log("적 골드 부족");
            return;
        }

        // (3) 스폰
        GameObject newEnemy = Instantiate(prefab, enemyBaseSpawnPoint.position, Quaternion.identity);
        newEnemy.name = prefab.name; // "(Clone)" 접미사 제거

        // (4) 태그 기반으로 공격 대상 설정 (프리팹의 사운드 등은 Inspector에 설정된 그대로 사용)
        BaseUnit baseUnitComp = newEnemy.GetComponent<BaseUnit>();
        if (baseUnitComp != null)
        {
            // 필요 시, 적 기지 태그만 재설정합니다.
            baseUnitComp.enemyBaseTag = "PlayerBase"; // 또는 "EnemyBase"
        }
        else
        {
            Debug.LogWarning($"[EnemyUnitSpawnManager] 소환된 Prefab({prefab.name})에 BaseUnit 컴포넌트가 없습니다.");
        }

        // (5) 마지막 스폰 시각 업데이트
        lastSpawnTimes[index] = Time.time;
    }

    int GetRandomIndexByWeight(float[] weights)
    {
        float total = 0;
        foreach (var w in weights)
            total += w;
        float rand = Random.Range(0, total);
        float sum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (rand < sum)
                return i;
        }
        return weights.Length - 1;
    }

    void SpawnRandomEnemyUnit()
    {
        int index = GetRandomIndexByWeight(spawnWeights);
        SpawnEnemyUnitByIndex(index);
    }

    void Update()
    {
        // 자동 소환: 일정 시간마다 SpawnRandomEnemyUnit() 호출
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
}
