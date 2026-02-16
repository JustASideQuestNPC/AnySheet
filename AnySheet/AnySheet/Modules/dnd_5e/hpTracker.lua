-- A shameless clone of the cool D&DBeyond HP tracker. Overly commented to act as some kind of cheat
-- sheet until I write proper documentation for the scripting API.

-- Module scripts are written in Lua; you can find a lot of resources for Lua itself online.
-- To actually make a module:
-- 1. Create a module using SheetModule.create() (that's at the bottom of this script)
-- 2. Give the module a list of elements. Every element takes different parameters, except for the
--    x and y coordinate.
-- 3. Return the module.
-- 4. Profit.

-- All module elements are created using <ElementName>.create(). The element types are:
-- Button
-- Divider
-- List
-- TextBox
-- MultilineTextBox
-- StaticText
-- ToggleButton
-- TripleToggle
-- If you're using VSCode and have the Lua extension installed, typing <ElementName>.create() will
-- show you all description and all the special parameters. Every element has an x and y coordinate,
-- and most have a width and height. The x and y coordinates must be greater than 0 and are at the
-- top left corner of the element. (0, 0) is the top left corner of the module.

-- heal/damage amount
local inputBox = NumberBox.create({ -- Don't forget the curly braces!
	x = 0,
	y = 4,
	width = 3,
	height = 2,
	minValue = 0,
	borderType = "full"
})

local maxHp = NumberBox.create({
	x = 0,
	y = 1,
	width = 3,
	height = 2,
	minValue = 0,
	borderType = "full",
})

local currentHp = NumberBox.create({
	x = 3,
	y = 1,
	width = 3,
	height = 2,
	minValue = 0,
	borderType = "full",
})

local tempHp = NumberBox.create({
	x = 6,
	y = 1,
	width = 3,
	height = 2,
	minValue = 0,
	borderType = "full",
})

-- Buttons can have a "callback" that runs when the button is clicked. This one deals damage when
-- the damage button is clicked
local function damageCallback()
	-- NumberBox.value gets the number typed in the box
	local damage = inputBox.value

	-- if there's enough temp hp to take the damage, just subtract from that
	if tempHp.value >= damage then
		tempHp.value = tempHp.value - damage
	-- otherwise, subtract temp hp and deal excess damage to current health
	else
		damage = damage - tempHp.value
		tempHp.value = 0
		currentHp.value = currentHp.value - damage
	end

	-- cap current hp at 0
	if currentHp.value < 0 then
		currentHp.value = 0
	end

	-- clear the input
	inputBox.value = 0
end

local damageButton = Button.create({
	x = 5,
	y = 4,
	width = 2,
	height = 2,
	icon = "heart-broken",
	callback = damageCallback
})

local healButton = Button.create({
	x = 3,
	y = 4,
	width = 2,
	height = 2,
	icon = "heart-plus",
	-- callback functions can also be defined "inline" without assigning them to a variable
	callback = function()
		print(tostring(currentHp.value) .. ", " .. tostring(inputBox.value))
		currentHp.value = currentHp.value + inputBox.value

		if currentHp.value > maxHp.value then
			currentHp.value = maxHp.value
		end

		inputBox.value = 0
	end
})

return SheetModule.create({
	-- sheet modules require a list of at least 1 element
	elements = {
        maxHp,
		currentHp,
        tempHp,
		inputBox,
        healButton,
		damageButton,

		-- elements can also be created without assigning them to a variable
		-- NOTE: you cannot do this if you want to access the element from a function!
		StaticText.create({
			x = 0,
			y = 3,
			width = 3,
			height = 1,
			text = "Max",
			alignment = "center",
			color = "secondary"
		}),

		StaticText.create({
			x = 0,
			y = 0,
			width = 9,
			height = 1,
			text = "Hit Points",
			alignment = "center",
            style = "bold",
			color = "accent"
		}),

		StaticText.create({
			x = 3,
			y = 3,
			width = 3,
			height = 1,
			text = "Current",
			alignment = "center",
			color = "secondary"
		}),

		StaticText.create({
			x = 6,
			y = 3,
			width = 3,
			height = 1,
			text = "Temp",
			alignment = "center",
			color = "secondary"
		}),
	}
});