---@sheetModule
---@name Modifier with Top Label (3x4)

return SheetModule.create({
    elements = {
        TextBox.create({
            x = 0,
            y = 0,
            width = 3,
            height = 1,
            alignment = "center",
            color = "accent",
            borderType = "none",
            defaultText = "Modifier",
            style = "bold"
        }),

        NumberBox.create({
            x = 0,
            y = 1,
            width = 3,
            height = 3,
            borderType = "full",
            isModifier = true
        })
    }
})