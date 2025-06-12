using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage;
    public Transform target; // 공격 목표(유닛 또는 기지의 Transform)

    void Update()
    {
        if (target != null)
        {
            // 타겟 방향으로 이동
            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else
        {
            // 타겟이 없으면 그냥 앞으로 이동
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 목표와 충돌했을 때만 데미지
        if (target != null && other.transform == target)
        {
            // 유닛
            BaseUnit unit = other.GetComponent<BaseUnit>() ?? other.GetComponentInParent<BaseUnit>();
            if (unit != null)
                unit.TakeDamage(damage);

            // 기지
            PlayerBase pb = other.GetComponent<PlayerBase>() ?? other.GetComponentInParent<PlayerBase>();
            EnemyBase eb = other.GetComponent<EnemyBase>() ?? other.GetComponentInParent<EnemyBase>();
            if (pb != null) pb.TakeDamage(damage);
            if (eb != null) eb.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
