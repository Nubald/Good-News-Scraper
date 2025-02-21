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
    public string name = "dRotation - Shadow Priest";
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

    /* Buffs */
    public Spell Shadowform = new Spell("Shadowform");
    public Spell InnerFire = new Spell("Inner Fire");
    public Spell VampiricEmbrace = new Spell("Vampiric Embrace");

    /* Spells for Rotation */
    public Spell Dispersion = new Spell("Dispersion");
    public Spell Fade = new Spell("Fade");
    public Spell VampiricTouch = new Spell("Vampiric Touch");
    public Spell DevouringPlague = new Spell("Devouring Plague");
    public Spell ShadowWordPain = new Spell("Shadow Word: Pain");
    public Spell Shadowfiend = new Spell("Shadowfiend");
    public Spell MindFlay = new Spell("Mind Flay");


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
        // Shadowform
        if (!ObjectManager.Me.HaveBuff(15473) && !ObjectManager.Me.IsMounted && !ObjectManager.Me.PlayerUsingVehicle)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Shadowform"")");
            Shadowform.Launch();
            return;
        }

        // Inner Fire
        if (!ObjectManager.Me.HaveBuff(48168) && !ObjectManager.Me.IsMounted && !ObjectManager.Me.PlayerUsingVehicle)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Inner Fire"")");
            InnerFire.Launch();
            return;
        }

        // Vampiric Embrace
        if (!ObjectManager.Me.HaveBuff(15286) && !ObjectManager.Me.IsMounted && !ObjectManager.Me.PlayerUsingVehicle)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Vampiric Embrace"")");
            VampiricEmbrace.Launch();
            return;
        }
    }

    /* CombatRotation() */
    public void CombatRotation()
    {
        // Dispersion
        if (Dispersion.KnownSpell && Dispersion.IsSpellUsable && Dispersion.IsDistanceGood && ObjectManager.Me.HealthPercent <= 10 || ObjectManager.Me.ManaPercentage <= 5)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Dispersion"")");
            Dispersion.Launch();
            return;
        }

        // Fade
        if (Fade.KnownSpell && Fade.IsSpellUsable && Fade.IsDistanceGood && ObjectManager.Target.IsTargetingMe)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Fade"")");
            Fade.Launch();
            return;
        }

        // Vampiric Touch
        if (VampiricTouch.KnownSpell && VampiricTouch.IsSpellUsable && VampiricTouch.IsDistanceGood && !ObjectManager.Target.HaveBuff(48160) || ObjectManager.Target.BuffTimeLeft(new List<uint> {48160}) < 1500)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Vampiric Touch"")");
            VampiricTouch.Launch();
            return;
        }

        // Devouring Plague
        if (DevouringPlague.KnownSpell && DevouringPlague.IsSpellUsable && DevouringPlague.IsDistanceGood && !ObjectManager.Target.HaveBuff(48300) || ObjectManager.Target.BuffTimeLeft(new List<uint> {48300}) < 1000)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Devouring Plague"")");
            DevouringPlague.Launch();
            return;
        }

        // Shadow Word: Pain
        if (ShadowWordPain.KnownSpell && ShadowWordPain.IsSpellUsable && ShadowWordPain.IsDistanceGood && !ObjectManager.Target.HaveBuff(48125) || ObjectManager.Target.BuffTimeLeft(new List<uint> {48125}) < 1000)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Shadow Word: Pain"")");
            ShadowWordPain.Launch();
            return;
        }

        // Shadowfiend
        if (Shadowfiend.KnownSpell && ShadowWordPain.IsSpellUsable && ShadowWordPain.IsDistanceGood && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Shadowfiend"")");
            ShadowWordPain.Launch();
            Lua.RunMacroText("/petattack");
            Lua.RunMacroText("/cast Shadowcrawl");
            return;
        }

        // Mind Flay
        if (MindFlay.KnownSpell && MindFlay.IsSpellUsable && MindFlay.IsDistanceGood && ObjectManager.Target.HaveBuff(48160) && ObjectManager.Target.HaveBuff(48300) && ObjectManager.Target.HaveBuff(48125))
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Mind Flay"")");
            MindFlay.Launch();
            Lua.RunMacroText("/petattack");
            Lua.RunMacroText("/cast Shadowcrawl");
            return;
        }

    }
}