---@sheetModule
---@name Ability Scores and Skills (Horizontal)

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

local i = 1
local xPos = 0
for abilityName, skills in pairs(SKILL_NAMES) do
    -- ability name
    table.insert(elements, StaticText.create({
        x = xPos + 3,
        y = 0,
        width = 4,
        height = 1,
        text = abilityName,
        alignment = "center"
    }))

    -- modifier box
    table.insert(elements, NumberBox.create({
        x = xPos,
        y = 1,
        width = 4,
        height = 4,
        alignment = "center",
        style = "bold",
        isModifier = true
    }))

    -- ability score box
    table.insert(elements, NumberBox.create({
        x = xPos + 1,
        y = 5,
        width = 2,
        height = 2
    }))

    -- skills
    for j, skillName in ipairs(skills) do
        table.insert(elements, ToggleButton.create({
            x = xPos + 4,
            y = j
        }))
        table.insert(elements, StaticText.create({
            x = xPos + 5,
            y = j,
            width = 4,
            height = 1,
            alignment = "left",
            text = skillName
        }))
    end

    -- divider
    if i < 6 then
        table.insert(elements, Divider.create({
            x = xPos + 9,
            y = 0,
            direction = "vertical",
            length = 7,
            color = "secondary",
            thickness = 2
        }))
    end

    xPos = xPos + 10
    i = i + 1
end

return SheetModule.create({
    elements = elements
})