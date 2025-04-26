using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSelfDestruction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
     StartCoroutine(SelfDestruction());   
    }

    IEnumerator SelfDestruction()
    {
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }

}
