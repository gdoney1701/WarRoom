using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderEntryUI : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI UnitTag;

    [SerializeField]
    private TextMeshProUGUI DestinationTag;

    [SerializeField]
    private GameObject inactiveOverlay;

    [SerializeField]
    private Button clearButton;

    private string defaultTag = "---";

    public Button ClearButton
    {
        get { return clearButton; }
    }

    public bool SetInactive
    {
        set { inactiveOverlay.SetActive(value); }
    }
    public string SetDestination
    {
        set { DestinationTag.SetText(value); }
    }

    public string SetUnit
    {
        set { UnitTag.SetText(value); }
    }

    public void ResetEntry()
    {
        UnitTag.SetText(defaultTag);
        DestinationTag.SetText(defaultTag);
    }

    private void Awake()
    {
        UnitTag.SetText(defaultTag);
        DestinationTag.SetText(defaultTag);
    }


}
