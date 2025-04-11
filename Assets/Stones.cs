using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stones : MonoBehaviour
{
    [SerializeField] private float _power, _radius;
    private bool _isBroken;
    private void PushAway(Vector3 point)
    {
        if (!_isBroken)
        {
            int childsCount = transform.childCount;
            for (int i = 0; i < childsCount; i++)
            {
                Transform parent = transform.GetChild(i);
                Transform childs = parent.GetChild(0).transform;
                print(parent.name + " " + childs.name);
                childs.parent = transform;
                childs.gameObject.SetActive(true);
                Destroy(parent.gameObject);
            }
            _isBroken = true;
        }
        RaycastHit[] hits = Physics.SphereCastAll(point, _radius,Vector3.one);
        print(hits.Length);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.TryGetComponent(out Rigidbody rigidbody))
                rigidbody.AddExplosionForce(_power, point, _radius);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pusher"))
        {
            PlayerController.Singleton.PlayEffect();
            PushAway(other.transform.position);
        }
    }
}