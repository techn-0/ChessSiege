using UnityEngine;

public enum UnitState
{
    Idle,       // 대기 상태 (필요 시)
    Move,       // 전진 중
    Attack      // 공격 중
}

public class BaseUnit : MonoBehaviour
{
    [Header("Unit Attributes")]
    public int cost = 1;                // 스폰 코스트 (spawn cost)
    public int maxHealth = 100;         // 최대 체력 (max health)
    [SerializeField] private int currentHealth; // 현재 체력 (current health)

    public float moveSpeed = 2f;        // 이동속도 (movement speed)
    public float attackRange = 1.5f;    // 사거리 (attack range)
    public int damage = 10;             // 공격력 (damage) ← 추가

    [Header("Attack Settings")]
    public float attackCooldown = 1.0f; // 공격 쿨타임 (attack cooldown)
    private float lastAttackTime;       // 마지막 공격 시각 (time of last attack)

    protected UnitState currentState = UnitState.Move;  // 초기 상태: 전진
    protected Transform targetBase;   // 적 기지 위치 (target base)

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        lastAttackTime = -Mathf.Infinity;
        // 적 기지(Enemy Base)는 미리 씬에 배치해 두고, 태그 등을 통해 참조합니다.
        GameObject baseObj = GameObject.FindGameObjectWithTag("EnemyBase");
        if (baseObj != null)
            targetBase = baseObj.transform;
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

    // 피해를 입힐 때 호출 (예: 다른 스크립트에서)
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        // 현재는 단순히 Destroy, 애니메이션 후 파괴 등으로 확장 가능
        Destroy(gameObject);
    }

    #region 상태 전환 및 감지 처리

    // 적(Unit) 또는 적 기지를 사거리(attackRange) 내에서 감지하여 상태 전환
    protected virtual void DetectAndSwitchState()
    {
        // 범위 내에 EnemyUnit 레이어에 속한 오브젝트가 있는지 검사
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, LayerMask.GetMask("EnemyUnit"));
        if (hit != null)
        {
            currentState = UnitState.Attack;
            return;
        }

        // 사거리 내 적이 없으면 전진 상태 유지
        currentState = UnitState.Move;
    }

    #endregion

    #region 이동 처리

    // 횡스크롤 게임이므로 단일 방향(예: 오른쪽)으로만 이동
    protected virtual void HandleMove()
    {
        Vector3 moveDir = Vector3.right;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }

    #endregion

    #region 공격 처리

    // 공격(Attack) 처리 로직
    protected virtual void HandleAttack()
    {
        // 공격 쿨타임 검사
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        // 사거리 내 EnemyUnit 레이어 오브젝트 중 가장 가까운 것 찾기
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("EnemyUnit"));
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
            // 유닛이면 데미지
            var targetUnit = nearest.GetComponent<BaseUnit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(damage);
            }
            else
            {
                // 성이면 데미지
                var baseScript = nearest.GetComponent<EnemyBase>();
                if (baseScript == null)
                    baseScript = nearest.GetComponentInParent<EnemyBase>();

                if (baseScript != null)
                {
                    baseScript.TakeDamage(damage);
                }
            }
        }

        lastAttackTime = Time.time;
    }

    #endregion

    // 사거리 시각화(디버그용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
