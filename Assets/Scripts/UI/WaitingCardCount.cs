using TMPro;
using UnityEngine;

public class WaitingCardCount : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject[] cardVisuals;

    public void SetCount(int count)
    {
        countText.text = count.ToString();

        for (int i = 0; i < cardVisuals.Length; i++)
            cardVisuals[i].SetActive(i < count);
    }
}
