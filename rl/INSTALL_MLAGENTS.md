# Installing ML-Agents for Training

## The Error

```
mlagents-learn : The term 'mlagents-learn' is not recognized
```

This means the ML-Agents Python package is not installed or not in your PATH.

## Solution: Install ML-Agents

### Step 1: Check Python Installation

First, verify Python is installed:

```powershell
python --version
```

Should show Python 3.9 or higher. If not, install Python from python.org.

### Step 2: Create Virtual Environment (Recommended)

```powershell
cd rl
python -m venv venv
.\venv\Scripts\Activate.ps1
```

**Note**: If you get an execution policy error, run:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Step 3: Install ML-Agents

```powershell
pip install mlagents
```

This installs:
- `mlagents` (training package)
- `mlagents-envs` (environment interface)
- `mlagents-learn` (command-line tool)

### Step 4: Verify Installation

```powershell
mlagents-learn --help
```

Should show help text. If it works, you're ready!

### Step 5: Install Additional Dependencies

```powershell
pip install -r requirements.txt
```

This installs PyTorch, TensorBoard, and other dependencies.

## Alternative: Install from Requirements

If you prefer to install everything at once:

```powershell
cd rl
python -m venv venv
.\venv\Scripts\Activate.ps1
pip install -r requirements.txt
pip install mlagents mlagents-envs
```

## Quick Start After Installation

1. **Activate virtual environment**:
   ```powershell
   cd rl
   .\venv\Scripts\Activate.ps1
   ```

2. **Start training**:
   ```powershell
   mlagents-learn ../unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run
   ```

3. **Wait for**: "Start training by pressing the Play button in the Unity Editor"

4. **Press Play in Unity**

5. **Watch training** in the terminal and TensorBoard

## Troubleshooting

### "Python not found"
- Install Python 3.9+ from python.org
- Make sure "Add Python to PATH" is checked during installation

### "pip not found"
- Python might not be in PATH
- Try `python -m pip install mlagents` instead

### "mlagents-learn still not found after install"
- Make sure virtual environment is activated
- Try `python -m mlagents.trainers.learn` instead
- Or use full path: `python -m mlagents.trainers.learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run`

### "Permission denied" or "Access denied"
- Run PowerShell as Administrator
- Or install with `--user` flag: `pip install --user mlagents`

## Using Python Module Instead

If `mlagents-learn` command doesn't work, you can use:

```powershell
python -m mlagents.trainers.learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run
```

This is equivalent to `mlagents-learn` but uses Python module syntax.

## Verify Everything Works

After installation, test with:

```powershell
# Check ML-Agents version
mlagents-learn --version

# Or
python -m mlagents.trainers.learn --version
```

Both should show version information.

