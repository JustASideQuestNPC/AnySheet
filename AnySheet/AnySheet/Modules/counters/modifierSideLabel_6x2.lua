return SheetModule.create({
    elements = {
        NumberBox.create({
            x = 0,
            y = 0,
            width = 2,
            height = 2,
            borderType = "full",
            isModifier = true
        }),

        TextBox.create({
            x = 2,
            y = 0,
            width = 4,
            height = 2,
            alignment = "center",
            color = "accent",
            borderType = "none",
            defaultText = "Modifier",
            style = "bold"
        }),
    }
})