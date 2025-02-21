using System;
using System.Threading;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : ICustomClass
{
    public string name = "dRotation (Arcane Mage)";
    public float Range { get { return 35.0f; } }

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

    /* Spells to Buff */
    public Spell MoltenArmor = new Spell("Molten Armor");


    /* Spells for Rotation */
    public Spell SpellSteal = new Spell("Spell Steal");
    public Spell IceBlock = new Spell("Ice Block");
    public Spell Evocation = new Spell("Evocation");
    public Spell IcyVeins = new Spell("Icy Veins");
    public Spell ArcanePower = new Spell("Arcane Power");
    public Spell MirrorImage = new Spell("Mirror Image");
    public Spell PresenceofMind = new Spell("Presence of Mind");
    public Spell ArcaneMissiles = new Spell("Arcane Missiles");
    public Spell ArcaneBlast = new Spell("Arcane Blast");

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
                    Buff(); // casting buffs
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

    public void Buff()
    {
        // Molten Armor
        if (!ObjectManager.Me.HaveBuff(43046) && !ObjectManager.Me.IsMounted && !ObjectManager.Me.PlayerUsingVehicle)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Molten Armor"")");
            MoltenArmor.Launch();
            return;
        }
    }

    /*
     * CombatRotation()
     */
    public void CombatRotation()
    {
        // Spell Steal
        if (SpellSteal.KnownSpell && SpellSteal.IsSpellUsable && SpellSteal.IsDistanceGood &&
            ObjectManager.Target.HaveBuff(17) ||
            ObjectManager.Target.HaveBuff(2825) ||
            ObjectManager.Target.HaveBuff(1719) ||
            ObjectManager.Target.HaveBuff(6940) ||
            ObjectManager.Target.HaveBuff(43242) ||
            ObjectManager.Target.HaveBuff(31884) ||
            ObjectManager.Target.HaveBuff(32182) ||
            ObjectManager.Target.HaveBuff(67108) ||
            ObjectManager.Target.HaveBuff(67107) ||
            ObjectManager.Target.HaveBuff(67106) ||
            ObjectManager.Target.HaveBuff(33763) ||
            ObjectManager.Target.HaveBuff(66228))
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Spell Steal"")");
            SpellSteal.Launch();
            return;
        }

        // Ice Block
        if (IceBlock.KnownSpell && ObjectManager.Me.HealthPercent <= 10)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""OOF, UNDER 10% HP ICE BLOCK."")");
            IceBlock.Launch();
            return;
        }

        // Evocation
        if (Evocation.KnownSpell && Evocation.IsSpellUsable && ObjectManager.Me.ManaPercentage <= 5)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Evocation"")");
            Evocation.Launch();
            Thread﻿.Sleep(Usefuls.Latency + 6000);
            return;
        }

        // Icy Veins
        if (IcyVeins.KnownSpell && IcyVeins.IsSpellUsable && ObjectManager.Me.BuffStack(36032) >= 1 && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Icy Veins"")");
            IcyVeins.Launch();
            return;
        }

        // Arcane Power
        if (ArcanePower.KnownSpell && ArcanePower.IsSpellUsable && ObjectManager.Me.BuffStack(36032) >= 1 && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Arcane Power"")");
            ArcanePower.Launch();
            return;
        }

        // Mirror Image
        if (MirrorImage.KnownSpell && MirrorImage.IsSpellUsable && ArcaneBlast.IsDistanceGood && ObjectManager.Me.BuffStack(36032) >= 1 && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Mirror Image"")");
            MirrorImage.Launch();
            return;
        }

        // Presence of Mind
        if (PresenceofMind.KnownSpell && PresenceofMind.IsSpellUsable && ArcaneBlast.IsDistanceGood && ObjectManager.Me.BuffStack(36032) >= 2 && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Presence of Mind"")");
            PresenceofMind.Launch();
            return;
        }

        // Arcane Missels
        if (ArcaneMissiles.KnownSpell && ArcaneMissiles.IsSpellUsable && ArcaneMissiles.IsDistanceGood && ObjectManager.Me.BuffStack(36032) >= 3 && ObjectManager.Me.HaveBuff(44401))
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Arcane Missels"")");
            ArcaneMissiles.Launch();
            return;
        }

        // Arcane Blast
        if (ArcaneBlast.KnownSpell && ArcaneBlast.IsSpellUsable && ArcaneBlast.IsDistanceGood)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Arcane Blast"")");
            ArcaneBlast.Launch();
            return;
        }
    }
}