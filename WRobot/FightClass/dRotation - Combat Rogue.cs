using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using robotManager;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : ICustomClass
{
    public string name = "dRotation (Combat Rogue)";
    public float Range { get { return 5.0f; } }

    private bool _isRunning;

    /*
    * Initialize()
    * When product started and launch Fightclass
    */
    public void Initialize()
    {
        _isRunning = true;
        Logging.Write(name + ": initialized.");
        dRotationSettings.Load();
        CreateStatusFrame();
        Rotation();
    }

    /*
    * Dispose()
    * When product is stopped
    */
    public void Dispose()
    {
        _isRunning = false;
        Logging.Write(name + ": Stop in progress.");
        Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Stopped!"")");
    }

    /* 
    * ShowConfiguration()
    * Fightclass settings in wRobot
    */
    public void ShowConfiguration()
    {
        dRotationSettings.Load();
        dRotationSettings.CurrentSetting.ToForm();
        dRotationSettings.CurrentSetting.Save();
    }

    /*
    * Spells for Rotation 
    */
    public Spell SinisterStrike = new Spell("Sinister Strike");
    public Spell SliceAndDice = new Spell("Slice and Dice");
    public Spell Eviscerate = new Spell("Eviscerate");
    public Spell Rupture = new Spell("Rupture");
    public Spell KillingSpree = new Spell("Killing Spree");
    public Spell TricksOfTheTrade = new Spell("Tricks of the Trade");
    public Spell BladeFlurry = new Spell("Blade Flurry");
    public Spell AdrenalineRush = new Spell("Adrenaline Rush");

    /* Rotation() */
    public void Rotation()
    {
        Logging.Write(name + ": Started.");

        while (_isRunning)
        {
            try
            {
                if (Products.InPause)
                {
                    Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Paused!"")");
                }

                if (!Products.InPause)
                {
                    if (!ObjectManager.Me.IsDeadMe)
                    {
                        if (!ObjectManager.Me.InCombat)
                        {
                            Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Active!"")");
                        }
                        else if (ObjectManager.Me.InCombat && ObjectManager.Me.Target > 0)
                        {
                            CombatRotation();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteError(name + " ERROR: " + e);
            }

            Thread.Sleep(10); // Pause 10 ms to reduce the CPU usage.
        }
        Logging.Write(name + ": Stopped.");
    }

    /*
    * CreateStatusFrame()
    * InGame Status frame to see which spells casting next
    */
    public void CreateStatusFrame()
    {
        Lua.LuaDoString(@"
        if not dRotationFrame then
            dRotationFrame = CreateFrame(""Frame"")
            dRotationFrame:ClearAllPoints()
            dRotationFrame:SetBackdrop(StaticPopup1:GetBackdrop())
            dRotationFrame:SetHeight(70)
            dRotationFrame:SetWidth(210)

            dRotationFrame.text = dRotationFrame:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
            dRotationFrame.text:SetAllPoints()
            dRotationFrame.text:SetText(""dRotation by Dreamful, Ready!"")
            dRotationFrame.text:SetTextColor(1, 1, 1, 6)
            dRotationFrame:SetPoint(""CENTER"", 0, -240)
            dRotationFrame:SetBackdropBorderColor(0, 0, 0, 0)

            dRotationFrame:SetMovable(true)
            dRotationFrame:EnableMouse(true)
            dRotationFrame:SetScript(""OnMouseDown"",function() dRotationFrame:StartMoving() end)
            dRotationFrame:SetScript(""OnMouseUp"",function() dRotationFrame:StopMovingOrSizing() end)

            dRotationFrame.Close = CreateFrame(""BUTTON"", nil, dRotationFrame, ""UIPanelCloseButton"")
            dRotationFrame.Close:SetWidth(15)
            dRotationFrame.Close:SetHeight(15)
            dRotationFrame.Close:SetPoint(""TOPRIGHT"", dRotationFrame, -8, -8)
            dRotationFrame.Close:SetScript(""OnClick"", function()
                dRotationFrame:Hide()
                DEFAULT_CHAT_FRAME:AddMessage(""dRotationStatusFrame |cffC41F3Bclosed |cffFFFFFFWrite /dRotation to enable again."") 	
            end)

            SLASH_WHATEVERYOURFRAMESARECALLED1=""/dRotation""
            SlashCmdList.WHATEVERYOURFRAMESARECALLED = function()
                if dRotationFrame:IsShown() then
                    dRotationFrame:Hide()
                else
                    dRotationFrame:Show()
                end
            end
        end");
    }

    /*
    * CombatRotation()
    * Spells in order to use with conditions
    */
    public void CombatRotation()
    {
        // Slice and Dice
        if (SliceAndDice.KnownSpell && SliceAndDice.IsSpellUsable && SliceAndDice.IsDistanceGood && O﻿bjectMa﻿nager.Me.BuffTimeLeft(new List<uint> { 6774 }) < 2000 && ObjectManager.Me.ComboPoint >= 3)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Slice and Dice"")");
            SliceAndDice.Launch();
            return;
        }

        // Rupture
        if (Rupture.KnownSpell && Rupture.IsSpellUsable && Rupture.IsDistanceGood && ObjectManager.Me.ComboPoint > 4 && !ObjectManager.Target.HaveBuff(48672) && O﻿bjectMa﻿nager.Target.BuffTimeLeft(new List<uint> { 48672 }) < 5000 && dRotationSettings.CurrentSetting.Rupture == true)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Rupture"")");
            Rupture.Launch();
            return;
        }

        // Eviscerate
        if (Eviscerate.KnownSpell && Eviscerate.IsSpellUsable && Eviscerate.IsDistanceGood && O﻿bjectMa﻿nager.Me.BuffTimeLeft(new List<uint> { 6774 }) > 4000 && ObjectManager.Me.ComboPoint > 4)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Eviscerate"")");
            Eviscerate.Launch();
            return;
        }

        // Killing Spree
        if (KillingSpree.KnownSpell && KillingSpree.IsSpellUsable && KillingSpree.IsDistanceGood && ObjectManager.Me.Energy < 40 && O﻿bjectMa﻿nager.Me.BuffTimeLeft(new List<uint> { 6774 }) > 5000 && !ObjectManager.Me.HaveBuff(13750) && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Killing Spree"")");
            KillingSpree.Launch();
            return;
        }

        // Tricks of The Trade
        if (TricksOfTheTrade.KnownSpell && TricksOfTheTrade.IsSpellUsable && TricksOfTheTrade.IsDistanceGood && ObjectManager.Me.HasFocus && ObjectManager.Me.Energy < 85)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Tricks of the Trade @focus"")");
            Lua.RunMacroText("/cast [target=focus] Tricks of the Trade");
            return;
        }

        // Blade Flurry
        if (BladeFlurry.KnownSpell && BladeFlurry.IsSpellUsable && O﻿bjectMa﻿nager.Me.BuffTimeLeft(new List<uint> { 6774 }) > 5000 && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Blade Flurry"")");
            BladeFlurry.Launch();
            return;
        }

        // Adrenaline Rush
        if (AdrenalineRush.KnownSpell && AdrenalineRush.IsSpellUsable && ObjectManager.Me.Energy <= 20 && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Adrenaline Rush"")");
            AdrenalineRush.Launch();
            return;
        }

        // Sinister Strike
        if (SinisterStrike.KnownSpell && SinisterStrike.IsSpellUsable && SinisterStrike.IsDistanceGood && ObjectManager.Me.ComboPoint <= 5)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Sinister Strike"")");
            SinisterStrike.Launch();
            Lua.RunMacroText("/use 10");
            return;
        }
    }
}

/*
 * dRotationSettings() : Settings
 * 
 */ 
[Serializable]
public class dRotationSettings : Settings
{
    [DefaultValue(false)]
    [Category("Abilitys")]
    [DisplayName("Use Rupture")]
    [Description("Using Rupture in Rotation at 5 Combopoints")]
    public bool Rupture { get; set; }

    private dRotationSettings()
    {
        ConfigWinForm(new System.Drawing.Point(400, 400), "dRotation - Combat Rogue " + Translate.Get("Settings"));
    }

    public static dRotationSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("dRotation - CombatRogue", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("dRotationsSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("dRotation - CombatRogue", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =Load<dRotationSettings>(AdviserFilePathAndName("dRotation - CombatRogue", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new dRotationSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("dRotationSettings > Load(): " + e);
        }
        return false;
    }
}