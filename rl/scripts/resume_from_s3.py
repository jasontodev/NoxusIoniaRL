#!/usr/bin/env python3
"""Resume training from S3 checkpoint"""

import argparse
import boto3
import os
from pathlib import Path
import yaml


def download_from_s3(s3_bucket: str, s3_prefix: str, local_dir: str):
    """Download checkpoint and config from S3"""
    s3 = boto3.client("s3")

    # List objects in S3 prefix
    response = s3.list_objects_v2(Bucket=s3_bucket, Prefix=s3_prefix)

    if "Contents" not in response:
        print(f"No objects found in s3://{s3_bucket}/{s3_prefix}")
        return None

    # Create local directory
    Path(local_dir).mkdir(parents=True, exist_ok=True)

    # Download files
    checkpoint_path = None
    for obj in response["Contents"]:
        key = obj["Key"]
        local_path = os.path.join(local_dir, os.path.basename(key))
        
        print(f"Downloading s3://{s3_bucket}/{key} -> {local_path}")
        s3.download_file(s3_bucket, key, local_path)

        if "checkpoint" in key.lower() or key.endswith(".onnx") or key.endswith(".nn"):
            checkpoint_path = local_path

    return checkpoint_path


def main():
    parser = argparse.ArgumentParser(description="Resume training from S3")
    parser.add_argument(
        "--s3-bucket",
        type=str,
        required=True,
        help="S3 bucket name",
    )
    parser.add_argument(
        "--s3-prefix",
        type=str,
        required=True,
        help="S3 prefix (path) to checkpoint",
    )
    parser.add_argument(
        "--local-dir",
        type=str,
        default="checkpoints/resumed",
        help="Local directory to download checkpoint",
    )
    parser.add_argument(
        "--config",
        type=str,
        default="src/config/ppo_config.yaml",
        help="Config YAML file",
    )

    args = parser.parse_args()

    print(f"Downloading checkpoint from s3://{args.s3_bucket}/{args.s3_prefix}")

    checkpoint_path = download_from_s3(args.s3_bucket, args.s3_prefix, args.local_dir)

    if checkpoint_path:
        print(f"\nCheckpoint downloaded: {checkpoint_path}")
        print(f"\nTo resume training, use:")
        print(f"  mlagents-learn {args.config} --resume --checkpoint-path {checkpoint_path}")
    else:
        print("\nNo checkpoint found. Check S3 path.")


if __name__ == "__main__":
    main()

