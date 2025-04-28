using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeWeaponScript : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("List of weapon prefabs that can be switched between")]
    [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();
    
    [Tooltip("Position where weapons will be instantiated")]
    [SerializeField] private Transform weaponHolder;
    
    [Tooltip("Should the first weapon be equipped on start?")]
    [SerializeField] private bool equipWeaponOnStart = true;
    [Header("Weapon Indicators")]
    [Tooltip("Ammo amount indicator for weapons")]
    [SerializeField] private TextMeshProUGUI ammoText;

    // Internal variables
    private GameObject currentWeapon;
    private int currentWeaponIndex = -1;

    void Start()
    {
        // Validate settings
        if (weaponPrefabs.Count == 0)
        {
            Debug.LogWarning("No weapon prefabs assigned to ChangeWeaponScript!");
            return;
        }

        if (weaponHolder == null)
        {
            weaponHolder = transform;
            Debug.LogWarning("No weapon holder assigned, using this object's transform");
        }

        // Equip first weapon if specified
        if (equipWeaponOnStart)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        // Check for number key inputs (1-9)
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                // Convert to 0-based index for our array
                int weaponIndex = i;
                
                // Only switch if the weapon exists in our list
                if (weaponIndex < weaponPrefabs.Count)
                {
                    EquipWeapon(weaponIndex);
                }
            }
        }
    }

    /// <summary>
    /// Equips the weapon at the specified index
    /// </summary>
    /// <param name="index">Index of the weapon in the weaponPrefabs list</param>
    public void EquipWeapon(int index)
    {
        // Don't do anything if it's already equipped
        if (index == currentWeaponIndex)
            return;

        // Valid index check
        if (index < 0 || index >= weaponPrefabs.Count)
        {
            Debug.LogError($"Tried to equip weapon at invalid index: {index}");
            return;
        }

        // Destroy current weapon if there is one
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        // Instantiate new weapon
        currentWeapon = Instantiate(weaponPrefabs[index], weaponHolder.position, weaponHolder.rotation);
        currentWeapon.transform.SetParent(weaponHolder);
        currentWeaponIndex = index;

        Debug.Log($"Equipped weapon {index + 1}: {weaponPrefabs[index].name}");

        // You might want to do additional setup here like:
        // - Adjust weapon position/rotation
        // - Initialize weapon stats
        // - Play weapon switch animation
        // - Update UI
    }

    /// <summary>
    /// Switches to the next weapon in the list
    /// </summary>
    public void NextWeapon()
    {
        int nextIndex = (currentWeaponIndex + 1) % weaponPrefabs.Count;
        EquipWeapon(nextIndex);
    }

    /// <summary>
    /// Switches to the previous weapon in the list
    /// </summary>
    public void PreviousWeapon()
    {
        int prevIndex = (currentWeaponIndex - 1 + weaponPrefabs.Count) % weaponPrefabs.Count;
        EquipWeapon(prevIndex);
    }
}