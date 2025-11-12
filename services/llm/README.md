# LLM Service

FastAPI service for LLM-powered post-game analysis, strategy generation, and optional agent communication.

## Features

- Post-game analysis: Natural-language match summaries
- Strategy generation: JSON playbooks with roles, priorities, tactics
- Agent communication: Compact intent messages (optional, feature-flagged)

## LLM Providers

- OpenAI (GPT-3.5/GPT-4)
- HuggingFace Transformers
- Template-based fallback (no LLM required)

## Installation

```bash
cd services/llm
pip install -r requirements.txt

# Optional: Install OpenAI or HuggingFace
pip install openai  # For OpenAI API
# or
pip install transformers torch  # For HuggingFace
```

## Usage

### Run Service

```bash
uvicorn app.main:app --reload --port 8002

# With LLM comms enabled
ENABLE_LLM_COMMS=true uvicorn app.main:app --reload --port 8002
```

### API Endpoints

- `POST /analyze`: Generate post-game analysis
- `POST /strategy`: Generate strategy playbook
- `POST /comms`: Generate agent communication (requires ENABLE_LLM_COMMS=true)

### Example

```python
import requests

# Post-game analysis
response = requests.post(
    "http://localhost:8002/analyze",
    json={
        "events": [...],  # Your event data
        "provider": "openai",
    },
)
print(response.json())

# Strategy generation
response = requests.post(
    "http://localhost:8002/strategy",
    json={
        "performance_data": {...},
        "recent_matches": [...],
    },
)
print(response.json())
```

## Configuration

Set environment variables:
- `ENABLE_LLM_COMMS`: Enable agent communication endpoint (default: false)
- `OPENAI_API_KEY`: OpenAI API key (if using OpenAI)

## Docker

```bash
docker build -t noxus-ionia-llm .
docker run -p 8002:8002 -e ENABLE_LLM_COMMS=true noxus-ionia-llm
```

