# Dashboard

Streamlit dashboard for visualizing RL training metrics, SNA analysis, and match summaries.

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

