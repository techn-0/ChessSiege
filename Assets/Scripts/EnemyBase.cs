using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public string baseName = "EnemyBase";
    public int maxHealth = 1000;
    [SerializeField] private int currentHealth;

    [Header("체력바 오브젝트")]
    public Transform healthBarForeground; // 포그라운드 스프라이트의 Transform

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthBar();
        if (currentHealth <= 0)
            DestroyBase();
    }

    private void UpdateHealthBar()
    {
        if (healthBarForeground != null)
        {
            float ratio = (float)currentHealth / maxHealth;
            healthBarForeground.localScale = new Vector3(ratio, 1f, 1f);
        }
    }

    private void DestroyBase()
    {
        Destroy(gameObject);
    }
}
