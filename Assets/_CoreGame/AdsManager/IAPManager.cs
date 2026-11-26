// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Net;
// using UnityEngine;
// using UnityEngine.Purchasing;
// using UnityEngine.Purchasing.Security;


// public class IAPManager : MonoBehaviour, IStoreListener
// {
//      public static IAPManager instance;
//      public static IStoreController m_StoreController;
//      private static IExtensionProvider m_StoreExtensionProvider;

//      public static string Deluxe = "com.cmgame.lifting.superhero.gym.clicker.goldmembership";
//      public static string RemoveAds = "com.cmgame.lifting.superhero.gym.clicker.removeads";
//      public static string DoubleIncome = "com.cmgame.lifting.superhero.gym.clicker.doubleincome";

//      public static string StarterPack = "com.cmgame.lifting.superhero.gym.clicker.starterpack";
//      public static string MiniPack = "com.cmgame.lifting.superhero.gym.clicker.minipack";
//      public static string BoosterPack = "com.cmgame.lifting.superhero.gym.clicker.boosterpack";
//      public static string ProPack = "com.cmgame.lifting.superhero.gym.clicker.propack";
//      public static string UltraPack = "com.cmgame.lifting.superhero.gym.clicker.ultrapack";
//      public static string RoyalPack = "com.cmgame.lifting.superhero.gym.clicker.royalpack";

//      private bool isRestore;
//      private void Awake()
//      {
//           if (instance == null)
//                instance = this;
//           else
//                Destroy(gameObject);
//           Debug.Log(PlayerPrefs.GetInt("RemoveAds"));
//           if (PlayerPrefs.GetInt("RemoveAds", 0) == 1)
//           {
//                // DemoRemoveAds.instance.btnAdsRemove.SetActive(false);
//           }
//           DontDestroyOnLoad(gameObject);
//      }
//      void Start()
//      {
//           // If we haven't set up the Unity Purchasing reference
//           if (m_StoreController == null)
//           {
//                // Begin to configure our connection to Purchasing
//                InitializePurchasing();
//           }
//      }

//      public void InitializePurchasing()
//      {
//           if (IsInitialized())
//           {
//                return;
//           }
//           var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
//           builder.AddProduct(Deluxe, ProductType.NonConsumable);
//           builder.AddProduct(RemoveAds, ProductType.NonConsumable);
//           builder.AddProduct(DoubleIncome, ProductType.NonConsumable);

//           builder.AddProduct(StarterPack, ProductType.Consumable);
//           builder.AddProduct(MiniPack, ProductType.Consumable);
//           builder.AddProduct(BoosterPack, ProductType.Consumable);
//           builder.AddProduct(ProPack, ProductType.Consumable);
//           builder.AddProduct(UltraPack, ProductType.Consumable);
//           builder.AddProduct(RoyalPack, ProductType.Consumable);
//           UnityPurchasing.Initialize(this, builder);
//      }


//      private bool IsInitialized()
//      {
//           return m_StoreController != null && m_StoreExtensionProvider != null;
//      }
//      Action _actionComplete;
//      Action _actionFail;
//      string idBuy;
//      public void BuyProduct(string productID, Action actionBuyComplete, Action actionBuyFail)
//      {
//           isRestore = false;
//           _actionComplete = actionBuyComplete;
//           _actionFail = actionBuyFail;
//           idBuy = productID;
//           BuyProductID(idBuy);
//      }
//      void BuyProductID(string productId)
//      {
//           if (IsInitialized())
//           {
//                Product product = m_StoreController.products.WithID(productId);
//                if (product != null && product.availableToPurchase)
//                {
//                     Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
//                     m_StoreController.InitiatePurchase(product);
//                }
//                else
//                {
//                     Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
//                }
//           }
//           else
//           {
//                Debug.Log("BuyProductID FAIL. Not initialized.");
//           }
//      }
//      /// <summary>
//      ///
//      /// </summary>
//      /// <param name="id">id product</param>
//      /// <returns></returns>
//      public string GetPrice(string id, string priceDefault)
//      {
//           string price = "";
//           try
//           {
//                price = m_StoreController.products.WithID(id).metadata.localizedPrice.ToString() + m_StoreController.products.WithID(id).metadata.isoCurrencyCode;
//                return price;
//           }
//           catch
//           {
//                return priceDefault;
//           }
//      }
//      public void RestorePurchases()
//      {
//           isRestore = true;
//           if (CheckNet())
//           {
//                // If Purchasing has not yet been set up ...
//                if (!IsInitialized())
//                {
//                     // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
//                     Debug.Log("RestorePurchases FAIL. Not initialized.");
//                     return;
//                }

