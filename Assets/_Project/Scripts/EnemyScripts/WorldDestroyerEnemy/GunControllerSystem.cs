using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls multiple gun systems based on the proximity of a target Transform (usually the player).
/// Activates the primary gun (FireGunSystem) when the target is close.
/// Activates the secondary gun (SecondGunSystem) when the target is far away.
/// </summary>
public class GunControllerSystem : MonoBehaviour
{
    [Header("System References")]
    [Tooltip("Animator to change mita vegas playing animation")]
    [SerializeField] private Animator mitaVegasAnimator;

    [Tooltip("The primary FireGunSystem script to control (activates when player is CLOSE).")]
    [SerializeField] private FireGunSystem primaryFireGunSystem; // Yakın mesafede aktifleşen sistem

    [Tooltip("The secondary GunSystem script to control (activates when player is FAR).")]
    [SerializeField] private GameObject rocketLauncher; // Uzak mesafede aktifleşen sistem (Bunun türünü kendi silah script'inizin adıyla değiştirin!)

    [Header("Target Tracking")]
    [Tooltip("The Transform of the target (e.g., the player) to track distance from.")]
    [SerializeField] private Transform playerTransform; // Hedef oyuncunun Transform'u
    [Tooltip("The distance threshold. Below this activates the primary gun, above activates the secondary gun.")]
    [SerializeField] private float activationDistance = 10.0f; // Aktivasyon mesafe eşiği

    // Internal state to track if the player is currently within activation range
    private bool _isPlayerInRange = false;
    private bool _isInitialized = false; // Başlangıç durumunu kontrol etmek için flag

    // --- Unity Lifecycle Methods ---

    void Start()
    {
        // Validate references on start
        if (primaryFireGunSystem == null)
        {
            Debug.LogWarning("Primary FireGunSystem reference is not set. Close-range attacks won't work.", this);
        }
        if (rocketLauncher == null)
        {
            Debug.LogWarning("Secondary FarGunSystem reference is not set. Far-range attacks won't work.", this);
        }

        // Player transform might be set later, so only warn
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform is not set initially. Ensure it's assigned via Inspector or SetPlayerTransform().", this);
            // Oyuncu yoksa başlangıçta her iki sistemi de durdurmayı dene
            StopBothSystems();
            _isPlayerInRange = false; // Default state
        }
        else
        {
             // Oyuncu varsa başlangıç mesafesini kontrol et ve uygun sistemi başlat
            InitializeSystemsBasedOnDistance();
        }
        _isInitialized = true; // Başlatma tamamlandı
    }

    void Update()
    {
        // Player transform olmadan devam etme
        if (playerTransform == null)
        {
            // Oyuncu kaybolduysa ve sistemler çalışıyorsa durdur
             if(_isInitialized) // Sadece başlangıç sonrası için kontrol et
             {
                 StopBothSystems();
             }
            return; // Player yoksa Update'ten çık
        }

        // Mesafeyi hesapla (performans için SqrMagnitude kullan)
        float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;
        float sqrActivationDistance = activationDistance * activationDistance;

        // Oyuncu menzil İÇİNDE mi?
        if (sqrDistance <= sqrActivationDistance)
        {
            // Eğer daha önce menzil DIŞINDA idiyse (yani YENİ GİRDİ ise)
            if (!_isPlayerInRange)
            {
                //Debug.Log("Player entered range. Activating Primary, Deactivating Secondary."); // Opsiyonel log
                _isPlayerInRange = true;
                ActivatePrimarySystem(); // Birincil sistemi çalıştır
                DeactivateSecondarySystem(); // İkincil sistemi durdur
            }
            // Zaten menzil içindeyse bir şey yapmaya gerek yok, sistemler doğru durumda olmalı
        }
        // Oyuncu menzil DIŞINDA mı?
        else
        {
            // Eğer daha önce menzil İÇİNDE idiyse (yani YENİ ÇIKTI ise)
            if (_isPlayerInRange)
            {
                //Debug.Log("Player exited range. Deactivating Primary, Activating Secondary."); // Opsiyonel log
                _isPlayerInRange = false;
                DeactivatePrimarySystem(); // Birincil sistemi durdur
                ActivateSecondarySystem(); // İkincil sistemi çalıştır
            }
            // Zaten menzil dışındaysa bir şey yapmaya gerek yok, sistemler doğru durumda olmalı
        }
    }

    // --- Public Methods ---

    /// <summary>
    /// Sets the target Transform (player) for distance checking.
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        // Hedef değişmeden önce mevcut sistemleri durdur
        StopBothSystems();

        if (player != null)
        {
            playerTransform = player;
            // Yeni oyuncu atandıktan sonra başlangıç durumunu ayarla
            InitializeSystemsBasedOnDistance();
        }
        else
        {
            Debug.LogWarning("Attempted to set a null Player Transform.", this);
            playerTransform = null;
            _isPlayerInRange = false; // Oyuncu yoksa menzil dışı kabul edilebilir
        }
    }

    // --- Helper Methods ---

    /// <summary>Checks initial distance and activates/deactivates systems accordingly.</summary>
    private void InitializeSystemsBasedOnDistance()
    {
        if (playerTransform == null) return; // Oyuncu yoksa bir şey yapma

        float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;
        float sqrActivationDistance = activationDistance * activationDistance;

        if (sqrDistance <= sqrActivationDistance)
        {
            // Başlangıçta menzil içinde
            _isPlayerInRange = true;
            ActivatePrimarySystem();
            DeactivateSecondarySystem();
        }
        else
        {
            // Başlangıçta menzil dışında
            _isPlayerInRange = false;
            DeactivatePrimarySystem();
            ActivateSecondarySystem();
        }
    }

    /// <summary>Activates the primary (close-range) gun system if available.</summary>
    private void ActivatePrimarySystem()
    {
        if (primaryFireGunSystem != null)
        {
            this.mitaVegasAnimator.SetBool("IsFıreSystemActivated", true);
            primaryFireGunSystem.StartFire();
            // Debug.Log("Primary System Activated");
        }
    }

    /// <summary>Deactivates the primary (close-range) gun system if available.</summary>
    private void DeactivatePrimarySystem()
    {
        if (primaryFireGunSystem != null)
        {
            this.mitaVegasAnimator.SetBool("IsFıreSystemActivated", false);
            primaryFireGunSystem.StopFire();
             // Debug.Log("Primary System Deactivated");
        }
    }

    /// <summary>Activates the secondary (far-range) gun system if available.</summary>
    private void ActivateSecondarySystem()
    {
        if (rocketLauncher != null)
        {
            this.mitaVegasAnimator.SetBool("IsRocketsActivated", true);
            // İkinci silah script'inizdeki başlatma metodunu buraya yazın
            // Örneğin: secondaryFarGunSystem.StartShooting(); veya secondaryFarGunSystem.Activate();
            rocketLauncher.SetActive(true); // Metod adını kendi scriptinize göre değiştirin!
            // Debug.Log("Secondary System Activated");
        }
    }

    /// <summary>Deactivates the secondary (far-range) gun system if available.</summary>
    private void DeactivateSecondarySystem()
    {
        if (rocketLauncher != null)
        {
            this.mitaVegasAnimator.SetBool("IsRocketsActivated", false);
            // İkinci silah script'inizdeki durdurma metodunu buraya yazın
            // Örneğin: secondaryFarGunSystem.StopShooting(); veya secondaryFarGunSystem.Deactivate();
            rocketLauncher.SetActive(false); // Metod adını kendi scriptinize göre değiştirin!
             // Debug.Log("Secondary System Deactivated");
        }
    }

     /// <summary>Stops both gun systems if they are assigned.</summary>
    private void StopBothSystems()
    {
        DeactivatePrimarySystem();
        DeactivateSecondarySystem();
    }


    // --- Gizmos for Editor Visualization ---

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // Activation distance rengi
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}

