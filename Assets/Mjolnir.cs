using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mjolnir : MonoBehaviour
{
    [SerializeField] private Material _material;
    private float _matIntensity;
    private void Start()
    {
        DOTween.To(() => _matIntensity, x => _matIntensity = x, 3, 1).SetLoops(-1,LoopType.Yoyo);
    }
    void Update()
    {
        _material.SetVector("_EmissionColor", new Vector4(0, 0.1950936f, 0.245283f) * _matIntensity);
        //_material.SetColor("_EMISSION", new Color(0.0927F, 0.4852F, 0.2416F, 0.42F));
    }
}
