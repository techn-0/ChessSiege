using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MinionData
{
    public GameObject prefab; // BaseUnit이 붙은 프리팹만 할당 (프리팹 내부에 attackClip 등이 설정되어 있음)
}

public class MinionSpawnManager : MonoBehaviour
{
    [Header("미니언 데이터 목록")]
    [Tooltip("순서대로 미니언 프리팹, 비용(골드/목재/식량), 쿨타임을 설정하세요.")]
    public List<MinionData> minionDataList = new List<MinionData>();

    [Header("단일 스폰 지점 (Spawn Point)")]
    [Tooltip("미니언이 생성될 위치를 할당하세요.")]
    public Transform spawnPoint;

    // 각 미니언 인덱스별 마지막 스폰 시각 기록용 (Time.time 기준)
    private float[] lastSpawnTimes;

    private void Awake()
    {
        if (minionDataList != null && minionDataList.Count > 0)
        {
            lastSpawnTimes = new float[minionDataList.Count];
            for (int i = 0; i < lastSpawnTimes.Length; i++)
            {
                lastSpawnTimes[i] = -Mathf.Infinity;
            }
        }
        else
        {
            Debug.LogWarning("[MinionSpawnManager] 미니언 데이터 리스트(minionDataList)가 비어 있거나 할당되지 않았습니다.");
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[MinionSpawnManager] spawnPoint가 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 외부(UI 버튼 등)에서 호출:
    /// index 번째 미니언을 소환 시도한다.
    /// </summary>
    /// <param name="index">0부터 시작하는 인덱스</param>
    public void SpawnMinionByIndex(int index)
    {
        if (minionDataList == null || index < 0 || index >= minionDataList.Count)
        {
            Debug.LogWarning($"[MinionSpawnManager] SpawnMinionByIndex: 인덱스 {index}가 범위를 벗어났습니다.");
            return;
        }

        GameObject prefab = minionDataList[index].prefab;
        if (prefab == null)
        {
            Debug.LogWarning($"[MinionSpawnManager] SpawnMinionByIndex: minionDataList[{index}]에 Prefab이 설정되지 않았습니다.");
            return;
        }

        BaseUnit unitInfo = prefab.GetComponent<BaseUnit>();
        if (unitInfo == null)
        {
            Debug.LogWarning("프리팹에 BaseUnit 컴포넌트가 없습니다.");
            return;
        }

        float nextAvailableTime = lastSpawnTimes[index] + unitInfo.spawnCooldown;
        if (Time.time < nextAvailableTime)
        {
            float remain = nextAvailableTime - Time.time;
            Debug.Log($"[MinionSpawnManager] {unitInfo.minionName} 은(는) 아직 쿨타임 중입니다. 남은 시간: {remain:F2}초");
            return;
        }

        bool canSpend = GameManager.Instance.TrySpendPlayerResources(unitInfo.costGold, unitInfo.costWood, unitInfo.costFood);
        if (!canSpend)
        {
            Debug.Log("자원 부족");
            return;
        }

        // 프리팹 Instantiate (프리팹 내부에 이미 attackClip, AudioSource가 설정되어 있음)
        GameObject newMinion = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        newMinion.name = prefab.name; // "(Clone)" 접미사 제거

        BaseUnit baseUnitComp = newMinion.GetComponent<BaseUnit>();
        if (baseUnitComp != null)
        {
            // 필요한 경우 적 기지 태그만 재설정 (사운드 관련 설정은 프리팹 Inspector 그대로 사용)
            baseUnitComp.enemyBaseTag = "EnemyBase";
        }
        else
        {
            Debug.LogWarning($"[MinionSpawnManager] 소환된 Prefab({prefab.name})에 BaseUnit 컴포넌트가 없습니다.");
        }

        lastSpawnTimes[index] = Time.time;

        Debug.Log($"[MinionSpawnManager] 미니언 소환 성공: {unitInfo.minionName} (Index {index}) at {spawnPoint.position}");
    }

    public float GetLastSpawnTime(int index)
    {
        if (lastSpawnTimes == null || index < 0 || index >= lastSpawnTimes.Length)
            return -Mathf.Infinity;
        return lastSpawnTimes[index];
    }
}
