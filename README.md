# Code Samples
Code samples from various projects written by Philip Fertsman.

C#

- <b>SoLA - Windows, Mac, Linux</b>

    - Raycast collider created for a 2D platforming character focused on tight controls and generous movement utilizes a collider squeezing system to bump the player up onto ledges and stops it from stuck on ceilings. Video showcase https://twitter.com/AavaGames/status/1249039578931003397
    
    - Save Data system creates a file that is updated whenever hitting invisible checkpoints throughout the game and read on load, allows for multiple profiles.

    - Cutscene / Dialogue system utilizing Yarn Spinner as a base, created commands to build out cutscenes in a 2D game with multiple paths depending on player actions during the scene.
    
    
- <b>Untitled Rhythm Game - Mobile, Windows, Mac</b>

    - Player controller can take input for mobile touch, controller or keyboard. 
    
    - Level Editor created to ease level design, allowing you to go to any part of the song and test it, without playing through the whole song first.
    
    - Bluetooth calibration for people playing with bluetooth audio devices.


- <b>Movement FPS (WIP) - Windows, Mac, Linux</b>

    - Created a fluid movement controller with sliding, wall running / jumping, an aerial ground pound and a projectile / hitscan weapon system. Includes QoL features such as jump input buffering.
    

- <b>Mass Actor AI - Windows, Mac</b>

    - Created an manager/actor based AI controller that can idle, move independantly, move as a group, or charge an actor.
    
    
C++

- <b>Dominos 1977 - Mac</b>

    - Input manager to pass inputs to the Game Manager which keeps track of the game state (i.e main menu, one player, two player)
    
    - Domino class and its subclasses
    
        - DominoUnit is the layout of the playable actor unit

        - Domino is what is left behind when a DominoUnit moves

        - DominoPlayer uses the InputManager to move

        - DominoEnemy uses a custom AI algorithm