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

## Creating a Lobby scene

In order to do online multiplayer, we will first need to have a scene where the user will enter their name, connect to the Photon servers, and be joined up with another player.

### Basic scene population

Create a new `Scenes/Lobby` scene.
We will now populate it as in this screenshot:

![img/lobby-components.png](img/lobby-components.png)

Namely, we will need to:

1. Right-click on the "Lobby" in the Hierarchy, and select "GameObject - UI - InputField - TextMeshPro" to add our input field. Name it "Name Field". This will automatically add a "Canvas" object for us.
1. Right-click on the "Canvas", and add a "GameObject - UI - Button - TextMeshPro". Name it "Connect Button".
1. Right-click on the "Canvas", and add a "GameObject - UI - Text - TextMeshPro". Name it "Status Text".
1. Tweak the positions and styling to your liking, as well as the placeholder text in the InputField and the text for the button.
1. Create an empty GameObject inside the Canvas called "Connect Panel", and move the "Name Field" and the "Connect Button" into it.
1. Make the "Status Text" inactive by default, so it only shows after clicking on the "Connect" button. (This can be done by selecting the entity in the Hierarchy, then unticking the checkbox on the top left of the Inspector.)

### Preparing the ConnectManager

We have the basic UI components ready: we need to actually use PUN now.

Create a new `Scripts/ConnectManager` C# script, and open it in Visual Studio.
We will need to make a number of changes in order to link it to the rest of the scene.

*Step 1.* Change the base class of this script to `MonoBehaviourPunCallbacks`.
This is the base class for any script that needs to react to PUN events (e.g. connecting to the server, or having players join/leave).
You will need to add a `using` statement at the top of the file:

```csharp
using Photon.Pun;

public class ConnectManager : MonoBehaviourPunCallbacks
{
	// ...
}
```

*Step 2.* Delete the default `Start` and `Update` methods: we will not use them.

*Step 3.* Add serialized fields to allow the script to refer to the "Connect Panel" and the "Status Text" (note that you will need `using TMPro;` for the `TextMeshProUGUI` field):

```csharp
[SerializeField]
private GameObject ConnectPanel;

[SerializeField]
private TextMeshProUGUI StatusText;
```

*Step 4.* Add fields to store whether we are currently making a connection attempt, and the version number for our game (in case we make breaking changes in the future):

```csharp
private bool isConnecting = false;
private const string gameVersion = "v1";
```

*Step 5.* Now save the script and return to Unity. Create an empty GameObject called "ConnectManager", and add our new script to it. Link the "Connect Panel" and the "Status Text" slots to the appropriate entities in the Hierarchy, like so:

![img/cmanager-link.png](img/cmanager-link.png)

### Setting the user nickname

Users will need to know each other's name: Photon has a static `PhotonNetwork.NickName` attribute to store that information, which we should set before we connect. The easiest way is to create a small script and attach it to the "Name Field", so it will update its value whenever the user types something into the field.

*Step 1.* Create a new `Scripts/NameInputField` script, and open it in Visual Studio.

*Step 2.* Delete the default `Start` and `Update` methods, and add the following method (you will need `using Photon.Pun;` at the top of the file in order to access `PhotonNetwork`):

```csharp
public void SetPlayerName(string name)
{
    if (string.IsNullOrEmpty(name))
    {
        Debug.LogError("Player name is empty");
        return;
    }
    PhotonNetwork.NickName = name;
}
```

*Step 3.* Add the script to the "Name Field" entity.

*Step 4.* Add an event handler to the "Name Field" that calls the `SetPlayerName` method when its value changes. It will look like this:

![img/nameinput-events.png](img/nameinput-events.png)

