// PlayerMoneyUI.cs
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoneyUI : MonoBehaviour
{
    public Text moneyText; // 인스펙터에서 PlayerMoneyText 연결

    // Update is called once per frame
    void Update()
    {
        if (moneyText != null && GameManager.Instance != null)
        {
            moneyText.text = string.Format("골드: {0}\n목재: {1}\n식량: {2}",
                GameManager.Instance.PlayerGold,
                GameManager.Instance.PlayerWood,
                GameManager.Instance.PlayerFood);
        }
    }
}
