---@sheetModule
---@name Labeled Multiline (12x6)

return SheetModule.create({
    elements = {
        TextBox.create({
            x = 0,
            y = 0,
            width = 12,
            height = 1,
            color = "accent",
            defaultText = "Multiline Box",
            alignment = "center"
        }),
        MultiLineTextBox.create({
            x = 0,
            y = 1,
            width = 12,
            height = 5,
            borderType = "full"
        })
    }
})