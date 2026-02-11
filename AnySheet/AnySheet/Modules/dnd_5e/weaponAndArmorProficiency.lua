return SheetModule.create({
    elements = {
        StaticText.create({
            x = 0,
            y = 0,
            width = 4,
            height = 1,
            color = "primary",
            alignment = "center",
            text = "Weapons"
        }),

        Divider.create({
            x = 0,
            y = 1,
            direction = "horizontal",
            length = 4,
            color = "secondary",
            thickness = 2,
        }),

        ToggleButton.create({
            x = 0,
            y = 2
        }),
        StaticText.create({
            x = 1,
            y = 2,
            width = 3,
            height = 1,
            color = "secondary",
            alignment = "left",
            text = "Simple"
        }),

        ToggleButton.create({
            x = 0,
            y = 3
        }),
        StaticText.create({
            x = 1,
            y = 3,
            width = 3,
            height = 1,
            color = "secondary",
            alignment = "left",
            text = "Martial"
        }),

        ToggleButton.create({
            x = 0,
            y = 4
        }),
        StaticText.create({
            x = 1,
            y = 4,
            width = 3,
            height = 1,
            color = "secondary",
            alignment = "left",
            text = "Shields"
        }),

        Divider.create({
            x = 4,
            y = 0,
            direction = "vertical",
            length = 5,
            color = "secondary",
            thickness = 2,
        }),

        StaticText.create({
            x = 5,
            y = 0,
            width = 4,
            height = 1,
            color = "primary",
            alignment = "center",
            text = "Armor"
        }),

        Divider.create({
            x = 5,
            y = 1,
            direction = "horizontal",
            length = 4,
            color = "secondary",
            thickness = 2,
        }),

        ToggleButton.create({
            x = 5,
            y = 2
        }),
        StaticText.create({
            x = 6,
            y = 2,
            width = 3,
            height = 1,
            color = "secondary",
            alignment = "left",
            text = "Light"
        }),

        ToggleButton.create({
            x = 5,
            y = 3
        }),
        StaticText.create({
            x = 6,
            y = 3,
            width = 3,
            height = 1,
            color = "secondary",
            alignment = "left",
            text = "Medium"
        }),

        ToggleButton.create({
            x = 5,
            y = 4
        }),
        StaticText.create({
            x = 6,
            y = 4,
            width = 3,
            height = 1,
            color = "secondary",
            alignment = "left",
            text = "Heavy"
        }),
    }
})