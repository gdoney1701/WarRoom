using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class FactionCoinVisuals : MonoBehaviour
{
    [SerializeField]
    private GameObject baseCoin;
    [SerializeField]
    private Image flagImage;
    [SerializeField]
    private TextMeshProUGUI totalNumberText;
    [SerializeField]
    private UnitTypeUI pooledUnitType;
    [SerializeField]
    private Transform unitTypeContainer;

    private UnitTypeUI[] unitArray = new UnitTypeUI[4];

    public void ModifyCoinVisuals(FactionData factionData, StackManager[] presentStacks)
    {
        baseCoin.GetComponent<Renderer>().material.color = factionData.VectorToColor();
        var assetBundle = AssetBundleManager.GetAssetBundle("AssetBundles/icons/flags");
        flagImage.sprite = assetBundle.LoadAsset<Sprite>(factionData.IconPath);

        int totalTroops = 0;
        int[] troopTypes = new int[4]; //Yellow, Blue, Green, Red

        foreach (StackManager stackManager in presentStacks)
        {
            var stack = stackManager.LocalData;
            totalTroops += stack.GetStackTotal();
            troopTypes[0] += stack.YellowTroopCount;
            troopTypes[1] += stack.BlueTroopCount;
            troopTypes[2] += stack.GreenTroopCount;
            troopTypes[3] += stack.RedTroopCount;
        }
        totalNumberText.text = totalTroops.ToString();

        foreach(UnitTypeUI entry in unitArray)
        {
            if(entry != null)
            {
                Destroy(entry);
            }
        }
        unitArray = new UnitTypeUI[4];
        PopulateUnitArray(troopTypes[0], Color.yellow, 0);
        PopulateUnitArray(troopTypes[1], Color.blue, 1);
        PopulateUnitArray(troopTypes[2], Color.green, 2);
        PopulateUnitArray(troopTypes[3], Color.red, 3);
    }

    private void PopulateUnitArray(int troopTotal, Color unitColor, int index)
    {
        if(troopTotal == 0)
        {
            return;
        }
        var newUnitType = Instantiate(pooledUnitType, unitTypeContainer);
        newUnitType.gameObject.SetActive(true);
        newUnitType.InitializeUnitType(unitColor, troopTotal);
        unitArray[index] = newUnitType;

    }

}
