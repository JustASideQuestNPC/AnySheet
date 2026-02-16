-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- @alias ModuleColor
--- | "primary"
--- | "secondary"
--- | "tertiary"
--- | "accent"

--- @alias TextAlignment
--- | "left"
--- | "center"
--- | "right"

--- @alias TextStyle
--- | "normal"
--- | "bold"
--- | "italic"
--- | "bold italic"

--- @alias BorderType
--- | "none"
--- | "underline"
--- | "full"

--- Abstract base class for character sheet elements.
--- @class (exact) ModuleElement
--- @field x integer The element's x position in the module's grid. (0, 0) is always the top left
---     corner of the module, regardless of the module's position in the character sheet.
--- @field y integer The element's y position in the module's grid. (0, 0) is always the top left
---     corner of the module, regardless of the module's position in the character sheet.
--- @field width integer The element's width in grid columns.
--- @field height integer The element's height in grid rows.
ModuleElement = {}

--- A character sheet module. Each module script should return exactly one module instance (created
--- using `SheetModule.create()`) and nothing else.
--- @class SheetModule
SheetModule = {}

--- Generates a character sheet module.
--- Parameters:
--- - `elements`: An array with all elements in the module. The module is automatically sized to fit
---     all the elements inside it.
--- @param args { elements: ModuleElement[] }
--- @return SheetModule
function SheetModule.create(args) end