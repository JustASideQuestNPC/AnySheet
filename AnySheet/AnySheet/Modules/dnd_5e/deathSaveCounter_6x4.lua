---@sheetModule
---@name Death Save Tracker (6x4)

local elements = {
    StaticText.create({
        x = 0,
        y = 0,
        width = 6,
        height = 1,
        text = "Death Saves",
        alignment = "center",
        color = "accent",
        style = "bold"
    }),

    Divider.create({
        x = 0,
        y = 1,
        direction = "horizontal",
        length = 6,
        color = "secondary",
        thickness = 2,
    }),

    StaticText.create({
        x = 0,
        y = 2,
        width = 3,
        height = 1,
        text = "Successes",
        alignment = "left",
        color = "primary"
    }),
    StaticText.create({
        x = 0,
        y = 3,
        width = 3,
        height = 1,
        text = "Failures",
        alignment = "left",
        color = "primary"
    })
}

---@type ToggleButton[]
local buttons = {} -- secondary array for the reset trigger
for i = 1, 3, 1 do
    local successButton = ToggleButton.create({
        x = i + 2,
        y = 2
    })
    local failureButton = ToggleButton.create({
        x = i + 2,
        y = 3
    })
    table.insert(buttons, successButton)
    table.insert(elements, successButton)
    table.insert(buttons, failureButton)
    table.insert(elements, failureButton)
end

return SheetModule.create({
    elements = elements,
    triggers = {
        ["Reset"] = function()
            for _, button in ipairs(buttons) do
                button.toggled = false
            end
        end
    }
})