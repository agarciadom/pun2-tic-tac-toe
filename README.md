# Unity + Photon 2 Tic Tac Toe

This is a toy online multiplayer game, using:

* [Unity](https://unity.com)
* [Photon Engine](https://www.photonengine.com/)

The game has two scenes: a starting "Lobby" scene which allows players to join a random room, and a "Game" scene with the actual Tic-Tac-Toe.

The default configuration is 3x3, but the `Size` constant can be changed for 4-in-a-row or more if desired.

Only one `PhotonView` is needed: this is contained in the `GameManager` that maintains the authoritative state of the game.
Moves from the non-master client are relayed to the master client via Photon events.
