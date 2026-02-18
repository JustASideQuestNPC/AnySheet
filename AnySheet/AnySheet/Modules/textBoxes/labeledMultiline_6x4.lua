---@sheetModule
---@name Labeled Multiline (6x4)

return SheetModule.create({
    elements = {
        TextBox.create({
            x = 0,
            y = 0,
            width = 6,
            height = 1,
            color = "accent",
            defaultText = "Multiline Box",
            alignment = "center"
        }),
        MultiLineTextBox.create({
            x = 0,
            y = 1,
            width = 6,
            height = 3,
            borderType = "full"
        })
    }
})