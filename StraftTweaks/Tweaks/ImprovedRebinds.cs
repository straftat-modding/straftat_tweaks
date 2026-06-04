
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ImprovedRebinds;

[HarmonyPatch(typeof(InputManager), "LoadBindingOverride")]
static class AddKeyPressInput
{
    // Hard coded values should be fine because this can only ever be bound to scroll AFAIK. The controller weapon swap is bound to some other action name I think....
    static void Prefix(string actionName)
    {
        if (actionName != "ChangeWeapon") return;
        InputAction inputAction = InputManager.inputActions.asset.FindAction(actionName);

        if (inputAction.bindings.Count == 1)
        {
            inputAction.ChangeBinding(0).Erase();
            inputAction.AddCompositeBinding("2DVector").With("Up", "");
            if (PlayerPrefs.GetString("PlayerControls (UnityEngine.InputSystem.InputActionAsset):PlayerChangeWeapon1").IsNullOrWhiteSpace())
                inputAction.AddBinding("<Mouse>/scroll");
        }
    }
}

[HarmonyPatch(typeof(InputManager), "DoRebind")]
static class OnBind
{
    static void Prefix(InputAction actionToRebind, ref bool allCompositeParts)
    {
        if (actionToRebind.name == "ChangeWeapon")
        {
            if (actionToRebind.bindings.Count == 3)
                actionToRebind.ChangeBinding(2).Erase();
            actionToRebind.expectedControlType = "Button";
            allCompositeParts = false;
        }
    }
}

[HarmonyPatch(typeof(ReBindUI), "UpdateUI")]
static class UpdateBindUI
{
    static void Postfix(ReBindUI __instance)
    {
        var inputAction = __instance.inputActionReference.action;
        if (inputAction.name == "ChangeWeapon")
        {
            var overrideString = PlayerPrefs.GetString("PlayerControls (UnityEngine.InputSystem.InputActionAsset):PlayerChangeWeapon1");
            if (overrideString.IsNullOrWhiteSpace())
            {
                InputManager.inputActions.asset.FindAction("ChangeWeapon").AddBinding("<Mouse>/scroll");
                __instance.rebindText.text = "Scroll";
            }
            else
            {
                __instance.rebindText.text = overrideString[(overrideString.IndexOf("/") + 1)..].ToUpper();
            }
        }
    }
}

[HarmonyPatch(typeof(InputManager), "LoadBindingOverride")]
static class ScrollJump
{
    internal static ConfigEntry<bool> isEnabled;

    internal static void Postfix()
    {
        var inputAction = InputManager.inputActions.asset.FindAction("Jump");
        var indexOfScroll = inputAction.bindings.IndexOf(b => b.path == "<Mouse>/scroll");
        if (isEnabled.Value)
        {
            if (indexOfScroll == -1)
                inputAction.AddBinding("<Mouse>/scroll", groups: "Keyboard&Mouse");
        }
        else
        {
            if (indexOfScroll != -1)
                inputAction.ChangeBinding(indexOfScroll).Erase();
        }
    }
}