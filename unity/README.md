# Unity ML-Agents Project

Unity game environment for Noxus vs Ionia RL training.

## Setup

1. Open Unity Hub
2. Open project in `unity/NoxusIoniaRL/`
3. Install ML-Agents package:
   - Window → Package Manager
   - Click `+` → Add package from git URL
   - Enter: `com.unity.ml-agents`
   - Click Add

4. Configure ML-Agents:
   - Edit → Project Settings → ML-Agents
   - Click "Create Settings Asset"

## Scene Setup

1. Create training scene: `Assets/Scenes/TrainingScene.unity`
2. Add GameManager to scene
3. Create heal zones (Noxus and Ionia)
4. Add forest area with obstacles
5. Add spawn points for agents
6. Create agent prefabs (NoxusAgent, IoniaAgent)

## Scripts

- `Assets/Scripts/Agents/`: Agent implementations
- `Assets/Scripts/Environment/`: Game mechanics
- `Assets/Scripts/Events/`: Event logging

## Training

See `rl/README.md` for training instructions.

## Configuration

ML-Agents config files in `unity/config/`:
- `ppo_config.yaml`: PPO hyperparameters

