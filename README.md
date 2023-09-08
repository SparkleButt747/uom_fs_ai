## Agent Controller Script

This Unity project contains the `Controller` script, which is responsible for controlling an AI-driven vehicle using Unity ML-Agents. The script utilizes a dynamic bicycle model to simulate the behavior of a vehicle on a track. The project also includes cone models used to define the track boundaries and checkpoints.

### Overview

The `Controller` script is a part of the Unity ML-Agents framework and extends the `Agent` class. It includes several components and functions for controlling the AI vehicle, collecting observations, and interacting with the environment.

#### Key Features

- **Dynamic Bicycle Model**: The script uses a dynamic bicycle model to simulate the vehicle's behavior, including steering and throttle control.

- **Physics and Collision Handling**: It handles physics and collision events, such as collisions with track boundaries, to provide rewards and reset the episode when necessary.

- **Checkpoints**: The script recognizes and rewards the vehicle for passing through checkpoints on the track.

- **AI Control Logic**: It contains AI control and training logic for the vehicle, including heuristics-based user control and AI input validators.

- **Path Generation**: The script generates a track layout with cones representing track boundaries and checkpoints. Cones are instantiated in the Unity scene to create a track for the vehicle to navigate.

### Getting Started

To use this script and Unity project:

1. Clone or download this repository to your local machine.

2. Open the Unity project in Unity Editor.

3. Attach the `Controller` script to a GameObject representing the vehicle in your scene (Add a ground / plane and a car model so when training starts the cones and cars don't just fall to the void).

4. Customize the project and script as needed for your specific use case.

5. Run the Unity project alongside the unity mlagents in python to observe the AI vehicle's behavior and make adjustments as necessary.




## Path Configuration Class 

The `PathConfig` class is a serializable configuration class used in your Unity project. It defines various parameters and settings for generating a path or track within the project. This class allows you to customize the characteristics of the generated path, such as its curvature, amplitude, frequency, and more.

### Overview

The `PathConfig` class is designed to encapsulate the configuration options for path generation. It is used to create instances of path configurations with specific settings. The class offers both a default constructor with predefined values and a custom constructor that allows you to set specific parameters.

#### Key Parameters

- `Seed`: The seed value for randomization during path generation.
- `MinCornerRadius`: The minimum radius of curvature for the path.
- `MaxFrequency`: The maximum frequency of oscillations in the path.
- `Amplitude`: The amplitude of the path's oscillations.
- `CheckSelfIntersection`: A boolean indicating whether to check for self-intersections in the generated path.
- `StartingAmplitude`: The initial amplitude of the path.
- `RelativeAccuracy`: The relative accuracy of the path generation.
- `Margin`: A margin value for the path.
- `StartingStraightLength`: The length of the initial straight segment of the path.
- `StartingStraightDownsample`: The downsample rate for the initial straight segment.
- `MinConeSpacing`: The minimum spacing between cones in the path.
- `MaxConeSpacing`: The maximum spacing between cones in the path.
- `TrackWidth`: The width of the generated track.
- `ConeSpacingBias`: A bias factor for cone spacing.
- `StartingConeSpacing`: The initial spacing between cones.



## Unity ML-Agents Training Configuration 

This configuration file specifies training settings for Unity ML-Agents. It defines various hyperparameters, network settings, and training options for a specific behavior (make sure these match in the editor and in the yaml file), such as `Car_CAM_NO-STATE_V3_8B`. The configuration also includes settings for the environment, engine, and checkpoints.

### Overview

The configuration file is written in YAML format and is used to configure and fine-tune the training process for a specific ML-Agents behavior. Below is an overview of the key sections and settings:

### Behaviors

- `Car_CAM_NO-STATE_V3_8B`: This section defines the training settings for a specific behavior, such as the PPO trainer type, hyperparameters (batch size, learning rate, etc.), network settings, and reward signals.

### Environment Settings

- `env_path`: Specifies the path to the Unity environment. (Set to `null`)

### Engine Settings

- `width` and `height`: Define the screen resolution for the training environment.
- `quality_level`: Sets the graphics quality level.
- `time_scale`: Scales the time in the environment.
- `target_frame_rate`: Specifies the target frame rate.
- `capture_frame_rate`: Sets the frame rate for capturing training data.
- `no_graphics`: A boolean indicating whether to run without graphics.

### Checkpoint Settings

- `run_id`: Specifies the run ID for training.
- `initialize_from`: Specifies the path to initialize from (set to `null`).
- `load_model`: Indicates whether to load a pre-trained model.
- `resume`: Indicates whether to resume training from a previous checkpoint.
- `force`: A boolean indicating whether to overwrite existing checkpoints.
- `train_model`: Specifies whether to train the model.
- `inference`: Indicates whether to perform model inference.
- `results_dir`: Defines the directory for saving training results.

### Usage

To use this configuration file for training with Unity ML-Agents, follow these steps:

1. Configure the settings in the YAML file to suit your training requirements.

2. Save the file with an appropriate name, such as `ppo_config.yaml`.

3. When running your ML-Agents training, specify the path to your configuration file using the `--config` command-line argument.

Example:
```bash
mlagents-learn my_behavior --run-id=my_run --config=ppo_config.yaml
```

Version 2 uses a Camera Input And Has A Curriculum Learning Implemented, Furthermore, It Also Uses Curiosity Reward Signals
