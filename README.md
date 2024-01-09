Multiverse Hub meant to be a web browser based, social building game like Habbo Hotel.

This Unity project combines a grid-based interior design system and an inventory management system to create a versatile environment for designing and interacting with virtual interiors. The system is composed of several scripts that facilitate grid management, interior object placement, and inventory control.

### Grid-Based Interior Design System:
The GridManager script establishes a grid layout within the Unity scene, allowing users to design interiors based on a grid structure. It supports customizable grid dimensions, visibility toggling, and mouse-based interactions. Interior objects are categorized into floors, walls, and furniture, each with distinct functionalities.

### MapEditor:
The MapEditor script serves as the control center for the interior design system. It enables mode switching between player and editor modes using the Tab key. Users can pick up, rotate, and place interior objects within the grid. The editor mode includes features such as object spawning from templates, tooltip displays, and the ability to change wall colors and visibility.

### Inventory Management System:
The Inventory script provides a comprehensive inventory management system for handling in-game items. It follows the singleton pattern to ensure a single inventory instance. Users can add items, remove them (with use tracking), and clear the entire inventory. The script supports a delegate for item change events, allowing for easy integration with other game systems.

### PlayerController:
The playerController script controls the movement of the player character within the grid-based environment. It utilizes a pathfinding algorithm to calculate the shortest path from the starting point to the desired location. The script includes features such as grid-based movement, mouse input for endpoint selection, and obstacle avoidance.

### Item Script:
The Item script, derived from Unity's ScriptableObject, represents individual items within the game. It includes essential properties such as name, icon, 3D mesh, and usage tracking. The script provides virtual methods for item use, consumption, and selection, allowing for customization based on specific item behaviors.


# Video
https://github.com/PauliusRaila/Multiverse-Hub/assets/28274535/89daab77-a685-4084-ad4d-e462951c6bdb
# Screenshots

![Screenshot 2023-12-23 141838](https://github.com/PauliusRaila/Multiverse-Hub/assets/28274535/1bd32abc-f07c-47ae-99cf-49501c06b8cc)
![Screenshot 2023-12-23 142343](https://github.com/PauliusRaila/Multiverse-Hub/assets/28274535/5244e2dd-e6f9-4113-b4fc-bf567e37444a)
![Screenshot 2023-12-23 142551](https://github.com/PauliusRaila/Multiverse-Hub/assets/28274535/3567d16e-651e-4b3b-819a-6953355aceb6)
![Screenshot 2023-12-23 142654](https://github.com/PauliusRaila/Multiverse-Hub/assets/28274535/30e863f4-fd0d-40d3-b76e-16cff5841081)
![Screenshot 2023-12-23 142805](https://github.com/PauliusRaila/Multiverse-Hub/assets/28274535/a3106003-0915-444d-9202-fb6222a3bee9)

</details>
