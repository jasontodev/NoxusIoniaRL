# Dashboard

Streamlit dashboard for visualizing RL training metrics, SNA analysis, and match summaries.

## ⚠️ Current Status

**The dashboard is NOT currently connected to ML-Agents training data.**

Currently, the dashboard shows placeholder data. To connect it to real training data:

1. **For TensorBoard metrics**: The dashboard needs to read from the `results/` directory where ML-Agents saves TensorBoard logs
2. **For SNA analysis**: The dashboard needs to read event logs from Unity (JSONL/Parquet files)
3. **For match summaries**: The dashboard needs to connect to the LLM service

**For now, use TensorBoard directly** to view ML-Agents training metrics:
```powershell
tensorboard --logdir results
```

## Features

- Overview: Training statistics and recent runs
- Learning Curves: Episode returns, win rates, distributions
- SNA Analysis: Network graphs, centrality metrics, community detection
- Match Summary: LLM-generated analysis, event timeline

## Installation

```bash
cd dash
pip install -r requirements.txt
```

## Usage

```bash
streamlit run app.py
```

Access at http://localhost:8501

## Docker

```bash
docker build -t noxus-ionia-dashboard .
docker run -p 8501:8501 noxus-ionia-dashboard
```

## Configuration

Set environment variables or use sidebar settings:
- S3 Bucket: For loading data from S3
- Analytics Service URL: For SNA analysis
- LLM Service URL: For match summaries

