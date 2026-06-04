using System;
using BepInEx;
using BepInEx.Configuration;
using ComputerysModdingUtilities;
using HarmonyLib;
using ImprovedRebinds;
using StraftTweaks;
using UnityEngine;
using HoverInfo;

[assembly: StraftatMod(isVanillaCompatible: true)]

[BepInDependency(ModMenu.PluginInfo.guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class STRAFTweakPlugin : BaseUnityPlugin
{
    internal static STRAFTweakPlugin Instance;

    Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    void Awake()
    {
        _harmony.PatchAll();
        Instance = this;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        InitConfig();

        var modMenuLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModMenu.PluginInfo.guid);
        if (modMenuLoaded) STRAFTweakModMenuCompat.Start();
    }

    void InitConfig()
    {
        SetJoinMessage.message = Config.Bind("Custom Join Message", "Message", "{USER} has joined the lobby", "{USER} will be replaced with your username.");

        ScrollJump.isEnabled = Config.Bind("Binding", "Scroll to jump", false);

        OutlineColor.weaponColor = Config.Bind("Outline Colors", "Weapon Outline Color", new Color(1, 0.8196079f, 0.427451f));
        OutlineColor.weaponTextColor = Config.Bind("Outline Colors", "Weapon Interact Text Color", OutlineColor.DefaultTextColor);
        OutlineColor.weaponTextBrightness = Config.Bind("Outline Colors", "Weapon Interact Text Brightness", 1f);
        ApplyRainbow.enable = Config.Bind("Outline Colors", "Enable Rainbow Outlines", false);
        ApplyRainbow.rainbowSpeed = Config.Bind("Outline Colors", "Rainbow Fluctuation Speed", 0.2f, "i have no idea what the unit is for this");

        WeaponInteractPrompt.enablePrompt = Config.Bind("Text Popups", "Enable Weapon Interact Text", true);
        DoorInteractPrompt.enablePrompt = Config.Bind("Text Popups", "Enable Door Interact Text", true);
        WeaponInteractPrompt.disableKey = Config.Bind("Text Popups", "Disable Key Prompt", true, "Transform \"Handgun [F]\" into just \"Handgun\". Does the same for door interactions.");

        MinHUDPatch.hideFPS = Config.Bind("HUD", "Hide FPS Display", true);
        MinHUDPatch.hidePing = Config.Bind("HUD", "Hide Ping Display", true);

        OutlineColor.weaponColor.SettingChanged += OutlineColor.UpdateColorsFromConfig;
        OutlineColor.weaponTextColor.SettingChanged += OutlineColor.UpdateColorsFromConfig;
        OutlineColor.weaponTextBrightness.SettingChanged += OutlineColor.UpdateColorsFromConfig;
        ScrollJump.isEnabled.SettingChanged += (_, __) => ScrollJump.Postfix();
        ApplyRainbow.enable.SettingChanged += OutlineColor.UpdateColorsFromConfig;
    }

    void OnDestroy()
    {
        _harmony.UnpatchSelf();
        OutlineColor.weaponColor.SettingChanged -= OutlineColor.UpdateColorsFromConfig;
        OutlineColor.weaponTextColor.SettingChanged -= OutlineColor.UpdateColorsFromConfig;
        OutlineColor.weaponTextBrightness.SettingChanged -= OutlineColor.UpdateColorsFromConfig;
        ScrollJump.isEnabled.SettingChanged -= (_, __) => ScrollJump.Postfix();
        ApplyRainbow.enable.SettingChanged -= OutlineColor.UpdateColorsFromConfig;
    }

    // Debug
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.V))
    //     {
    //         if (gameObject.TryGetComponent(out TestBehav test))
    //             UnityEngine.Object.Destroy(test);
    //         else
    //             gameObject.AddComponent<TestBehav>();
    //     }
    // }
}

static class STRAFTweakModMenuCompat
{
    internal static void Start()
    {
        // Hot relaod
        try
        {
            ModMenu.Api.ModMenuCustomisation.SetPluginDescription("Tweak things like weapon outline color or mouse wheel binds :D\n\nChanges are applied immediately.\n\n");
            // ModMenu.Api.ModMenuCustomisation.RegisterContentBuilder(ConfigBuilder);
        }
        catch (InvalidOperationException) { }
    }

    static void ConfigBuilder(ModMenu.Api.OptionListContext c)
    {
    }
}