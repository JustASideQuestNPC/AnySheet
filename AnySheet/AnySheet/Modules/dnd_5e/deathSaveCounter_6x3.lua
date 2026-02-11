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
    StaticText.create({
        x = 0,
        y = 1,
        width = 3,
        height = 1,
        text = "Successes",
        alignment = "left",
        color = "primary"
    }),
    StaticText.create({
        x = 0,
        y = 2,
        width = 3,
        height = 1,
        text = "Failures",
        alignment = "left",
        color = "primary"
    })
}

for i = 1, 3, 1 do
    table.insert(elements, ToggleButton.create({
        x = i + 2,
        y = 1
    }))
    table.insert(elements, ToggleButton.create({
        x = i + 2,
        y = 2
    }))
end

return SheetModule.create({
    elements = elements
})