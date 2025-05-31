using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 전체를 관리하는 싱글톤 매니저.
/// 플레이어/적 자원 관리, 자원 회복, 스폰 요청 처리 등을 담당.
/// </summary>
public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    [Header("---- 플레이어 자원(Settings) ----")]
    [Tooltip("플레이어의 초기 자원(골드) 값")]
    public int playerStartingResources = 0;
    [Tooltip("플레이어 자원 최대치")]
    public int playerMaxResources = 100;
    [Tooltip("플레이어 자원 회복 속도 (초당)")]
    public int playerRegenPerSecond = 1;

    [Header("---- 적 자원(Settings) ----")]
    [Tooltip("적의 초기 자원(골드) 값")]
    public int enemyStartingResources = 0;
    [Tooltip("적 자원 최대치")]
    public int enemyMaxResources = 100;
    [Tooltip("적 자원 회복 속도 (초당)")]
    public int enemyRegenPerSecond = 1;

    // 현재 자원 값 (런타임)
    private int playerResources;
    private int enemyResources;

    // 외부에서 현재 자원 값을 볼 수 있도록 프로퍼티 제공
    public int PlayerResources => playerResources;
    public int EnemyResources => enemyResources;

    void Awake()
    {
        // 싱글톤 패턴 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기 자원 값 세팅
        playerResources = playerStartingResources;
        enemyResources = enemyStartingResources;
    }

    void Start()
    {
        // 자원 회복 코루틴 시작
        StartCoroutine(RegenPlayerResources());
        StartCoroutine(RegenEnemyResources());
    }

    #region 자원 회복 Coroutine

    private IEnumerator RegenPlayerResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            playerResources = Mathf.Min(playerResources + playerRegenPerSecond, playerMaxResources);
        }
    }

    private IEnumerator RegenEnemyResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            enemyResources = Mathf.Min(enemyResources + enemyRegenPerSecond, enemyMaxResources);
        }
    }

    #endregion

    #region 외부 호출용 메서드

    /// <summary>
    /// 플레이어가 유닛을 스폰하기 위해 자원을 소비 시도.
    /// 성공하면 해당 코스트만큼 차감하고 true 반환, 부족하면 false 반환.
    /// </summary>
    /// <param name="cost">소비할 자원량</param>
    public bool TrySpendPlayerResources(int cost)
    {
        if (playerResources >= cost)
        {
            playerResources -= cost;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 적이 유닛을 스폰하기 위해 자원을 소비 시도.
    /// 성공하면 해당 코스트만큼 차감하고 true 반환, 부족하면 false 반환.
    /// </summary>
    /// <param name="cost">소비할 자원량</param>
    public bool TrySpendEnemyResources(int cost)
    {
        if (enemyResources >= cost)
        {
            enemyResources -= cost;
            return true;
        }
        return false;
    }

    #endregion

    // (옵션) 디버그용: Editor 또는 UI에서 실시간으로 자원 값을 확인하고 싶을 때 사용
    void OnGUI()
    {
        // 화면 좌측 상단에 자원 수치 출력
        GUI.Label(new Rect(10, 10, 200, 20), $"Player Resources: {playerResources}");
        GUI.Label(new Rect(10, 30, 200, 20), $"Enemy Resources: {enemyResources}");
    }
}
