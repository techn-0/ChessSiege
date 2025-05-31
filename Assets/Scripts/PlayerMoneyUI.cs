// PlayerMoneyUI.cs
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoneyUI : MonoBehaviour
{
    public Text moneyText; // 인스펙터에서 PlayerMoneyText 연결

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (moneyText != null)
            moneyText.text = GameManager.Instance.PlayerResources.ToString();
    }
}
