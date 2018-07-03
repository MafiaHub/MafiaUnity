This document is still work in progress.

Some formats will receive their own document describing their structure based on what we know so far.

# Data Files

This document describes the structure of Mafia's native data formats as well as their purpose. Some formats will also get detailed technical description based on the researched information.

Game data is stored in `*.dta` archive files within the game directory. The archives are encrypted and some assets are compressed.

If you extract all DTA files, you realize each archive contains a specific data separated by format:

* `MAPS` contains game textures. `bmp` is the uncompressed true color format we use in our port, following `565`, `dx1`, `dx2` and `dx3` compressed formats that were used on weaker PCs with low VRAM VGA cards. `tga` format is used for GUI elements, such as the HUD in game.
* `MODELS` contains `4ds` model files. This is the proprietary model format that represents the visuals, as well as special effects or is used as dummy object used as parent by other objects.
* `MISSIONS` contains all mission data within the game. Each mission consists at least of:
  * `scene2.bin` which describes the mission metadata as well as the list of objects and their properties.
  * `scene.4ds` that is a single model representing the terrain/city layout, which is also used to parent some Scene2 objects.
  * `tree.klz` defines static collisions or references objects that should make use of Mesh Collider.
  * Mission can also consist of few optional files that define extra behaviour or visuals used by the map:
    * `cache.bin` which contains list of all static visual geometry used within the mission. This is used mostly by the city missions as it is a simpler version of `scene2.bin` object listing, but without hierarchy and special properties.
    * `check.bin` defines AI waypoints.
    * `road.bin` is a slightly more advanced version of `check.bin` that also specifies the speed cars should go at each crossroad.
    * `car_table.bin`
    * `clusters.seg`
    * `effects.bin`
* `PATCH` contains data released in form of game patches
* `SYSTEM` 
* `TABLES` contains game texts and translations as well as menu definitions:
  * TODO `menu.def` files.
  * TODO `*.gmf` game fonts?
  * TODO `zidle01.tbl`? (zidle means chair in czech)
  * TODO `MENU` directory.
