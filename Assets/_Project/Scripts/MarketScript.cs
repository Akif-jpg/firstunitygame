using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the functionality of the in-game market/shop system.
/// Handles showing/hiding the market UI and processing purchases.
/// </summary>
public class MarketScript : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI References")]
    [Tooltip("The canvas containing all market UI elements")]
    [SerializeField] private Canvas marketCanvas;
    [Tooltip("For see warning when try use market more than one time in same stage")]
    [SerializeField] private Canvas warningCanvas;

    [Header("Game References")]
    [Tooltip("Reference to the main game controller")]
    [SerializeField] private GameController gameController;

    [Header("Weapon References")]
    [Tooltip("Reference to the plasma rifle script for upgrades")]
    [SerializeField] private PlasmaRif plasmaRiffleBulletSpawner;
    [Tooltip("Reference to the plasma rifle GameObject (to check if active)")]
    [SerializeField] private GameObject plasmaRiffleObject;

    [Tooltip("Reference to the plasma shotgun script for upgrades")]
    [SerializeField] private PlasmaShotgunSpawner plasmaShotgunBulletSpawner;
    [Tooltip("Reference to the plasma shotgun GameObject (to check if active)")]
    [SerializeField] private GameObject plasmaShotgunObject;

    [Header("Player References")]
    [Tooltip("Reference to the player's health component for healing upgrades")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Upgrade Values")]
    [Tooltip("Amount of health to add per upgrade")]
    [SerializeField] private int healthUpgradeAmount = 30;

    [Tooltip("Base amount of rifle ammo to add (will be multiplied by game stage)")]
    [SerializeField] private int rifleAmmoBaseAmount = 5;

    [Tooltip("Base amount of shotgun ammo to add (will be multiplied by game stage)")]
    [SerializeField] private int shotgunAmmoBaseAmount = 2;

    [Tooltip("Whether to scale ammo upgrades with game stage")]
    [SerializeField] private bool scaleAmmoWithStage = true;

    #endregion

    #region Private Variables

    /// <summary>
    /// Tracks whether the player is currently in the market trigger area
    /// </summary>
    private bool isMarketActive;
    /// <summary>
    /// For prevent use market again and again in same game stage.
    /// </summary>
    private int latestGameStageUsed;

    #endregion

    #region Unity Lifecycle Methods

    /// <summary>
    /// Initialize market state when the script awakens
    /// </summary>
    private void Awake()
    {
        this.latestGameStageUsed = 0;
        // Ensure market is hidden by default
        marketCanvas.gameObject.SetActive(false);
        warningCanvas.gameObject.SetActive(false);
        this.isMarketActive = false;
    }

    /// <summary>
    /// Checks for market interaction input every frame
    /// </summary>
    private void Update()
    {
        // Press E to interact with the market when in range
        if (Input.GetKeyDown(KeyCode.E) && this.isMarketActive)
        {
            ApplyMarket();
        }
    }

    /// <summary>
    /// Activates the market when player enters the trigger area
    /// </summary>
    /// <param name="other">The collider entering this trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.isMarketActive = true;
            marketCanvas.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Deactivates the market when player leaves the trigger area
    /// </summary>
    /// <param name="other">The collider exiting this trigger</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.isMarketActive = false;
            marketCanvas.gameObject.SetActive(false);
            warningCanvas.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Market Methods

    /// <summary>
    /// Processes market purchases and applies selected upgrades
    /// based on the current game stage/wave
    /// </summary>
    private void ApplyMarket()
    {
        int gameStage = this.gameController.GetWaveState();

        // Check if already used market in this stage
        if (this.latestGameStageUsed <= gameStage)
        {
            // Apply health upgrade
            ApplyHealthUpgrade();

            // Apply weapon upgrades
            ApplyWeaponUpgrades(gameStage);

            // Update the latest stage used
            this.latestGameStageUsed += 1;
        }
        else
        {
            // Show warning if already used in this stage
            this.warningCanvas.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Applies health upgrade to the player
    /// </summary>
    private void ApplyHealthUpgrade()
    {
        if (playerHealth != null)
        {
            playerHealth.AddCharacterHealth(healthUpgradeAmount);
        }
    }

    /// <summary>
    /// Applies weapon upgrades if the weapons are active
    /// </summary>
    /// <param name="gameStage">Current game stage for scaling upgrades</param>
    private void ApplyWeaponUpgrades(int gameStage)
    {
        // Calculate ammo amounts
        int rifleAmmo = scaleAmmoWithStage ? rifleAmmoBaseAmount * gameStage : rifleAmmoBaseAmount;
        int shotgunAmmo = scaleAmmoWithStage ? shotgunAmmoBaseAmount * gameStage : shotgunAmmoBaseAmount;

        // Apply rifle ammo upgrade if the weapon is active
        if (plasmaRiffleObject != null && plasmaRiffleObject.activeInHierarchy &&
            plasmaRiffleBulletSpawner != null)
        {
            plasmaRiffleBulletSpawner.AddAdditionalAmmo(rifleAmmo);
            Debug.Log($"Added {rifleAmmo} ammo to Plasma Rifle");
        }
        else if (plasmaRiffleObject != null && plasmaRiffleBulletSpawner != null)
        {
            plasmaRiffleObject.SetActive(true);
            plasmaRiffleBulletSpawner.AddAdditionalAmmo(rifleAmmo);
            plasmaRiffleObject.SetActive(false);
        }

        // Apply shotgun ammo upgrade if the weapon is active
        if (plasmaShotgunObject != null && plasmaShotgunObject.activeInHierarchy &&
            plasmaShotgunBulletSpawner != null)
        {
            plasmaShotgunBulletSpawner.AddAdditionalAmmo(shotgunAmmo);
            Debug.Log($"Added {shotgunAmmo} ammo to Plasma Shotgun");
        }
        else if (plasmaShotgunObject != null && plasmaShotgunBulletSpawner != null)
        {
            plasmaShotgunObject.SetActive(true);
            plasmaShotgunBulletSpawner.AddAdditionalAmmo(shotgunAmmo);
            plasmaShotgunObject.SetActive(false);
        }
    }

    #endregion
}