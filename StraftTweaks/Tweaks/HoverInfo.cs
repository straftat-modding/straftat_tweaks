
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace HoverInfo;

[HarmonyPatch(typeof(PauseManager), "Awake")]
static class OutlineColor
{
    internal static readonly Color DefaultTextColor = new Color(2, 1.106f, 0);
    internal static ConfigEntry<Color> weaponColor;
    internal static ConfigEntry<Color> weaponTextColor;
    internal static ConfigEntry<float> weaponTextBrightness;

    static bool HasRanOnStartup = false;
    static void Postfix()
    {
        if (!HasRanOnStartup)
        {
            UpdateColorsFromConfig();
            HasRanOnStartup = true;
        }
    }

    internal static readonly int weaponOutline = Shader.PropertyToID("_Color_Outline");
    internal static readonly int textOutline = Shader.PropertyToID("_OutlineColor");
    internal static readonly int outlineShaderID = Shader.Find("S_WeaponOutline_00").GetInstanceID();
    static Lazy<Material[]> _weaponMaterials = new(() =>
    {
        return Resources.FindObjectsOfTypeAll<Material>()
        .Where(mat => mat.shader.GetInstanceID() == outlineShaderID).ToArray();
    });

    internal static void UpdateColorsFromConfig(object sender, EventArgs args)
    {
        UpdateColorsFromConfig();
    }

    internal static void UpdateColorsFromConfig()
    {
        // Update stored materials
        _weaponMaterials.Value.Do(mat => mat.SetColor(weaponOutline, weaponColor.Value));

        // Update live materials
        foreach (var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat && mat.shader.GetInstanceID() == outlineShaderID)
                    mat.SetColor(weaponOutline, weaponColor.Value);
            }
        }

        // Weapon text yoinked from kestrel; Default text is RGBA(2.000, 1.106, 0.000, 1.000)

        Color HDR_color = weaponTextColor.Value;
        if (HDR_color != DefaultTextColor)
            HDR_color = weaponTextColor.Value * Vector4.one * weaponTextBrightness.Value;
        PauseManager.Instance.grabPopup.fontSharedMaterial.SetColor(textOutline, HDR_color);
        PauseManager.Instance.interactPopup.fontSharedMaterial.SetColor(textOutline, HDR_color);
    }
}

[HarmonyPatch(typeof(ItemBehaviour), "OnFocus")]
static class WeaponInteractPrompt
{
    internal static ConfigEntry<bool> enablePrompt;
    internal static ConfigEntry<bool> disableKey;

    // Could be transpiled to avoid double string set / extra function call
    static void Postfix(ItemBehaviour __instance)
    {
        PauseManager.Instance.grabPopup.gameObject.SetActive(enablePrompt.Value);
        if (disableKey.Value)
            PauseManager.Instance.grabPopup.text = __instance.weaponName.ToLower();
    }
}

[HarmonyPatch(typeof(PhysicsProp), "OnFocus")]
static class BarrelInteractPrompt
{
    static void Postfix(PhysicsProp __instance)
    {
        PauseManager.Instance.interactPopup.gameObject.SetActive(WeaponInteractPrompt.enablePrompt.Value);
        if (WeaponInteractPrompt.disableKey.Value)
            PauseManager.Instance.interactPopup.text = (__instance.sync___get_value_grabbed() ? "drop" : "grab") + " " + __instance.popupText.ToLower();
    }
}

[HarmonyPatch(typeof(Door), "OnFocus")]
static class DoorInteractPrompt
{
    internal static ConfigEntry<bool> enablePrompt;

    static void Postfix(Door __instance)
    {
        PauseManager.Instance.interactPopup.gameObject.SetActive(enablePrompt.Value);
        if (WeaponInteractPrompt.disableKey.Value)
            PauseManager.Instance.interactPopup.text = (__instance.sync___get_value_isOpen() ? __instance.closeDoor.ToLower() : __instance.popupText.ToLower());
    }
}

[HarmonyPatch(typeof(ItemBehaviour), "OnFocus")]
static class ApplyRainbow
{
    internal static ConfigEntry<bool> enable;
    internal static ConfigEntry<float> rainbowSpeed;

    // IDK why this method needs to be called every frame TwT
    static void Postfix(ItemBehaviour __instance)
    {
        if (enable.Value && !__instance.gameObject.GetComponent<RainbowOutline>())
            __instance.gameObject.AddComponent<RainbowOutline>().mats = __instance.hoveredObjectMat;
    }
}

[HarmonyPatch(typeof(ItemBehaviour), "OnLoseFocus")]
static class RemoveRainbow
{
    static void Postfix(ItemBehaviour __instance)
    {
        if (__instance.gameObject.GetComponent<RainbowOutline>())
            UnityEngine.Object.Destroy(__instance.gameObject.GetComponent<RainbowOutline>());
    }
}


class RainbowOutline : MonoBehaviour
{
    internal List<Material> mats;

    void Update()
    {
        float hue = Time.time * ApplyRainbow.rainbowSpeed.Value % 1.0f;
        Color rainbowColor = Color.HSVToRGB(hue, 1.0f, 1.0f);
        foreach (var mat in mats)
        {
            mat.SetColor(OutlineColor.weaponOutline, rainbowColor);
        }

        var HDR_color = rainbowColor * Vector4.one * OutlineColor.weaponTextBrightness.Value;
        PauseManager.Instance.grabPopup.fontSharedMaterial.SetColor(OutlineColor.textOutline, HDR_color);
        PauseManager.Instance.interactPopup.fontSharedMaterial.SetColor(OutlineColor.textOutline, HDR_color);
    }
}