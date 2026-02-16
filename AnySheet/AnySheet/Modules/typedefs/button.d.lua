-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A button that runs a callback when clicked.
--- @class (exact) Button : ModuleElement
--- @field create function
Button = {}

--- Creates the element.
--- 
--- Special parameters:
--- - `icon`: The name of the icon to display on the button. You can use *any*
---   [Material Design](https://pictogrammers.com/library/mdi/) icon.
--- - `callback`: A callback function; this is run once whenever the button is clicked.
--- @param args { x: integer, y: integer, width: integer, height: integer, icon: string,
---               callback: fun(): nil }
--- @return Button
function Button.create(args) end