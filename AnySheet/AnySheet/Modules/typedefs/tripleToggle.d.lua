-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- Like a toggle button, but has a third state (activated by right-clicking) that grays out the
--- button and disables normal input. This element's width and height are always 1.
--- 
--- `state` is 0 if the button is disabled, 1 if it is enabled and toggled off, and 2 if it is
--- enabled and toggled on.
--- @class (exact) TripleToggle : ModuleElement
--- @field create function
--- @field state integer
TripleToggle = {}

--- Creates the element.
--- 
--- Special parameters:
--- - `onToggle` (optional, default=`nil`): Called whenever the button gets toggled on or off. Takes
---   a single argument, which is whether the button was toggled on or off.
--- - `onStateChange` (optional, default=`nil`): Called whenever the button is toggled on or off, or when it is disabled or
---   enabled. Takes a single argument, which is the new state of the button: 0 if it is disabled,
---   1 if it is enabled and toggled off, and 2 if it is enabled and toggled on. 
--- 
--- **Note:** Toggling the bottom on or off will call both `onToggle` and `onStateChange`. If it
--- matters to you for some reason, `onStateChange` is called first.
--- @param args { x: integer, y: integer, onToggle: (fun(toggled: boolean): nil)?,
---               onStateChange: (fun(state: integer): nil)? }
--- @return TripleToggle
function TripleToggle.create(args) end