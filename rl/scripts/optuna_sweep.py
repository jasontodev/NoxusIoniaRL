#!/usr/bin/env python3
"""Optuna hyperparameter sweep for PPO"""

import argparse
import optuna
from pathlib import Path
import yaml
import json
import subprocess
import sys


def load_config(config_path: str) -> dict:
    """Load YAML configuration file"""
    with open(config_path, "r") as f:
        return yaml.safe_load(f)


def save_config(config: dict, output_path: str):
    """Save YAML configuration file"""
    with open(output_path, "w") as f:
        yaml.dump(config, f, default_flow_style=False)


def objective(trial, base_config_path: str, unity_env_path: str = None, max_steps: int = 100000):
    """Optuna objective function for hyperparameter optimization"""

    # Load base config
    config = load_config(base_config_path)

    # Suggest hyperparameters
    learning_rate = trial.suggest_float("learning_rate", 1e-5, 1e-2, log=True)
    batch_size = trial.suggest_int("batch_size", 64, 2048, log=True)
    gamma = trial.suggest_float("gamma", 0.9, 0.999)
    gae_lambda = trial.suggest_float("gae_lambda", 0.8, 1.0)
    entropy_coeff = trial.suggest_float("entropy_coeff", 1e-4, 1e-1, log=True)

    # Update config with suggested values
    if "hyperparameters" not in config:
        config["hyperparameters"] = {}
    
    config["hyperparameters"]["learning_rate"] = learning_rate
    config["hyperparameters"]["batch_size"] = batch_size
    
    if "reward_signals" not in config:
        config["reward_signals"] = {}
    if "extrinsic" not in config["reward_signals"]:
        config["reward_signals"]["extrinsic"] = {}
    
    config["reward_signals"]["extrinsic"]["gamma"] = gamma
    config["hyperparameters"]["lambd"] = gae_lambda
    config["hyperparameters"]["beta"] = entropy_coeff

    # Save trial config
    trial_config_path = f"config_trial_{trial.number}.yaml"
    save_config(config, trial_config_path)

    # Run training (simplified - in practice, use mlagents-learn or TrainerController)
    run_id = f"optuna_trial_{trial.number}"
    
    print(f"\nTrial {trial.number}:")
    print(f"  learning_rate: {learning_rate:.6f}")
    print(f"  batch_size: {batch_size}")
    print(f"  gamma: {gamma:.4f}")
    print(f"  gae_lambda: {gae_lambda:.4f}")
    print(f"  entropy_coeff: {entropy_coeff:.6f}")

    # Build mlagents-learn command
    cmd = [
        "mlagents-learn",
        trial_config_path,
        "--run-id", run_id,
        "--max-steps", str(max_steps),
    ]
    
    if unity_env_path:
        cmd.extend(["--env", unity_env_path])

    # Run training and capture results
    try:
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=3600,  # 1 hour timeout per trial
        )

        # Parse final reward/return from output
        # In practice, you'd read from TensorBoard or log files
        final_reward = 0.0
        
        # Try to extract reward from output
        for line in result.stdout.split("\n"):
            if "Cumulative reward" in line or "Mean return" in line:
                # Simple parsing - adjust based on actual output format
                try:
                    parts = line.split()
                    for i, part in enumerate(parts):
                        if "reward" in part.lower() or "return" in part.lower():
                            if i + 1 < len(parts):
                                final_reward = float(parts[i + 1])
                                break
                except:
                    pass

        # Report to Optuna
        trial.set_user_attr("config_path", trial_config_path)
        trial.set_user_attr("run_id", run_id)
        
        return final_reward

    except subprocess.TimeoutExpired:
        print(f"Trial {trial.number} timed out")
        return float("-inf")
    except Exception as e:
        print(f"Trial {trial.number} failed: {e}")
        return float("-inf")


def main():
    parser = argparse.ArgumentParser(description="Optuna hyperparameter sweep")
    parser.add_argument(
        "--config",
        type=str,
        default="src/config/ppo_config.yaml",
        help="Base config YAML file",
    )
    parser.add_argument(
        "--unity-env",
        type=str,
        default=None,
        help="Path to Unity executable",
    )
    parser.add_argument(
        "--n-trials",
        type=int,
        default=20,
        help="Number of trials",
    )
    parser.add_argument(
        "--max-steps",
        type=int,
        default=100000,
        help="Max steps per trial",
    )
    parser.add_argument(
        "--study-name",
        type=str,
        default="noxus_ionia_ppo_sweep",
        help="Optuna study name",
    )
    parser.add_argument(
        "--storage",
        type=str,
        default=None,
        help="Optuna storage URL (e.g., sqlite:///study.db)",
    )

    args = parser.parse_args()

    base_config_path = Path(__file__).parent.parent / args.config

    # Create Optuna study
    study = optuna.create_study(
        direction="maximize",
        study_name=args.study_name,
        storage=args.storage,
        load_if_exists=True,
    )

    print(f"Starting Optuna sweep: {args.study_name}")
    print(f"Trials: {args.n_trials}")
    print(f"Max steps per trial: {args.max_steps}")
    print(f"Base config: {base_config_path}\n")

    # Run optimization
    study.optimize(
        lambda trial: objective(trial, str(base_config_path), args.unity_env, args.max_steps),
        n_trials=args.n_trials,
        show_progress_bar=True,
    )

    # Print results
    print("\n" + "=" * 60)
    print("Optimization Results")
    print("=" * 60)
    print(f"Best trial: {study.best_trial.number}")
    print(f"Best value: {study.best_value:.4f}")
    print(f"\nBest parameters:")
    for key, value in study.best_params.items():
        print(f"  {key}: {value}")

    # Save best config
    best_config = load_config(str(base_config_path))
    for key, value in study.best_params.items():
        if key == "learning_rate":
            best_config["hyperparameters"]["learning_rate"] = value
        elif key == "batch_size":
            best_config["hyperparameters"]["batch_size"] = value
        elif key == "gamma":
            best_config["reward_signals"]["extrinsic"]["gamma"] = value
        elif key == "gae_lambda":
            best_config["hyperparameters"]["lambd"] = value
        elif key == "entropy_coeff":
            best_config["hyperparameters"]["beta"] = value

    best_config_path = Path(__file__).parent.parent / "src" / "config" / "ppo_config_best.yaml"
    save_config(best_config, str(best_config_path))
    print(f"\nBest config saved to: {best_config_path}")


if __name__ == "__main__":
    main()

