using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class SpawnedLayerTray
{
    public List<Tray> spawnedTrays;
}
public class LevelManager : Singleton<LevelManager>
{

    public CoffeeWaitingQueue coffeeWaitingQueue;
    public bool isTest = false;
    public bool debugLevel;
    public LevelData testLevel;
    public LevelData[] allLevels;
    [HideInInspector] public LevelData currentLevelData;
    public List<SpawnedLayerTray> spawnedLayerTrays;
    public float offsetLayer = 1.5f;
    public int currentLevelCount
    {
        get { return PlayerPrefs.GetInt("currentLevelCount", 0); }
        set { PlayerPrefs.SetInt("currentLevelCount", value); }
    }
    private void Start()
    {
        SpawnLevel();
        if (currentLevelCount == 0)
        {
            TutorialManager.i.StartTutorial();
        }
    }
    public void NextLevel()
    {

        currentLevelCount++;
        if (currentLevelCount >= allLevels.Length)
        {
            currentLevelCount = Random.Range((allLevels.Length - 1) / 2, allLevels.Length);
            currentLevelCount = Mathf.Clamp(currentLevelCount, 0, allLevels.Length - 1);
        }
        ClearTray();
        coffeeWaitingQueue.ClearCup();
        SpawnLevel();

    }
    public void RestartLevel()
    {
        ClearTray();
        coffeeWaitingQueue.ClearCup();
        SpawnLevel();
    }
    private void ClearTray()
    {
        foreach (SpawnedLayerTray spawnedLayerTray in spawnedLayerTrays)
        {
            foreach (Tray tray in spawnedLayerTray.spawnedTrays)
            {
                Destroy(tray.gameObject);
            }
        }
        spawnedLayerTrays.Clear();
    }
    private void SpawnLevel()
    {
        spawnedLayerTrays = new List<SpawnedLayerTray>();

        currentLevelData = isTest ? testLevel : allLevels[currentLevelCount];
        for (int layerIndex = 0; layerIndex < currentLevelData.layers.Count; layerIndex++)
        {
            var layer = currentLevelData.layers[layerIndex];
            var newLayerTray = new SpawnedLayerTray();
            newLayerTray.spawnedTrays = new List<Tray>();
            spawnedLayerTrays.Add(newLayerTray);
            foreach (var tray in layer.trays)
            {
                GameObject trayGO = TrayData.i.GetTrayPrefab(tray.size);

                Vector3 worldPosition = new Vector3(tray.position.x, layerIndex * offsetLayer, tray.position.y);

                GameObject newTray = Instantiate(trayGO, worldPosition, Quaternion.Euler(0f, (int)tray.direction * 90f, 0f));

                Tray trayInfo = newTray.GetComponent<Tray>();
                spawnedLayerTrays[layerIndex].spawnedTrays.Add(trayInfo);

                trayInfo.typeOfCoffee = tray.trayColor;

                MeshRenderer[] meshRenderers = trayInfo.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mesh in meshRenderers)
                {
                    if (mesh.gameObject.name == "Khay" || mesh.gameObject.name == "khay") continue;

                    Material[] trayMat = mesh.materials;
                    trayMat[0] = TrayData.i.GetColor(trayInfo.typeOfCoffee).material;
                    mesh.materials = trayMat;
                }
            }
        }
        coffeeWaitingQueue.SpawnCup(currentLevelData);
    }

    private void OnDrawGizmos()
    {
        if (!debugLevel) return;
        if (testLevel == null) return;


        if (testLevel == null) return;

        for (int layerIndex = 0; layerIndex < testLevel.layers.Count; layerIndex++)
        {
            var layer = testLevel.layers[layerIndex];

            foreach (var tray in layer.trays)
            {
                Vector3 position = new Vector3(tray.position.x, layerIndex * offsetLayer, tray.position.y);

                Gizmos.color = GetColorFromTrayColorEnum(tray.trayColor);

                Quaternion rotation = Quaternion.Euler(0f, (int)tray.direction * 90f, 0f);
                Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 1f, (int)tray.size / 2) * 1.2f);
            }
        }

    }
    public List<Tray> GetAllTrays()
    {
        List<Tray> tempTray = new List<Tray>();

        for (int i = 0; i < spawnedLayerTrays.Count; i++)
        {
            tempTray.AddRange(spawnedLayerTrays[i].spawnedTrays);
        }

        return tempTray;
    }
    private Color GetColorFromTrayColorEnum(ECoffeeColor trayColor)
    {
        switch (trayColor)
        {
            case ECoffeeColor.yellow:
                return Color.yellow;
            case ECoffeeColor.red:
                return Color.red;
            case ECoffeeColor.pink:
                return new Color(1f, 0.75f, 0.8f);
            case ECoffeeColor.purple:
                return new Color(0.5f, 0f, 0.5f);
            case ECoffeeColor.blue:
                return Color.blue;
            case ECoffeeColor.green:
                return Color.green;
            case ECoffeeColor.orange:
                return new Color(1f, 0.647f, 0f);
            default:
                return Color.white;
        }


    }




}
