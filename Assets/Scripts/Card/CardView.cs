using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private HPBar hpBar;

    public void Bind(CardInstance instance)
    {
        CardVisualConfig visual = instance.data.visual;

        if (visual != null)
        {
            illustrationImage.sprite = visual.illustration;
            frameImage.color = visual.frameColor;
            typeIconImage.sprite = visual.typeIcon;
            typeIconImage.gameObject.SetActive(visual.typeIcon != null);
        }

        cardNameText.text = instance.data.cardName;
        hpBar.Init(instance.data.maxHP, instance.currentHP);
    }
}
