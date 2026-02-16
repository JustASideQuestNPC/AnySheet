-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A button that can be toggled on and off. This element's width and height are always 1.
--- @class (exact) ToggleButton : ModuleElement
--- @field create function
--- @field toggled boolean
ToggleButton = {}

--- Creates the element.
--- 
--- Special parameters:
--- - `onToggle` (optional, default=`nil`): Called whenever the button is toggled on or off. Takes
---   a single argument, which is whether the button was toggled on or off.
--- @param args { x: integer, y: integer, onToggle: (fun(toggled: boolean): nil)? }
--- @return ToggleButton
function ToggleButton.create(args) end