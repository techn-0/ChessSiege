using UnityEngine;
using System.Collections;

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
    public UnitType unitType;

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

        // 프리팹에 AudioSource가 이미 있으면 사용, 없으면 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Awake() 시점에 Inspector에 설정된 attackClip 확인 (디버그 용)
        Debug.Log($"[{name}] Awake(): attackClip={(attackClip != null ? attackClip.name : "null")}");

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
        yield return new WaitForEndOfFrame();

        Collider2D unitCollider = GetComponent<Collider2D>();
        if (unitCollider == null)
        {
            Debug.LogWarning($"[{name}] ApplyCollisionIgnore(): 유닛에 Collider2D가 없습니다.");
            yield break;
        }

        string ownBaseTag = CompareTag("PlayerUnit") ? "PlayerBase" :
                            CompareTag("EnemyUnit") ? "EnemyBase" : null;
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
        if (CompareTag("EnemyUnit"))
            moveDir = Vector3.left;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }
    #endregion

    #region 공격 처리
    protected virtual void HandleAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        Debug.Log($"[{name}] 공격 전: attackClip={(attackClip != null ? attackClip.name : "null")}, audioSource={(audioSource != null ? "존재함" : "null")}");

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

            // 공격 처리 부분, 사운드 재생 구조
            if (attackClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackClip);
            }
            lastAttackTime = Time.time;
        }
    }
    #endregion

    protected float GetAdvantageMultiplier(BaseUnit target)
    {
        if (unitType == UnitType.Infantry && target.unitType == UnitType.Archer)
            return 1.2f;
        else if (unitType == UnitType.Archer && target.unitType == UnitType.Cavalry)
            return 1.2f;
        else if (unitType == UnitType.Cavalry && target.unitType == UnitType.Infantry)
            return 1.2f;
        return 1f;
    }

    protected virtual void OnEnable()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }
    }
}
