local currentUses = NumberBox.create({
	x = 0,
	y = 1,
	width = 2,
	height = 2,
	minValue = 0,
})
local maxUses = NumberBox.create({
	x = 3,
	y = 1,
	width = 2,
	height = 2,
	minValue = 0,
	maxValue = 10
})

return SheetModule.create({
	elements = {
		TextBox.create({
			x = 0,
			y = 0,
			width = 5,
			height = 1,
			color = "accent",
			alignment = "center"
		}),
		currentUses,
		StaticText.create({
			x = 0,
			y = 3,
			width = 2,
			height = 1,
			text = "Current",
			alignment = "center",
			color = "secondary"
		}),

		Button.create({
			x = 2,
			y = 1,
			width = 1,
			height = 1,
			icon = "plus",
			callback = function()
				if (currentUses.value < maxUses.value) then
					currentUses.value = currentUses.value + 1
				end
			end
		}),

		Button.create({
			x = 2,
			y = 2,
			width = 1,
			height = 1,
			icon = "minus",
			callback = function()
				if (currentUses.value > 0) then
					currentUses.value = currentUses.value - 1
				end
			end
		}),

		maxUses,
		StaticText.create({
			x = 3,
			y = 3,
			width = 2,
			height = 1,
			text = "Max",
			alignment = "center",
			color = "secondary"
		}),
	}
});