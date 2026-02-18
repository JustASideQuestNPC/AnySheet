---@sheetModule
---@name Single Value with Side Label (9x2)

return SheetModule.create({
    elements = {
        NumberBox.create({
            x = 0,
            y = 0,
            width = 2,
            height = 2,
            borderType = "full"
        }),

        TextBox.create({
            x = 2,
            y = 0,
            width = 7,
            height = 2,
            alignment = "center",
            color = "accent",
            borderType = "none",
            defaultText = "Single Value",
            style = "bold"
        }),
    }
})