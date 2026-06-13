using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Color highColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;

    private int _maxHP;

    public void Init(int maxHP, int currentHP)
    {
        _maxHP = maxHP;
        SetHP(currentHP);
    }

    public void SetHP(int currentHP)
    {
        float ratio = _maxHP > 0 ? (float)currentHP / _maxHP : 0f;
        fillImage.fillAmount = ratio;
        fillImage.color = ratio > 0.5f ? highColor : ratio > 0.25f ? midColor : lowColor;

        if (hpText != null)
            hpText.text = $"{currentHP}/{_maxHP}";
    }
}
