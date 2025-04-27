using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSelfDestruction : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float destructionDelay = 1f;
    void Start()
    {
     StartCoroutine(SelfDestruction());   
    }

    IEnumerator SelfDestruction()
    {
        yield return new WaitForSeconds(destructionDelay);
        Destroy(this.gameObject);
    }

}
