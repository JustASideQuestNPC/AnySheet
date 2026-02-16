-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A text box that only accepts numbers.
--- @class (exact) NumberBox : ModuleElement
--- @field create function
--- @field value number
--- @field minValue number
--- @field maxValue number
NumberBox = {}

--- Creates the element.
--- 
--- Special parameters:
--- - `allowDecimal` (optional, default=`false`): If true, non-integers can be entered.
--- - `minValue` (optional, default=`-Infinity`): Minimum allowed value.
--- - `maxValue` (optional, default=`Infinity`): Maximum allowed value.
--- - `defaultValue` (optional, default=`0`): Default value if nothing is typed in the box.
--- - `isModifier` (optional, default=`false`): If true, numbers >= 0 will appear with a + in front
---   of them (+0, +1, etc.), which is useful for displaying bonuses/penalties you add to a die
---   roll. This does not affect the actual number, only how it is displayed.
--- - `borderType` (optional, default=`"underline"`): The border style. `"full"` draws a border all
---   the way around the text, and `"underline"` only draws the border under the text.
--- - `borderColor` (optional, default=`"primary"`): The color of the border. Does nothing if
---   `borderType` is set to `"none"`.
--- 
--- @param args { x: integer, y: integer, width: integer, height: integer, allowDecimal: boolean?,
---               minValue: number?, maxValue: number?, defaultValue: number?, isModifier: boolean?,
---               borderType: BorderType?, borderColor: ModuleColor? }
--- @return NumberBox
function NumberBox.create(args) end