# Unity + Photon 2 Tic Tac Toe

This repository contains two Unity projects:

* [`starting-version`](./starting-version), a toy Unity project with a single scene implementing a "hotseat" version of Tic-Tac-Toe.
* [`final-version`](./final-version), an extended version of the above project that uses [PUN](https://www.photonengine.com/) to add online multiplayer capabilities.

The repository is intended as a high-level starter guide to adding online multiplayer capabilities to a turn-based game: instructions are below.

## Introduction

This guide assumes basic familiarity with Unity concepts, such as:

* Basic installation and setup of the Unity Hub and Unity Editor.
* The [Entity-Component-System](https://en.wikipedia.org/wiki/Entity_component_system) architectural pattern.
* The organisation of entities into scene trees, and the use of prefabs to avoid repetition.

If you are not familiar with the above, it's probably best to first go through the [Junior Programmer pathway](https://learn.unity.com/pathway/junior-programmer) in Unity Learn.

The guide also assumes some basic familiarity with the C# programming language: if you are familiar with Java, it will be very familiar, but there are some differences.
Most importantly, this project makes use of C# [properties](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties) and [delegates](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/).

You can either directly open the `final-version` project and skip the rest of this document, or you can open the `starting-version` project and try making the changes yourself step by step. It's up to you!

## Overview of the starting project

After adding the `starting-project` to your Unity Hub and opening it, open the scene in `Scenes/Game`.

Your scene will look like this:

![img/starting-game.png](img/starting-game.png)

The key parts of this scene are as follows:

* The `Main Camera` is set up for 2D graphics, using an orthogonal projection. You will not need to change it in this guide.
* The `Grid Container` is responsible for laying out the cells of the Tic-Tac-Toe board onto a grid:
  * A Grid Layout Group component arranges the children components into rows.
  * A script component (`Scripts/SquareGridPopulator.cs`) automatically instantiates and sets up the necessary `Cell` prefabs. `SquareGridPopulator` uses a [UnityEvent](https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html) to allow other components to react when a `Cell` is created.
  * The container has a number of `Cell` prefabs already added, but these are just to help preview the scene: they will be destroyed and replaced with the necessary number of instances based on the `GameManager.Size` constant.
* The `Cell` prefab represents each of the cells on the board:
  * Two square sprites to draw the cell border and fill.
  * A BoxCollider2D component detects clicks on the cell (without having to use raycasts).
  * A LayoutElement component allows the Grid Layout Group to control its size and position.
  * Two initially inactive children (`XMarker` and `OMarker`) represent two possible end states of the cell.
  * A script component (`Scripts/GridCell.cs`) provides a `UnityEvent` field to allow other components to react to clicks, and defines a `Mark` property that when set, will also display the appropriate mark on this cell.
* The `Game Manager` entity uses the `Scripts/GameManager.cs` script to store the state of the game and implement the game logic, including the winning condition. The state of the game is defined by the `Turn` property, the `Winner` property, and the list of `cells` of the board.
* Finally, a [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) component is used to display whose turn it is, and whether there has been a winner.

The game is fully functional already: try starting play mode and going through a few games of it.