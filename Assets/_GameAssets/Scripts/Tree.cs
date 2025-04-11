using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    IEnumerator Start()
    {
        while (true)
        {
            if (Vector3.Distance(PlayerController.Singleton.transform.position, transform.position) < 100)
            {
                gameObject.SetActive(true);
            }
            else { 
                gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.5f); 
        }
    }

}
