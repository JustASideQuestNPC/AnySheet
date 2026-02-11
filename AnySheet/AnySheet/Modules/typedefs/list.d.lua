-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A single-line text input.
--- @class (exact) List : ModuleElement
--- @field create function
List = {}

--- Creates the element. `defaultText` is displayed when the text box is initially created and
--- nothing has been entered into it.
--- @param args { x: integer, y: integer, width: integer, height: integer }
function List.create(args) end