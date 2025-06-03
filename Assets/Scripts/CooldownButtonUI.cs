using UnityEngine;
using UnityEngine.UI;

public class CooldownButtonUI : MonoBehaviour
{
    [Header("UI Components")]
    public Text costText;         // 유닛 비용 표시 (예: "골드: X 목재: Y 식량: Z")
    public Image previewImage;    // 유닛 미리보기 이미지
    public Image cooldownDial;    // 쿨다운 다이얼 (Fill Method: Radial)
    public MinionSpawnManager spawnManager;
    public int minionIndex;

    private Button button;
    private BaseUnit unitInfo;    // 캐싱한 유닛 정보

    void Start()
    {
        button = GetComponent<Button>();

        // 스폰 매니저에서 프리팹과 그에 붙은 BaseUnit을 가져와서 캐시하고, UI 초기 세팅 진행.
        if (spawnManager != null && spawnManager.minionDataList.Count > minionIndex)
        {
            GameObject prefab = spawnManager.minionDataList[minionIndex].prefab;
            if (prefab != null)
            {
                unitInfo = prefab.GetComponent<BaseUnit>();
                if (unitInfo != null)
                {
                    // 비용 정보 표기
                    if (costText != null)
                    {
                        costText.text = string.Format("골드: {0}\n목재: {1}\n식량: {2}",
                            unitInfo.costGold, unitInfo.costWood, unitInfo.costFood);
                    }
                    // 미리보기 이미지 설정 (프리팹에 SpriteRenderer가 있을 경우)
                    SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                    if (previewImage != null && sr != null)
                    {
                        previewImage.sprite = sr.sprite;
                    }
                }
            }
        }

        // 텍스트 대신 다이얼을 사용할 경우, 필요하다면 기존 쿨다운 텍스트는 감춥니다.
        // (만약 쿨다운 텍스트도 사용하고 싶다면 이 부분을 조정하세요.)
    }

    void Update()
    {
        if (spawnManager == null || spawnManager.minionDataList.Count <= minionIndex)
            return;

        // 혹시 캐싱이 실패한 경우 재시도
        if (unitInfo == null)
        {
            GameObject prefab = spawnManager.minionDataList[minionIndex].prefab;
            if (prefab != null)
                unitInfo = prefab.GetComponent<BaseUnit>();
        }
        if (unitInfo == null)
            return;

        // 현재 쿨다운 진행상황 계산
        float lastSpawn = spawnManager.GetLastSpawnTime(minionIndex);
        float cooldown = unitInfo.spawnCooldown;
        float remain = (lastSpawn + cooldown) - Time.time;

        // 다이얼 업데이트: 0부터 시작해서 시간이 지날수록 채워짐 (fillAmount: 0 ~ 1)
        if (cooldownDial != null)
        {
            // fillAmount를 1 - (remain/cooldown)로 계산하여 채워지게 함.
            float fill = 1 - Mathf.Clamp01(remain / cooldown);
            cooldownDial.fillAmount = fill;
        }

        // 버튼 상호작용: 쿨다운이 완료되면(true) 버튼이 활성화됨.
        if (button != null)
            button.interactable = (remain <= 0f);
    }

    public void OnMinionButtonClick()
    {
        spawnManager.SpawnMinionByIndex(minionIndex);
    }
}
