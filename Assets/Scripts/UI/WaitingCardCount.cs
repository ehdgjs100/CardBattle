using TMPro;
using UnityEngine;

public class WaitingCardCount : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;

    public void SetCount(int count)
    {
        countText.text = count.ToString();
    }
}
