---@sheetModule
---@name Modifier with Side Label (12x3)

return SheetModule.create({
    elements = {
        NumberBox.create({
            x = 0,
            y = 0,
            width = 3,
            height = 3,
            borderType = "full",
            isModifier = true
        }),

        TextBox.create({
            x = 3,
            y = 0,
            width = 9,
            height = 3,
            alignment = "center",
            color = "accent",
            borderType = "none",
            defaultText = "Modifier",
            style = "bold"
        }),
    }
})