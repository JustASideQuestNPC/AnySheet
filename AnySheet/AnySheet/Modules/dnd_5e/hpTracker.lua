local inputBox = NumberBox.create({
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

return SheetModule.create({
	elements = {
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

        maxHp,
		StaticText.create({
			x = 0,
			y = 3,
			width = 3,
			height = 1,
			text = "Max",
			alignment = "center",
			color = "secondary"
		}),

		currentHp,
		StaticText.create({
			x = 3,
			y = 3,
			width = 3,
			height = 1,
			text = "Current",
			alignment = "center",
			color = "secondary"
		}),

        tempHp,
		StaticText.create({
			x = 6,
			y = 3,
			width = 3,
			height = 1,
			text = "Temp",
			alignment = "center",
			color = "secondary"
		}),

		inputBox,

        -- heal button
        Button.create({
			x = 3,
			y = 4,
			width = 2,
			height = 2,
			icon = "heart-plus",
			callback = function()
                print(tostring(currentHp.value) .. ", " .. tostring(inputBox.value))
                currentHp.value = currentHp.value + inputBox.value

                if currentHp.value > maxHp.value then
                    currentHp.value = maxHp.value
                end

				inputBox.value = 0
			end
		}),

        -- damage button
		Button.create({
			x = 5,
			y = 4,
			width = 2,
			height = 2,
			icon = "heart-broken",
			callback = function()
                print(tostring(currentHp.value) .. ", " .. tostring(inputBox.value))
                local damage = inputBox.value

                if tempHp.value >= damage then
                    tempHp.value = tempHp.value - damage
                else
                    damage = damage - tempHp.value
                    tempHp.value = 0
                    currentHp.value = currentHp.value - damage
                end

                if currentHp.value < 0 then
                    currentHp.value = 0
                end

				inputBox.value = 0
			end
		}),
	}
});