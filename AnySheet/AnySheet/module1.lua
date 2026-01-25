local module = SheetModule.create({
	noBorder = true,
	elements = {
		StaticText.create({
			x = 0,
			y = 0,
			width = 6,
			height = 2,
			text = "two\nlines",
			style = "bold"
		}),
		TextBox.create({
			x = 0,
			y = 2,
			width = 6,
			height = 2
		}),
		StaticText.create({
			x = 0,
			y = 4,
			width = 6,
			height = 2,
			text = "text text text text text text text text",
			style = "bold italic"
		}),
	}
});
return module;