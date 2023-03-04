using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterMeshGenerator {
    public class CharacterMeshCreatorManager : MonoBehaviour
    {
        // this is a tool for the editor to create a new prefab for a character and auto-save it!
        public string saveGeneratedMaterialPath = "C:/Users/jmanf/Documents/Github/DiceGame/Assets/Materials/Characters/Generated";
        public string saveGeneratedMeshPath = "C:/Users/jmanf/Documents/Github/DiceGame/Assets/Models/Generated/Characters";
        public string savePrefabPath = "C:/Users/jmanf/Documents/Github/DiceGame/Assets/Prefabs/Characters/Generated";
        public Texture2D imageTexture;
        public Texture2D imageOutline;
        public CharacterMeshGenerator meshGenerator;

        [ContextMenu("Find Save Locations")]
        public void FindSaveLocations() {
            #if UNITY_EDITOR
            string temp;
            temp = UnityEditor.EditorUtility.OpenFolderPanel("Pick Where To Save Materials", "", "");
            if (temp.Length > 0) {
                saveGeneratedMaterialPath = temp;
            }
            temp = UnityEditor.EditorUtility.OpenFolderPanel("Pick Where To Save Meshes", "", "");
            if (temp.Length > 0) {
                saveGeneratedMeshPath = temp;
            }
            temp = UnityEditor.EditorUtility.OpenFolderPanel("Pick Where To Save Final Prefabs", "", "");
            if (temp.Length > 0) {
                savePrefabPath = temp;
            }
            #endif
        }

        [ContextMenu("Set Jordan Specific Paths")]
        public void SetJordanSpecificPaths() {
            saveGeneratedMaterialPath = "Assets/Materials/Characters/Generated";
            saveGeneratedMeshPath = "Assets/Models/Generated/Characters";
            savePrefabPath = "Assets/Prefabs/Characters/Generated";
        }

        [ContextMenu("Create Prefab")]
        public void CreatePrefab() {
            meshGenerator.imageTexture = imageTexture;
            meshGenerator.imageOutline = imageOutline;
            MeshGeneratorData data = meshGenerator.CreateCharacterPrefab();
            #if UNITY_EDITOR
            // now gotta save everything!

            // save the mesh, then re-assign everything to make sure
            // save the material, then reassign it
            // then save the prefab.
            string meshFilepath = System.IO.Path.Combine(saveGeneratedMeshPath, imageTexture.name + "_Mesh.mesh");
            Mesh m = data.meshFilter.sharedMesh;
            UnityEditor.AssetDatabase.CreateAsset(m, meshFilepath);

            string materialFilepath = System.IO.Path.Combine(saveGeneratedMaterialPath, imageTexture.name + "_Mat.mat");
            Material mat = meshGenerator.meshRenderer.sharedMaterials[0]; // get the generated material
            UnityEditor.AssetDatabase.CreateAsset(mat, materialFilepath);



            string prefabFilepath = System.IO.Path.Combine(savePrefabPath, meshGenerator.gameObject.name + ".prefab");
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(meshGenerator.gameObject, prefabFilepath);
            

            // now save everything!
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("Unless it errored it worked!");
            #endif
        }
    }
}