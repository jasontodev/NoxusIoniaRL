#!/bin/bash
# EC2 bootstrap script for training environment

set -e

echo "Starting EC2 bootstrap for Noxus-Ionia RL training..."

# Update system
sudo apt-get update
sudo apt-get upgrade -y

# Install Docker
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    rm get-docker.sh
fi

# Install NVIDIA drivers and Docker GPU support (if GPU instance)
if lspci | grep -i nvidia &> /dev/null; then
    echo "Installing NVIDIA drivers..."
    sudo apt-get install -y nvidia-driver-535
    sudo apt-get install -y nvidia-container-toolkit
    sudo systemctl restart docker
fi

# Install Python and pip
sudo apt-get install -y python3 python3-pip python3-venv

# Install AWS CLI
if ! command -v aws &> /dev/null; then
    echo "Installing AWS CLI..."
    curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
    unzip awscliv2.zip
    sudo ./aws/install
    rm -rf aws awscliv2.zip
fi

# Install Git
sudo apt-get install -y git

# Create directories
mkdir -p ~/noxus-ionia
mkdir -p ~/noxus-ionia/data/logs
mkdir -p ~/noxus-ionia/data/checkpoints
mkdir -p ~/noxus-ionia/data/artifacts

# Clone repository (if using Git)
# git clone https://github.com/your-org/noxus-ionia.git ~/noxus-ionia

# Or sync from S3
# aws s3 sync s3://your-bucket/noxus-ionia ~/noxus-ionia

echo "Bootstrap complete!"
echo "Next steps:"
echo "1. Configure AWS credentials: aws configure"
echo "2. Pull Docker image or build: docker build -t noxus-ionia-rl infra/docker/Dockerfile.rl"
echo "3. Run training: ./infra/aws/launch_training.sh"

