
# UOM FS AI Simulation In Unity

An attempt at creating training environment using the dynamic bicycle model for a racing agent through Unity MLAgents Toolkit

## Table of Contents

- [Getting Started / Prerequisites](#getting-started/prerequisites) 
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [Training](#training)
- [Acknowledgements](#acknowledgements)

## Getting-Started/Prerequisites

Have The latest STABLE version of Unity Installed; Ideally Version 2022.3.4f1. Along with the latest version of Python 3.8; Using which Create a VENV (Virtual Environment) within the project folder. Install 
- MLAgents
- numpy
- Tensorboard
Using pip

Then In Unity Install The Following Packages:
- MLAgents
- YamlDotNet



## Usage

There are two main scripts that control everything...
### Controller Script Documentation
#### Overview
The `Controller` script is a core component of the project, aiming to create a realistic training environment for a racing agent using the dynamic bicycle model within the Unity MLAgents Toolkit.

This script handles various aspects of the agent's control, physics simulation, collision detection, AI decision-making, and rewards calculation. By interfacing with the Unity MLAgents Toolkit, it orchestrates the training process of the racing agent, capturing essential interactions and behaviors.
#### Key Components
#### Model and State Management
- The `DynamicBicycle` model is instantiated to simulate the dynamic bicycle model.
- `State` and `Input` objects manage the current state and input of the dynamic bicycle.

#### Physics and Movement
- The script interacts with the Unity physics engine and The Custom Dynamic Bicycle Model (Thanks EUFS) to simulate the vehicle's movement.
- Distance, average speed, and rotational difference are tracked to compute rewards and metrics.

#### AI Control
- The script processes AI outputs, mapping them to appropriate inputs for the dynamic bicycle model.
- Throttle and steering angle calculations are performed based on AI decisions.
- Confident outputs are rewarded to encourage accurate AI behavior.

#### Heuristics User Control
- The `Heuristic` function allows manual control of the agent using keyboard inputs for steering and throttle.

#### Validation and Calculations
- Validation and mapping functions ensure accurate input values for the dynamic bicycle model.
- Functions for calculating throttle and steering angle based on AI inputs are implemented.

#### Auxillary Functions
- Utility functions are provided for calculating magnitude, converting radians to degrees, and degrees to radians.
- A collision callback function detects when the vehicle collides with specific objects.

#### Reset and Termination Logic
- Logic for resetting the agent's state is implemented using `OnEpisodeBegin`.
- Termination conditions are managed based on speed, distance, and rotational difference.

#### Usage Of Controller Script
1. Attach this script to the GameObject representing the racing agent in your Unity scene.
2. Ensure the script references the necessary components such as the `DynamicBicycle` model and any external scripts like `LoadTrack` for track generation. (The CreateTrack GameObject Should Be Attached To The Controller Script In The Unity Editor...)
3. Customize AI control and behavior within the `OnActionReceived` function, specifying AI outputs and computing rewards.
4. Optional: Adjust collision handling and termination conditions based on your preferences.



### LoadTrack Script Documentation
#### Overview
The `LoadTrack` script is a crucial component responsible for generating and importing track boundaries into the Unity environment. It serves as a bridge between your Python-based track generation process and Unity, facilitating the creation of a dynamic training environment for the racing agent.

This script orchestrates the process of importing track models, parsing CSV files, generating track boundary objects, and managing their properties.

#### Key Components
#### Track Model Import
- The `ImportModelFromFBX` function loads an FBX model from the project's assets and instantiates it in the scene.
- The function returns a GameObject representing the instantiated model.

#### Object Removal
- The `RemoveObjectsWithTag` function finds and removes all GameObjects with a specified tag from the scene.

#### Track Generation
- The `GenerateTrack` function coordinates the entire track generation process.
- It calls a Python script to generate a CSV file that contains track boundary information.
- The CSV file is then read, and track boundaries are created based on the color and location information.

#### Unity-Python Interaction
- The script utilizes `ProcessStartInfo` to execute a Python script externally.
- The generated CSV output from the Python script is read and parsed to determine the track boundary points.
#### Usage
1. Attach this script to an empty GameObject in your Unity scene.
2. Assign the necessary asset paths for the track models (`path_yellow_n_black` and `path_blue_n_white`).
3. Configure the paths for Python (`pythonExecutable` and `pythonScriptPath`) and working directory (`workingDirectory`).
4. Call the `GenerateTrack` function, which coordinates the process of generating and importing the track boundaries.

- Both scripts should be attached and appropriately configured on launch...
- Consult MLAgents Documentation For Setting Up Unity MLAgents...


## Training
Config Files Of Different Training Versions Are In The Config Folder And The Results In The Results Folder
To See The Results Run `tensorboard --logdir results`

## Acknowledgements

### EUFS
For the Dynamic Bicycle Model code...
