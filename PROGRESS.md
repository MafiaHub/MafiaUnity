# Project Status

This is a document describing the current progress of the project as well as how far it is from reaching the goals.

## Primary focus

Our primary focus at this stage is to have the ability to load Mafia assets and make use of them. Once the majority of the formats has been taken care of, we will focus on gameplay features and miscellaneous tasks.

## Terminology

* [ ] - Not worked on yet
* [ ] WIP - Work in progress
* [x] - Completed

## Current objectives

### General

* [x] Implement VFS, responsible for loading data in a configurable order, supporting custom mod paths
* [x] Implement basic Mafia Editor, serving as a tool to import mission/city/model into the scene to work with in Unity's editor
* [ ] WIP Mod support (to be revealed soon)
* [x] Implement Object Injector. Its purpose is to resolve access to specified scene objects and take actions upon them, such as removal, etc.
* [ ] WIP Design Object Injector's editor tool.

### Native formats

* [x] Load mission's static visuals into the scene
* [x] Load 4DS static model
* [x] Support texture transparency
* [x] Support animated alpha/diffuse texture maps
* [ ] WIP Support skeletal meshes
* [ ] Skinned mesh animation support
* [ ] Parse and generate Mafia's menu
* [ ] MafiaScript (native scripting language) interpreter implementation
* [ ] Ability to load data from DTA archive files
* [ ] Scene lighting support
* [ ] Use of scene's native lightmaps (to be decided)
* [ ] WIP Parse tree.klz, which defines static collision inside of scene.
* [ ] Generate colliders based on data parsed from tree.klz

### Gameplay features

Nothing has been decided to work on so far, to be added later.
