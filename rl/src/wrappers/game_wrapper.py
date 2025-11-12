"""Gym-style wrapper for ML-Agents Noxus-Ionia environment"""

import numpy as np
from typing import Dict, Any, Optional
from mlagents_envs.base_env import BaseEnv
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.side_channel.environment_parameters_channel import EnvironmentParametersChannel


class GameWrapper:
    """
    Wrapper for ML-Agents environment with observation normalization
    and action masking utilities.
    """

    def __init__(
        self,
        env: BaseEnv,
        normalize_observations: bool = True,
        normalize_rewards: bool = False,
        reward_scale: float = 1.0,
    ):
        self.env = env
        self.normalize_observations = normalize_observations
        self.normalize_rewards = normalize_rewards
        self.reward_scale = reward_scale

        # Observation normalization stats
        self.obs_mean = None
        self.obs_std = None
        self.obs_count = 0

        # Reward normalization stats
        self.reward_mean = 0.0
        self.reward_std = 1.0
        self.reward_count = 0

    def reset(self) -> Dict[str, np.ndarray]:
        """Reset environment and return initial observations"""
        self.env.reset()
        decision_steps, terminal_steps = self.env.get_steps(self.env.get_behavior_names()[0])
        
        obs_dict = {}
        for behavior_name in self.env.get_behavior_names():
            decision_steps, terminal_steps = self.env.get_steps(behavior_name)
            if len(decision_steps) > 0:
                obs = decision_steps.obs[0]  # First observation vector
                if self.normalize_observations:
                    obs = self._normalize_observation(obs)
                obs_dict[behavior_name] = obs

        return obs_dict

    def step(self, actions: Dict[str, np.ndarray]) -> tuple:
        """
        Step environment with actions.
        Returns: (observations, rewards, dones, infos)
        """
        # Set actions
        for behavior_name, action in actions.items():
            self.env.set_actions(behavior_name, action)

        # Step environment
        self.env.step()

        # Get results
        obs_dict = {}
        reward_dict = {}
        done_dict = {}
        info_dict = {}

        for behavior_name in self.env.get_behavior_names():
            decision_steps, terminal_steps = self.env.get_steps(behavior_name)

            # Combine decision and terminal steps
            all_obs = []
            all_rewards = []
            all_dones = []
            all_infos = []

            # Decision steps (agents still active)
            if len(decision_steps) > 0:
                obs = decision_steps.obs[0]
                if self.normalize_observations:
                    obs = self._normalize_observation(obs)
                all_obs.append(obs)
                all_rewards.append(decision_steps.reward)
                all_dones.append(np.zeros(len(decision_steps), dtype=bool))
                all_infos.append({})

            # Terminal steps (agents done)
            if len(terminal_steps) > 0:
                obs = terminal_steps.obs[0]
                if self.normalize_observations:
                    obs = self._normalize_observation(obs)
                all_obs.append(obs)
                all_rewards.append(terminal_steps.reward)
                all_dones.append(np.ones(len(terminal_steps), dtype=bool))
                all_infos.append({"episode_end": True})

            if all_obs:
                obs_dict[behavior_name] = np.concatenate(all_obs) if len(all_obs) > 1 else all_obs[0]
                reward_dict[behavior_name] = np.concatenate(all_rewards) if len(all_rewards) > 1 else all_rewards[0]
                done_dict[behavior_name] = np.concatenate(all_dones) if len(all_dones) > 1 else all_dones[0]
                info_dict[behavior_name] = all_infos[0] if len(all_infos) == 1 else {}

                # Normalize rewards if enabled
                if self.normalize_rewards:
                    reward_dict[behavior_name] = self._normalize_reward(reward_dict[behavior_name])

                # Scale rewards
                reward_dict[behavior_name] *= self.reward_scale

        return obs_dict, reward_dict, done_dict, info_dict

    def _normalize_observation(self, obs: np.ndarray) -> np.ndarray:
        """Normalize observation using running statistics"""
        obs = np.asarray(obs, dtype=np.float32)

        if self.obs_mean is None:
            self.obs_mean = np.zeros_like(obs)
            self.obs_std = np.ones_like(obs)

        # Update running statistics
        self.obs_count += 1
        alpha = 1.0 / self.obs_count
        delta = obs - self.obs_mean
        self.obs_mean += alpha * delta
        self.obs_std += alpha * (delta * (obs - self.obs_mean) - self.obs_std)

        # Avoid division by zero
        self.obs_std = np.maximum(self.obs_std, 1e-8)

        # Normalize
        normalized = (obs - self.obs_mean) / self.obs_std
        return normalized

    def _normalize_reward(self, reward: np.ndarray) -> np.ndarray:
        """Normalize reward using running statistics"""
        reward = np.asarray(reward, dtype=np.float32)

        # Update running statistics
        self.reward_count += 1
        alpha = 1.0 / self.reward_count
        delta = reward - self.reward_mean
        self.reward_mean += alpha * delta.mean()
        self.reward_std += alpha * (delta * (reward - self.reward_mean) - self.reward_std).mean()

        # Avoid division by zero
        self.reward_std = max(self.reward_std, 1e-8)

        # Normalize
        normalized = (reward - self.reward_mean) / self.reward_std
        return normalized

    def close(self):
        """Close environment"""
        self.env.close()

    def get_action_space_size(self, behavior_name: str) -> int:
        """Get action space size for behavior"""
        spec = self.env.get_behavior_spec(behavior_name)
        return spec.action_size

    def get_observation_space_size(self, behavior_name: str) -> int:
        """Get observation space size for behavior"""
        spec = self.env.get_behavior_spec(behavior_name)
        return spec.observation_specs[0].shape[0] if spec.observation_specs else 0

