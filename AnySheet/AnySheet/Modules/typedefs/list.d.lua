-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A scrollable list that entries can be added to.
--- @class (exact) List : ModuleElement
--- @field create function
List = {}

--- Creates the element. The list itself doesn't extend all the way to the bottom of the element
--- because of the text box used to add entries.
--- @param args { x: integer, y: integer, width: integer, height: integer }
--- @return List
function List.create(args) end