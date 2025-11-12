# Fixing ML-Agents with Python 3.11

## The Issue

ML-Agents 0.28.0 has compatibility issues with Python 3.11+ due to changes in the `typing` module. The error is:
```
TypeError: Invalid first argument to `register()`. typing.Dict[...] is not a class or union type.
```

## Solutions

### Option 1: Use Python 3.10 (Recommended)

ML-Agents 0.28.0 works best with Python 3.9-3.10:

1. **Install Python 3.10** from python.org
2. **Recreate venv with Python 3.10**:
   ```powershell
   cd "D:\Gamer to Developer\Code\noxus-ionia"
   Remove-Item -Recurse -Force venv  # Delete old venv
   python3.10 -m venv venv
   .\venv\Scripts\Activate.ps1
   pip install mlagents mlagents-envs
   pip install "protobuf<3.21"
   ```

### Option 2: Use Unity's Training Window (Easiest)

Since you're using Unity ML-Agents 2.0.2, you can use Unity's built-in training:

1. **In Unity Editor**: Window → ML-Agents → Training
2. **Configure settings** in the UI
3. **Click Start Training**
4. No Python command line needed!

This avoids all Python compatibility issues.

### Option 3: Upgrade ML-Agents (If Available)

Try installing a newer version that supports Python 3.11:

```powershell
.\venv\Scripts\Activate.ps1
pip install --upgrade mlagents mlagents-envs
```

However, ML-Agents 0.28.0 is the latest stable version, so this may not work.

### Option 4: Downgrade cattrs (Workaround)

This is a hacky workaround but might work:

```powershell
.\venv\Scripts\Activate.ps1
pip install "cattrs<1.6"
```

But this may break other dependencies.

## Recommended: Use Unity Training Window

For Unity ML-Agents 2.0.2, the **easiest solution** is to use Unity's built-in training window:

1. Open Unity Editor
2. Window → ML-Agents → Training
3. Select your config file: `unity/NoxusIoniaRL/config/ppo_config.yaml`
4. Click "Start Training"
5. Press Play in Unity

This completely bypasses Python command line issues!

