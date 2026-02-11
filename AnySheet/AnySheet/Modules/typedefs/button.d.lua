-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A button that runs a callback when clicked.
--- @class (exact) Button : ModuleElement
--- @field create function
Button = {}

--- Creates the element. `callback` is run once whenever the button is clicked.
--- @param args { x: integer, y: integer, width: integer, height: integer, icon: string,
---               callback: fun(): nil }
--- @return Button
function Button.create(args) end