-- This file contains type definitions for the VSCode Lua Language Server. It is not a module and
-- has no functionality on its own.
---@meta

--- A divider line.
--- @class (exact) Divider : ModuleElement
--- @field create function
Divider = {}

--- Creates the element.
--- 
--- Special parameters:
--- - `direction`: A vertical divider extends downward from its position. A horizontal divider
---   extends to the right from its position.
--- - `length`: The length of the divider in grid cells.
--- - `color`: The color of the line.
--- - `thickness`: The thickness of the line in pixels.
--- - `capStart` (optional, default=`false`): If true, the start of the line (the left/top endpoint)
---   ends at the center of that grid cell. Otherwise, it extends all the way to the left/top edge
---   of the cell.
--- - `capEnd` (optional, default=`false`): Same as `capStart`, but for the end of the line (the
---   right/bottom endpoint).
--- - `betweenSquares` (option, default=`false`): If true, the line is aligned width the right (for
---   a vertical line) or bottom (for a horizontal line) edge of the cell at (`x`, `y`). Otherwise,
---   it is aligned with the center of the cell.
--- @param args { x: integer, y: integer, direction: "vertical" | "horizontal", length: integer,
---               color: ModuleColor, thickness: integer?, capStart: boolean?, capEnd: boolean?,
---               betweenSquares: boolean? }
function Divider.create(args) end