If you do not know how to do this, check [Unit 5 - User Interface](https://learn.unity.com/project/unit-5-user-interface) in the Unity Learn Junior Programmer pathway.

### Completing the Connect Manager

We've got most of the wiring done here, we just need to add the code to react to the various events coming from the user and from PUN.

*Step 1.* Open the `ConnectManager.cs` script again in Visual Studio. Add the following method:

```csharp
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
```

This turns on the automated scene synchronisation in Photon, which allows the "master client" (the player that "hosts" the game session) to be in control of the currently loaded scene for all players.

*Step 2.* Next, add the method to be called when the "Connect Button" is clicked, and a method to activate the "Status Text" and update its text (you will need `using Photon.Pun;` at the top of the file to use `PhotonNetwork`):

```csharp
    public void Connect()
    {
        isConnecting = true;
        ConnectPanel.SetActive(false);
        ShowStatus("Connecting...");

        if (PhotonNetwork.IsConnected)
        {
            ShowStatus("Joining Random Room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            ShowStatus("Connecting...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    private void ShowStatus(string text)
    {
        if (StatusText == null)
        {
            // do nothing
            return;
        }

        StatusText.gameObject.SetActive(true);
        StatusText.text = text;
    }
```

Some notes about the above code:

* As mentioned in the PUN demos, the `isConnecting` flag is needed so other event handlers can tell if we are genuinely trying to connect, or if we are simply being returned to this scene from another one (e.g. when the game ends).
* We hide the "Connect Panel" once "Connect" is clicked, so the user cannot make a second connection attempt while we are still dealing with the first one.
* The `PhotonNetwork.IsConnected` is a static attribute set by Photon that tells us whether we are already connected to the Photon servers (in which case we try to join a random room), or not (in which case we start a connection attempt to Photon's servers, specifying our game version to separate ourselves from other game clients).

*Step 3.* Save and return to the Unity editor. Select the "Connect Button", and in "On Click" make it call the "Connect" method of our "Game Manager".

*Step 4.* Open the `ConnectManager.cs` script again. We need to add a few more event handling methods.
First, we will add an event handler for when we establish our connection to the Photon servers:

```csharp
    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            ShowStatus("Connected, joining room...");
            PhotonNetwork.JoinRandomRoom();
        }
    }
```

Notice how we use `isConnecting` so we only react to this when the method has been triggered by a click of the "Connect" button, and not by simply returning to this scene from another one.

*Step 5.* The attempt to join a random room may fail, if there is no such room available. In that case, we will create a new room of our own:

```csharp
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        ShowStatus("Creating new room...");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }
```

*Step 6.* The attempt to connect may fail, or we may be disconnected from the server for some other reason (you will need `using Photon.Realtime;` at the top of the file for this):

```csharp
    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        ConnectPanel.SetActive(true);
    }
```

*Step 7.* When we join a room, we can let the player know:

```csharp
    public override void OnJoinedRoom()
    {
        ShowStatus("Joined room - waiting for another player.");
    }
```

*Step 8.* Finally, once we have two players in the room, the player that hosts the game (the "master client") can change everyone to the proper "Game" scene:

```csharp
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }
```

Notice how we use `PhotonNetwork.LoadLevel` instead of `SceneManager.LoadScene` as we would normally in Unity.
This function performs additional message queue management during scene switches, and ensures that all players change to the new scene (thanks to our prior setting of `PhotonNetwork.AutomaticallySyncScene`).

Congratulations - the matchmaking is done!

### Adding the Lobby scene to the Build Settings

If we want to be able to properly test multiplayer, we will need to do a standalone build of the game, so we can run one instance of the game through the editor's play mode, and another instance from the standalone executable.

To do so, we will need to update the build settings of the project to include the Lobby scene, and to make it the starting scene for the game. Make sure you have the Lobby scene open before starting these steps.

*Step 1.* In the Unity Editor, select the "File" menu and click on "Build Settings...".

*Step 2.* With the Lobby scene open, click on "Add Open Scenes". `Scenes/Lobby` should be added with build index 1.

*Step 3.* Drag `Scenes/Lobby` to the top: it should now have build index 0. This will be the starting scene for the game.
It should look as follows:

![img/build-settings.png](img/build-settings.png)

*Step 4.* Click on "Build and Run", and select a destination folder for your build that is outside your Unity project.
After some time, the build should complete and your game should be running by itself.

*Step 5.* From the standalone instance of your game, enter a name and click on Connect: after some time, it should be waiting for the other player.

*Step 6.* Use play mode in the Editor to run your other instance: enter a different name, and click on Connect. After a bit more time, both game clients should change to the game scene: you are done with the matchmaking!

You can now exit play mode and close the standalone instance of your game.
It's time to add the actual multiplayer aspects to your game.

## Adding online multiplayer to the Game scene

## Further reading

* [Photon PUN2 documentation](https://doc.photonengine.com/en-US/pun/current/getting-started/pun-intro)
* [Unity Learn](https://learn.unity.com/)