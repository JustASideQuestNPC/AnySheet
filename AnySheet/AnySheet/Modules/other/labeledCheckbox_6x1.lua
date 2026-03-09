---@sheetModule
---@name Labeled Checkbox (6x1)

local button = ToggleButton.create({
    x = 0,
    y = 0
})

return SheetModule.create({
    elements = {
        button,
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
    },
    triggers = {
        ["Check"] = function ()
            button.toggled = true
        end,
        ["Uncheck"] = function ()
            button.toggled = false
        end
    }
})