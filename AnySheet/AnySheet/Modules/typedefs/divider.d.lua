-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A divider line.
--- @class (exact) Divider : ModuleElement
--- @field create function
Divider = {}

--- Creates the element.
--- @param args { x: integer, y: integer, direction: "vertical" | "horizontal", length: integer,
---               color: ModuleColor, thickness: integer?, capStart: boolean?, capEnd: boolean?,
---               betweenSquares: boolean? }
function Divider.create(args) end