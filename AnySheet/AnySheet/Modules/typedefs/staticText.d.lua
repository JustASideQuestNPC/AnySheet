-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- Static, non-modifiable text.
--- @class (exact) StaticText : ModuleElement
--- @field create function
StaticText = {}

--- Creates the element.
--- 
--- Special parameters:
--- - `text`: The text to display. Text is automatically sized to fit the width and height.
--- - `color`: The color of the text.
--- - `alignment`: The horizontal alignment of the text. Text is always centered vertically.
--- - `style` (optional, default=`"normal"`): The font style.
--- @param args { x: integer, y: integer, width: integer, height: integer, text: string,
---               color: ModuleColor, alignment: TextAlignment?, style: TextStyle? }
--- @return StaticText
function StaticText.create(args) end