using UnityEngine;
using UnityEngine.UI;

public class CooldownButtonUI : MonoBehaviour
{
    public Text cooldownText;
    public MinionSpawnManager spawnManager;
    public int minionIndex;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
    }

    void Update()
    {
        if (spawnManager == null || spawnManager.minionDataList.Count <= minionIndex)
            return;

        var prefab = spawnManager.minionDataList[minionIndex].prefab;
        var unitInfo = prefab.GetComponent<BaseUnit>();
        if (unitInfo == null)
            return;

        float lastSpawn = spawnManager.GetLastSpawnTime(minionIndex);
        float cooldown = unitInfo.spawnCooldown;
        float remain = (lastSpawn + cooldown) - Time.time;

        if (remain > 0f)
        {
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = Mathf.CeilToInt(remain).ToString();
            }
            if (button != null)
                button.interactable = false;
        }
        else
        {
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
            if (button != null)
                button.interactable = true;
        }
    }

    public void OnMinionButtonClick()
    {
        spawnManager.SpawnMinionByIndex(minionIndex);
    }
}
