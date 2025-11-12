# Starting Training with ML-Agents 2.0.2

## ML-Agents 2.0.2 Training Method

ML-Agents 2.0.2 **does NOT have a Unity Training Window**. Training is done via **command line** using `mlagents-learn`.

## Current Issue: Python 3.11 Compatibility

You have Python 3.11 in your venv, which has compatibility issues with ML-Agents 0.28.0. Here are your options:

## Solution 1: Fix Python 3.11 Compatibility (Quick Fix)

Try downgrading `cattrs` which is causing the typing issue:

```powershell
cd "D:\Gamer to Developer\Code\noxus-ionia"
.\venv\Scripts\Activate.ps1
pip install "cattrs<1.6" "typing-extensions<4.6"
```

Then test:
```powershell
mlagents-learn --version
```

## Solution 2: Use Python 3.10 (Most Reliable)

1. **Install Python 3.10** from python.org
2. **Recreate venv**:
   ```powershell
   cd "D:\Gamer to Developer\Code\noxus-ionia"
   Remove-Item -Recurse -Force venv
   python3.10 -m venv venv
   .\venv\Scripts\Activate.ps1
   pip install mlagents mlagents-envs "protobuf<3.21"
   ```

## Solution 3: Use Python Module Syntax (Workaround)

If the command doesn't work, try using Python module syntax:

```powershell
cd "D:\Gamer to Developer\Code\noxus-ionia"
.\venv\Scripts\Activate.ps1
python -m mlagents.trainers.learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run
```

## Starting Training (Once mlagents-learn Works)

### Step 1: Activate Virtual Environment

```powershell
cd "D:\Gamer to Developer\Code\noxus-ionia"
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\venv\Scripts\Activate.ps1
```

### Step 2: Start Training

```powershell
mlagents-learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run
```

**Or if command doesn't work**:
```powershell
python -m mlagents.trainers.learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run
```

### Step 3: Wait for Connection Message

You should see:
```
Start training by pressing the Play button in the Unity Editor.
```

### Step 4: Press Play in Unity

1. Open Unity Editor
2. Load your training scene
3. **Press Play**
4. Unity will connect to the Python trainer
5. Training will begin!

### Step 5: Monitor Training

**In Terminal**: Watch training progress and metrics

**In TensorBoard**:
```powershell
# Open new terminal
cd "D:\Gamer to Developer\Code\noxus-ionia"
.\venv\Scripts\Activate.ps1
tensorboard --logdir results
```

Then open browser to `http://localhost:6006`

## Training Command Options

```powershell
mlagents-learn unity/NoxusIoniaRL/config/ppo_config.yaml \
  --run-id=test_run \
  --max-steps=1000000 \
  --force \
  --resume
```

**Options**:
- `--run-id`: Unique identifier for this training run
- `--max-steps`: Maximum training steps
- `--force`: Overwrite existing run
- `--resume`: Resume from checkpoint
- `--env`: Path to Unity executable (if not using Editor)

## Troubleshooting

### "Couldn't connect to trainer"
- Make sure `mlagents-learn` is running
- Make sure Unity Editor is in Play mode
- Check that behavior names match config file

### "mlagents-learn not found"
- Activate venv first: `.\venv\Scripts\Activate.ps1`
- Or use: `python -m mlagents.trainers.learn`

### Python 3.11 Errors
- Try Solution 1 (downgrade cattrs) first
- Or use Solution 2 (Python 3.10)

## Quick Start Checklist

- [ ] Activate venv
- [ ] Fix Python compatibility (if needed)
- [ ] Run `mlagents-learn` command
- [ ] Wait for "Press Play" message
- [ ] Press Play in Unity
- [ ] Watch training in terminal
- [ ] Open TensorBoard for visualization

