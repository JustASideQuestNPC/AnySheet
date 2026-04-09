-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A multiline text input.
--- @class (exact) MultilineTextBox : ModuleElement
--- @field create function
--- @field text string All text currently in the box.
MultilineTextBox = {}

--- Creates the element.
--- @param args { x: integer, y: integer, width: integer, height: integer }
--- @return MultilineTextBox
function MultilineTextBox.create(args) end