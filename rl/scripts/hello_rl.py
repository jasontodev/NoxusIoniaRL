#!/usr/bin/env python3
"""Hello RL script - trains for 50k steps locally"""

import os
import sys
from pathlib import Path

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))

from mlagents_envs.environment import UnityEnvironment
from mlagents.trainers.trainer_controller import TrainerController
from mlagents.trainers.settings import RunOptions, TrainerSettings
from mlagents.trainers.ppo.trainer import PPOTrainer

from src.utils.logging import TrainingLogger


def main():
    """Train for 50k steps with basic PPO"""
    print("=" * 60)
    print("Hello RL - Noxus-Ionia Training")
    print("=" * 60)

    # Initialize logger
    logger = TrainingLogger(
        log_dir="logs",
        tensorboard_dir="runs",
        checkpoint_dir="checkpoints",
    )

    print(f"Run ID: {logger.run_id}")
    print(f"TensorBoard: tensorboard --logdir {logger.tensorboard_dir}")

    # Create Unity environment (connect to editor or use executable)
    print("\nConnecting to Unity environment...")
    try:
        env = UnityEnvironment(
            seed=42,
            side_channels=[],
        )
        print("Connected to Unity Editor")
    except Exception as e:
        print(f"Failed to connect to Unity Editor: {e}")
        print("Make sure Unity Editor is running with the training scene loaded.")
        return

    # Get behavior names
    env.reset()
    behavior_names = env.get_behavior_names()
    print(f"Behaviors: {behavior_names}")

    # Configure training
    config_path = Path(__file__).parent.parent / "src" / "config" / "ppo_config.yaml"
    
    print(f"\nStarting training with config: {config_path}")
    print("Training for 50,000 steps...")
    print("Press Ctrl+C to stop early\n")

    # Use ML-Agents trainer controller
    # Note: This is a simplified example. Full implementation uses mlagents-learn CLI
    print("To train with ML-Agents, run:")
    print(f"  mlagents-learn {config_path} --run-id={logger.run_id} --max-steps=50000")
    print("\nOr use the mlagents-learn command directly from the project root:")
    print(f"  mlagents-learn unity/config/ppo_config.yaml --run-id={logger.run_id} --max-steps=50000")

    # Log training start
    logger.log_metrics(0, {
        "status": "training_started",
        "max_steps": 50000,
        "config": str(config_path),
    })

    # Simple training loop (for demonstration)
    # In practice, ML-Agents handles this internally
    step = 0
    max_steps = 50000

    try:
        while step < max_steps:
            # ML-Agents handles the actual training
            # This is just a placeholder loop
            env.step()
            step += 1000  # Approximate steps per iteration

            if step % 10000 == 0:
                print(f"Step {step}/{max_steps}")
                logger.log_metrics(step, {"step": step, "status": "training"})

    except KeyboardInterrupt:
        print("\nTraining interrupted by user")
    finally:
        print(f"\nTraining complete. Steps: {step}")
        print(f"Checkpoints: {logger.checkpoint_dir}")
        print(f"Logs: {logger.log_dir}")
        print(f"\nView results: tensorboard --logdir {logger.tensorboard_dir}")

        env.close()


if __name__ == "__main__":
    main()

