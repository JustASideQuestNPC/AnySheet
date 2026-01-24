local module = SheetModule.create({
	noBorder = false,
	elements = {
		StaticText.create({
			x = 0,
			y = 0,
			width = 4,
			height = 1,
			text = "small text",
			style = "bold"
		}),
		StaticText.create({
			x = 0,
			y = 1,
			width = 8,
			height = 3,
			text = "big text",
			style = "bold italic"
		}),
	}
});
return module;