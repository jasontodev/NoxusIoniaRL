"""Custom trainer with reward shaping, curriculum learning, and self-play"""

from typing import Dict, Any, Optional
import numpy as np
from mlagents.trainers.trainer import Trainer
from mlagents.trainers.ppo.trainer import PPOTrainer
from mlagents.trainers.sac.trainer import SACTrainer


class CustomTrainer:
    """
    Extended trainer wrapper with custom reward shaping,
    curriculum learning, and self-play scheduling.
    """

    def __init__(
        self,
        base_trainer: Trainer,
        reward_weights: Optional[Dict[str, float]] = None,
        curriculum_config: Optional[Dict[str, Any]] = None,
        self_play_config: Optional[Dict[str, Any]] = None,
    ):
        self.base_trainer = base_trainer
        self.reward_weights = reward_weights or {}
        self.curriculum_config = curriculum_config
        self.self_play_config = self_play_config

        # Curriculum state
        self.curriculum_step = 0
        self.current_lesson = 0

        # Self-play state
        self.self_play_step = 0
        self.opponent_pool = []

    def apply_reward_shaping(self, rewards: np.ndarray, reward_info: Dict[str, Any]) -> np.ndarray:
        """
        Apply custom reward shaping based on reward_info.
        reward_info should contain keys like 'deposit', 'pickup', 'elimination', etc.
        """
        shaped_rewards = rewards.copy()

        for reward_type, weight in self.reward_weights.items():
            if reward_type in reward_info:
                shaped_rewards += weight * reward_info[reward_type]

        return shaped_rewards

    def update_curriculum(self, step: int, performance_metric: float) -> Optional[Dict[str, Any]]:
        """
        Update curriculum based on performance.
        Returns environment parameter updates if lesson should advance.
        """
        if not self.curriculum_config:
            return None

        self.curriculum_step = step

        thresholds = self.curriculum_config.get("thresholds", [])
        min_lesson_length = self.curriculum_config.get("min_lesson_length", 100000)

        if self.current_lesson < len(thresholds):
            if step >= min_lesson_length and performance_metric >= thresholds[self.current_lesson]:
                self.current_lesson += 1
                return self.curriculum_config.get("parameters", {})

        return None

    def update_self_play(self, step: int) -> Optional[str]:
        """
        Update self-play opponent selection.
        Returns path to opponent model if opponent should be swapped.
        """
        if not self.self_play_config:
            return None

        self.self_play_step = step

        swap_steps = self.self_play_config.get("swap_steps", 2000)
        play_against_latest_ratio = self.self_play_config.get("play_against_latest_model_ratio", 0.5)

        if step % swap_steps == 0:
            # Decide whether to play against latest or random opponent
            if np.random.random() < play_against_latest_ratio:
                return "latest"  # Use latest model
            elif self.opponent_pool:
                return np.random.choice(self.opponent_pool)  # Use random opponent

        return None

    def save_opponent(self, model_path: str):
        """Save current model to opponent pool"""
        if self.self_play_config:
            save_steps = self.self_play_config.get("save_steps", 50000)
            window = self.self_play_config.get("window", 10)

            if self.self_play_step % save_steps == 0:
                self.opponent_pool.append(model_path)
                # Keep only recent opponents
                if len(self.opponent_pool) > window:
                    self.opponent_pool.pop(0)

