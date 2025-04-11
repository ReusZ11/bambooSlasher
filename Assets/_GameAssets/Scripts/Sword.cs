using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Sword : MonoBehaviour
{
    [SerializeField] private DOTweenAnimation _slashAnimation;
    [SerializeField] private ParticleSystem _slashParticle;
    [SerializeField] private ParticleSystem _hitParticle;
    [SerializeField] private Ease _returnEase;
    [SerializeField] private Transform _lightsTranform;
    private Vector3 _basePos;
    private Vector3 _baseRot;
    private Vector3 _baseRotWorld;
    private void Start()
    {
        _basePos = transform.localPosition;
        _baseRot = transform.localEulerAngles;
        _baseRotWorld = transform.eulerAngles;
    }
    private void Update()
    {
        if (_lightsTranform)
            _lightsTranform.transform.eulerAngles = _baseRotWorld;
    }
    public void PlayParticle() {
        _slashParticle.Play();
    }
    public void PlayAnimation() {
        _slashAnimation.DORewind();
        _slashAnimation.DORestart();
    }
    public void PlayHitEffect() {
        if (_hitParticle)
        {
            _hitParticle.Play();

        }
    }
    public void SetDuration(float value) {
        _slashAnimation.duration *= value;
    }
    public void ResetPositionAndRotation(float setDelay = 0) {
        transform.DOLocalMove(_basePos, 0.2f).SetDelay(setDelay).SetEase(_returnEase);
        transform.DOLocalRotate(_baseRot, 0.2f).SetDelay(setDelay);
    }
}