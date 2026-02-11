-- Tracks all 6 ability scores and your skill/saving throw proficiencies.

local SKILL_NAMES = {
    Strength = {
        "Saving Throws",
        "Athletics"
    },
    Dexterity = {
        "Saving Throws",
        "Acrobatics",
        "Sleight of Hand",
        "Stealth"
    },
    Constitution = {
        "Saving Throws"
    },
    Intelligence = {
        "Saving Throws",
        "Arcana",
        "History",
        "Investigation",
        "Nature",
        "Religion"
    },
    Wisdom = {
        "Saving Throws",
        "Animal Handling",
        "Insight",
        "Medicine",
        "Perception",
        "Survival"
    },
    Charisma = {
        "Saving Throws",
        "Deception",
        "Intimidation",
        "Performance",
        "Persuasion"
    }
}

local elements = {}

local yPos = 0
for abilityName, skills in pairs(SKILL_NAMES) do
    -- ability name
    table.insert(elements, StaticText.create({
        x = 0,
        y = yPos,
        width = 4,
        height = 1,
        text = abilityName,
        alignment = "center"
    }))

    -- modifier box
    table.insert(elements, NumberBox.create({
        x = 0,
        y = yPos + 1,
        width = 4,
        height = 4,
        alignment = "center",
        style = "bold",
        isModifier = true
    }))

    -- ability score box
    table.insert(elements, NumberBox.create({
        x = 1,
        y = yPos + 5,
        width = 2,
        height = 2
    }))

    -- divider
    table.insert(elements, Divider.create({
        x = 4,
        y = yPos,
        direction = "horizontal",
        length = 5,
        color = "secondary",
        thickness = 2
    }))

    -- skills
    for i, skillName in ipairs(skills) do
        table.insert(elements, ToggleButton.create({
            x = 4,
            y = yPos + i
        }))
        table.insert(elements, StaticText.create({
            x = 5,
            y = yPos + i,
            width = 4,
            height = 1,
            alignment = "left",
            text = skillName
        }))
    end

    yPos = yPos + 7;
end

return SheetModule.create({
    elements = elements
})