local elements = {
    StaticText.create({
        x = 0,
        y = 0,
        width = 12,
        height = 2,
        color = "accent",
        style = "bold",
        text = "Attacks",
        alignment = "center"
    }),
}

for i = 2, 8, 2 do
    -- name
    table.insert(elements, TextBox.create({
        x = 0,
        y = i,
        width = 5,
        height = 2,
        color = "primary",
        alignment = "left",
        defaultText = "Name",
        borderType = "underline"
    }))

    -- attack modifier
    table.insert(elements, NumberBox.create({
        x = 5,
        y = i,
        width = 2,
        height = 2,
        color = "primary",
        borderType = "underline",
        isModifier = true
    }))

    -- damage
    table.insert(elements, TextBox.create({
        x = 7,
        y = i,
        width = 5,
        height = 2,
        color = "primary",
        alignment = "left",
        defaultText = "Name",
        borderType = "underline"
    }))
end

return SheetModule.create({
    elements = elements
})