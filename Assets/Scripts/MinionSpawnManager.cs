using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용 시 필요
using System.Collections.Generic;

public class MinionSpawnManager : MonoBehaviour
{
    [Header("미니언 프리팹 목록 (List of Minion Prefabs)")]
    [Tooltip("인스펙터에 드래그하여 소환할 미니언 Prefab을 추가하세요.")]
    public List<GameObject> minionPrefabs = new List<GameObject>();

    [Header("스폰 지점 (Spawn Point)")]
    [Tooltip("미니언이 생성될 위치 Transform을 할당하세요.")]
    public Transform spawnPoint;

    /// <summary>
    /// 인스펙터 상의 minionPrefabs 리스트에 순서대로 미니언을 저장해주세요.
    /// 버튼 클릭 시, 해당 인덱스를 전달하여 소환합니다.
    /// </summary>
    /// <param name="index">minionPrefabs 리스트에서 몇 번째 미니언을 소환할지 인덱스(0부터 시작)</param>
    public void SpawnMinionByIndex(int index)
    {
        // 인덱스 범위 체크
        if (index < 0 || index >= minionPrefabs.Count)
        {
            Debug.LogWarning($"SpawnMinionByIndex 오류: index {index}가 범위를 벗어남");
            return;
        }

        GameObject prefabToSpawn = minionPrefabs[index];
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"SpawnMinionByIndex 오류: minionPrefabs[{index}]가 할당되지 않음");
            return;
        }

        // 스폰 지점이 없으면 오류 메시지 출력
        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnMinionByIndex 오류: spawnPoint가 할당되지 않음");
            return;
        }

        // Instantiate를 통해 미니언 생성
        GameObject newMinion = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
        newMinion.name = prefabToSpawn.name; // "(Clone)" 접미사 제거 옵션

        // 필요 시 생성된 미니언 초기화 메서드 호출(예: AI 초기화)
        // newMinion.GetComponent<MinionAI>()?.Initialize(...);

        Debug.Log($"[MinionSpawnManager] 미니언 스폰됨: {newMinion.name} at {spawnPoint.position}");
    }
}
