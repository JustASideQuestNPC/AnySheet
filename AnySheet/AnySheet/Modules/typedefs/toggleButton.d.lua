-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A button that can be toggled on and off. This element's width and height are always 1.
--- @class (exact) ToggleButton : ModuleElement
--- @field create function
--- @field toggled boolean
ToggleButton = {}

--- Creates the element. `onToggle` is called whenever the button is clicked and is passed whether
--- the button is now toggled on.
--- @param args { x: integer, y: integer, onToggle: (fun(toggled: boolean): nil)? }
--- @return ToggleButton
function ToggleButton.create(args) end