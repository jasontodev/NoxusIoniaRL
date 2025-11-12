# Infrastructure

AWS deployment scripts and Docker configurations for the Noxus-Ionia RL game.

## AWS Setup

### Prerequisites

1. AWS CLI configured with credentials
2. IAM role/policy with S3 and EC2 permissions (see `aws/iam_policy.json`)
3. Key pair for EC2 access

### EC2 Bootstrap

Bootstrap script installs Docker, NVIDIA drivers, Python, and AWS CLI:

```bash
bash infra/aws/ec2_bootstrap.sh
```

### Launch Training Instance

Launch EC2 spot instance for training:

```bash
export INSTANCE_TYPE=g5.xlarge
export SPOT_PRICE=0.50
export KEY_NAME=your-key-name
export S3_BUCKET=your-bucket-name

bash infra/aws/launch_training.sh
```

### S3 Sync

Sync checkpoints, logs, and artifacts to/from S3:

```bash
export S3_BUCKET=your-bucket-name
export S3_PREFIX=training-runs
export SYNC_DIRECTION=upload  # or download, or both

bash infra/aws/s3_sync.sh
```

## Docker

### Build Images

```bash
# RL training
docker build -f infra/docker/Dockerfile.rl -t noxus-ionia-rl .

# Services
docker build -t noxus-ionia-analytics services/analytics
docker build -t noxus-ionia-llm services/llm
docker build -t noxus-ionia-dashboard dash
```

### Docker Compose

Run all services:

```bash
cd infra/docker
docker-compose up -d
```

View logs:

```bash
docker-compose logs -f rl-training
```

## Configuration

Set environment variables in `.env` or export before running:

```bash
export S3_BUCKET=your-bucket-name
export S3_PREFIX=training-runs
export UNITY_ENV_PATH=/path/to/unity/build
export ENABLE_LLM_COMMS=false
```

