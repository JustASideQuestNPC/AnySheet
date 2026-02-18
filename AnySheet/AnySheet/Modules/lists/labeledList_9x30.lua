---@sheetModule
---@name Labeled List (9x30)

return SheetModule.create({
    elements = {
        TextBox.create({
            x = 0,
            y = 0,
            width = 9,
            height = 2,
            color = "accent",
            defaultText = "List",
            alignment = "center"
        }),
        List.create({
            x = 0,
            y = 2,
            width = 9,
            height = 28
        })
    }
})