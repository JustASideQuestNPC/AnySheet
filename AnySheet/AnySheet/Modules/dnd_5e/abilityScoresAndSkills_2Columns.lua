---@sheetModule
---@name Ability Scores and Skills (2 Columns)

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

local xPos = 0
local yPos = 0
for abilityName, skills in pairs(SKILL_NAMES) do
    -- ability name
    table.insert(elements, StaticText.create({
        x = xPos,
        y = yPos,
        width = 4,
        height = 1,
        text = abilityName,
        alignment = "center"
    }))

    -- modifier box
    table.insert(elements, NumberBox.create({
        x = xPos,
        y = yPos + 1,
        width = 4,
        height = 4,
        alignment = "center",
        style = "bold",
        isModifier = true
    }))

    -- ability score box
    table.insert(elements, NumberBox.create({
        x = xPos + 1,
        y = yPos + 5,
        width = 2,
        height = 2
    }))

    -- divider
    table.insert(elements, Divider.create({
        x = xPos + 4,
        y = yPos,
        direction = "horizontal",
        length = 5,
        color = "secondary",
        thickness = 2
    }))

    -- skills
    for i, skillName in ipairs(skills) do
        table.insert(elements, ToggleButton.create({
            x = xPos + 4,
            y = yPos + i
        }))
        table.insert(elements, StaticText.create({
            x = xPos + 5,
            y = yPos + i,
            width = 4,
            height = 1,
            alignment = "left",
            text = skillName
        }))
    end

    yPos = yPos + 7;
    if yPos == 21 then
        yPos = 0
        xPos = xPos + 9
    end
end

table.insert(elements, Divider.create({
    x = 8,
    y = 0,
    direction = "vertical",
    length = 21,
    color = "secondary",
    thickness = 2,
    betweenSquares = true
}))

return SheetModule.create({
    elements = elements
})