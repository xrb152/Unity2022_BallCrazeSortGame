using System;
using System.Linq;
using UnityEngine;
using Observer;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class CurrenciesManager : Singleton<CurrenciesManager>
{
    public Currency[] currencies;

    public override void Awake()
    {
        base.Awake();

    }
    private void Start()
    {
        foreach (var currency in currencies)
        {
            currency.Initialize();
        }
    }
    public void UpdateValue(TypeCurrency typeCurrency, int value)
    {
        Currency currency = currencies.FirstOrDefault(x => x.typeCurrency == typeCurrency);
        if (currency != null)
        {
            currency.UpdateValueCurrency(value);

            EventDispatcher.Instance.PostEvent(EventID.OnChangeMoney);

            currency.SetText();
        }
        else
        {
            Debug.LogWarning($"Currency of type {typeCurrency} not found.");
        }
    }

    public int GetValue(TypeCurrency typeCurrency)
    {
        Currency currency = currencies.FirstOrDefault(x => x.typeCurrency == typeCurrency);
        return currency != null ? currency.currentValue : 0;
    }
    public float radius = 100f;
    public void FlyingIcon(Transform startPos, bool isOnWorld, Transform parent, int num = 1)
    {
        GlobalAudioPlayer.PlaySFX("FlyingMoney");
        int totalIcon = num;
        // value /= totalIcon;
        float delay = 0;
        while (totalIcon > 0)
        {
            GameObject eff = new GameObject();
            eff.transform.SetParent(parent);
            eff.transform.localScale = Vector3.zero;
            eff.transform.localRotation = Quaternion.identity;

            if (startPos == null) eff.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            else eff.transform.position = startPos.position;
            if (isOnWorld)
            {
                Vector3 viewportPos = GameManager.i.mainCamera.WorldToViewportPoint(startPos.position + Vector3.up);
                Vector3 canvasPos = new Vector3(
                    viewportPos.x * UIManager.i.mainCanvas.sizeDelta.x - UIManager.i.mainCanvas.sizeDelta.x * 0.5f,
                    viewportPos.y * UIManager.i.mainCanvas.sizeDelta.y - UIManager.i.mainCanvas.sizeDelta.y * 0.5f,
                    0
                );


                eff.transform.localPosition = canvasPos;

            }

            Image image = eff.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = currencies[0].icon;
            image.SetNativeSize();

            Vector3 tempPos = (Vector2)eff.transform.position - (UnityEngine.Random.insideUnitCircle * 500f);
            Transform tempTarget = currencies[0].moneyTarget;

            Vector3[] path = new Vector3[2]
            {
               tempPos,
               tempTarget.position
            };

            eff.transform.DOScale(1f, 0.1f).SetDelay(delay);
            eff.transform.DOPath(path, 0.5f, PathType.CatmullRom).SetEase(Ease.OutQuad).SetDelay(delay).OnComplete(() =>
            {
                Destroy(eff.gameObject);
                tempTarget.parent.localScale = Vector3.one;

                EventDispatcher.Instance.PostEvent(EventID.OnChangeMoney);
                GlobalAudioPlayer.PlaySFX("MoneyInBar");

                tempTarget.parent.DOScale(1.1f, 0.1f).OnComplete(() =>
                {
                    tempTarget.parent.DOScale(1f, 0.1f);

                });
            });

            delay += 0.05f;
            totalIcon--;
        }


    }
    // Vector2 GetRandomPointOnCircleEdge(Vector2 center, float radius)
    // {
    //     float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

    //     float x = center.x + Mathf.Cos(angle) * radius;
    //     float y = center.y + Mathf.Sin(angle) * radius;

    //     return new Vector2(x, y);
    // }
}

[Serializable]
public class Currency
{
    public TypeCurrency typeCurrency;
    public int startValue;
    public TextMeshProUGUI currencyText;
    public Sprite icon;
    public Transform moneyTarget;
    private int _currentValue;

    public int currentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            PlayerPrefs.SetInt("currentValue" + typeCurrency, _currentValue);
        }

    }

    public void Initialize()
    {
        _currentValue = PlayerPrefs.GetInt("currentValue" + typeCurrency, startValue);
        SetText();
    }

    public void SetText()
    {
        currencyText.text = Util.Convert(_currentValue);
    }
    public void UpdateValueCurrency(int value)
    {
        currentValue += value;

    }
}

public enum TypeCurrency
{
    Coin
}