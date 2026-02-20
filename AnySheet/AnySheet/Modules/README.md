# AnySheet Scripting Docs
Every AnySheet module is a [Lua](https://www.lua.org/about.html) script, and you can create your own
modules by writing new scripts. Don't worry if you've never written code before, I promise it's not
hard (seriously, I wrote all the scripts here in about 3 hours and most of that was deciding what I
wanted them to look like).

### Contents
- [Getting Started](#getting-started)
    - [Module Files](#module-files)
    - [Your First Module](#your-first-module)
    - [Module Elements](#module-elements)
    - [Updating Elements](#updating-elements)
- [Full Documentation](#full-documentation)
    - [Enums](#enums)
    - [SheetModule](#sheet-module)
    - [ModuleElementBase](#module-element-base)
    - [Button](#button)
    - [Divider](#divider)
    - [List](#list)
    - [MultilineTextBox](#multilinetextbox)
    - [NumberBox](#numberbox)
    - [StaticText](#statictext)
    - [TextBox](#textbox)
    - [ToggleButton](#togglebutton)
    - [TripleToggle](#tripletoggle)

## Getting Started
If you've never written Lua before, I highly recommend installing
[VSCode](https://code.visualstudio.com/download) and the
[Lua extension](https://marketplace.visualstudio.com/items?itemName=sumneko.lua) for it. AnySheet
includes type definitions that work with the Lua extension's autocomplete.

### Module Files
All modules are in the Modules folder, which is located in the same place you installed AnySheet. If
you used the default install location, the Modules folder is at
`C:/Users/<your name>/AppData/Roaming/AnySheet/Modules` (if you can't see AppData in the file
explorer, make sure it's showing hidden files). Alternatively, open AnySheet, click "new sheet" so
you can see the module sidebar, and click the folder icon to open the Modules folder in the file
explorer.

**Note:** For autocomplete to work in VSCode, you need to open the entire Modules folder in it, not
just whatever folder you're putting your script in.

### Your First Module
For your first module, create a file in any of the folders (or make a new one) inside the Modules
folder and name it `CustomModule.lua`. Modules won't appear in the sidebar unless they're in a
subfolder. If AnySheet is currently running, you'll need to reload the sidebar to see the module.

Every module script returns a single module object, which you can create using
`SheetModule.create()`:
```lua
local customModule = SheetModule.create({})
return customModule
```
If you try to load this module, it'll fail because your module doesn't have anything inside it.

### Module Elements
Every module requires at least one element. Elements are the most basic building blocks of the
module, and there are 9 different kinds:
- [Button](#button)
- [Divider](#divider)
- [List](#list)
- [MultilineTextBox](#multilinetextbox)
- [NumberBox](#numberbox)
- [StaticText](#statictext)
- [TextBox](#textbox)
- [ToggleButton](#togglebutton)
- [TripleToggle](#tripletoggle)

Elements are created using `<elementName>.create()` and placed in a list when you create the module:
```lua
local customNumberBox = NumberBox.create({
    x = 0,
    y = 0,
    width = 2,
    height = 2
})

local customText = StaticText.create({
    x = 0,
    y = 2,
    width = 4,
    height = 1,
    alignment = "center",
    text = "Some Text"
})

local customModule = SheetModule.create({
    elements = {
        customNumberBox,
        customText
    }
})
return customModule;
```
All elements have an x and y coordinate, and most have a width and height. For modules with a width
and height, (x, y) is the position of the module's top left corner. Higher x coordinates move to the
right, and higher y coordinates move down. Width and height must be positive integers, but x and y
can be any integers, including negative ones. The module will resize itself to fit all elements.

### Updating Elements
Most elements also have things you can change from inside the script. Usually you'll do this by
attaching a function to one of the two button elements, which will run when the button is clicked.
This big block of code will create a module with a place to enter a number, and a button that
increases the number when clicked.
```lua
local customNumberBox = NumberBox.create({
    x = 0,
    y = 0,
    width = 2,
    height = 2
})

local customText = StaticText.create({
    x = 0,
    y = 2,
    width = 4,
    height = 1,
    alignment = "center",
    text = "Some Text"
})

local function onClick()
    customNumberBox.value = customNumberBox.value + 1
end

local customButton = Button.create({
    x = 2,
    y = 0,
    width = 2,
    height = 2,
    icon = "arrow-up-bold-outline"
    callback = onClick
})

local customModule = SheetModule.create({
    elements = {
        customNumberBox,
        customText,
        customButton
    }
})
return customModule;
```
### Module Naming
By default, your module's file name will appear in the sidebar. If you want a custom name, you can
put this comment at the very top of the file:
```
---@sheetModule
---@name <Name>
```

## Full Documentation
### Enums
These enums are used as parameter types in multiple modules:

**ModuleColor:** Can be `"primary"` (black), `"secondary"` (dark gray), `"tertiary"` (light gray),
or `"accent"` (red).

**TextAlignment:** Can be `"left"`, `"center"`, or `"right"`.

**TextStyle:** Can be `"normal"`, `"bold"`, `"italic"`, or `"bold italic"`.

**BorderType:** Can be `"none"` (no border), `"underline"` (border only on the bottom of the
element), or `"full"` (border all the way around the element).

### `SheetModule`
A character sheet module. Each module script should return exactly one module instance (created
using `SheetModule.create()`) and nothing else.

#### `SheetModule.create()`
Generates a new module.

**Parameters:**
- `elements`: An array with all elements in the module. The module is automatically scaled to fit
all the elements in side it.

**Returns:** `SheetModule`

**Signature:**
```lua
SheetModule.create({
    elements: ModuleElement[]
})
```

### `ModuleElementBase`
Abstract base class for character sheet elements.

#### Fields
- `x` (integer): The x position of the element's top left corner.
- `y` (integer): The y position of the element's top left corner.
- `width` (integer): The width of the module.
- `height` (integer): The height of the module.

### `Button`
A button that runs a callback when clicked.

#### `Button.create()`
Generates a new element.

**Special Parameters:**
- `icon`: The name of the icon to display on the button. You can use *any*
  [Material Design](https://pictogrammers.com/library/mdi/) icon.
- `callback`: A callback function; this is run once whenever the button is clicked.

**Returns:** `Button`

**Signature:**
```lua
Button.create({
    x: integer,
    y: integer,
    width: integer,
    height: integer,
    icon: string,
    callback: fun(): nil
})
```

### `Divider`
A divider line.

#### `Divider.create()`
Generates a new element.

**Special Parameters:**
- `direction`: A vertical divider extends downward from its position. A horizontal divider extends
  to the right from its position.
- `length`: The length of the divider in grid cells.
- `color`: The color of the line.
- `thickness`: The thickness of the line in pixels.
- `capStart` (optional, default=`false`): If true, the start of the line (the left/top endpoint)
  ends at the center of that grid cell. Otherwise, it extends all the way to the left/top edge of
  the cell.
- `capEnd` (optional, default=`false`): Same as `capStart`, but for the end of the line (the
  right/bottom endpoint).
- `betweenSquares` (option, default=`false`): If true, the line is aligned width the right (for a
  vertical line) or bottom (for a horizontal line) edge of the cell at (`x`, `y`). Otherwise, it is
  aligned with the center of the cell.

**Returns:** `Divider`

**Signature:**
```lua
Divider.create({
    x: integer,
    y: integer,
    direction: "vertical" | "horizontal",
    length: integer,
    color: ModuleColor,
    thickness: integer?,
    capStart: boolean?,
    capEnd: boolean?,
    betweenSquares: boolean?
})
```

### `List`
A scrollable list that entries can be added to.

#### `List.create()`
Generates a new element.

**Note:** The list itself doesn't extend all the way to the bottom of the element because of the
text box used to add entries.

**Returns:** `List`

**Signature:**
```lua
List.create({
    x: integer,
    y: integer,
    width: integer,
    height: integer
})
```

### `MultilineTextBox`
A multiline text input.

#### Fields
- `text` (string): All text currently in the box.

#### `TextBox.create()`
Generates a new element.

**Returns:** `TextBox`

**Signature:**
```lua
TextBox.create({
    x: integer,
    y: integer,
    width: integer,
    height: integer
})
```

### `NumberBox`
A text box that only accepts numbers.

#### Fields
- `value` (number): The number currently in the box.
- `minValue` (number): The minimum allowed number.
- `maxValue` (number): The maximum allowed number.

#### `NumberBox.create()`
Generates a new element.

**Special Parameters:**
- `allowDecimal` (optional, default=`false`): If true, non-integers can be entered.
- `minValue` (optional, default=`-Infinity`): Minimum allowed value.
- `maxValue` (optional, default=`Infinity`): Maximum allowed value.
- `defaultValue` (optional, default=`0`): Default value if nothing is typed in the box.
- `isModifier` (optional, default=`false`): If true, numbers >= 0 will appear with a + in front of
  them (+0, +1, etc.), which is useful for displaying bonuses/penalties that you add to a die roll.
  This does not affect the actual number, only how it is displayed.
- `borderType` (optional, default=`"underline"`): The border style.
- `borderColor` (optional, default=`"primary"`): The color of the border. Does nothing if
  `borderType` is set to `"none"`.

**Returns:** `NumberBox`

**Signature:**
```lua
NumberBox.create({
    x: integer,
    y: integer,
    width: integer,
    height: integer,
    allowDecimal: boolean?,
    minValue: number?,
    maxValue: number?,
    defaultValue: number?,
    isModifier: boolean?,
    borderType: BorderType?
    borderColor: ModuleColor?
})
```

### `StaticText`
Static, non-modifiable text.

#### `StaticText.create()`
Generates a new element.

**Special Parameters:**
- `text`: The text to display. Text is automatically sized to fit the width and height.
- `color`: The color of the text.
- `alignment`: The horizontal alignment of the text. Text is always centered vertically.
- `style` (optional, default=`"normal"`): The font style.

**Returns:** `StaticText`

**Signature:**
```lua
StaticText.create({
    x: integer,
    y: integer,
    width: integer,
    height: integer,
    text: string,
    color: ModuleColor,
    alignment: TextAlignment?,
    style: TextStyle?
})
```

### `TextBox`
A single-line text input.

#### Fields
- `text` (string): The text currently in the element.

#### `TextBox.create()`
Generates a new element.

**Special Parameters:**
- `color`: The color of the text itself.
- `defaultText` (optional, default=`""`): Text to display before anything is typed into the box.
- `alignment` (optional, default=`"left"`): Horizontal alignment of the text. Text is always
  centered vertically.
- `style` (optional, default=`"normal"`): The font style.
- `borderType` (optional, default=`"underline"`): The border style.
- `borderColor` (optional, default=`"primary"`): The color of the border. Does nothing if
  `borderType` is set to `"none"`.

**Returns:** `TextBox`

**Signature:**
```lua
TextBox.create({
    x: integer,
    y: integer,
    width: integer,
    height: integer,
    color: ModuleColor,
    defaultText: string?,
    alignment: TextAlignment?,
    style: TextStyle?,
    borderType: BorderType?,
    borderColor: ModuleColor?
})
```

### `ToggleButton`
A button that can be toggled on and off. This element's width and height are always 1.

#### Fields
- `toggled` (boolean): Whether the button is currently toggled on.

#### `ToggleButton.create()`
Generates a new element.

**Special Parameters:**
- `onToggle` (optional, default=`nil`): Called whenever the button is toggled on or off. Takes a
  single argument, which is whether the button was toggled on or off.

**Returns:** `ToggleButton`

**Signature:**
```lua
ToggleButton.create({
    x: integer,
    y: integer,
    onToggle: (fun(toggled: boolean): nil)?
})
```

### `TripleToggle`
Like a toggle button, but has a third state (activated by right-clicking) that grays out the button
and disables normal input. This element's width and height are always 1.

#### Fields
- `state` (integer): 0 if the button is disabled, 1 if it is enabled and toggled off, and 2 if it is
  enabled and toggled on.

#### `TripleToggle.create()`
Generates a new element.

**Special Parameters:**
- `onToggle` (optional, default=`nil`): Called whenever the button gets toggled on or off. Takes a
  single argument, which is whether the button was toggled on or off.
- `onStateChange` (optional, default=`nil`): Called whenever the button is toggled on or off, or
  when it is disabled or enabled. Takes a single argument, which is the new state of the button: 0
  if it is disabled, 1 if it is enabled and toggled off, and 2 if it is enabled and toggled on. 

**Note:** Toggling the bottom on or off will call both `onToggle` and `onStateChange`. If it matters
to you for some reason, `onStateChange` is called first.

**Returns:** `TripleToggle`

**Signature:**
```lua
TripleToggle.create({
    x: integer,
    y: integer,
    onToggle: (fun(toggled: boolean): nil)?,
    onStateChange: (fun(state: integer): nil)?
})
```