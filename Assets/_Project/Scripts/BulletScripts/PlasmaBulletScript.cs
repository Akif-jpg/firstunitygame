using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaBulletScript : MonoBehaviour
{
    public GameObject damageArea; // Hasar alanı objesi
    public ParticleSystem explosionEffect; // Patlama efekti
    public float waitBeforeExplosion = 2.0f; // Çarpışmadan patlama efektine kadar geçecek süre
    public float destroyAfterExplosion = 1.0f; // Patlama sonrası yok olma süresi
    
    private bool hasCollided = false; // Merminin bir şeye çarpıp çarpmadığını kontrol etmek için
    
    // Start is called before the first frame update
    void Start()
    {
        // Başlangıçta hasar alanını ve patlama efektini devre dışı bırak
        if (damageArea != null)
        {
            damageArea.SetActive(false);
        }
        
        if (explosionEffect != null)
        {
            explosionEffect.gameObject.SetActive(false);
        }
    }

    
    // Mermi herhangi bir şeye çarptığında
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided && collision.gameObject.tag != "PlasmaBomb")
        {
            hasCollided = true;
            
            // Merminin Rigidbody bileşenini al ve kinematik yap (artık fiziksel etkilerden etkilenmeyecek)
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // Çarpışma sonrası patlama etkisini başlat
            StartCoroutine(ExplodeAfterDelay());
        }
    }
    
    // Çarpışmadan belirli bir süre sonra patlamayı gerçekleştir
    private IEnumerator ExplodeAfterDelay()
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(waitBeforeExplosion);
        
        // Patlama efektini etkinleştir ve çalıştır
        if (explosionEffect != null)
        {
            explosionEffect.gameObject.SetActive(true);
            explosionEffect.Play();
        }
        
        // Hasar alanını etkinleştir
        if (damageArea != null)
        {
            damageArea.SetActive(true);
        }
        
        // Merminin görsel bileşenlerini devre dışı bırak
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // Patlama sonrası belirli bir süre sonra kendini yok et
        Destroy(gameObject, destroyAfterExplosion);
    }
}