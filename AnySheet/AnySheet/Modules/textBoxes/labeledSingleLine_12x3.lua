---@sheetModule
---@name Labeled Single Line (12x3)

return SheetModule.create({
    elements = {
        TextBox.create({
            x = 0,
            y = 0,
            width = 12,
            height = 2,
            borderType = "underline"
        }),
        TextBox.create({
            x = 0,
            y = 2,
            width = 12,
            height = 1,
            borderType = "none",
            color = "secondary"
        })
    }
})