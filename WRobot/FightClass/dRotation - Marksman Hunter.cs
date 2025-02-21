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
    public string name = "dRotation - Marksman hunter";
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
    public Spell TrueshotAura = new Spell("Trueshot Aura");
    public Spell AspectOfTheDragonhawk = new Spell("Aspect of the Dragonhawk");
    public Spell AspectOfTheViper = new Spell("Aspect of the Viper");
    public Spell HuntersMark = new Spell("Hunter's Mark");

    /* Spells for Rotation */
    public Spell TranquilizingShot = new Spell("Tranquilizing Shot");
    public Spell Readiness = new Spell("Readiness");
    public Spell Misdirection = new Spell("Misdirection");
    public Spell RapidFire = new Spell("Rapid Fire");
    public Spell CallOfTheWild = new Spell("Call of the Wild");
    public Spell KillCommand = new Spell("Kill Command");
    public Spell SilencingShot = new Spell("Silencing Shot");
    public Spell KillShot = new Spell("Kill Shot");
    public Spell SerpentSting = new Spell("Serpent Sting");
    public Spell ChimeraShot = new Spell("Chimera Shot");
    public Spell AimedShot = new Spell("Aimed Shot");
    public Spell SteadyShot = new Spell("Steady Shot");


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
                        Buff();
                        if (!ObjectManager.Me.InCombat)
                        {
                            Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Active!"")");
                        }
                        else if (ObjectManager.Me.InCombat && ObjectManager.Me.Target > 0)
                        {
                            CombatRotation();
                            Lua.LuaDoString("PetAttack();");
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

    /* Buff() */
    public void Buff()
    {
        // Trueshot Aura
        if (TrueshotAura.KnownSpell && TrueshotAura.IsSpellUsable && !ObjectManager.Me.HaveBuff(19506))
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Trueshot Aura"")");
            TrueshotAura.Launch();
            return;
        }

        // Aspect of The Dragonhawk
        if (AspectOfTheDragonhawk.KnownSpell && AspectOfTheDragonhawk.IsSpellUsable && !ObjectManager.Me.HaveBuff(61847) && ObjectManager.Me.ManaPercentage >= 10)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Aspect Of The Dragonhawk"")");
            AspectOfTheDragonhawk.Launch();
            return;
        }

        // Aspect of the Viper
        if (AspectOfTheViper.KnownSpell && AspectOfTheViper.IsSpellUsable && !ObjectManager.Me.HaveBuff(34074) && ObjectManager.Me.ManaPercentage <= 10)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Aspect Of The Viper"")");
            AspectOfTheViper.Launch();
            return;
        }

        // Hunters Mark
        if (HuntersMark.KnownSpell && HuntersMark.IsSpellUsable && !ObjectManager.Target.HaveBuff(53338) && ObjectManager.Me.HasTarget)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Buffing Hunter's Mark"")");
            HuntersMark.Launch();
            return;
        }
    }

    /* CombatRotation() */
    public void CombatRotation()
    {
        // Misdirection
        if (Misdirection.KnownSpell && Misdirection.IsSpellUsable && ObjectManager.Me.HasFocus)
        {
            Lua.RunMacroText("/cast [target=focus] Misdirection");
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Misdirection @focus"")");
            return;
        }

        // Rapid Fire
        if (RapidFire.KnownSpell && RapidFire.IsSpellUsable && !ObjectManager.Me.HaveBuff(3045) && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Rapid Fire"")");
            RapidFire.Launch();
            return;
        }

        // Readiness
        if (Readiness.KnownSpell && Readiness.IsSpellUsable && !ObjectManager.Me.HaveBuff(3045) && !RapidFire.IsSpellUsable && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Readiness"")");
            Readiness.Launch();
            return;
        }

        // Call of the Wild
        if (CallOfTheWild.IsSpellUsable && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Call of The Wild"")");
            CallOfTheWild.Launch();
            return;
        }

        // Kill Command
        if (KillCommand.KnownSpell && KillCommand.IsSpellUsable && ObjectManager.Target.IsBoss)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Kill Command"")");
            KillCommand.Launch();
            return;
        }

        // Silencing Shot
        if (SilencingShot.KnownSpell && SilencingShot.IsSpellUsable && SilencingShot.IsDistanceGood)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Silencing Shot"")");
            SilencingShot.Launch();
            return;
        }

        // Kill Shot
        if (KillShot.KnownSpell && KillShot.IsSpellUsable && KillShot.IsDistanceGood && ObjectManager.Target.HealthPercent <= 20)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Kill Shot"")");
            KillShot.Launch();
            return;
        }

        // Serpent Sting
        if (SerpentSting.KnownSpell && SerpentSting.IsSpellUsable && SerpentSting.IsDistanceGood && !ObjectManager.Target.HaveBuff(49001))
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Serpent Sting"")");
            SerpentSting.Launch();
            return;
        }

        // Chimera Shot
        if (ChimeraShot.KnownSpell && ChimeraShot.IsSpellUsable && ChimeraShot.IsDistanceGood)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Chimera Shot"")");
            ChimeraShot.Launch();
            return;
        }

        // Aimed Shot
        if (AimedShot.KnownSpell && AimedShot.IsSpellUsable && AimedShot.IsDistanceGood)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Aimed Shot"")");
            AimedShot.Launch();
            return;
        }

        // Steady Shot
        if (SteadyShot.KnownSpell && SteadyShot.IsSpellUsable && SteadyShot.IsDistanceGood)
        {
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Casting Steady Shot"")");
            SteadyShot.Launch();
            return;
        }
    }
}