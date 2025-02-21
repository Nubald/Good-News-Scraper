using System;
using System.Collections.Generic;
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
    public string name = "dRotation (Rogue All Specs)";
    public float Range { get { return 5.0f; } }

    private bool _isRunning;
    private DateTime _lastBackstabAttempt = DateTime.MinValue;
    private DateTime _lastBackstabFail = DateTime.MinValue;
    private int _backstabFailCount = 0;

    // Stealth and Openers
    private Spell Stealth = new Spell("Stealth");
    private Spell Ambush = new Spell("Ambush");
    private Spell CheapShot = new Spell("Cheap Shot");
    private Spell Garrote = new Spell("Garrote");
    private Spell KidneyShot = new Spell("Kidney Shot");

    // Combat Abilities
    private Spell AdrenalineRush = new Spell("Adrenaline Rush");
    private Spell Backstab = new Spell("Backstab");
    private Spell BladeFlurry = new Spell("Blade Flurry");
    private Spell KillingSpree = new Spell("Killing Spree");

    // Assassination Abilities
    private Spell ColdBlood = new Spell("Cold Blood");
    private Spell HungerForBlood = new Spell("Hunger For Blood");
    private Spell Mutilate = new Spell("Mutilate");
    private Spell Envenom = new Spell("Envenom");
    private Spell ExposeArmor = new Spell("Expose Armor");
    private Spell Dismantle = new Spell("Dismantle");

    // Subtlety Abilities
    private Spell Hemorrhage = new Spell("Hemorrhage");
    private Spell ShadowDance = new Spell("Shadow Dance");
    private Spell Shadowstep = new Spell("Shadowstep");

    // Finishers
    private Spell SliceAndDice = new Spell("Slice and Dice");
    private Spell Rupture = new Spell("Rupture");
    private Spell Eviscerate = new Spell("Eviscerate");

    // Defensive & Utility
    private Spell Evasion = new Spell("Evasion");
    private Spell CloakOfShadows = new Spell("Cloak of Shadows");
    private Spell Kick = new Spell("Kick");
    private Spell Gouge = new Spell("Gouge");
    private Spell Blind = new Spell("Blind");
    private Spell TricksOfTheTrade = new Spell("Tricks of the Trade");
    private Spell Vanish = new Spell("Vanish");
    private Spell Sprint = new Spell("Sprint");

    public void Initialize()
    {
        _isRunning = true;
        Logging.Write(name + ": initialized.");
        CreateStatusFrame();
        Rotation();
    }

    public void Dispose()
    {
        _isRunning = false;
        Logging.Write(name + ": Stop in progress.");
        Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Stopped!"")");
    }

    public void ShowConfiguration()
    {
        Logging.Write(name + " No settings for this Fightclass.");
    }

    private void CreateStatusFrame()
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
            dRotationFrame.text:SetText(""dRotation All Specs Ready!"")
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
                            HandleOutOfCombat();
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

            Thread.Sleep(10);
        }
        Logging.Write(name + ": Stopped.");
    }

    private void HandleOutOfCombat()
    {
        Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Active!"")");

        // Stealth when out of combat
        if (!ObjectManager.Me.HaveBuff(1784) && Stealth.IsSpellUsable)
        {
            Stealth.Launch();
            return;
        }
    }

    private void CombatRotation()
    {
        // Emergency Abilities
        if (ObjectManager.Me.HealthPercent <= 30)
        {
            if (Vanish.IsSpellUsable)
            {
                Vanish.Launch();
                return;
            }
            if (Evasion.IsSpellUsable)
            {
                Evasion.Launch();
                return;
            }
        }

        if (ObjectManager.Me.HealthPercent <= 40 && Sprint.IsSpellUsable)
        {
            Sprint.Launch();
            return;
        }

        // Check for and blind healers
        if (Blind.IsSpellUsable && !ObjectManager.Me.HaveBuff(1784)) // Don't break stealth to blind
        {
            bool hasHealer = Lua.LuaDoString<bool>(@"
                local hasHealer = false
                for i = 1, 40 do
                    local unit = 'nameplate' .. i
                    if UnitExists(unit) and UnitGUID(unit) ~= UnitGUID('target') then
                        -- Check if unit is casting a healing spell
                        if UnitCastingInfo(unit) then
                            local spell = UnitCastingInfo(unit)
                            if spell then
                                -- List of healing spells to check for
                                local healingSpells = {
                                    'Flash Heal', 'Greater Heal', 'Healing Wave', 'Holy Light',
                                    'Heal', 'Flash of Light', 'Lesser Healing Wave', 'Healing Touch',
                                    'Regrowth', 'Chain Heal', 'Prayer of Healing', 'Divine Light',
                                    'Holy Nova', 'Circle of Healing', 'Wild Growth', 'Nourish'
                                }
                                
                                for _, healSpell in ipairs(healingSpells) do
                                    if spell == healSpell then
                                        -- Check if they're healing our target
                                        if UnitGUID(UnitTarget(unit)) == UnitGUID('target') then
                                            TargetUnit(unit)
                                            return true
                                        end
                                    end
                                end
                            end
                        end
                        
                        -- Also check for channeled heals
                        if UnitChannelInfo(unit) then
                            local spell = UnitChannelInfo(unit)
                            if spell then
                                local channeledHeals = {
                                    'Tranquility', 'Divine Hymn', 'Penance'
                                }
                                
                                for _, healSpell in ipairs(channeledHeals) do
                                    if spell == healSpell then
                                        if UnitGUID(UnitTarget(unit)) == UnitGUID('target') then
                                            TargetUnit(unit)
                                            return true
                                        end
                                    end
                                end
                            end
                        end
                    end
                end
                return false
            ");

            if (hasHealer)
            {
                Blind.Launch();
                Lua.LuaDoString("TargetLastTarget()"); // Return to original target
                return;
            }
        }

        // High Priority Debuffs
        if (ExposeArmor.KnownSpell && ExposeArmor.IsSpellUsable && 
            ObjectManager.Me.ComboPoint >= 1 &&
            !Lua.LuaDoString<bool>("return UnitIsPlayer('target')") &&
            !ObjectManager.Target.HaveBuff(8647) && // Expose Armor
            !ObjectManager.Target.HaveBuff(48669) && // Sunder Armor
            ObjectManager.Target.HealthPercent > 15)
        {
            ExposeArmor.Launch();
            return;
        }

        // Hunger for Blood - only refresh when about to expire
        if (HungerForBlood.KnownSpell && HungerForBlood.IsSpellUsable)
        {
            float hfbTimeLeft = Lua.LuaDoString<float>(@"
                local _, _, _, _, _, _, expirationTime = UnitBuff('player', 'Hunger For Blood')
                if expirationTime then
                    return expirationTime - GetTime()
                end
                return 0
            ");

            // Only refresh if not present or about to expire (< 1.5 seconds)
            if (!ObjectManager.Me.HaveBuff(63848) || hfbTimeLeft < 1.5)
            {
                HungerForBlood.Launch();
                return;
            }
        }

        // Dismantle on weapon-wielding targets
        if (Dismantle.KnownSpell && Dismantle.IsSpellUsable && 
            !ObjectManager.Target.HaveBuff(51722)) // Not already disarmed
        {
            // Check if target has a weapon equipped
            bool hasWeapon = Lua.LuaDoString<bool>(@"
                local mainHand = GetInventoryItemLink('target', 16) -- Main hand slot
                local offHand = GetInventoryItemLink('target', 17)  -- Off hand slot
                local ranged = GetInventoryItemLink('target', 18)   -- Ranged slot
                
                if mainHand or offHand or ranged then
                    local _, class = UnitClass('target')
                    -- Only use on melee classes or hunters in melee range
                    return class == 'WARRIOR' or class == 'PALADIN' or 
                           class == 'ROGUE' or class == 'DEATHKNIGHT' or 
                           (class == 'HUNTER' and IsSpellInRange('Wing Clip', 'target') == 1)
                end
                return false
            ");

            if (hasWeapon)
            {
                Dismantle.Launch();
                return;
            }
        }

        // Slice and Dice - only refresh when about to expire
        if (SliceAndDice.KnownSpell && SliceAndDice.IsSpellUsable)
        {
            float sndTimeLeft = Lua.LuaDoString<float>(@"
                local _, _, _, _, _, _, expirationTime = UnitBuff('player', 'Slice and Dice')
                if expirationTime then
                    return expirationTime - GetTime()
                end
                return 0
            ");

            // Only refresh if not present or about to expire (< 1.5 seconds)
            if (!ObjectManager.Me.HaveBuff(6774) || sndTimeLeft < 1.5)
            {
                // Use more combo points if available for longer duration
                if (ObjectManager.Me.ComboPoint >= 1)
                {
                    SliceAndDice.Launch();
                    return;
                }
            }
        }

        // Rupture in PvP - only on non-healers with decent health
        if (Rupture.KnownSpell && Rupture.IsSpellUsable && 
            ObjectManager.Target.HealthPercent > 30 &&
            ObjectManager.Me.ComboPoint >= 4 &&
            Lua.LuaDoString<bool>(@"
                local _, class = UnitClass('target')
                return class ~= 'PRIEST' and class ~= 'PALADIN' and 
                       class ~= 'DRUID' and class ~= 'SHAMAN'
            "))
        {
            float ruptureTimeLeft = Lua.LuaDoString<float>(@"
                local _, _, _, _, _, _, expirationTime = UnitDebuff('target', 'Rupture')
                if expirationTime then
                    return expirationTime - GetTime()
                end
                return 0
            ");
            
            // Only cast if target doesn't have Rupture or it's about to expire (< 1.5 seconds)
            if (!ObjectManager.Target.HaveBuff(48672) || ruptureTimeLeft < 1.5)
            {
                Rupture.Launch();
                return;
            }
        }

        // Envenom in PvP - prioritize burst damage
        if (Envenom.KnownSpell && Envenom.IsSpellUsable && 
            ObjectManager.Me.ComboPoint >= 4 && 
            ObjectManager.Me.HaveBuff(6774)) // Slice and Dice is up
        {
            // Check for Deadly Poison stacks
            if (Lua.LuaDoString<int>("local _, _, _, count = UnitDebuff('target', 'Deadly Poison'); return count or 0") >= 3)
            {
                Envenom.Launch();
                return;
            }
        }

        // Offensive Cooldowns - more aggressive in PvP
        if (ObjectManager.Me.HealthPercent >= 60 && 
            Lua.LuaDoString<bool>("return UnitIsPlayer('target')"))
        {
            if (ColdBlood.IsSpellUsable && ObjectManager.Me.ComboPoint >= 4)
            {
                ColdBlood.Launch();
                return;
            }
            if (AdrenalineRush.IsSpellUsable)
            {
                AdrenalineRush.Launch();
                return;
            }
            if (KillingSpree.IsSpellUsable && ObjectManager.Me.Energy < 40)
            {
                KillingSpree.Launch();
                return;
            }
        }

        // Interrupt casting
        if (ObjectManager.Target.IsCast && 
            !ObjectManager.Target.HaveBuff("Spell Reflection") && 
            !ObjectManager.Target.HaveBuff("Grounding Totem Effect"))
        {
            // Try Kick first
            if (Kick.IsSpellUsable)
            {
                Kick.Launch();
                return;
            }
            // Use Gouge as backup if Kick is on cooldown
            else if (Gouge.IsSpellUsable && !ObjectManager.Target.HaveBuff("Gouge"))
            {
                Gouge.Launch();
                return;
            }
        }

        // Tricks of the Trade
        if (TricksOfTheTrade.IsSpellUsable && ObjectManager.Me.HasFocus)
        {
            Lua.RunMacroText("/cast [target=focus] Tricks of the Trade");
            return;
        }

        // Stealth and Shadow Dance Abilities
        if (ObjectManager.Me.HaveBuff(1784) || ObjectManager.Me.HaveBuff(58427)) // In Stealth or Shadow Dance
        {
            // Check if target is already stunned
            bool isStunned = Lua.LuaDoString<bool>(@"
                return UnitIsPlayer('target') and (
                    UnitDebuff('target', 'Cheap Shot') or
                    UnitDebuff('target', 'Kidney Shot') or
                    UnitDebuff('target', 'Hammer of Justice') or
                    UnitDebuff('target', 'Charge Stun') or
                    UnitDebuff('target', 'Intercept Stun') or
                    UnitDebuff('target', 'Concussion Blow') or
                    UnitDebuff('target', 'Impact') or
                    UnitDebuff('target', 'War Stomp')
                )
            ");

            if (!isStunned && CheapShot.IsSpellUsable)
            {
                CheapShot.Launch();
                return;
            }
            else
            {
                // Check if we're behind target for Ambush
                bool isBehindTarget = Lua.LuaDoString<bool>(@"
                    local px, py = ObjectPosition('player')
                    local tx, ty = ObjectPosition('target')
                    if not px or not tx then return false end

                    local facing = ObjectFacing('target')
                    local angle = math.deg(math.atan2(py - ty, px - tx))
                    if angle < 0 then angle = angle + 360 end
                    
                    facing = math.deg(facing)
                    if facing < 0 then facing = facing + 360 end
                    
                    local relativeAngle = angle - facing
                    if relativeAngle < 0 then relativeAngle = relativeAngle + 360 end
                    
                    return relativeAngle >= 135 and relativeAngle <= 225
                ");

                if (isBehindTarget && Ambush.IsSpellUsable)
                {
                    Ambush.Launch();
                    return;
                }
                else if (Hemorrhage.IsSpellUsable && ObjectManager.Me.Energy >= 35)
                {
                    Hemorrhage.Launch();
                    return;
                }
            }
        }

        // Kidney Shot for stun maintenance
        if (KidneyShot.IsSpellUsable && ObjectManager.Me.ComboPoint >= 3 && 
            !ObjectManager.Target.HaveBuff("Cheap Shot") && // Not already stunned by Cheap Shot
            !ObjectManager.Target.HaveBuff("Kidney Shot")) // Not already stunned by Kidney Shot
        {
            KidneyShot.Launch();
            return;
        }

        // Backstab in PvP
        if (Backstab.IsSpellUsable && ObjectManager.Me.Energy >= 60)
        {
            bool isBehindTarget = Lua.LuaDoString<bool>(@"
                -- Get player and target positions
                local px, py = ObjectPosition('player')
                local tx, ty = ObjectPosition('target')
                if not px or not tx then return false end

                -- Calculate angle between player and target
                local facing = ObjectFacing('target')
                local angle = math.deg(math.atan2(py - ty, px - tx))
                if angle < 0 then angle = angle + 360 end
                
                -- Convert target's facing to degrees and normalize
                facing = math.deg(facing)
                if facing < 0 then facing = facing + 360 end
                
                -- Calculate relative angle
                local relativeAngle = angle - facing
                if relativeAngle < 0 then relativeAngle = relativeAngle + 360 end
                
                -- Check if player is behind (135 to 225 degrees relative to target's facing)
                return relativeAngle >= 135 and relativeAngle <= 225
            ");

            if (isBehindTarget)
            {
                Backstab.Launch();
                return;
            }
        }

        // Combo Point Builders
        if (ObjectManager.Me.ComboPoint < 5)
        {
            if (Mutilate.IsSpellUsable && ObjectManager.Me.Energy >= 60)
            {
                Mutilate.Launch();
                return;
            }
            if (Hemorrhage.IsSpellUsable && ObjectManager.Me.Energy >= 35)
            {
                Hemorrhage.Launch();
                return;
            }
        }

        // PvP Finishers - prioritize burst damage
        if (ObjectManager.Me.ComboPoint >= 5)
        {
            // Against healers, always use Eviscerate for burst
            if (Lua.LuaDoString<bool>(@"
                local _, class = UnitClass('target')
                return class == 'PRIEST' or class == 'PALADIN' or 
                       class == 'DRUID' or class == 'SHAMAN'
            "))
            {
                Eviscerate.Launch();
                return;
            }
            // Against non-healers, use Eviscerate if they're below 50% health
            else if (ObjectManager.Target.HealthPercent < 50)
            {
                Eviscerate.Launch();
                return;
            }
        }
    }
}
