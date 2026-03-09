---@sheetModule
---@name Spell Slot Tracker (Full Caster)
-- Spell slot tracker for full caster classes (bard, cleric, druid, sorcerer, wizard)

local SPELL_SLOT_AMOUNTS = {4, 3, 3, 3, 3, 2, 2, 1, 1}

local elements = {
	StaticText.create({
		x = 0,
		y = 0,
		width = 6,
		height = 2,
		text = "Spell Slots"
	}),
}

---@type TripleToggle[]
local spellSlotToggles = {} -- secondary array for reset triggger
for level, slots in ipairs(SPELL_SLOT_AMOUNTS) do
	table.insert(elements, StaticText.create({
		x = level - 1,
		y = 2,
		width = 1,
		height = 1,
		text = tostring(level),
		alignment = "center",
		color = "secondary"
	}))

	for i = 1, slots, 1 do
		local toggle = TripleToggle.create({
			x = level - 1,
			y = i + 2
		})
		table.insert(spellSlotToggles, toggle)
		table.insert(elements, toggle)
	end
end

local module = SheetModule.create({
	elements = elements,
	triggers = {
		["Reset all"] = function ()
			for _, toggle in ipairs(spellSlotToggles) do
				toggle.state = 2
			end
		end
	}
});
return module;