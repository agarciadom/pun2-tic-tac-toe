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

## Adding PUN2 to the project

We're going to add the multiplayer aspect to it now, but instead of implementing it from scratch, we will use version 2 of the [Photon Engine](https://www.photonengine.com/) to do it.

The first part is to go to the [Unity Asset Store page for PUN2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922):

![img/pun2-asset-store.png](img/pun2-asset-store.png)

Click on "Add to My Assets": keep in mind that you will need to be signed in to your Unity account for this.

Once the button has changed to "Open in Unity", you can now return to the Unity Editor.

Select "Window - Package Manager", and in the top dropdown change from "Packages: In Project" to "My Assets".
It should now look like this, except you may have a "Download" button instead (as you will not have previously downloaded this package):

![img/pun2-package-manager.png](img/pun2-package-manager.png)

Click on "Download", and when done click on "Import".
You will now see a window like the following:

![img/pun2-import.png](img/pun2-import.png)

You can simply click on "Import" to extract the entire package into your project, but we will not need all of it.
In particular, you can simply uncheck the `PhotonChat` folder, as well as the `PhotonUnityNetworking/Demos` folder.
If at some point in the future you needed them, you can simply re-import PUN2 into your project.

It will take some time to compile the C# scripts in PUN2.
Once it's done, the "PUN Wizard" window will pop up:

![img/pun2-wizard.png](img/pun2-wizard.png)

This wizard is aimed at helping you do the basic configuration of PUN for your project.
Simply enter your email address into the field, and click on "Setup Project".

If this is your first time using PUN, the wizard will create a new account for you, set up a new app and add its AppId to the field.
You can then simply click on "Close": your basic PUN setup is done.
You will notice that PUN has selected the `PhotonServerSettings` entity for you in the Inspector: you won't need to make any further changes to it in this guide.

*Note*: if you already had a PUN account, it will ask you to log into the web-based dashboard and manually set up a new AppId: this process is outside the scope of this guide!
