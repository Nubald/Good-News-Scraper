using System;
using System.Collections.Generic;
using System.Threading;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : ICustomClass
{
    public string name = "dRotation (Mutilate Rogue)";
    public float Range { get { return 5.0f; } }

    private bool _isRunning;

    /*
    * Initialize()
    * When product started, initialize and launch Fightclass
    */
    public void Initialize()
    {
        _isRunning = true;
        Logging.Write(name + " Is initialized.");
        CreateStatusFrame();
        Rotation();
    }

    /*
    * Dispose()
    * When product stopped
    */
    public void Dispose()
    {
        _isRunning = false;
        Logging.Write(name + " Stop in progress.");
        Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Stopped!"")");
    }

    /*
    * ShowConfiguration()
    * When use click on Fightclass settings
    */
    public void ShowConfiguration()
    {
        Logging.Write(name + " No settings for this Fightclass.");
    }

    /*
    * Spells for Rotation 
    */
    public Spell ColdBlood = new Spell("Cold Blood");
    public Spell Envenom = new Spell("Envenom");
    public Spell Garrote = new Spell("Garrote");
    public Spell HungerForBlood = new Spell("Hunger For Blood");
    public Spell Mutilate = new Spell("Mutilate");
    public Spell SliceAndDice = new Spell("Slice and Dice");
    public Spell Rupture = new Spell("Rupture");
    public Spell TricksOfTheTrade = new Spell("Tricks of the Trade");
    public Spell Vanish = new Spell("Vanish");

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
    */
    public void CombatRotation()
    {
        // Garrote
        if (Garrote.KnownSpell && Garrote.IsSpellUsable && Garrote.IsDistanceGood && ObjectManager.Me.HaveBuff(58427))
        {
            Garrote.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Garrote"")");
            return;
        }

        // Rupture (Only if no one is apply bleed and you not garrote)
        if (!(ObjectManager.Target.HaveBuff(47465) || ObjectManager.Target.HaveBuff(49800) || ObjectManager.Target.HaveBuff(48672) || ObjectManager.Target.HaveBuff(48676)) && !ObjectManager.Me.HaveBuff(63848) && Rupture.KnownSpell && Rupture.IsSpellUsable && ObjectManager.Me.ComboPoint >= 1)
        {
            Rupture.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Rupture"")");
            return;
        }

        // Slice and Dice
        if (SliceAndDice.KnownSpell && SliceAndDice.IsSpellUsable && SliceAndDice.IsDistanceGood && O﻿bjectMa﻿nager.Me.BuffTimeLeft(new List<uint> { 6774 }) < 500 && ObjectManager.Me.ComboPoint >= 1)
        {
            SliceAndDice.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Slice and Dice"")");
            return;
        }

        // Cold Blood
        if (ColdBlood.KnownSpell && ColdBlood.IsSpellUsable && ColdBlood.IsDistanceGood && ObjectManager.Me.ComboPoint >= 5 && ObjectManager.Target.IsBoss)
        {
            ColdBlood.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Cold Blood"")");
            return;
        }

        // Envenom
        if (Envenom.KnownSpell && Envenom.IsSpellUsable && Envenom.IsDistanceGood && ObjectManager.Me.ComboPoint >= 4 && ObjectManager.Me.HaveBuff(6774))
        {
            Envenom.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Envenom"")");
            return;
        }

        // Hunger For Blood
        if (HungerForBlood.KnownSpell && HungerForBlood.IsSpellUsable && !ObjectManager.Me.HaveBuff(63848))
        {
            HungerForBlood.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Hunger For Blood"")");
            return;
        }

        // Tricks of The Trade
        if (TricksOfTheTrade.KnownSpell && TricksOfTheTrade.IsSpellUsable && TricksOfTheTrade.IsDistanceGood && ObjectManager.Target.IsAttackable && ObjectManager.Me.HasFocus)
        {
            Lua.RunMacroText("/cast [target=focus] Tricks of the Trade");
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Tricks of the Trade @focus"")");
            return;
        }

        // Mutilate
        if (Mutilate.KnownSpell && Mutilate.IsSpellUsable && Mutilate.IsDistanceGood && ObjectManager.Me.ComboPoint <= 4)
        {
            Mutilate.Launch();
            Lua.RunMacroText("/use 10"); // Engineering Gloves RunMacro solution
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Mutilate"")");
            return;
        }

        // Vanish
        if (Vanish.KnownSpell && Vanish.IsSpellUsable && Vanish.IsDistanceGood && !ObjectManager.Me.HaveBuff(58427) && !ObjectManager.Me.HaveBuff(1784) && ObjectManager.Target.IsBoss)
        {
            Vanish.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Vanish"")");
            return;
        }
    }
}
