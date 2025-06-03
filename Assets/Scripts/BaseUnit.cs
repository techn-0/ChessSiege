using UnityEngine;
using System.Collections;

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

    [Tooltip("이 유닛이 공격할 적 기지(Enemy Base) 오브젝트의 태그를 지정하세요.")]
    public string enemyBaseTag;

    protected UnitState currentState = UnitState.Move;
    protected Transform targetBase;

    [Header("------ 사운드 설정(Sound) ------")]
    public AudioClip attackClip;
    private AudioSource audioSource;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        lastAttackTime = -Mathf.Infinity;

        audioSource = GetComponent<AudioSource>()
                      ?? gameObject.AddComponent<AudioSource>();

        // 공격 대상 기지(Transform) 설정만 하고,
        // 물리 충돌 무시 처리는 Start()로 이동합니다.
        GameObject targetObj = GameObject.FindGameObjectWithTag(enemyBaseTag);
        if (targetObj != null)
            targetBase = targetObj.transform;
        else
            Debug.LogWarning($"[{name}] Awake(): 태그 '{enemyBaseTag}'인 기지를 찾을 수 없습니다.");
    }

    protected virtual void Start()
    {
        StartCoroutine(ApplyCollisionIgnore());
    }

    private IEnumerator ApplyCollisionIgnore()
    {
        // EndOfFrame까지 대기하여 모든 기지가 완전히 초기화되도록 합니다.
        yield return new WaitForEndOfFrame();

        Collider2D unitCollider = GetComponent<Collider2D>();
        if (unitCollider == null)
        {
            Debug.LogWarning($"[{name}] ApplyCollisionIgnore(): 유닛에 Collider2D가 없습니다.");
            yield break;
        }

        // 태그를 통해 자신의 기지 태그 결정
        string ownBaseTag = CompareTag("PlayerUnit") ? "PlayerBase"
                        : CompareTag("EnemyUnit")  ? "EnemyBase"
                        : null;
        if (string.IsNullOrEmpty(ownBaseTag))
        {
            Debug.LogWarning($"[{name}] ApplyCollisionIgnore(): 유닛 태그가 PlayerUnit 또는 EnemyUnit이 아닙니다.");
            yield break;
        }
        Debug.Log($"[{name}] 자신의 기지 태그는 {ownBaseTag}");

        GameObject ownBase = GameObject.FindGameObjectWithTag(ownBaseTag);
        if (ownBase == null)
        {
            Debug.LogWarning($"[{name}] ApplyCollisionIgnore(): '{ownBaseTag}' 오브젝트를 찾을 수 없습니다.");
            yield break;
        }
        Debug.Log($"[{name}] 발견된 기지 오브젝트: {ownBase.name}");

        Collider2D[] baseColliders = ownBase.GetComponentsInChildren<Collider2D>();
        if (baseColliders.Length == 0)
        {
            Debug.LogWarning($"[{name}] ApplyCollisionIgnore(): '{ownBaseTag}'에 Collider2D가 없습니다.");
        }
        else
        {
            Debug.Log($"[{name}] '{ownBaseTag}'에 포함된 Collider 수: {baseColliders.Length}");
            foreach (var baseCol in baseColliders)
            {
                Debug.Log($"[{name}] 무시할 대상 Collider: {baseCol.name}");
                Physics2D.IgnoreCollision(unitCollider, baseCol);
            }
        }
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        bool foundTarget = false;
        foreach (Collider2D col in hits)
        {
            if (CompareTag("PlayerUnit"))
            {
                if (col.CompareTag("EnemyUnit") || col.CompareTag(enemyBaseTag))
                {
                    foundTarget = true;
                    break;
                }
            }
            else if (CompareTag("EnemyUnit"))
            {
                if (col.CompareTag("PlayerUnit") || col.CompareTag(enemyBaseTag))
                {
                    foundTarget = true;
                    break;
                }
            }
        }
        currentState = foundTarget ? UnitState.Attack : UnitState.Move;
    }
    #endregion

    #region 이동 처리
    protected virtual void HandleMove()
    {
        Vector3 moveDir = Vector3.right;
        // 태그로 적 유닛 판단
        if (CompareTag("EnemyUnit"))
            moveDir = Vector3.left;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }
    #endregion

    #region 공격 처리
    protected virtual void HandleAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        if (hits.Length == 0)
        {
            currentState = UnitState.Move;
            return;
        }

        Collider2D nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var col in hits)
        {
            // enemyBaseTag 변수로 기지 태그 검사
            bool isEnemy = CompareTag("PlayerUnit")
                           ? (col.CompareTag("EnemyUnit") || col.CompareTag(enemyBaseTag))
                           : (col.CompareTag("PlayerUnit") || col.CompareTag(enemyBaseTag));
            if (!isEnemy) continue;

            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = col;
            }
        }

        if (nearest != null)
        {
            BaseUnit tu = nearest.GetComponent<BaseUnit>();
            if (tu != null)
            {
                float mul = GetAdvantageMultiplier(tu);
                tu.TakeDamage(Mathf.RoundToInt(damage * mul));
            }
            else
            {
                // 기지일 때는 enemyBaseTag로 구분된 태그에 맞춰 데미지 호출
                var go = nearest.gameObject;
                if (go.CompareTag(enemyBaseTag))
                {
                    var pb = go.GetComponent<PlayerBase>()
                             ?? go.GetComponentInParent<PlayerBase>();
                    var eb = go.GetComponent<EnemyBase>()
                             ?? go.GetComponentInParent<EnemyBase>();
                    if (pb != null) pb.TakeDamage(damage);
                    if (eb != null) eb.TakeDamage(damage);
                }
            }

            if (attackClip != null && audioSource != null)
                audioSource.PlayOneShot(attackClip);
        }
        lastAttackTime = Time.time;
    }
    #endregion

    protected float GetAdvantageMultiplier(BaseUnit target)
    {
        // 예시 규칙:
        // Infantry(보병) → Archer(궁병)에 대해 1.2배
        // Archer(궁병) → Cavalry(기병)에 대해 1.2배
        // Cavalry(기병) → Infantry(보병)에 대해 1.2배
        if (unitType == UnitType.Infantry && target.unitType == UnitType.Archer)
            return 1.2f;
        else if (unitType == UnitType.Archer && target.unitType == UnitType.Cavalry)
            return 1.2f;
        else if (unitType == UnitType.Cavalry && target.unitType == UnitType.Infantry)
            return 1.2f;
        return 1f;
    }

    // 디버깅용 사거리 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
