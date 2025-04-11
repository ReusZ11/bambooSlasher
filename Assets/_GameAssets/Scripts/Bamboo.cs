using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bamboo : MonoBehaviour
{
    [SerializeField] private RayfireRigid[] _bambooRayfire = new RayfireRigid[] { };
    [SerializeField] private float _power, _radius;
    private List<Rigidbody> _rigidbodys = new List<Rigidbody>();
    private bool _isActivated;
    private void Start()
    {
        Invoke(nameof(OnDisableKinematicRigidbody), .1f);
    }
    private void OnDisableKinematicRigidbody()
    {
        for (int i = 0; i < _bambooRayfire.Length; i++)
        {
            if (_bambooRayfire[i].GetComponent<Rigidbody>() != null)
            {
                _bambooRayfire[i].GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _bambooRayfire[i].GetComponent<Rigidbody>().isKinematic = true;
                _rigidbodys.Add(_bambooRayfire[i].GetComponent<Rigidbody>());
            }
        }
    }
    private void ActivateRigids()
    {
        _isActivated = true;
        //GetComponent<Collider>().enabled = false;
        for (int i = 0; i < _bambooRayfire.Length; i++)
        {
            _bambooRayfire[i].physics.useGravity = true;
            _rigidbodys[i].isKinematic = false;
            _rigidbodys[i].useGravity = true;
        }
    }
    private void PushAway(Vector3 point) {
        RaycastHit[] hits=Physics.BoxCastAll(transform.position, Vector3.one, Vector3.up*0.5f);
        for (int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider.TryGetComponent(out Rigidbody rigidbody))
                rigidbody.AddExplosionForce(_power, point, _radius);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Blade"))
        {
            PlayerController.Singleton.PlayEffect();
            if (!_isActivated)
                ActivateRigids();
        }
        else if (other.CompareTag("Pusher")) {
            PushAway(other.transform.position);
        }
    }
}