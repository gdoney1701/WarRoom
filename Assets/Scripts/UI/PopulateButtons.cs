using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PopulateButtons : MonoBehaviour
{
    [SerializeField]
    Button pooledButton;
    [SerializeField]
    Transform buttonLayout;
    [SerializeField]
    GameObject disableGroup;

    private GameObject[] spawnedButtons;


    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += CreateButtons;
        disableGroup.SetActive(true);
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= CreateButtons;
        disableGroup.SetActive(false);
    }
    public void ClickButton(FactionData faction)
    {
        Debug.Log(string.Format("You Selected {0}", faction.LongName));
        disableGroup.SetActive(false);
    }

    private void CreateButtons(MapMeshGenerator.MeshGenerationData data, SaveData saveData)
    {
        FactionData[] factionData = saveData.saveBelligerents.WarParticipants;
        var assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/icons/flags"));

        for (int i = 0; i < factionData.Length; i++)
        {
            GameObject tempObject = Instantiate(pooledButton.gameObject);
            Button newButton = tempObject.GetComponent<Button>();
            Image newImage = tempObject.GetComponent<Image>();
           
            newImage.sprite = assetBundle.LoadAsset<Sprite>(factionData[i].IconPath);
            int currentFaction = i;
            newButton.onClick.AddListener(() => ClickButton(factionData[currentFaction]));
            tempObject.transform.SetParent(buttonLayout);
            tempObject.SetActive(true);
        }
    }
}
