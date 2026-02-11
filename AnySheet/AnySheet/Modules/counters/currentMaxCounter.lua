-- Basic counter that tracks current and maximum uses of a feature.

local remainingUses = NumberBox.create({
	x = 0,
	y = 5,
	width = 4,
	height = 2,
	minValue = 0,
	borderType = "full"
})
local maxUses = NumberBox.create({
	x = 0,
	y = 3,
	width = 4,
	height = 2,
	minValue = 0,
	maxValue = 10,
	borderType = "full"
})

return SheetModule.create({
	elements = {
		TextBox.create({
			x = 0,
			y = 0,
			width = 6,
			height = 2,
			color = "accent",
			alignment = "center",
			borderType = "none",
			defaultText = "Feature Name"
		}),


		StaticText.create({
			x = 0,
			y = 2,
			width = 6,
			height = 1,
			alignment = "left",
			color = "secondary",
			text = "Max Uses"
		}),

		maxUses,
		remainingUses,

		StaticText.create({
			x = 0,
			y = 7,
			width = 5,
			height = 1,
			alignment = "left",
			color = "secondary",
			text = "Remaining Uses"
		}),

		Button.create({
			x = 4,
			y = 3,
			width = 2,
			height = 2,
			icon = "plus",
			callback = function()
				if remainingUses.value < maxUses.value then
					remainingUses.value = remainingUses.value + 1
				end
			end
		}),

		Button.create({
			x = 4,
			y = 5,
			width = 2,
			height = 2,
			icon = "minus",
			callback = function()
				if remainingUses.value > 0 then
					remainingUses.value = remainingUses.value - 1
				end
			end
		})
	}
});