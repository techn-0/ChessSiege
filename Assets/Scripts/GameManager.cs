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
    [Tooltip("플레이어의 초기 골드")]
    public int playerStartingGold = 500;
    [Tooltip("플레이어의 초기 목재")]
    public int playerStartingWood = 300;
    [Tooltip("플레이어의 초기 식량")]
    public int playerStartingFood = 200;
    [Tooltip("플레이어 자원 최대치")]
    public int playerMaxGold = 1000;
    public int playerMaxWood = 500;
    public int playerMaxFood = 400;
    [Tooltip("플레이어 자원 회복 속도 (초당)")]
    public int playerRegenGoldPerSecond = 1;
    public int playerRegenWoodPerSecond = 1;
    public int playerRegenFoodPerSecond = 1;

    [Header("---- 적 자원(Settings) ----")]
    [Tooltip("적의 초기 골드")]
    public int enemyStartingGold = 500;
    [Tooltip("적의 초기 목재")]
    public int enemyStartingWood = 300;
    [Tooltip("적의 초기 식량")]
    public int enemyStartingFood = 200;
    [Tooltip("적 자원 최대치")]
    public int enemyMaxGold = 1000;
    public int enemyMaxWood = 500;
    public int enemyMaxFood = 400;
    [Tooltip("적 자원 회복 속도 (초당)")]
    public int enemyRegenGoldPerSecond = 1;
    public int enemyRegenWoodPerSecond = 1;
    public int enemyRegenFoodPerSecond = 1;

    // 현재 자원 값 (런타임)
    private int playerGold, playerWood, playerFood;
    private int enemyGold, enemyWood, enemyFood;

    // 외부에서 현재 자원 값을 볼 수 있도록 프로퍼티 제공
    public int PlayerGold => playerGold;
    public int PlayerWood => playerWood;
    public int PlayerFood => playerFood;
    public int EnemyGold => enemyGold;
    public int EnemyWood => enemyWood;
    public int EnemyFood => enemyFood;

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
        playerGold = playerStartingGold;
        playerWood = playerStartingWood;
        playerFood = playerStartingFood;
        enemyGold = enemyStartingGold;
        enemyWood = enemyStartingWood;
        enemyFood = enemyStartingFood;
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
            playerGold = Mathf.Min(playerGold + playerRegenGoldPerSecond, playerMaxGold);
            playerWood = Mathf.Min(playerWood + playerRegenWoodPerSecond, playerMaxWood);
            playerFood = Mathf.Min(playerFood + playerRegenFoodPerSecond, playerMaxFood);
        }
    }

    private IEnumerator RegenEnemyResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            enemyGold = Mathf.Min(enemyGold + enemyRegenGoldPerSecond, enemyMaxGold);
            enemyWood = Mathf.Min(enemyWood + enemyRegenWoodPerSecond, enemyMaxWood);
            enemyFood = Mathf.Min(enemyFood + enemyRegenFoodPerSecond, enemyMaxFood);
        }
    }

    #endregion

    #region 외부 호출용 메서드

    /// <summary>
    /// 플레이어가 유닛을 스폰하기 위해 자원을 소비 시도.
    /// 성공하면 해당 코스트만큼 차감하고 true 반환, 부족하면 false 반환.
    /// </summary>
    /// <param name="goldCost">소비할 골드 자원량</param>
    /// <param name="woodCost">소비할 목재 자원량</param>
    /// <param name="foodCost">소비할 식량 자원량</param>
    public bool TrySpendPlayerResources(int goldCost, int woodCost, int foodCost)
    {
        if (playerGold >= goldCost && playerWood >= woodCost && playerFood >= foodCost)
        {
            playerGold -= goldCost;
            playerWood -= woodCost;
            playerFood -= foodCost;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 적이 유닛을 스폰하기 위해 자원을 소비 시도.
    /// 성공하면 해당 코스트만큼 차감하고 true 반환, 부족하면 false 반환.
    /// </summary>
    /// <param name="goldCost">소비할 골드 자원량</param>
    /// <param name="woodCost">소비할 목재 자원량</param>
    /// <param name="foodCost">소비할 식량 자원량</param>
    public bool TrySpendEnemyResources(int goldCost, int woodCost, int foodCost)
    {
        if (enemyGold >= goldCost && enemyWood >= woodCost && enemyFood >= foodCost)
        {
            enemyGold -= goldCost;
            enemyWood -= woodCost;
            enemyFood -= foodCost;
            return true;
        }
        return false;
    }

    #endregion
}
