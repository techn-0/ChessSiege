using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MinionData
{
    public GameObject prefab; // BaseUnit이 붙은 프리팹만 할당
}

/// <summary>
/// 플레이어가 버튼 클릭 등으로 “미니언(index)”을 소환할 때 사용하는 매니저.
/// GameManager의 다중 자원(골드, 목재, 식량) 시스템과 각 미니언의 비용/쿨타임을 체크하여 인스턴스화.
/// </summary>
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
        // minionDataList 개수만큼 lastSpawnTimes 배열 초기화
        if (minionDataList != null && minionDataList.Count > 0)
        {
            lastSpawnTimes = new float[minionDataList.Count];
            // 초기값을 음수 무한대로 설정하여, 처음엔 쿨타임 체크가 항상 통과하도록 함
            for (int i = 0; i < lastSpawnTimes.Length; i++)
            {
                lastSpawnTimes[i] = -Mathf.Infinity;
            }
        }
        else
        {
            Debug.LogWarning("[MinionSpawnManager] 미니언 데이터 리스트(minionDataList)가 비어 있거나 할당되지 않았습니다.");
        }

        // spawnPoint가 할당되지 않았다면 경고
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
        // 1) 인덱스 유효 범위 검사
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

        // 2) 쿨타임(Time.time) 검사: 마지막 스폰 시각 + unitInfo.spawnCooldown <= 현재 시간
        float nextAvailableTime = lastSpawnTimes[index] + unitInfo.spawnCooldown;
        if (Time.time < nextAvailableTime)
        {
            float remain = nextAvailableTime - Time.time;
            Debug.Log($"[MinionSpawnManager] {unitInfo.minionName} 은(는) 아직 쿨타임 중입니다. 남은 시간: {remain:F2}초");
            return;
        }

        // 3) GameManager를 통해 플레이어 자원 체크
        // 업데이트된 기획서에 따라 유닛은 골드, 목재, 식량 비용을 소비하도록 함.
        bool canSpend = GameManager.Instance.TrySpendPlayerResources(unitInfo.costGold, unitInfo.costWood, unitInfo.costFood);
        if (!canSpend)
        {
            Debug.Log("자원 부족");
            return;
        }

        // 실제 미니언 Instantiate
        GameObject newMinion = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        newMinion.name = prefab.name; // "(Clone)" 접미사 제거


        BaseUnit baseUnitComp = newMinion.GetComponent<BaseUnit>();
        if (baseUnitComp != null)
        {
            // 만약 추가 설정이 필요하다면 태그 기반으로 처리
            // 예: baseUnitComp.enemyBaseTag = "EnemyBase";
        }
        else
        {
            Debug.LogWarning($"[MinionSpawnManager] 소환된 Prefab({prefab.name})에 BaseUnit 컴포넌트가 없습니다.");
        }

        // 마지막 스폰 시각 업데이트
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
