return SheetModule.create({
    elements = {
        ToggleButton.create({
            x = 0,
            y = 0
        }),
        TextBox.create({
            x = 1,
            y = 0,
            width = 5,
            height = 1,
            alignment = "left",
            color = "accent",
            borderType = "none",
            defaultText = "Single Value",
            style = "bold"
        }),
    }
})