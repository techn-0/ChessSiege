using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int maxHealth = 1000;
    [SerializeField] private int currentHealth; // 인스펙터에서 확인 가능

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            DestroyBase();
    }

    private void DestroyBase()
    {
        // 기지가 파괴되었을 때 일어날 이벤트 처리(게임 종료 등)
        Destroy(gameObject);
    }
}
