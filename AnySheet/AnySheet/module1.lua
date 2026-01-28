return SheetModule.create({
	elements = {
		TextBox.create({
			x = 0,
			y = 0,
			width = 3,
			height = 1,
			color = "accent"
		}),
		NumberBox.create({
			x = 0,
			y = 1,
			width = 2,
			height = 2,
			minValue = 0,
		}),
		StaticText.create({
			x = 0,
			y = 3,
			width = 2,
			height = 1,
			text = "Current",
			alignment = "center",
			color = "secondary"
		}),
		
		NumberBox.create({
			x = 3,
			y = 1,
			width = 2,
			height = 2,
			minValue = 0,
		}),
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