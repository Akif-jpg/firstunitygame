using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float damageValue = 10f;
    [SerializeField] private float checkInterval = 0.2f; // Her 0.2 saniyede bir kontrol et
    
    private PlayerHealth playerHealth;
    private string damageId;
    private bool isApplyDamage;
    
    void Awake()
    {
        damageId = Guid.NewGuid().ToString();
        isApplyDamage = false;
    }
    
    void Start()
    {
        // CheckPlayerInRange fonksiyonunu periyodik olarak çağır
        InvokeRepeating("CheckPlayerInRange", 0f, checkInterval);
    }
    
    void CheckPlayerInRange()
    {
        bool isPlayerInDamageArea = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach(Collider collider in colliders)
        {
            if(collider.CompareTag("Player")) // CompareTag daha verimlidir
            {
                isPlayerInDamageArea = true;
                if(!isApplyDamage)
                {
                    this.playerHealth = collider.gameObject.GetComponent<PlayerHealth>();
                    playerHealth.AddDamage(damageValue, damageId);
                    isApplyDamage = true;
                    Debug.Log("Oyuncu hasar alanına girdi, hasar uygulanıyor: " + damageValue);
                }
                break; // Oyuncu bulundu, döngüden çık
            }
        }
        
        // Oyuncu artık hasar alanında değilse
        if(!isPlayerInDamageArea && isApplyDamage)
        {
            playerHealth.RemoveDamage(damageId);
            isApplyDamage = false;
            Debug.Log("Oyuncu hasar alanından çıktı, hasar durduruldu");
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    
    void OnDestroy()
    {
        // Script yok edildiğinde veya GameObject deaktive edildiğinde InvokeRepeating'i iptal et
        CancelInvoke("CheckPlayerInRange");
        
        // Eğer hala hasar veriyorsak, hasarı durdur
        if(isApplyDamage && playerHealth != null)
        {
            playerHealth.RemoveDamage(damageId);
        }
    }
}