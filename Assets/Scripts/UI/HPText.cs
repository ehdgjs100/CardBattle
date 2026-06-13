using TMPro;
using UnityEngine;

public class HPText : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    private int _maxHP;

    public void Init(int maxHP, int currentHP)
    {
        _maxHP = maxHP;
        SetHP(currentHP);
    }

    public void SetHP(int currentHP)
    {
        hpText.text = $"{currentHP}/{_maxHP}";
    }
}
