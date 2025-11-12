# Noxus vs Ionia RL Game

A Unity-based reinforcement learning game featuring Noxus and Ionia teams competing in a strategic battle environment. Agents learn through ML-Agents (PyTorch backend) with comprehensive analytics, LLM integration, and AWS deployment infrastructure.

## Project Structure

```
noxus-ionia/
├── unity/              # Unity ML-Agents game environment
├── rl/                 # Python RL training package
├── services/           # FastAPI services (LLM, Analytics)
├── infra/              # AWS infrastructure scripts
├── dash/               # Dashboard for visualization
├── data/               # Logs, checkpoints, artifacts
└── notebooks/          # Analysis notebooks
```

## Features

- **Reinforcement Learning**: Unity ML-Agents with PPO/SAC algorithms
- **Social Network Analysis**: Team coordination patterns and centrality metrics
- **LLM Integration**: Post-game analysis, strategy generation, optional agent communication
- **AWS Deployment**: EC2 training runs, S3 artifact storage
- **Visualization**: TensorBoard, Streamlit dashboard, SNA graphs

## Quick Start

### Prerequisites

- Unity 2022.3+ LTS
- Python 3.9+
- PyTorch 2.0+
- AWS CLI (for cloud deployment)

### Local Development

1. **Unity Setup**:
   - Open Unity project in `unity/NoxusIoniaRL/`
   - Install ML-Agents package via Package Manager

2. **Python Environment**:
   ```bash
   cd rl
   pip install -r requirements.txt
   pip install -e .
   ```

3. **Train Locally**:
   ```bash
   python scripts/train.py --config config/ppo_config.yaml
   ```

4. **Run Services**:
   ```bash
   # Analytics service
   cd services/analytics
   uvicorn app.main:app --reload

   # LLM service
   cd services/llm
   uvicorn app.main:app --reload
   ```

5. **Launch Dashboard**:
   ```bash
   cd dash
   streamlit run app.py
   ```

### AWS Deployment

See `infra/aws/README.md` for EC2 setup and S3 configuration.

## Documentation

- [RL Training Guide](rl/README.md)
- [Analytics Service](services/analytics/README.md)
- [LLM Service](services/llm/README.md)
- [Infrastructure Setup](infra/README.md)

## License

MIT

