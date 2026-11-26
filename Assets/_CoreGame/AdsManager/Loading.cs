using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Image fillBar;
    public TextMeshProUGUI percentText;
    private AsyncOperation asyncOperation;
    private void Start()
    {
        Timing.RunCoroutine(StartLoading());
    }
    private IEnumerator<float> StartLoading()
    {
        DOVirtual.Float(0f, 1f, 5f, UpdateFillAmount).OnComplete(LoadNextScene);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        asyncOperation = SceneManager.LoadSceneAsync(nextSceneIndex);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            yield return Timing.WaitForSeconds(0);
        }
    }
    private void UpdateFillAmount(float fillAmount)
    {
        fillBar.fillAmount = fillAmount;
        percentText.text = Util.Convert(fillAmount * 100) + "%";
    }
    private void LoadNextScene()
    {
        asyncOperation.allowSceneActivation = true;
        // AdManager.instance.ShowOpen();
    }
}
