using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitTypeUI : MonoBehaviour
{
    [SerializeField]
    private Image unitToken;
    [SerializeField]
    private TextMeshProUGUI unitAmount;


    public void InitializeUnitType(Color unitColor, int unitNumber)
    {
        unitToken.color = unitColor;
        unitAmount.text = unitNumber.ToString();
    }
}
