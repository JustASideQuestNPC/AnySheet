-- Spell slot tracker for half caster classes

local SPELL_SLOT_AMOUNTS = {4, 3, 3, 3, 2}

local elements = {
	StaticText.create({
		x = 0,
		y = 0,
		width = 6,
		height = 2,
		text = "Spell Slots"
	}),
}

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
		table.insert(elements, TripleToggle.create({
			x = level - 1,
			y = i + 2
		}))
	end
end

local module = SheetModule.create({
	elements = elements
});
return module;