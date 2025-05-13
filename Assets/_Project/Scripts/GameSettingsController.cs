using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Controls game settings by reading from a JSON file and applying settings to the game
/// </summary>
public class GameSettingsController : MonoBehaviour
{
    // Path to the settings file
    private string settingsFilePath;
    
    // Settings data structure
    [System.Serializable]
    public class GameSettings
    {
        public bool fullscreen = true;
        public int textureQuality = 0;
        public int antialiasing = 0;
        public int vSync = 0;
        public int resolutionIndex = 0;
        public float volume = 1.0f;
    }
    
    // Current game settings
    private GameSettings currentSettings;
    
    // Available screen resolutions
    private Resolution[] resolutions;

    void Awake()
    {
        // Initialize the settings file path
        settingsFilePath = Application.persistentDataPath + "/gamesettings.json";
        
        // Get available resolutions
        resolutions = Screen.resolutions;
        
        // Load and apply settings
        LoadSettings();
    }
    
    /// <summary>
    /// Loads settings from JSON file and applies them
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            // Check if the settings file exists
            if (File.Exists(settingsFilePath))
            {
                // Read settings from file
                string settingsJson = File.ReadAllText(settingsFilePath);
                currentSettings = JsonUtility.FromJson<GameSettings>(settingsJson);
                
                // Apply the loaded settings
                ApplySettings();
            }
            else
            {
                // Create default settings if file doesn't exist
                currentSettings = new GameSettings();
                SaveSettings();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading settings: " + e.Message);
            // Create default settings if there was an error
            currentSettings = new GameSettings();
        }
    }
    
    /// <summary>
    /// Applies current settings to the game
    /// </summary>
    public void ApplySettings()
    {
        // Apply fullscreen setting
        Screen.fullScreen = currentSettings.fullscreen;
        
        // Apply texture quality (0 = High, 1 = Medium, 2 = Low)
        QualitySettings.globalTextureMipmapLimit = currentSettings.textureQuality;
        
        // Apply anti-aliasing (0 = Off, 2 = 2x, 4 = 4x, 8 = 8x)
        QualitySettings.antiAliasing = currentSettings.antialiasing;
        
        // Apply VSync (0 = Off, 1 = Every V-Blank, 2 = Every Second V-Blank)
        QualitySettings.vSyncCount = currentSettings.vSync;
        
        // Apply screen resolution if valid index
        if (currentSettings.resolutionIndex >= 0 && currentSettings.resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[currentSettings.resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
        
        // Apply volume (assuming we're using AudioListener.volume)
        AudioListener.volume = currentSettings.volume;
        
        Debug.Log("Game settings applied successfully");
    }
    
    /// <summary>
    /// Saves current settings to JSON file
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            string settingsJson = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(settingsFilePath, settingsJson);
            Debug.Log("Settings saved to: " + settingsFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving settings: " + e.Message);
        }
    }
    
    /// <summary>
    /// Updates the fullscreen setting
    /// </summary>
    public void SetFullscreen(bool isFullscreen)
    {
        currentSettings.fullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
    }
    
    /// <summary>
    /// Updates the texture quality setting
    /// </summary>
    public void SetTextureQuality(int qualityIndex)
    {
        currentSettings.textureQuality = qualityIndex;
        QualitySettings.globalTextureMipmapLimit = qualityIndex;
    }
    
    /// <summary>
    /// Updates the anti-aliasing setting
    /// </summary>
    public void SetAntiAliasing(int aaIndex)
    {
        // Convert index to actual anti-aliasing value (0, 2, 4, 8)
        int aaValue = 0;
        switch (aaIndex)
        {
            case 1: aaValue = 2; break;
            case 2: aaValue = 4; break;
            case 3: aaValue = 8; break;
            default: aaValue = 0; break;
        }
        
        currentSettings.antialiasing = aaValue;
        QualitySettings.antiAliasing = aaValue;
    }
    
    /// <summary>
    /// Updates the VSync setting
    /// </summary>
    public void SetVSync(int vsyncIndex)
    {
        currentSettings.vSync = vsyncIndex;
        QualitySettings.vSyncCount = vsyncIndex;
    }
    
    /// <summary>
    /// Updates the resolution setting
    /// </summary>
    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            currentSettings.resolutionIndex = resolutionIndex;
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
    
    /// <summary>
    /// Updates the volume setting
    /// </summary>
    public void SetVolume(float volume)
    {
        currentSettings.volume = volume;
        AudioListener.volume = volume;
    }
    
    /// <summary>
    /// Gets the current settings object
    /// </summary>
    public GameSettings GetCurrentSettings()
    {
        return currentSettings;
    }
    
    /// <summary>
    /// Gets the available screen resolutions
    /// </summary>
    public Resolution[] GetAvailableResolutions()
    {
        return resolutions;
    }
}