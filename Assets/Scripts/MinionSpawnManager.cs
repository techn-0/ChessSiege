using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class MinionData
{
    public GameObject prefab; // BaseUnit이 붙은 프리팹만 할당
}

/// <summary>
/// 플레이어가 버튼 클릭 등으로 “미니언(index)”을 소환할 때 사용하는 매니저.
/// GameManager의 자원 시스템과 각 미니언의 코스트/쿨타임을 체크하여 인스턴스화.
/// </summary>
public class MinionSpawnManager : MonoBehaviour
{
    [Header("미니언 데이터 목록")]
    [Tooltip("순서대로 미니언 프리팹, 코스트, 쿨타임을 설정하세요.")]
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
            Debug.LogWarning("프리팹에 BaseUnit이 없습니다.");
            return;
        }

        // 2) 쿨타임(Time.time) 검사: 마지막 스폰 시각 + data.cooldown <= 현재 시간
        float nextAvailableTime = lastSpawnTimes[index] + unitInfo.spawnCooldown;
        if (Time.time < nextAvailableTime)
        {
            float remain = nextAvailableTime - Time.time;
            Debug.Log($"[MinionSpawnManager] {unitInfo.minionName} 은(는) 아직 쿨타임 중입니다. 남은 시간: {remain:F2}초");
            return;
        }

        // 3) GameManager를 통해 플레이어 자원 체크
        bool canSpend = GameManager.Instance.TrySpendPlayerResources(unitInfo.cost);
        if (!canSpend)
        {
            Debug.Log("자원 부족");
            return;
        }

        // 4) 실제 미니언 Instantiate
        GameObject newMinion = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        newMinion.name = prefab.name; // "(Clone)" 접미사 제거

        // 5) 생성된 미니언에 레이어/BaseUnit 세팅
        BaseUnit baseUnitComp = newMinion.GetComponent<BaseUnit>();
        if (baseUnitComp != null)
        {
            // → 미니언이 아군(플레이어 유닛)이므로, 유닛 레이어를 "PlayerUnit"으로 설정
            newMinion.layer = LayerMask.NameToLayer("PlayerUnit");

            // → 공격 대상은 "EnemyUnit" 레이어와 "EnemyBase" 태그를 가진 오브젝트
            baseUnitComp.enemyLayerMask = LayerMask.GetMask("EnemyUnit");
            baseUnitComp.enemyBaseTag = "EnemyBase";
        }
        else
        {
            Debug.LogWarning($"[MinionSpawnManager] 소환된 Prefab({prefab.name})에 BaseUnit 컴포넌트가 없습니다.");
        }

        // 6) 마지막 스폰 시각 업데이트
        lastSpawnTimes[index] = Time.time;

        Debug.Log($"[MinionSpawnManager] 미니언 소환 성공: {unitInfo.minionName} (Index {index}) at {spawnPoint.position}");
    }
}
