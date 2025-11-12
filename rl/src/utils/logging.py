"""Logging utilities for RL training"""

import os
import json
from datetime import datetime
from typing import Dict, Any, Optional
from pathlib import Path


class TrainingLogger:
    """Logger for training metrics and checkpoints"""

    def __init__(
        self,
        log_dir: str = "logs",
        tensorboard_dir: str = "runs",
        checkpoint_dir: str = "checkpoints",
        run_id: Optional[str] = None,
    ):
        self.log_dir = Path(log_dir)
        self.tensorboard_dir = Path(tensorboard_dir)
        self.checkpoint_dir = Path(checkpoint_dir)
        self.run_id = run_id or datetime.now().strftime("%Y%m%d_%H%M%S")

        # Create directories
        self.log_dir.mkdir(parents=True, exist_ok=True)
        self.tensorboard_dir.mkdir(parents=True, exist_ok=True)
        self.checkpoint_dir.mkdir(parents=True, exist_ok=True)

        # Log file
        self.log_file = self.log_dir / f"training_{self.run_id}.jsonl"

    def log_metrics(self, step: int, metrics: Dict[str, Any]):
        """Log metrics to JSONL file"""
        log_entry = {
            "step": step,
            "timestamp": datetime.now().isoformat(),
            **metrics,
        }

        with open(self.log_file, "a") as f:
            f.write(json.dumps(log_entry) + "\n")

    def log_checkpoint(self, step: int, checkpoint_path: str, metadata: Optional[Dict[str, Any]] = None):
        """Log checkpoint information"""
        checkpoint_info = {
            "step": step,
            "timestamp": datetime.now().isoformat(),
            "checkpoint_path": checkpoint_path,
            "metadata": metadata or {},
        }

        checkpoint_log = self.log_dir / f"checkpoints_{self.run_id}.jsonl"
        with open(checkpoint_log, "a") as f:
            f.write(json.dumps(checkpoint_info) + "\n")

    def get_tensorboard_path(self) -> Path:
        """Get TensorBoard log directory"""
        return self.tensorboard_dir / self.run_id

    def get_checkpoint_path(self, step: int) -> Path:
        """Get checkpoint file path"""
        return self.checkpoint_dir / f"checkpoint_{self.run_id}_step_{step}.pt"

