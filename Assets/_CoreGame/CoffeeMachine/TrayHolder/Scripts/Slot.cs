using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public bool isUnlocked = true;
    public GameObject adsGO;
    public Transform posHolder;
    [SerializeField] Collider collider;
    public ParticleSystem boxInEff;
    private void Start()
    {
        collider.enabled = !isUnlocked;
    }

    public void ToggleAds(bool isUnlocked)
    {
        this.isUnlocked = isUnlocked;
        adsGO.SetActive(!isUnlocked);
        collider.enabled = !isUnlocked;
    }

}
