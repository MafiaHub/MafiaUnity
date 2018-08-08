# Modding overview
Mods in MafiaUnity are organized packages stored in `mods/` folder of the MafiaUnity's directory.

They have the following structure:

* ExampleMod
  * mod.json - contains metadata such as mod name, author, version and supported game version.
  * Data/ - contains assets that will be injected to the game
  * Bundles/ - contains Unity packaged asset bundles
  * Scripts/ - contains scripts that extend or implement the gameplay of MafiaUnity
  * Tables/ - consists of various text files acting as resources used by scripts, such as voice lines or vehicle names, etc. This folder might get replaced by an actual database later on.
  
## mod.json
This file describes the mod as well as its dependencies:

```json
{
    "name": "Example Mod",
    "author": "Someone 'Really' Unknown",
    "version": "1.32",
    "gameVersion": "1.0",

    "dependencies": [
        "MafiaBase", "SomeOtherMod"
    ]
}
```

## Data
Mirrors the folder structure of Mafia's game dir with extracted game contents, therefore it consists of folders such as `Models`, `Maps` or `Missions`. Once the game loads, if the mod has a higher load priority, it can load its own asset replacements instead of loading original Mafia's assets.

## Bundles
Asset bundles can be used to access Unity assets packaged together. Instead of re-creating your assets programatically, you can design your assets using Unity Editor directly, once you're done, make sure your asset is assigned to your asset bundle.

To build, view or inspect asset bundles, navigate to `Window -> AssetBundle Browser`. Make sure you export asset bundles used by your mods with the respect to the used platform. Please, follow Unity's official documentation for more information about bundles, [more info here.](https://docs.unity3d.com/Manual/AssetBundlesIntro.html)

To load an asset bundle from a mod, you can use `Mod::LoadFromFile` method, which follows the same interface as [Unity's AssetBundle::LoadFromFile](https://docs.unity3d.com/ScriptReference/AssetBundle.LoadFromFile.html) which we recommend to check out for more information.

## Scripts
These are C# scripts compiled at runtime that extend the functionality of MafiaUnity and bring gameplay features. Say you wanted to implement actual drivable boats in game, well such a mod should consist of a boat audiovisual assets as well as a C# script that implements Boat handling. The point is, you can easily extend MafiaUnity's gameplay features by simply building up blocks of mods.

The script's entry point is a class called ScriptMain which implements IModScript interface. From this script, you can spawn your own objects, setup the game and also attach your own MonoBehaviour scripts that will provide the gameplay changes.

This is the minimal C# entry point:
```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MafiaUnity;

class ScriptMain : IModScript
{
    void IModScript.Start(Mod mod)
    {
        Debug.Log("ExampleMod was initialized!");
    }
}
```


## Tables
This folder contains resource text files that can be used by scripts to load additional parameters. Say we made a mod that adds new car dealing service to the game, where you could purchase a car. What we need for that is a C# script that implements such functionality, but also a table file such as CarDealerVehicleList.txt which contains list of vehicles that you could purchase. Even better, you can make use of JSONUtility to store extra info such as price and engine parameters of the vehicle.

## Modding Tools
The whole format of our new modding system might change over time, however we do plan to simplify the mod creation process by developing set of tools that would accelerate it for you.

## Load order
Load order is important when it comes to mods. That's why MafiaUnity notes the order you load your mods in and also makes sure activated mods meet all criterias for being activated, such as if mod has all dependencies present or whether the game version matches.
