using System.Collections.Generic;
using System.Linq;
using MEC;
using Observer;
using UnityEngine;



public class BoosterManager : Singleton<BoosterManager>
{
    public CoffeeWaitingQueue coffeeWaitingQueue;
    public TrayHolder trayHolder;


    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            SortCoffee();
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            ShuffleTray();
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            ClearTray();
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            MegaClearTray();
        }
#endif
    }
    bool canUseBooster;
    public bool CheckCanUseBooster(TypeBooster typeBooster)
    {

        switch (typeBooster)
        {
            case TypeBooster.SortCoffee:
                canUseBooster = true;
                break;

            case TypeBooster.ShuffleTray:
                canUseBooster = LevelManager.i.GetAllTrays().Count(x => x.canShuffle) > 1;
                break;

            case TypeBooster.ClearTray:
                canUseBooster = trayHolder.CountNullSlot() >= 1 && LevelManager.i.GetAllTrays().Count(x => x.readyToTake) >= 1;
                break;

            case TypeBooster.MegaClearTray:
                canUseBooster = trayHolder.CountNullSlot() >= 3 && LevelManager.i.GetAllTrays().Count(x => x.readyToTake) >= 3;
                break;
        }
        if (!canUseBooster)
        {
            UIManager.i.Notification("现在还不能使用道具");
        }
        return canUseBooster;

    }
    public void UseBooster(TypeBooster typeBooster)
    {
        switch (typeBooster)
        {
            case TypeBooster.SortCoffee:
                SortCoffee();
                break;

            case TypeBooster.ShuffleTray:
                ShuffleTray();
                break;

            case TypeBooster.ClearTray:
                ClearTray();
                break;

            case TypeBooster.MegaClearTray:
                MegaClearTray();
                break;
        }
    }


    #region SortCoffee
    public List<ECoffeeColor> trueColors;
    public void SortCoffee()
    {
        List<ECoffeeColor> cupColors = new List<ECoffeeColor>();
        int totalSlot = 0;
        int maxCountTray = 4;
        int indexCount = 0;
        foreach (var tray in LevelManager.i.GetAllTrays())
        {
            int cupCount = tray.CountAvailableSlot();
            if (indexCount < maxCountTray)
            {
                indexCount++;
                totalSlot += cupCount;
            }
            for (int i = 0; i < cupCount; i++)
            {
                cupColors.Add(tray.typeOfCoffee);
            }
        }
        cupColors.Reverse();

        trueColors = cupColors.GetRange(0, totalSlot);

        cupColors.RemoveRange(0, totalSlot);
        Shuffle(cupColors);

        trueColors.AddRange(cupColors);

        for (int i = 0; i < coffeeWaitingQueue.coffeeInfos.Count; i++)
        {
            CoffeeInfo coffeeInfo = coffeeWaitingQueue.coffeeInfos[i];
            if (coffeeInfo.isInTray) continue;
            coffeeWaitingQueue.coffeeInfos[i].coffeeColor = CofeeMaterialsData.i.GetColor(trueColors[i]);

            coffeeInfo.coffeeColor = CofeeMaterialsData.i.GetColor(trueColors[i]);

            Renderer coffeeRenderer = coffeeInfo.GetComponentInChildren<Renderer>();
            Material[] coffeeMaterials = coffeeRenderer.materials;

            for (int j = 0; j < coffeeMaterials.Length; j++)
            {
                if (coffeeMaterials[j].name.Contains("Color"))
                {
                    coffeeMaterials[j] = coffeeInfo.coffeeColor.material;
                    break;
                }
            }

            coffeeRenderer.materials = coffeeMaterials;
        }


        this.PostEvent(EventID.OnCallCheckTakeCoffee);
    }

    #endregion
    #region ShuffleTray
    public void ShuffleTray()
    {
        List<ECoffeeColor> trayColors = new List<ECoffeeColor>();
        List<Tray> tempTrays = new List<Tray>();

        for (int i = 0; i < LevelManager.i.GetAllTrays().Count; i++)
        {
            Tray trayInfo = LevelManager.i.GetAllTrays()[i];
            if (!trayInfo.canShuffle) continue;
            trayColors.Add(trayInfo.typeOfCoffee);
            tempTrays.Add(trayInfo);
        }
        Shuffle(trayColors);

        for (int i = 0; i < tempTrays.Count; i++)
        {
            Tray trayInfo = tempTrays[i];
            if (!trayInfo.isInBar)
                trayInfo.readyToTake = true;
            trayInfo.typeOfCoffee = trayColors[i];

            MeshRenderer[] meshRenderers = trayInfo.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mesh in meshRenderers)
            {
                if (mesh.gameObject.name == "Khay" || mesh.gameObject.name == "khay") continue;

                Material[] trayMat = mesh.materials;
                trayMat[0] = TrayData.i.GetColor(trayInfo.typeOfCoffee).material;
                mesh.materials = trayMat;

                trayMat = mesh.materials;
                if (trayInfo.blocked)
                {
                    Color newColor;
                    ColorUtility.TryParseHtmlString("#848484", out newColor);
                    trayMat[0].SetColor("_GoochBrightColor", newColor);
                }
                else
                {
                    trayMat[0].SetColor("_GoochBrightColor", Color.white);
                }
                mesh.materials = trayMat;
            }
        }
        GetNewColor();
        this.PostEvent(EventID.OnCallCheckTakeCoffee);
    }
    private void GetNewColor()
    {
        List<ECoffeeColor> cupColors = new List<ECoffeeColor>();
        foreach (var tray in LevelManager.i.GetAllTrays())
        {
            int cupCount = tray.CountAvailableSlot();
            for (int i = 0; i < cupCount; i++)
            {
                cupColors.Add(tray.typeOfCoffee);
            }
        }
        Shuffle(cupColors);
        for (int i = 0; i < coffeeWaitingQueue.coffeeInfos.Count; i++)
        {
            CoffeeInfo coffeeInfo = coffeeWaitingQueue.coffeeInfos[i];
            if (coffeeInfo.isInTray) continue;
            coffeeWaitingQueue.coffeeInfos[i].coffeeColor = CofeeMaterialsData.i.GetColor(cupColors[i]);
            Material[] coffeeMat = coffeeInfo.GetComponentInChildren<Renderer>().materials;
            coffeeMat[0] = coffeeInfo.coffeeColor.material;
            coffeeInfo.GetComponentInChildren<Renderer>().materials = coffeeMat;
        }
    }
    #endregion
    #region AutoCompleteBox
    private void ClearTray()
    {
        SpawnedLayerTray spawnedLayerTray = LevelManager.i.spawnedLayerTrays
        .AsEnumerable()
        .Reverse()
        .FirstOrDefault(x => x.spawnedTrays.Any(y => y.readyToTake));


        Tray targetTray = spawnedLayerTray.spawnedTrays
        .Where(x => x.readyToTake)
        .Aggregate((t1, t2) => (int)t1.size > (int)t2.size ? t1 : t2);

        trayHolder.CheckAvailableSlot(targetTray);
        targetTray.isUsingBooster = true;
        Timing.RunCoroutine(_DelayTakeCoffee(targetTray));

    }
    private IEnumerator<float> _DelayTakeCoffee(Tray targetTray)
    {
        yield return Timing.WaitForSeconds(0.5f);



        List<CoffeeInfo> tempCoffeeInfo = new List<CoffeeInfo>();
        tempCoffeeInfo = coffeeWaitingQueue.coffeeInfos
                        .Where(x => x.coffeeColor.eCofeeColor == targetTray.typeOfCoffee)
                        .Take((int)targetTray.size)
                        .ToList();

        for (int i = 0; i < tempCoffeeInfo.Count; i++)
        {
            coffeeWaitingQueue.coffeeInfos.Remove(tempCoffeeInfo[i]);
            targetTray.CheckAvailableSlot(tempCoffeeInfo[i]);
        }
        coffeeWaitingQueue.MoveCoffee();

    }


    private void MegaClearTray()
    {
        for (int i = 0; i < 3; i++)
        {
            ClearTray();
        }
    }
    #endregion


    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }



}
