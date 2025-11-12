# Analytics Service

FastAPI service for social network analysis and statistical modeling of RL game events.

## Features

- Event log processing (JSONL, Parquet)
- Social network analysis (NetworkX)
  - Centrality metrics (degree, betweenness, closeness)
  - Clustering coefficients
  - Community detection (Louvain)
  - Assortativity
- Statistical analysis
  - Correlation with performance metrics
  - Learning curve analysis
  - Confidence intervals

## Installation

```bash
cd services/analytics
pip install -r requirements.txt
```

## Usage

### Run Service

```bash
uvicorn app.main:app --reload --port 8001
```

### API Endpoints

- `POST /process-events`: Process event log file
- `POST /sna/analyze`: Perform SNA analysis
- `POST /sna/centrality`: Compute centrality metrics
- `POST /stats/correlate`: Correlate SNA metrics with performance
- `POST /stats/learning-curves`: Analyze learning curves

### Example

```python
import requests

# Process events
with open("events.jsonl", "rb") as f:
    response = requests.post(
        "http://localhost:8001/process-events",
        files={"file": f},
        data={"format": "jsonl"},
    )
    print(response.json())

# SNA analysis
events = [...]  # Your event data
response = requests.post(
    "http://localhost:8001/sna/analyze",
    json=events,
)
print(response.json())
```

## Docker

```bash
docker build -t noxus-ionia-analytics .
docker run -p 8001:8001 noxus-ionia-analytics
```

