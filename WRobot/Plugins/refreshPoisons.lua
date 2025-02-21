function round(num, numDecimalPlaces)
  local mult = 10^(numDecimalPlaces or 0)
  return math.floor(num * mult + 0.5) / mult
end
function applyMainHandPoisonByName(name)
	RunMacroText('/use ' .. name)
	RunMacroText('/use 16')
	RunMacroText('/click StaticPopup1Button1')
end
function applyOffHandPoisonByName(name)
	RunMacroText('/use ' .. name)
	RunMacroText('/use 17')
	RunMacroText('/click StaticPopup1Button1')
end
function isAbleToApplyPoison(item)
	return (GetUnitSpeed("player") == 0) and not (UnitCastingInfo("player")) and not (UnitChannelInfo("player")) and not (UnitAffectingCombat("player")) and not (IsMounted())  and not (IsFlying())  and not (IsFalling())  and not (IsResting())  and not (UnitIsAFK("player")) and GetItemCount(item) and IsUsableItem(item)
end

local poisonMain;
local poisonOff;
local poisonIdOrNameMain={mainweaponpoison};
local poisonIdOrNameOff={offweaponpoison};
local refreshPoisionAtMinLeft = 13;
local now=GetTime();

--[[ print("Loading Items..."); ]]
poisonMain = GetItemInfo(poisonIdOrNameMain);
--[[print("Using MainPoison: " .. poisonMain);]]
poisonOff = GetItemInfo(poisonIdOrNameOff);
--[[print("Using OffPoison: " .. poisonOff);]]

--[[ print("Applying to Weapons..."); ]]
hasMainHandEnchant, mainHandExpiration, mainHandCharges, mainHandEnchantID, hasOffHandEnchant, offHandExpiration, offHandCharges, offHandEnchantId = GetWeaponEnchantInfo();

	
if hasMainHandEnchant then
--[===[	 DO NOTHING! WAIT UNTIL ENCHANT EXPIRES
	local minUntilExpire = round((mainHandExpiration/1000/60))
	--[[ print("- MainHand") ]]
	--[[ print("-- HasMainhandPoison = " .. hasMainHandEnchant) ]]
	--[[ print("-- ExpirationMainhand = " .. minUntilExpire .."min")	]]
	
	if isAbleToApplyPoison(poisonMain) and minUntilExpire < refreshPoisionAtMinLeft then
		applyMainHandPoisonByName(poisonMain)
	elseif not isAbleToApplyPoison(poisonMain) then
		print("Can not apply MainHandPoison maybe no items in bag!")
	end
		]===]
else 
	if isAbleToApplyPoison(poisonMain) then
		--[[ print("Apply MainHandEnchant: " .. poisonMain) ]]
		applyMainHandPoisonByName(poisonMain)
	else
		print("Can not apply MainHandPoison maybe no items in bag!")
	end
end 

if hasOffHandEnchant then
--[===[  DO NOTHING! WAIT UNTIL ENCHANT EXPIRES  ---> AND OFFHAND NOT WORKING CORRECTLY (return values are crazy)

	local minUntilExpire = round((offHandExpiration/1000/60))
	print("- OffHand")
	print("-- HasOffhandPoison = " .. hasOffHandEnchant)
	print("-- ExpirationOffhand = " .. minUntilExpire .. "min")

	if isAbleToApplyPoison(poisonOff) and minUntilExpire < refreshPoisionAtMinLeft then
		applyOffHandPoisonByName(poisonOff)
	elseif not isAbleToApplyPoison(poisonMain) then
		print("Can not apply OffHandPoison maybe no items in bag!")
	end

	]===]
else
	if isAbleToApplyPoison(poisonOff) then
		--[[ print("Apply OffHandEnchant: " .. poisonOff) ]]
		applyOffHandPoisonByName(poisonOff)
	else
		print("Can not apply OffHandPoison maybe no items in bag!")
	end
end