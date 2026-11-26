using DG.Tweening;
using Observer;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class CoffeeWaitingQueue : MonoBehaviour
{
     public MeshRenderer conveyder;
     public GameObject coffeePrefab;
     public Transform coffeeHolder;
     public DOTweenPath waitingPath;
     public Transform movingPath;

     public List<CoffeeInfo> coffeeInfos;
     public Vector3[] pathPoints;
     public float coffeeSpacing = 1f;
     public TrayHolder trayHolder;

     public TextMeshPro coffeeWaitingCount;
     private void OnEnable()
     {
          EventDispatcher.Instance.RegisterListener(EventID.OnCallCheckTakeCoffee, CheckCanTakeCoffee);
     }
     private void OnDisable()
     {
          this.RemoveListener(EventID.OnCallCheckTakeCoffee, CheckCanTakeCoffee);
     }
     public void ClearCup()
     {
          for (int i = 0; i < coffeeInfos.Count; i++)
          {
               Destroy(coffeeInfos[i].gameObject);
          }
          coffeeInfos.Clear();

          trayHolder.ClearTray();
     }
     public void SpawnCup(LevelData level)
     {
          LoadLevelData(level);

          GeneratePathPoints();

     }

     private void LoadLevelData(LevelData level)
     {
          for (int layerIndex = level.layers.Count - 1; layerIndex >= 0; layerIndex--)
          {
               var layer = level.layers[layerIndex];
               List<ECoffeeColor> cupColors = new List<ECoffeeColor>();

               foreach (var tray in layer.trays)
               {
                    int cupCount = (int)tray.size;
                    for (int i = 0; i < cupCount; i++)
                    {
                         cupColors.Add(tray.trayColor);
                    }
               }
               Shuffle(cupColors);
               int indexTheme = (int)level.theme;
               foreach (var color in cupColors)
               {
                    GameObject coffee = Instantiate(coffeePrefab, coffeeHolder);
                    coffee.transform.position = waitingPath.wps[0];
                    coffee.transform.localRotation = Quaternion.identity;
                    coffee.transform.GetChild(0).GetChild(indexTheme).gameObject.SetActive(true);

                    CoffeeInfo coffeeInfo = coffee.GetComponent<CoffeeInfo>();
                    AddCoffee(coffeeInfo);

                    coffeeInfo.coffeeColor = CofeeMaterialsData.i.GetColor(color);

                    Renderer coffeeRenderer = coffee.GetComponentInChildren<Renderer>();
                    Material[] coffeeMaterials = coffeeRenderer.materials;

                    for (int i = 0; i < coffeeMaterials.Length; i++)
                    {
                         if (coffeeMaterials[i].name.Contains("Color"))
                         {
                              coffeeMaterials[i] = coffeeInfo.coffeeColor.material;
                              break;
                         }
                    }

                    coffeeRenderer.materials = coffeeMaterials;

               }
          }
     }
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
     private void AddCoffee(CoffeeInfo coffeeInfo)
     {
          coffeeInfos.Add(coffeeInfo);
     }
     void Update()
     {
          if (Input.GetKeyDown(KeyCode.Space))
          {
               CheckCanTakeCoffee(null);
          }
          coffeeWaitingCount.text = coffeeInfos.Count.ToString();
     }
     private void CheckCanTakeCoffee(object obj)
     {
          if (trayHolder.barSlots.All(x => x.Value == null)) return;




          if (coffeeInfos[0].readyToTake)
               TakeCoffee();
     }

     public void TakeCoffee()
     {
          if (coffeeInfos.Count > 0)
          {
               Tray targetTray = trayHolder.FindTrayWithColor(coffeeInfos[0].coffeeColor.eCofeeColor);
               if (targetTray == null)
               {
                    GameManager.i.CheckLoseCondition();
                    return;
               }
               if (!targetTray.isInBar) return;
               if (targetTray.isUsingBooster) return;

               if (targetTray.CheckAvailableSlot(coffeeInfos[0]))
               {
                    coffeeInfos.RemoveAt(0);

                    MoveCoffee();
               }

          }
     }
     public void MoveCoffee()
     {
          for (int i = 0; i < coffeeInfos.Count; i++)
          {
               Vector3[] subPath = GetSubPathForCoffee(i);
               if (i == 0)
               {
                    Material material = conveyder.materials[1];

                    material.SetVector("_Speed", new Vector3(0f, 0.05f, 0f));
               }
               MoveCoffee(i, subPath);
          }
     }

     public void AssignItemsToPathPoints()
     {
          int pointCount = pathPoints.Length;
          int coffeeCount = coffeeInfos.Count;

          for (int i = 0; i < coffeeCount; i++)
          {
               if (i < pointCount)
               {
                    if (coffeeInfos[i] != null)
                    {
                         Vector3[] subPath = GetSubPathForCoffee(i);
                         MoveCoffee(i, subPath);
                    }
               }
               else
               {
                    //  Vector3[] subPath = GetSubPathForCoffee(coffeeCount);

               }
          }
     }

     private Vector3[] GetSubPathForCoffee(int targetIndex)
     {
          List<Vector3> subPath = new List<Vector3>();

          for (int i = 0; i < pathPoints.Length - targetIndex; i++)
          {
               subPath.Add(pathPoints[i]);
          }


          return subPath.ToArray();
     }
     private void MoveCoffee(int index, Vector3[] subPath)
     {
          coffeeInfos[index].MoveAlongPath(subPath, index != 0 ? null : () =>
          {
               this.PostEvent(EventID.OnCallCheckTakeCoffee);

               Material material = conveyder.materials[1];

               material.SetVector("_Speed", new Vector3(0f, 0f, 0f));

          });
     }
     private void GeneratePathPoints()
     {

          if (pathPoints.Length > 0)
          {
               AssignItemsToPathPoints();
               return;
          }

          List<Vector3> generatedPoints = new List<Vector3>();
          // Transform[] childPoints = waitingPath.GetComponentsInChildren<Transform>();

          Vector3[] path = waitingPath.wps.ToArray();

          movingPath.position = waitingPath.wps[0];
          Vector3 lastPosition = movingPath.position;
          generatedPoints.Add(lastPosition);
          movingPath.DOPath(path, 2f, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.Linear).OnUpdate(() =>
          {
               Vector3 currentPosition = movingPath.position;

               if (Vector3.Distance(currentPosition, lastPosition) >= coffeeSpacing)
               {
                    generatedPoints.Add(lastPosition);
                    lastPosition = currentPosition;
               }
          }).SetUpdate(UpdateType.Fixed).OnComplete(() =>
          {

               pathPoints = generatedPoints.ToArray();

               AssignItemsToPathPoints();
          });



     }
}
