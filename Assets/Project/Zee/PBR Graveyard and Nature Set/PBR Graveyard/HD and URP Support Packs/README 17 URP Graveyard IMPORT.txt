
BEFORE YOU START:

- you need Unity 6+
- you need URP SRP pipline 17 or higher
- wind setup is in wind prefab at each scene

Step 1 - You can improve FPS amount by 30% if you change rendering path from forward to deferred at rendering setting. 
Find File "UniversalRenderer" and change Rendering path from forward to deferred. Forward render is ok too but it's slower for big open scenes.


Step 2 - Setup Shadows and other render setups. Find File "PC_RPAsset" 
        - Change shadow distance to 150 or higer
	- Optionaly use 1k or 2k shadow resolution. We used 2k.
	- Turn on HDR if its turned off

Step 3 Go to project settings: 
    - Player and set:  Color Space to Linear
    - Quality settings: Go to quality settings and: 
	     * use ultra level 
	     * turn turn off vsync
	     * lod bias should be around 1.5-2 and 1 for low end devices.
                        
Step 4 Find "Graveyard_Demo" and open it.

Step 5 - HIT PLAY!:)

About scene construction:
		- There is post process profile: Manage post process by scene post process object.
		- Prefab wind manage wind speed and direction at the scene

