---@sheetModule
---@name Labeled Multiline (12x12)

return SheetModule.create({
    elements = {
        TextBox.create({
            x = 0,
            y = 0,
            width = 12,
            height = 2,
            color = "accent",
            defaultText = "Multiline Box",
            alignment = "center"
        }),
        MultiLineTextBox.create({
            x = 0,
            y = 2,
            width = 12,
            height = 10,
            borderType = "full"
        })
    }
})