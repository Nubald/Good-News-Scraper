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
    public string name = "dRotation - (Fire Mage)";
    public float Range { get { return 39.5f; } }

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

    /* Spells for Buff */
    public Spell MoltenArmor = new Spell("Molten Armor");

    /* Spells for Rotation */
    public Spell SpellSteal = new Spell("Spell Steal");
    public Spell IceBlock = new Spell("Ice Block");
    public Spell Evocation = new Spell("Evocation");
    public Spell Pyroblast = new Spell("Pyroblast");
    public Spell LivingBomb = new Spell("Living Bomb");
    public Spell Scorch = new Spell("Scorch");
    public Spell MirrorImage = new Spell("Mirror Image");
    public Spell Combustion = new Spell("Combustion");
    public Spell Fireball = new Spell("Fireball");

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

    /*
     * Buff()
     */
    internal void Buff()
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
    internal void CombatRotation()
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

        // Pyroblast
        if (Pyroblast.KnownSpell && Pyroblast.IsSpellUsable && Pyroblast.IsDistanceGood && ObjectManager.Me.HaveBuff(48108))
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Pyroblast"")");
            Pyroblast.Launch();
            return;
        }

        // Living Bomb
        if (LivingBomb.KnownSpell && LivingBomb.IsSpellUsable && LivingBomb.IsDistanceGood && !ObjectManager.Target.HaveBuff(55360))
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Living Bomb"")");
            LivingBomb.Launch();
            return;
        }

        // Scorch
        if (Scorch.KnownSpell && Scorch.IsSpellUsable && Scorch.IsDistanceGood && !(ObjectManager.Target.HaveBuff(22959) || ObjectManager.Target.HaveBuff(17800)) && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Scorch"")");
            Scorch.Launch();
            return;
        }

        // Mirror Image
        if (MirrorImage.KnownSpell && MirrorImage.IsSpellUsable && Fireball.IsDistanceGood && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Mirror Image"")");
            MirrorImage.Launch();
            return;
        }

        // Combustion
        if (Combustion.KnownSpell && Combustion.IsSpellUsable && Fireball.IsDistanceGood && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Combustion"")");
            Combustion.Launch();
            return;
        }

        // Fireball
        if (Fireball.KnownSpell && Fireball.IsSpellUsable && Fireball.IsDistanceGood)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Fireball"")");
            Fireball.Launch();
            return;
        }
    }
}
