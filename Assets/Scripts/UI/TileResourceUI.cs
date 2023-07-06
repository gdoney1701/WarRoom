using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileResourceUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI resourceText;
    [SerializeField]
    private Image resourceBackground;

    public void SetVisuals(Color background, int resourceCount)
    {
        resourceBackground.color = background;
        resourceText.SetText(resourceCount.ToString());
    }
}
