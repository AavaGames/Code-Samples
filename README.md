# Code Samples
Code samples from various projects written by Philip Fertsman.

C#

- <b>SoLA - Windows, Mac, Linux</b>

    - Player controller and raycast collider created for a 2D platforming character focused on tight controls and generous movement.  
    
    - Save Data system creates a file that is updated whenever hitting invisible checkpoints throughout the game and read on load, allows for multiple profiles.
    
    
- <b>Untitled Rhythm Game - Mobile, Windows, Mac</b>

    - Player controller can take input for mobile touch, controller or keyboard. 
    
    - Level Editor created to ease level design, allowing you to go to any part of the song and test it, without playing through the whole song first.
    
    - Bluetooth calibration for people playing with bluetooth audio devices.
    
    
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
      
      
 GML
 
 - <b>N1rV4nA - Windows</b>
 
    - o_Player is the player controller of a 2D racing game, featured with input management, collision detection, physics, and all movement states (i.e onWall, onRamp, inAir)
    
    - o_Rival features the same physics but two different kinds of AIs
