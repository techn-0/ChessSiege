using UnityEngine;

// 새 유닛 타입 열거형 추가 (기획서에 맞게 필요 시 수정)
public enum UnitType
{
    Infantry,  // 보병
    Archer,    // 궁병
    Cavalry    // 기병
}

public enum UnitState
{
    Idle,
    Move,
    Attack
}

public class BaseUnit : MonoBehaviour
{
    [Header("------ 미니언 정보(Minion Info) ------")]
    public string minionName = "Minion";
    // 기존 cost 필드는 제거하고, 다중 자원 비용으로 교체
    public int costGold = 1;
    public int costWood = 1;
    public int costFood = 1;
    public float spawnCooldown = 1.0f;
    public float attackCooldown = 1.0f;

    [Header("------ 유닛 속성(Attributes) ------")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public int damage = 10;
    public UnitType unitType;  // 유닛 타입 (상성 처리를 위한)

    private float lastAttackTime;

    [Header("---- 적(Enemy) 탐지용 설정 ----")]
    [Tooltip("이 유닛이 공격할 대상(적) 레이어를 설정하세요.")]
    public LayerMask enemyLayerMask;
    [Tooltip("이 유닛이 공격할 적 기지(Enemy Base) 오브젝트의 태그를 지정하세요.")]
    public string enemyBaseTag = "EnemyBase";

    protected UnitState currentState = UnitState.Move;
    protected Transform targetBase;

    [Header("------ 사운드 설정(Sound) ------")]
    public AudioClip attackClip;
    private AudioSource audioSource;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        lastAttackTime = -Mathf.Infinity;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        GameObject baseObj = GameObject.FindGameObjectWithTag(enemyBaseTag);
        if (baseObj != null)
            targetBase = baseObj.transform;
        else
            Debug.LogWarning($"[{name}] Awake(): 태그 '{enemyBaseTag}'인 기지를 찾을 수 없습니다.");
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case UnitState.Move:
                HandleMove();
                DetectAndSwitchState();
                break;
            case UnitState.Attack:
                HandleAttack();
                DetectAndSwitchState();
                break;
            case UnitState.Idle:
                // 필요 시 구현
                break;
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    #region 상태 전환 및 감지 처리

    protected virtual void DetectAndSwitchState()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, enemyLayerMask);
        if (hit != null)
        {
            currentState = UnitState.Attack;
            return;
        }
        currentState = UnitState.Move;
    }
    #endregion

    #region 이동 처리

    protected virtual void HandleMove()
    {
        Vector3 moveDir = Vector3.right;
        if (gameObject.layer == LayerMask.NameToLayer("EnemyUnit"))
            moveDir = Vector3.left;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }
    #endregion

    #region 공격 처리

    protected virtual void HandleAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayerMask);
        if (hits.Length == 0)
        {
            currentState = UnitState.Move;
            return;
        }

        Collider2D nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var col in hits)
        {
            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col;
            }
        }

        if (nearest != null)
        {
            // 만약 공격 대상이 BaseUnit 인 경우 상성 배수를 적용한다.
            BaseUnit targetUnit = nearest.GetComponent<BaseUnit>();
            if (targetUnit != null)
            {
                float multiplier = GetAdvantageMultiplier(targetUnit);
                targetUnit.TakeDamage(Mathf.RoundToInt(damage * multiplier));
            }
            else
            {
                // 대상이 유닛이 아니라 기지인 경우
                GameObject obj = nearest.gameObject;
                EnemyBase baseScript = obj.GetComponent<EnemyBase>();
                if (baseScript == null)
                    baseScript = obj.GetComponentInParent<EnemyBase>();
                if (baseScript != null)
                    baseScript.TakeDamage(damage);
                else
                {
                    PlayerBase playerBaseScript = obj.GetComponent<PlayerBase>();
                    if (playerBaseScript == null)
                        playerBaseScript = obj.GetComponentInParent<PlayerBase>();
                    if (playerBaseScript != null)
                        playerBaseScript.TakeDamage(damage);
                }
            }

            if (attackClip != null && audioSource != null)
                audioSource.PlayOneShot(attackClip);
        }

        lastAttackTime = Time.time;
    }

    // 상성 배수 계산: 기획서에 따라 공격자가 특정 타입이면 1.2배 데미지 적용
    protected float GetAdvantageMultiplier(BaseUnit target)
    {
        // 예시 규칙:
        // 보병(Infantry)은 궁병(Archer)에 대해 1.2배, 
        // 궁병(Archer)은 기병(Cavalry)에 대해 1.2배, 
        // 기병(Cavalry)은 보병(Infantry)에 대해 1.2배
        if (unitType == UnitType.Infantry && target.unitType == UnitType.Archer)
            return 1.2f;
        else if (unitType == UnitType.Archer && target.unitType == UnitType.Cavalry)
            return 1.2f;
        else if (unitType == UnitType.Cavalry && target.unitType == UnitType.Infantry)
            return 1.2f;
        return 1f;
    }
    #endregion

    // 디버깅용 사거리 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
