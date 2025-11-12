#!/usr/bin/env python3
"""Main training script for Noxus-Ionia RL game"""

import argparse
import os
import sys
from pathlib import Path
import yaml

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))

from mlagents_envs.environment import UnityEnvironment
from mlagents.trainers.trainer_controller import TrainerController
from mlagents.trainers.settings import RunOptions, TrainerSettings
from mlagents.trainers.ppo.trainer import PPOTrainer
from mlagents.trainers.sac.trainer import SACTrainer
from mlagents.trainers.stats import StatsReporter

from src.utils.logging import TrainingLogger
from src.wrappers.game_wrapper import GameWrapper
from src.trainers.custom_trainer import CustomTrainer


def load_config(config_path: str) -> dict:
    """Load YAML configuration file"""
    with open(config_path, "r") as f:
        return yaml.safe_load(f)


def create_trainer_settings(config: dict) -> TrainerSettings:
    """Create trainer settings from config"""
    # This is a simplified version - ML-Agents uses a more complex settings system
    # In practice, you'd use mlagents.trainers.settings.TrainerSettings
    pass


def main():
    parser = argparse.ArgumentParser(description="Train RL agents for Noxus-Ionia game")
    parser.add_argument(
        "--config",
        type=str,
        default="src/config/ppo_config.yaml",
        help="Path to config YAML file",
    )
    parser.add_argument(
        "--unity-env",
        type=str,
        default=None,
        help="Path to Unity executable (or None for editor)",
    )
    parser.add_argument(
        "--run-id",
        type=str,
        default=None,
        help="Run ID for logging (auto-generated if not provided)",
    )
    parser.add_argument(
        "--resume",
        type=str,
        default=None,
        help="Path to checkpoint to resume from",
    )
    parser.add_argument(
        "--seed",
        type=int,
        default=None,
        help="Random seed",
    )

    args = parser.parse_args()

    # Load config
    config_path = Path(__file__).parent.parent / args.config
    config = load_config(str(config_path))

    # Override config with command-line args
    if args.unity_env:
        config["unity_env_path"] = args.unity_env
    if args.run_id:
        config["run_id"] = args.run_id
    if args.seed is not None:
        config["seed"] = args.seed

    # Initialize logger
    logger = TrainingLogger(
        log_dir=config.get("log_dir", "logs"),
        tensorboard_dir=config.get("tensorboard_dir", "runs"),
        checkpoint_dir=config.get("checkpoint_dir", "checkpoints"),
        run_id=config.get("run_id"),
    )

    print(f"Starting training run: {logger.run_id}")
    print(f"Config: {config_path}")
    print(f"Unity env: {config.get('unity_env_path', 'Editor')}")

    # Create Unity environment
    unity_env_path = config.get("unity_env_path")
    if unity_env_path and os.path.exists(unity_env_path):
        env = UnityEnvironment(
            file_name=unity_env_path,
            seed=config.get("seed", 42),
            side_channels=[],
        )
    else:
        # Connect to Unity Editor
        print("Connecting to Unity Editor...")
        env = UnityEnvironment(
            seed=config.get("seed", 42),
            side_channels=[],
        )

    # Wrap environment
    wrapper = GameWrapper(
        env,
        normalize_observations=config.get("network_settings", {}).get("normalize", True),
    )

    # Initialize trainer (simplified - actual implementation uses ML-Agents trainer controller)
    print("Initializing trainer...")
    print("Note: This is a simplified training script.")
    print("For full ML-Agents integration, use mlagents-learn command or TrainerController API.")

    # Log initial metrics
    logger.log_metrics(0, {"status": "training_started", "config": str(config_path)})

    # Training loop would go here
    # In practice, you'd use mlagents-learn or TrainerController
    print("\nTo train with ML-Agents, use:")
    print(f"  mlagents-learn {config_path} --run-id={logger.run_id}")
    if unity_env_path:
        print(f"  --env={unity_env_path}")

    # Cleanup
    wrapper.close()
    env.close()

    print(f"\nTraining setup complete. Logs: {logger.log_dir}")


if __name__ == "__main__":
    main()

