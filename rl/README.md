# RL Training Package

Python package for training reinforcement learning agents in the Noxus-Ionia game using Unity ML-Agents.

## Installation

```bash
cd rl
pip install -r requirements.txt
pip install -e .
```

## Usage

### Basic Training

Train with default PPO configuration:

```bash
python scripts/train.py --config src/config/ppo_config.yaml
```

### Hello RL (50k steps)

Quick training run for testing:

```bash
python scripts/hello_rl.py
```

Or use ML-Agents CLI directly:

```bash
mlagents-learn unity/config/ppo_config.yaml --run-id=my_run --max-steps=50000
```

### Hyperparameter Sweep

Run Optuna optimization:

```bash
python scripts/optuna_sweep.py --n-trials=20 --max-steps=100000
```

### Resume from S3

Download and resume training from S3 checkpoint:

```bash
python scripts/resume_from_s3.py --s3-bucket=my-bucket --s3-prefix=checkpoints/run_123
```

## Configuration

Edit `src/config/ppo_config.yaml` or `src/config/sac_config.yaml` to adjust hyperparameters.

## TensorBoard

View training metrics:

```bash
tensorboard --logdir runs
```

## Project Structure

- `src/config/`: Configuration YAML files
- `src/wrappers/`: Environment wrappers
- `src/trainers/`: Custom trainer extensions
- `src/utils/`: Logging and utilities
- `scripts/`: Training scripts

