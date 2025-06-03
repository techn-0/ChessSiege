using UnityEngine;

public class UnitHealthBar : MonoBehaviour
{
    // 유닛 체력바의 빨간 막대기의 Transform (PlayerBase의 healthBarForeground와 동일한 역할)
    public Transform redBar; 
    // 유닛 위에 표시할 오프셋 (필요에 따라 조정; 단, 프리팹에서 미리 배치)
    public Vector3 offset = new Vector3(0.3f, 0.6f, 0);
    private BaseUnit baseUnit;

    void Awake()
    {
        // 부모 오브젝트에서 BaseUnit 컴포넌트를 찾습니다.
        baseUnit = GetComponentInParent<BaseUnit>();
        if (baseUnit == null)
        {
            Debug.LogWarning("BaseUnit 컴포넌트를 찾을 수 없습니다.", this);
        }
    }

    void Update()
    {
        if (baseUnit != null && redBar != null)
        {
            // 체력 비율 계산
            float ratio = (float)baseUnit.CurrentHealth / baseUnit.MaxHealth;
            // 빨간 막대기의 x 스케일을 체력 비율로 업데이트 (y와 z는 그대로)
            redBar.localScale = new Vector3(ratio, 1f, 1f);
            // 만약 HealthBar 프리팹이 BaseUnit의 자식이라면, 매 프레임 전체 오브젝트의 로컬 위치를 갱신하면 안 됩니다.
            // 프리팹에 기본 offset이 이미 설정되어 있다면 여기서는 위치 갱신 코드를 제거합니다.
            // transform.localPosition = offset;  // 이 줄은 제거합니다.
        }
    }
}
