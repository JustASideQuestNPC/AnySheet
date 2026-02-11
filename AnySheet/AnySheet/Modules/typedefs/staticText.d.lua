-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- Static, non-modifiable text.
--- @class (exact) StaticText : ModuleElement
--- @field create function
StaticText = {}

--- Creates the element. The width and height parameters specify the size of the container; the font
--- size is automatically determined to fit the text.
--- @param args { x: integer, y: integer, width: integer, height: integer, text: string,
---               color: ModuleColor, alignment: TextAlignment?, style: TextStyle? }
--- @return StaticText
function StaticText.create(args) end