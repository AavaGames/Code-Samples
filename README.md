# Code Samples
Code samples from various projects written by Aava Fertsman.

C# Unity

- <b>Fishnet PvP Character State Machine</b>

    - Client-side predicted player state machine using Fishnet

- <b>SoLA - Windows, Mac, Linux</b>

    - Raycast collider created for a 2D platforming character focused on tight controls and generous movement utilizes a collider squeezing system to bump the player up onto ledges and stops it from stuck on ceilings. Video showcase https://twitter.com/AavaGames/status/1249039578931003397

    - Cutscene / Dialogue system utilizing Yarn Spinner as a base, created commands to build out cutscenes in a 2D game with multiple paths depending on player actions during the scene.

- <b>Movement FPS (WIP) - Windows, Mac, Linux</b>

    - Created a fluid movement controller with sliding, wall running / jumping, an aerial ground pound and a projectile / hitscan weapon system. Includes QoL features such as jump input buffering.

C 

- <b>Tyalband</b>

    - A distance/flood map for a 2D grid used in a traditional ASCII roguelike for the Playdate.

    
C++ w/ SDL

- <b>Dominos 1977</b>

    - Input manager to pass inputs to the Game Manager which keeps track of the game state (i.e main menu, one player, two player)
    
    - Domino class and its subclasses
    
        - DominoUnit is the layout of the playable actor unit

        - Domino is what is left behind when a DominoUnit moves

        - DominoPlayer uses the InputManager to move

        - DominoEnemy uses a custom AI algorithm
