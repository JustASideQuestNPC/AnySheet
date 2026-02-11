-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A text box that only accepts numbers.
--- @class (exact) NumberBox : ModuleElement
--- @field create function
--- @field value number
--- @field minValue number
--- @field maxValue number
NumberBox = {}

--- Creates the element.
--- @param args { x: integer, y: integer, width: integer, height: integer, allowDecimal: boolean?,
---               minValue: integer?, maxValue: integer?, defaultValue: number?,
---               borderType: BorderType?, borderColor: ModuleColor?, isModifier: boolean? }
--- @return NumberBox
function NumberBox.create(args) end