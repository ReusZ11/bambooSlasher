using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _distanceToHit=1;
    [SerializeField] private float _distanceToMove=1;
    [Space] 
    [SerializeField] private bool _isHammer;
    [SerializeField] private float _cameraShakeDelay=0.1f;
    [SerializeField] private List<Sword> _swords = new List<Sword>();
    [SerializeField] private Ease _hitEase;
    [SerializeField] private float _delayBetweenSlashesOnUlt;
    [SerializeField] private PostProcessVolume _postProcessing;
    [SerializeField] private ParticleSystem _accelerationParticle;
    [SerializeField] private Transform _pointsToMove;
    private List<Transform> _bamboosPoints = new List<Transform>();
    private int _nextBambooIndex = -1;
    private Vector3 _startPos;
    private Vector3 _endedPos;
    private float _startMoveSpeed;
    private bool _canMove = false;
    private bool _canAutoSlash;
    private bool _isNear;
    public static PlayerController Singleton;
    private void Awake()
    {
        Singleton = this;
    }
    private void Start()
    {
        _startMoveSpeed = _movementSpeed;
        for (int i = 0; i < _pointsToMove.childCount; i++)
        {
            _bamboosPoints.Add(_pointsToMove.GetChild(i));
        }
    }
    private void Update()
    {
        if (_canMove)
            MoveToNextBamboo();
        Swipe();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_nextBambooIndex < _bamboosPoints.Count - 1)
            {
                _isNear = false;
                _canMove = true;
                _nextBambooIndex++;
                SetAccelerationEffect(true);
                DOTween.To(() => _movementSpeed, x => _movementSpeed = x, _startMoveSpeed, 0.5f);
            }
            else
            {
                _canMove = false;
            }
        }
        if (Input.GetKey(KeyCode.S) && !_canAutoSlash)
        {
            _canAutoSlash = true;
            StartCoroutine(Ulting());
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            _canAutoSlash = false;
        }
    }
    private void MoveToNextBamboo()
    {
        if (Vector3.Distance(transform.position, _bamboosPoints[_nextBambooIndex].transform.position) < 5* _distanceToMove)
        {
            if (!_isNear)
            {
                _isNear = true;
                SetAccelerationEffect(false);
                DOTween.To(() => _movementSpeed, x => _movementSpeed = x, _movementSpeed / 1000, 0.5f);
            }
            transform.position = Vector3.MoveTowards(transform.position, _bamboosPoints[_nextBambooIndex].transform.position, Time.deltaTime * _movementSpeed);
            if (Vector3.Distance(transform.position, _bamboosPoints[_nextBambooIndex].transform.position) < 2* _distanceToMove)
            {
                _canMove = false;
            }
        }
        else
            transform.position = Vector3.MoveTowards(transform.position, _bamboosPoints[_nextBambooIndex].transform.position, Time.deltaTime * _movementSpeed);

        var targetRotation = Quaternion.LookRotation(_bamboosPoints[_nextBambooIndex].transform.position - transform.position);
        // Smoothly rotate towards the target point.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

    }
    private void Swipe()
    {
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _startPos = _mainCamera.ScreenToViewportPoint(Input.GetTouch(0).position);
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                _endedPos = _mainCamera.ScreenToViewportPoint(Input.GetTouch(0).position);
                SliceWithSword();
            }
        }
#endif
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            _startPos = _mainCamera.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _endedPos = _mainCamera.ScreenToViewportPoint(Input.mousePosition);
            SliceWithSword();
        }
#endif
    }
    private void SliceWithSword()
    {
        _endedPos -= Vector3.one * 0.5f;
        _startPos -= Vector3.one * 0.5f;
        if (Vector2.Distance(_endedPos, _startPos) < 0.1f /*|| !_canSlash*/) return;
        _startPos *= 2;
        _endedPos *= 2;
        _startPos.z = 1;
        _endedPos.z = 1;
        Vector3 deltaPos = _startPos + (_endedPos - _startPos) / 2f;
        deltaPos.z = _distanceToHit;

        float angleZ = -Mathf.Atan2(_endedPos.y - _startPos.y, _endedPos.x - _startPos.x) * Mathf.Rad2Deg - 180;
        Sword sword = _swords[0];
        sword.transform.DOLocalRotate(new Vector3(190, 0, angleZ), 0.2f);
        sword.transform.DOLocalMove(deltaPos, 0.2f).SetEase(_hitEase).OnStart(() =>
        {
            if(_isHammer)
                sword.PlayParticle();
        }).OnComplete(() =>
        {
            _mainCamera.DORewind();
            _mainCamera.DOShakePosition(0.4f, 0.2f).SetDelay(_cameraShakeDelay);
            if(!_isHammer)
            sword.PlayParticle();
            sword.PlayAnimation();
        });
        sword.ResetPositionAndRotation(0.5f);
    }
    public void PlayEffect() {
        _swords[0].PlayHitEffect();
    }
    private IEnumerator Ulting()
    {
        foreach (var item in _swords)
        {
            item.SetDuration(0.5f);
        }
        while (_canAutoSlash)
        {
            OverSlice(_swords[0]);
            yield return new WaitForSeconds(_delayBetweenSlashesOnUlt);
        }
        foreach (var item in _swords)
        {
            item.SetDuration(2f);
        }
    }
    private void OverSlice(Sword sword)
    {
        _endedPos = Random.insideUnitSphere;
        _startPos = Random.insideUnitSphere;

        Vector3 deltaPos = _startPos + (_endedPos - _startPos) / 2f;
        deltaPos.z = _distanceToHit;

        float angleZ = -Mathf.Atan2(_endedPos.y - _startPos.y, _endedPos.x - _startPos.x) * Mathf.Rad2Deg - 180;

        sword.transform.DOLocalRotate(new Vector3(190, 0, angleZ), 0.1f);
        sword.transform.DOLocalMove(deltaPos, 0.1f).OnComplete(() =>
        {
            _mainCamera.DORewind();
            _mainCamera.DOShakePosition(0.1f, 0.2f);
            sword.PlayParticle();
            sword.PlayAnimation();
        });
        sword.ResetPositionAndRotation(0.3f);
    }
    private void SetAccelerationEffect(bool value)
    {
        _postProcessing.profile.TryGetSettings(out ChromaticAberration _chromaticAberration);
        DOTween.To(() => _chromaticAberration.intensity.value, x => _chromaticAberration.intensity.value = x, value ? 1 : 0.1f, 0.5f);

        if (_postProcessing.profile.TryGetSettings(out LensDistortion lensDistortion))
            DOTween.To(() => lensDistortion.intensity.value, x => lensDistortion.intensity.value = x, value ? -60 : 0, 0.5f);
        if (value) _accelerationParticle.Play();
        else _accelerationParticle.Stop();
    }
}