//                // If we are running on an Apple device ...
//                if (Application.platform == RuntimePlatform.IPhonePlayer ||
//                    Application.platform == RuntimePlatform.OSXPlayer)
//                {
//                     // ... begin restoring purchases
//                     Debug.Log("RestorePurchases started ...");

//                     // Fetch the Apple store-specific subsystem.
//                     var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
//                     // Begin the asynchronous process of restoring purchases. Expect a confirmation response in
//                     // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
//                     apple.RestoreTransactions((result) =>
//                 {
//                      isRestore = false;

//                      if (result)
//                      {
//                           // No previous purchases to restore, so disable Restore button and re-enable Purchase button
//                           Debug.Log("restore nothing");
//                      }
//                      else
//                      {
//                           // Restore Fail Annoucement
//                           Debug.Log("restore fail");
//                      }
//                 });
//                }
//                // Otherwise ...
//                else
//                {
//                     // We are not running on an Apple device. No work is necessary to restore purchases.
//                     Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
//                }
//           }
//           else
//           {
//                Debug.Log("No network connection!");
//           }
//      }
//      //check internet
//      public bool CheckNet()
//      {
//           if (Application.internetReachability == NetworkReachability.NotReachable)
//           {
//                return false;
//           }
//           else
//           {
//                return true;
//           }
//      }

//      public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//      {
//           // Purchasing has succeeded initializing. Collect our Purchasing references.
//           Debug.Log("OnInitialized: PASS");

//           // Overall Purchasing system, configured with products for this application.
//           m_StoreController = controller;
//           // Store specific subsystem, for accessing device-specific store features.
//           m_StoreExtensionProvider = extensions;
// #if UNITY_ANDROID
//           //auto restore android
//           if (m_StoreController.products.WithID(Deluxe).hasReceipt)
//           {
//                PlayerPrefs.SetInt("DoubleIncome", 1);
//                GameManager.isDoubleMoney = true;
//                PlayerPrefs.SetInt("RemoveAds", 1);
//                AdManager.instance?.HideBanner();


//           }
//           if (m_StoreController.products.WithID(RemoveAds).hasReceipt)
//           {
//                //restore product
//                PlayerPrefs.SetInt("RemoveAds", 1);
//                AdManager.instance?.HideBanner();

//           }
//           if (m_StoreController.products.WithID(DoubleIncome).hasReceipt)
//           {
//                //restore product
//                GameManager.isDoubleMoney = true;
//                PlayerPrefs.SetInt("DoubleIncome", 1);

//           }

// #endif
//      }

//      public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
//      {
//           if (args == null || args.purchasedProduct == null)
//                return PurchaseProcessingResult.Pending;
//           if (!isRestore)
//           {
//                if (String.Equals(args.purchasedProduct.definition.id, idBuy, StringComparison.Ordinal))
//                {
//                     Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
//                     _actionComplete.Invoke();
//                }
//                // Or ... an unknown product has been purchased by this user. Fill in additional products here....
//                else
//                {
//                     _actionFail?.Invoke();
//                     Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
//                }
//           }
//           else
//           {
//                //restore product complete on ios
//                if (String.Equals(args.purchasedProduct.definition.id, Deluxe, StringComparison.Ordinal))
//                {
//                     PlayerPrefs.SetInt("DoubleIncome", 1);
//                     GameManager.isDoubleMoney = true;
//                     PlayerPrefs.SetInt("RemoveAds", 1);
//                     AdManager.instance?.HideBanner();
//                }
//                if (String.Equals(args.purchasedProduct.definition.id, RemoveAds, StringComparison.Ordinal))
//                {
//                     //restore product
//                     PlayerPrefs.SetInt("RemoveAds", 1);
//                     AdManager.instance?.HideBanner();

//                }
//                if (String.Equals(args.purchasedProduct.definition.id, DoubleIncome, StringComparison.Ordinal))
//                {
//                     //restore product
//                     GameManager.isDoubleMoney = true;
//                     PlayerPrefs.SetInt("DoubleIncome", 1);

//                }
//                isRestore = false;
//           }

//           return PurchaseProcessingResult.Complete;
//      }


//      public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//      {
//           _actionFail?.Invoke();
//           Debug.Log("OnPurchaseFailed: " + product.definition.id + "===reason: " + failureReason.ToString());
//      }

//      public void OnInitializeFailed(InitializationFailureReason error, string message)
//      {
//           Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
//      }

//      public void OnInitializeFailed(InitializationFailureReason error)
//      {
//           Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
//      }
// }
