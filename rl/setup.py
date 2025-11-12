"""Setup script for RL training package"""

from setuptools import setup, find_packages
from pathlib import Path

readme = Path(__file__).parent / "README.md"
long_description = readme.read_text() if readme.exists() else ""

setup(
    name="noxus-ionia-rl",
    version="0.1.0",
    description="RL training package for Noxus-Ionia game",
    long_description=long_description,
    long_description_content_type="text/markdown",
    author="Noxus-Ionia Team",
    packages=find_packages(where="src"),
    package_dir={"": "src"},
    python_requires=">=3.9",
    install_requires=[
        "mlagents>=0.30.0",
        "mlagents-envs>=0.30.0",
        "torch>=2.0.0",
        "tensorboard>=2.13.0",
        "optuna>=3.0.0",
        "numpy>=1.24.0",
        "pandas>=2.0.0",
        "pyyaml>=6.0",
        "boto3>=1.28.0",
    ],
    extras_require={
        "dev": [
            "pytest>=7.4.0",
            "pytest-cov>=4.1.0",
            "black>=23.0.0",
            "flake8>=6.0.0",
        ],
        "wandb": [
            "wandb>=0.15.0",
        ],
    },
    entry_points={
        "console_scripts": [
            "noxus-ionia-train=scripts.train:main",
            "noxus-ionia-hello=scripts.hello_rl:main",
            "noxus-ionia-optuna=scripts.optuna_sweep:main",
        ],
    },
)

