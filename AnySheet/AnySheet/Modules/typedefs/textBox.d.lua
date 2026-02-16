-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A single-line text input.
--- @class (exact) TextBox : ModuleElement
--- @field create function
--- @field text string
TextBox = {}

--- Creates the element.
--- 
--- Special parameters:
--- - `color`: The color of the text itself.
--- - `defaultText` (optional, default=`""`): Text to display before anything is typed into the box.
--- - `alignment` (optional, default=`"left"`): Horizontal alignment of the text. Text is always
---   centered vertically.
--- - `style` (optional, default=`"normal"`): The font style.
--- - `borderType` (optional, default=`"underline"`): The border style. `"full"` draws a border all
---   the way around the text, and `"underline"` only draws the border under the text.
--- - `borderColor` (optional, default=`"primary"`): The color of the border. Does nothing if
---   `borderType` is set to `"none"`.
--- @param args { x: integer, y: integer, width: integer, height: integer, color: ModuleColor,
---               defaultText: string?, alignment: TextAlignment?, style: TextStyle?,
---               borderType: BorderType?, borderColor: ModuleColor? }
function TextBox.create(args) end