---@sheetModule
---@name Attack Quick Reference

local elements = {
    StaticText.create({
        x = 0,
        y = 0,
        width = 9,
        height = 1,
        color = "accent",
        text = "Attacks",
        alignment = "center"
    }),
    Divider.create({
        x = 0,
        y = 0,
        direction = "horizontal",
        length = 9,
        color = "accent",
        thickness = 2,
        betweenSquares = true
    })
}

for i = 1, 7, 1 do
    -- name
    table.insert(elements, TextBox.create({
        x = 0,
        y = i,
        width = 4,
        height = 1,
        color = "primary",
        alignment = "left",
        defaultText = "Name",
        borderType = "underline"
    }))

    -- attack modifier
    table.insert(elements, NumberBox.create({
        x = 4,
        y = i,
        width = 2,
        height = 1,
        color = "primary",
        borderType = "underline",
        isModifier = true
    }))

    -- damage
    table.insert(elements, TextBox.create({
        x = 6,
        y = i,
        width = 3,
        height = 1,
        color = "primary",
        alignment = "left",
        defaultText = "Damage",
        borderType = "underline"
    }))
end

return SheetModule.create({
    elements = elements
})