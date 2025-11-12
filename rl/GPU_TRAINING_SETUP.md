# GPU Training Setup with Python 3.9.13

## Your Setup

- **Environment**: `mlagents39` (Python 3.9.13)
- **ML-Agents**: Installed
- **GPU**: CUDA 11.8
- **Hardware**: GPU training enabled

This is an **excellent setup**! Python 3.9 is well-supported by ML-Agents, and GPU training will be much faster than CPU.

## Activating Your Environment

```powershell
conda activate mlagents39
```

Or if using venv:
```powershell
# Navigate to where mlagents39 is located
# Then activate it
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

.\mlagents39\Scripts\Activate.ps1
```

## Verify GPU Setup

Check that PyTorch can see your GPU:

```powershell
python -c "import torch; print(f'CUDA available: {torch.cuda.is_available()}'); print(f'CUDA version: {torch.version.cuda}'); print(f'GPU: {torch.cuda.get_device_name(0) if torch.cuda.is_available() else \"None\"}')"
```

Should show:
- `CUDA available: True`
- `CUDA version: 11.8`
- Your GPU name

## Starting Training with GPU

### Step 1: Activate Environment

```powershell
conda activate mlagents39
```

### Step 2: Navigate to Project

```powershell
cd "D:\Gamer to Developer\Code\noxus-ionia"
```

### Step 3: Start Training

```powershell
mlagents-learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run --force
```

**Important Notes:**
- Use `--force` flag to overwrite previous runs with the same run ID
- Or use a new `--run-id` each time (e.g., `--run-id=run_001`, `--run-id=run_002`)
- ML-Agents will automatically use GPU if:
  - PyTorch was installed with CUDA support
  - GPU is available
  - No special flags needed - it detects automatically!

### Step 4: Verify GPU Usage

During training, you should see in the output that it's using GPU. You can also monitor GPU usage with:

```powershell
# In another terminal
nvidia-smi
```

## Training Command Options

```powershell
mlagents-learn unity/NoxusIoniaRL/config/ppo_config.yaml \
  --run-id=test_run \
  --max-steps=1000000 \
  --force
```

**GPU will be used automatically** - no special flags needed!

## Monitor Training

### Terminal Output
Watch training progress in the terminal where `mlagents-learn` is running.

### TensorBoard

**Start TensorBoard:**
```powershell
# In another terminal (same environment)
conda activate mlagents39
cd "D:\Gamer to Developer\Code\noxus-ionia"
tensorboard --logdir results
```

**Open browser to:** `http://localhost:6006`

**What to Look For in TensorBoard:**

1. **Learning Curves** (Main Tab):
   - `Cumulative Reward` - Should increase over time (agents learning)
   - `Policy Loss` - Should decrease (policy improving)
   - `Value Loss` - Should decrease (value function learning)
   - `Entropy` - Should decrease (less random actions over time)
   - `Learning Rate` - Shows learning rate schedule

2. **Episode Statistics**:
   - `Episode Length` - Average episode duration
   - `Episode Reward` - Per-episode rewards
   - `Value Estimate` - What the agent thinks the state is worth

3. **Behavior-Specific Metrics**:
   - Look for `NoxusAgent` and `IoniaAgent` tabs
   - Each behavior has its own learning curves
   - Compare how each team is learning

4. **Training Progress**:
   - `Steps` - Total training steps completed
   - `Time Elapsed` - How long training has been running
   - `Environment Interactions` - Total agent actions taken

**Tips:**
- Use the smoothing slider to reduce noise in graphs
- Compare multiple runs by selecting different run folders
- Watch for plateaus (flat lines) - may need to adjust hyperparameters
- Check if rewards are increasing - if not, may need reward tuning

### GPU Monitoring
```powershell
# Watch GPU usage
nvidia-smi -l 1  # Updates every second
```

## Training Workflow

1. **Activate environment**: `conda activate mlagents39` (or activate your venv)
2. **Start training**: `mlagents-learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run --force`
3. **Wait for**: "Start training by pressing the Play button in the Unity Editor"
4. **Press Play in Unity** - Training will begin automatically
5. **Monitor**: Terminal + TensorBoard + nvidia-smi

## Stopping Training

### Method 1: Stop in Unity
- **Press Stop** in Unity Editor (stops the environment)
- Training will pause and wait for Unity to reconnect

### Method 2: Stop in Terminal
- Press `Ctrl+C` in the terminal running `mlagents-learn`
- This will gracefully stop training and save the current checkpoint
- **Note**: Press once and wait - it may take a moment to save

### Method 3: Force Stop
- Press `Ctrl+C` twice quickly (force interrupt)
- May not save the latest checkpoint

**Best Practice**: Let an episode finish, then press `Ctrl+C` once to stop gracefully.

## GPU Performance Tips

- **Batch Size**: GPU allows larger batch sizes. You can increase `batch_size` in config for faster training
- **Multiple Environments**: GPU can handle multiple parallel environments efficiently
- **Memory**: Monitor GPU memory with `nvidia-smi` - adjust batch size if you run out

## Troubleshooting

### "Previous data from this run ID was found"
- Use `--force` flag: `mlagents-learn ... --run-id=test_run --force`
- Or use a new run ID: `--run-id=run_001`

### "CUDA not available"
- Check PyTorch was installed with CUDA: `pip show torch`
- Reinstall PyTorch with CUDA: `pip install torch torchvision --index-url https://download.pytorch.org/whl/cu118`

### "Out of memory"
- Reduce `batch_size` in config
- Reduce `buffer_size` in config
- Close other GPU applications

### Training seems slow
- Verify GPU is being used (check nvidia-smi)
- Increase `batch_size` to utilize GPU better
- Check GPU utilization with `nvidia-smi`

### "FileNotFoundError: The demonstration file or directory None does not exist"
- This was fixed by removing `behavioral_cloning` section from config
- If you see this, check your config file doesn't have `behavioral_cloning` with `demo_path: null`

### "Sampler configuration does not contain sampler_parameters"
- Fixed by nesting `min_value` and `max_value` under `sampler_parameters` in `environment_parameters`
- Check config format matches the updated structure

## Expected Performance

With GPU training:
- **Much faster** than CPU (often 5-10x speedup)
- Can handle larger batch sizes
- Better for long training runs
- More efficient for multiple agents

## Next Steps

Once training starts:
1. Let it run and collect data
2. Monitor TensorBoard for learning curves
3. Event logs will be generated for SNA/LLM analysis
4. Checkpoints will be saved for resuming
5. Sync to S3 when ready

Your setup is perfect for production training!

