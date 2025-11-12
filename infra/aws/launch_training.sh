#!/bin/bash
# Launch training on EC2 spot instance

set -e

# Configuration
INSTANCE_TYPE=${INSTANCE_TYPE:-g5.xlarge}
SPOT_PRICE=${SPOT_PRICE:-0.50}
KEY_NAME=${KEY_NAME:-your-key-name}
SECURITY_GROUP=${SECURITY_GROUP:-sg-xxxxxxxxx}
SUBNET_ID=${SUBNET_ID:-subnet-xxxxxxxxx}
AMI_ID=${AMI_ID:-ami-0c55b159cbfafe1f0}  # Deep Learning AMI (adjust for your region)
S3_BUCKET=${S3_BUCKET:-your-bucket-name}
S3_PREFIX=${S3_PREFIX:-training-runs}

echo "Launching EC2 spot instance for training..."

# Create user data script
cat > /tmp/user-data.sh << 'EOF'
#!/bin/bash
cd /home/ec2-user
git clone https://github.com/your-org/noxus-ionia.git || true
cd noxus-ionia
bash infra/aws/ec2_bootstrap.sh
EOF

# Launch spot instance
INSTANCE_ID=$(aws ec2 run-instances \
    --image-id $AMI_ID \
    --instance-type $INSTANCE_TYPE \
    --key-name $KEY_NAME \
    --security-group-ids $SECURITY_GROUP \
    --subnet-id $SUBNET_ID \
    --instance-market-options '{
        "MarketType": "spot",
        "SpotOptions": {
            "MaxPrice": "'$SPOT_PRICE'",
            "SpotInstanceType": "one-time"
        }
    }' \
    --user-data file:///tmp/user-data.sh \
    --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value=noxus-ionia-training}]' \
    --query 'Instances[0].InstanceId' \
    --output text)

echo "Instance launched: $INSTANCE_ID"
echo "Waiting for instance to be running..."

aws ec2 wait instance-running --instance-ids $INSTANCE_ID

# Get public IP
PUBLIC_IP=$(aws ec2 describe-instances \
    --instance-ids $INSTANCE_ID \
    --query 'Reservations[0].Instances[0].PublicIpAddress' \
    --output text)

echo "Instance is running!"
echo "Instance ID: $INSTANCE_ID"
echo "Public IP: $PUBLIC_IP"
echo ""
echo "SSH into instance:"
echo "  ssh -i $KEY_NAME.pem ec2-user@$PUBLIC_IP"
echo ""
echo "To run training:"
echo "  cd ~/noxus-ionia"
echo "  docker-compose -f infra/docker/docker-compose.yml up rl-training"

rm /tmp/user-data.sh

