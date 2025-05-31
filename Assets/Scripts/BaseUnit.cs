using UnityEngine;

// 유닛의 상태
public enum UnitState
{
    Idle,   // 대기
    Move,   // 전진
    Attack  // 공격
}

public class BaseUnit : MonoBehaviour
{
    [Header("------ 미니언 정보(Minion Info) ------")]
    public string minionName = "Minion";
    public int cost = 1;                // 소환 비용
    public float spawnCooldown = 1.0f;  // 소환 쿨타임
    public float attackCooldown = 1.0f; // 공격 쿨타임

    [Header("------ 유닛 속성(Attributes) ------")]
    public int maxHealth = 100;         // 최대 체력
    [SerializeField] private int currentHealth;

    public float moveSpeed = 2f;        // 이동 속도
    public float attackRange = 1.5f;    // 사거리
    public int damage = 10;             // 공격력

    private float lastAttackTime;

    [Header("---- 적(Enemy) 탐지용 설정 ----")]
    [Tooltip("이 유닛이 공격할 대상(적) 레이어를 에디터에서 Drag & Drop 또는 레이어 이름으로 설정하세요.")]
    public LayerMask enemyLayerMask;    // ▶ 공격 대상을 찾기 위한 LayerMask

    [Tooltip("이 유닛이 공격할 적 기지(Enemy Base) 오브젝트의 태그를 지정하세요.")]
    public string enemyBaseTag = "EnemyBase"; 
    // 플레이어 유닛은 적 기지를 'EnemyBase' 태그로 달아 놓고, 
    // 적 유닛은 'PlayerBase' 태그로 달아 둔다고 가정

    protected UnitState currentState = UnitState.Move;
    protected Transform targetBase;       

    [Header("------ 사운드 설정(Sound) ------")]
    public AudioClip attackClip; // 공격 사운드
    private AudioSource audioSource;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        lastAttackTime = -Mathf.Infinity;

        // AudioSource 자동 추가 또는 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // enemyBaseTag에 지정된 태그를 가진 첫 번째 오브젝트를 찾는다.
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
                // 필요하다면 추가 구현
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
        // 1) 적(Unit) 혹은 적(Base)을 attackRange 내에서 검사한다.
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
        // 횡스크롤 단일 방향(오른쪽) 예시
        Vector3 moveDir = Vector3.right;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }

    #endregion

    #region 공격 처리

    protected virtual void HandleAttack()
    {
        // 공격 쿨타임 체크
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        // 1) 공격 대상(적 유닛/적 기지)이 있는지 모두 가져온다.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayerMask);
        if (hits.Length == 0)
        {
            currentState = UnitState.Move;
            return;
        }

        // 2) 가장 가까운(거리 최소) 오브젝트를 찾아낸다.
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

        // 3) nearest가 유닛인지(=BaseUnit) 확인하고, 없다면 기지(=enemyBaseTag)로 가정
        if (nearest != null)
        {
            // 3-1) 유닛(BaseUnit)인 경우
            BaseUnit targetUnit = nearest.GetComponent<BaseUnit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(damage);
            }
            else
            {
                // 3-2) 유닛이 아닌(=기지) : enemyBaseTag로 태그된 오브젝트일 것이라 가정
                GameObject obj = nearest.gameObject;

                // EnemyBase 또는 PlayerBase 모두 체크
                EnemyBase baseScript = obj.GetComponent<EnemyBase>();
                if (baseScript == null)
                    baseScript = obj.GetComponentInParent<EnemyBase>();

                if (baseScript != null)
                {
                    baseScript.TakeDamage(damage);
                }
                else
                {
                    // PlayerBase도 체크
                    PlayerBase playerBaseScript = obj.GetComponent<PlayerBase>();
                    if (playerBaseScript == null)
                        playerBaseScript = obj.GetComponentInParent<PlayerBase>();

                    if (playerBaseScript != null)
                    {
                        playerBaseScript.TakeDamage(damage);
                    }
                }
            }

            // 공격 사운드 재생
            if (attackClip != null && audioSource != null)
                audioSource.PlayOneShot(attackClip);
        }

        lastAttackTime = Time.time;
    }

    #endregion

    // 디버깅용 사거리 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