// --- ÖNEMLİ NOTLAR ---
// 1. İkinci Silah Script'inizin Adı:
//    Kodda `SecondGunSystem` olarak geçen yeri, sizin uzak mesafe silahınızı kontrol eden script'in **gerçek adıyla** değiştirmeniz gerekiyor.
//    Örneğin, script'inizin adı `LaserCannonController` ise, `private LaserCannonController secondaryFarGunSystem;` yapmalısınız.
//
// 2. İkinci Silah Başlatma/Durdurma Metodları:
//    `ActivateSecondarySystem()` ve `DeactivateSecondarySystem()` metodları içinde yorum satırı olarak belirtilen yerlerde,
//    ikinci silah script'inizdeki **gerçek başlatma ve durdurma fonksiyonlarının adlarını** kullanmalısınız.
//    Örnek olarak `StartSystem()` ve `StopSystem()` kullanıldı, bunları kendi metod adlarınızla değiştirin (`StartShooting`, `StopShooting`, `Activate`, `Deactivate` vb.).
//
// 3. Inspector Ayarları:
//    - Bu script'i eklediğiniz GameObject'te Inspector panelinde yeni bir alan göreceksiniz: "Secondary Far Gun System".
//    - Buraya, sahnenizdeki ikinci silah sistemini kontrol eden script'in bulunduğu GameObject'i veya Component'i sürükleyip bırakın.
//    - "Primary Fire Gun System" alanına da ilk silahınızın (yakın mesafe) script'ini atadığınızdan emin olun.
//    - "Player Transform" alanına oyuncu GameObject'inizin Transform'unu atayın.
//    - "Activation Distance" değerini istediğiniz gibi ayarlayın. Bu mesafenin altı birincil silahı, üstü ikincil silahı tetikleyecektir.