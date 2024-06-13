# Unity + Photon 2 Tic Tac Toe

This repository contains two Unity projects (developed with Unity 2020.3.25f1):

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

With online multiplayer, the main challenge is to keep all the game clients in sync: all players should be seeing the same game state.
Photon has several ways to communicate the various game clients:

* By using [PhotonView](https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_view.html), we can automatically synchronise transforms, animations, or any component that implements [IPunObservable](https://doc-api.photonengine.com/en/pun/v2/interface_photon_1_1_pun_1_1_i_pun_observable.html). A `PhotonView` is "owned" by the player that instantiated it into the scene: that player is the single source of truth about its state in the entire room.
* Besides the automated synchronisation of PhotonViews, we can call a method on a remote object (a Remote Procedure Call), or raise an event across all clients, as documented [here](https://doc.photonengine.com/en-us/pun/current/gameplay/rpcsandraiseevent).

Before we can start with the coding, we need to think about where the various pieces of the game state will "live" for Tic-Tac-Toe.
Given this is a turn-based game, we can come up with the following:

* The master client will own the global state of the "Game Manager" object, and is responsible for updating it based on everyone's moves.
* Other clients cannot directly change the state of the "Game Manager": instead, they will raise an event that the master client can process.
* Each client will have a bit of private state: specifically, which of the two players they are (either `X` or `O`).

Let's start making those changes!

### Adding automated synchronisation to the Game Manager

The first part is to add a `PhotonView` to any entity with a component whose state needs to be synchronised across players.

*Step 1.* Open the Game scene, and add a PhotonView component to the "Game Manager" entity.

*Step 2.* Change the "Synchronization" in the PhotonView component to be "Reliable Delta Compressed".
This is a turn-based game with very "discrete" changes, rather than an action game with many small updates.
We do not want to miss any updates (as we may miss a change of turn!), and the reliability will not be a large overhead anyway as we will not have that many updates.

*Step 3.* Open the `GameManager.cs` script.
Change its base class to `MonoBehaviourPun` (you will need `using Photon.Pun;`): this allows us easy access to the `photonView`.
In addition, make it implement `IPunObservable` so we can synchronise the relevant parts of its state across the network:

```csharp
public class GameManager : MonoBehaviourPun, IPunObservable {
	// ...
}
```

*Step 4.* You will need to implement the `OnPhotonSerializeView` method.
I suggest that you add it within the `#region`  dedicated to "Photon event handling and synchronisaton", just to keep things tidy.
The method will be as follows:

```csharp
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.Winner);
            stream.SendNext(this.Turn);

            foreach (GridCell cell in cells)
            {
                stream.SendNext(cell.Mark);
            }
        }
        else
        {
            this.Winner = (MarkType)stream.ReceiveNext();
            this.Turn = (MarkType)stream.ReceiveNext();

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Mark = (MarkType)stream.ReceiveNext();
            }
        }
    }
```

This method works as follows:

* For the master client (our source of truth), `stream.IsWriting` will be true, and it will send the current winner, turn, and the state of each cell across the network multiple times per second. (Note: Photon optimises this by only sending the information when it changes.)
* For other clients, `stream.IsWriting` will be false, and it will update the state of the `GameManager` by reading the current winner, turn, and the state of the cells from the master client.
* You will notice that the `ReceiveNext` calls must map *exactly* to the `SendNext` calls, and that you must [cast the returned value](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/casting-and-type-conversions#explicit-conversions) to the type you are expecting (see the `(MarkType)` casts?).

*Step 5.* Save the script and return to the editor.
You should be able to build and run your application, and test your multiplayer capabilities.
Try starting a 2-player game, and play for a bit from both sides: do you notice anything?

Yes, only the moves from the master client seem to matter - anything that the other player tries to do is immediately overwritten by the master client! Clearly, there is still work to do.

### Letting the other player communicate moves

The main issue is that the other player is not the source of truth of the GameManager PhotonView: they should not be attempting to change it directly. Instead, they should tell the master client about the move they are trying to make, and let the master client update the game state accordingly. We will use events for this.

*Step 1.* Open the `GameManager.cs` script and go to the `OnCellClicked` method.
Replace this code:

```csharp
// Really change the cell
CellPlayed(cell);
```

With this code (you will need `using Photon.Realtime;` and `using ExitGames.Client.Photon;` at the top of the file):

```csharp
if (photonView.IsMine)
{
  // Really change the cell
  CellPlayed(cell);
}
else
{
    // Send the move, but don't change the cell:
    // we do not own the game state.
    PhotonNetwork.RaiseEvent(EVENT_MOVE,
      new int[] { cell.Row, cell.Column },
      RaiseEventOptions.Default,
      SendOptions.SendReliable);
}
```

The meaning of this code is as follows:

* If we are the master client, `photonView.IsMine` will be true (we are the source of truth for it), and we can directly manipulate the cell and let all other players synchronise themselves.
* If we are NOT the master client, we will instead raise an event saying that we want to make a move by selecting the cell at that row and column. `EVENT_MOVE` is just an integer constant (1) that we have defined at the top of `GameManager` (we can use any number from 0 to 199), and we can send any object that Photon can serialise (arrays, primitives, and enums all work well). We are using the default options, and we want the event to be sent in a reliable way as we do not want any moves to be missed.

*Step 2.* The next part is to allow the master client to react to those events.
Change the base class of the `GameManager` to `MonoBehaviourPunCallbacks`, and add the `IOnEventCallback` interface:

```csharp
public class GameManager : MonoBehaviourPun, IPunObservable, IOnEventCallback {
	// ...
}
```

*Step 3.* Implement the `OnEvent` method of the `IOnEventCallback` interface:

```csharp
    public void OnEvent(EventData photonEvent)
    {
        if (photonView.IsMine)
        {
            switch (photonEvent.Code)
            {
                case EVENT_MOVE:
                    int[] data = (int[])photonEvent.CustomData;
                    int row = data[0];
                    int col = data[1];
                    CellPlayed(cells[row * Size + col]);
                    break;
            }
        }
    }
```

This function says that if we are the source of truth for this "GameManager", and the event code is `EVENT_CODE`, then we will take the data from the move from the other player and execute that move on their behalf.

*Step 4.* Try rebuilding the game and playing again.
Try playing several moves from both sides: do you see the problem?

Yes - both players can make moves now, but they're not taking turns!

### Enforcing turns

We should only let a player play during their turn - it wouldn't be fair otherwise!
This means that we need to allow each player to remember who they are in the game: are they the X player, or the O player?

*Step 1.* Open the `GameManager.cs` script.
Within the `#region` "Private game state" (to keep things tidy: this is entirely optional!), add this field:

```csharp
// Turn of the connected player (if playing online)
private MarkType MyTurn;
```

This field will allow each player to know who they are in the game.

*Step 2.* Change the body of the `Start` method to this:

```csharp
if (photonView.IsMine)
{
    Winner = MarkType.EMPTY;
    MyTurn = MarkType.O;
    Turn = Random.Range(0, 2) == 0 ? MarkType.O : MarkType.X;
}
else
{
    MyTurn = MarkType.X;
}
```

The master client will always be the O player, and the other player will always be the X player.
To keep things more interesting, we've decided to make the starting player random: 50% of the time it will be O, and 50% it will be X.

*Step 3.* Go to the `OnCellClicked` method.
Right after the `if(Winner != MarkType.EMPTY)` conditional block, add this `else if` block:

```csharp
if (Winner != MarkType.EMPTY)
{
  // Game has finished, do nothing
  return;
}
else if (PhotonNetwork.IsConnected && Turn != MyTurn)
{
  // We are in an online game, and it's not your turn!
  return;
}
```

This is simple: if we're playing an online game, and it's not your turn, ignore the click!

*Step 4.* Try playing the game once more.
You should now find that turns are correctly enforced.
However, when you finish a game, the second player can press SPACE and nothing will happen on the other side.
Should they even be allowed to do that?

### Keeping the master client in control of resets

We have decided that only the master client should be able to press space to reset the game at the end of a round.
We need to tweak the code accordingly.

*Step 1.* Go to the `GameManager.cs` script, and go to the "Update" method.
Replace this code:

```csharp
if (Winner != MarkType.EMPTY && Input.GetKeyDown(KeyCode.Space))
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
```

With this code:

```csharp
if (photonView.IsMine && Winner != MarkType.EMPTY && Input.GetKeyDown(KeyCode.Space))
{
    PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex);
}
```

This wil only let the master client reset the game by pressing space when there has been a winner declared.
Key presses from other clients will be ignored.

*Step 2.* It may confuse a player to be told that they can press space, only to be ignored!
Let's tweak the `Winner` property to provide accurate text depending on the situation.
Change this:

```csharp
turnText.text = $"Winner: {winnerName}! - SPACE to reset, ESC to quit";
```

To this:

```csharp
turnText.text = photonView.IsMine
    ? $"Winner: {winnerName}! - SPACE to reset, ESC to quit"
    :  $"Winner: {winnerName}! - ESC to quit";
```

Likewise, change this:

```csharp
turnText.text = $"Tied! - SPACE to reset, ESC to quit";
```

To this:

```csharp
turnText.text = photonView.IsMine
  ? "Tied! - SPACE to reset, ESC to quit"
  : "Tied! - ESC to quit";
```

*Step 3.* Save the script and try playing the game: only the master client should be able to reset the game now.
As an optional exercise, you may want to try using a Photon event to propagate the reset from the second client, and still allow both clients to initiate resets.

What happens if one player decides to quit the game by pressing Escape, though?

### Reacting to the other player leaving the game

Right now, the game does not react when the other player disconnects or leaves the room.
We need to add more code for it!

*Step 1.* Open the `GameManager.cs` script, and add this method to the region discussing Photon event handling:

```csharp
public override void OnPlayerLeftRoom(Player other)
{
    // The other player left - might as well leave, too!
    PhotonNetwork.LeaveRoom();
}
```

This method makes it so that if the other player leaves the room, we will leave the room as well.

*Step 2.* Add this other method to complete the work:

```csharp
public override void OnLeftRoom()
{
    SceneManager.LoadScene("Lobby");
}
```

With this method, when we leave the room, we will return to the lobby.
Notice how we use `SceneManager.LoadScene` for this, and not `PhotonNetwork.LoadLevel`: we are not in a room anymore, so there is nothing to synchronise.

*Step 3.* As usual, try playing the game again - see how it reacts to someone leaving?

### Using Photon nicknames

There is only one more detail to cover: right now, it's never quite clear if it's our turn's or the opponent's.
It would be nicer to have the UI tell us, and remind us of the other player's nickname, so we can recognise the player the next time we meet them.

*Step 1.* Open the `GameManager.cs` script.
We need to add a method that will tell us who is our opponent, in case it's not our turn.
Add this method to the class:

```csharp
private Player GetOpponent()
{
    foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values) {
        if (!player.IsLocal)
        {
            return player;
        }
    }
    return null;
}
```

This method will give us the first player who is not local (i.e. us): in other words, our opponent.
We can get the nickname of the player from that object.

*Step 2.* Let's update the `Turn` property first.
Within its `set` method, change this code:

```csharp
turnText.text = $"Turn: {value}";
```

To this code, which changes how things work for online games:

```csharp
if (PhotonNetwork.IsConnected)
{
    turnText.text = MyTurn == Turn
        ? $"Your turn, {PhotonNetwork.NickName}"
        : $"Waiting for {GetOpponent().NickName}";
}
else
{
    turnText.text = $"Turn: {value}";
}
```

If we are in an online game, it will clearly say if it's our turn or the opponent's, and use the appropriate nickname.

*Step 3.* Now, let's update the `Winner` property as well.
In the `set` method, change this code:

```csharp
string winnerName = value.ToString();
```

To this:

```csharp
string winnerName;
if (PhotonNetwork.IsConnected)
{
    winnerName = MyTurn == value
        ? PhotonNetwork.NickName
        : GetOpponent().NickName;
}
else
{
    winnerName = value.ToString();
}
```

*Step 4.* One last time, try building and playing your game.
It should clearly signal whose turn it is now, based on the nickname you entered in the lobby.

And with that, congratulations - you've finished your first online turn-based multiplayer game with Unity and PUN2!

## Further reading

* [Photon PUN2 documentation](https://doc.photonengine.com/en-US/pun/current/getting-started/pun-intro)
* [Unity Learn](https://learn.unity.com/)
