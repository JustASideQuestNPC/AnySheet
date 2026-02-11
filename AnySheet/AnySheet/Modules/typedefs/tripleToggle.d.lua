-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- Like a toggle button, but has a third state that grays out the button and disables normal input.
--- This element's width and height are always 1.
--- 
--- `state` is 0 if the button is disabled, 1 if it is enabled and toggled off, and 2 if it is
--- enabled and toggled on.
--- @class (exact) TripleToggle : ModuleElement
--- @field create function
--- @field state integer
TripleToggle = {}

--- Creates the element.
--- - `onToggle`: Called whenever the button is enabled and gets toggled on or off. The `toggled`
---     parameter is whether the button is now toggled on.
--- - `onStateChange`: Called whenever the button is toggled on or off, or when it is disabled. The
---     `state` parameter is 0, 1, or 2 depending on the button's new state: 0 if it is disabled, 1
---     if it is enabled and toggled off, and 2 if it is disabled and toggled on.
--- @param args { x: integer, y: integer, onToggle: (fun(toggled: boolean): nil)?,
---               onStateChange: (fun(state: integer): nil)? }
--- @return TripleToggle
function TripleToggle.create(args) end