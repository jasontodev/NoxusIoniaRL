#!/bin/bash
# S3 sync utility for checkpoints, logs, and artifacts

set -e

# Configuration
S3_BUCKET=${S3_BUCKET:-your-bucket-name}
S3_PREFIX=${S3_PREFIX:-training-runs}
LOCAL_DIR=${LOCAL_DIR:-./data}
SYNC_DIRECTION=${SYNC_DIRECTION:-upload}  # upload, download, or both

S3_PATH="s3://${S3_BUCKET}/${S3_PREFIX}"

echo "S3 Sync Utility"
echo "Bucket: $S3_BUCKET"
echo "Prefix: $S3_PREFIX"
echo "Direction: $SYNC_DIRECTION"
echo ""

# Upload to S3
if [ "$SYNC_DIRECTION" == "upload" ] || [ "$SYNC_DIRECTION" == "both" ]; then
    echo "Uploading to S3..."
    
    # Sync checkpoints
    if [ -d "$LOCAL_DIR/checkpoints" ]; then
        echo "  Syncing checkpoints..."
        aws s3 sync "$LOCAL_DIR/checkpoints" "$S3_PATH/checkpoints" --exclude "*" --include "*.pt" --include "*.onnx" --include "*.nn"
    fi
    
    # Sync logs
    if [ -d "$LOCAL_DIR/logs" ]; then
        echo "  Syncing logs..."
        aws s3 sync "$LOCAL_DIR/logs" "$S3_PATH/logs"
    fi
    
    # Sync TensorBoard events
    if [ -d "runs" ]; then
        echo "  Syncing TensorBoard events..."
        aws s3 sync runs "$S3_PATH/tensorboard"
    fi
    
    # Sync artifacts
    if [ -d "$LOCAL_DIR/artifacts" ]; then
        echo "  Syncing artifacts..."
        aws s3 sync "$LOCAL_DIR/artifacts" "$S3_PATH/artifacts"
    fi
    
    echo "Upload complete!"
fi

# Download from S3
if [ "$SYNC_DIRECTION" == "download" ] || [ "$SYNC_DIRECTION" == "both" ]; then
    echo "Downloading from S3..."
    
    # Create directories
    mkdir -p "$LOCAL_DIR/checkpoints"
    mkdir -p "$LOCAL_DIR/logs"
    mkdir -p "$LOCAL_DIR/artifacts"
    mkdir -p runs
    
    # Download
    aws s3 sync "$S3_PATH/checkpoints" "$LOCAL_DIR/checkpoints"
    aws s3 sync "$S3_PATH/logs" "$LOCAL_DIR/logs"
    aws s3 sync "$S3_PATH/tensorboard" runs
    aws s3 sync "$S3_PATH/artifacts" "$LOCAL_DIR/artifacts"
    
    echo "Download complete!"
fi

echo ""
echo "S3 sync finished!"

