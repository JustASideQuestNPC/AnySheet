-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A single-line text input.
--- @class (exact) TextBox : ModuleElement
--- @field create function
--- @field text string
TextBox = {}

--- Creates the element. `defaultText` is displayed when the text box is initially created and
--- nothing has been entered into it.
--- @param args { x: integer, y: integer, width: integer, height: integer, defaultText: string?,
---               color: ModuleColor, alignment: TextAlignment?, style: TextStyle?,
---               borderType: BorderType?, borderColor: ModuleColor? }
function TextBox.create(args) end