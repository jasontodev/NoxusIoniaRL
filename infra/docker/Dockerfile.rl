# Dockerfile for RL training environment

FROM nvidia/cuda:11.8.0-cudnn8-runtime-ubuntu22.04

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    python3.9 \
    python3-pip \
    git \
    wget \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install Python dependencies
COPY ../../rl/requirements.txt ./rl/requirements.txt
RUN pip3 install --no-cache-dir -r rl/requirements.txt

# Install ML-Agents
RUN pip3 install mlagents mlagents-envs

# Install Unity ML-Agents Python package
# Note: Adjust version as needed
RUN pip3 install --no-cache-dir \
    torch==2.0.0 \
    torchvision==0.15.0 \
    --index-url https://download.pytorch.org/whl/cu118

# Copy project files
COPY ../../rl ./rl
COPY ../../unity/config ./unity/config

# Set environment variables
ENV PYTHONPATH=/app
ENV MLAGENTS_LOG_LEVEL=INFO

# Default command
CMD ["python3", "rl/scripts/train.py", "--config", "rl/src/config/ppo_config.yaml"]

