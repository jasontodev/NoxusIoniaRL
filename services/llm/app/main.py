"""FastAPI service for LLM-powered analysis and strategy"""

from fastapi import FastAPI, HTTPException, Body
from fastapi.responses import JSONResponse
from typing import List, Dict, Any, Optional
import os

from .analyzers import PostGameAnalyzer
from .strategies import StrategyGenerator
from .models import AnalysisRequest, StrategyRequest, CommsRequest

app = FastAPI(
    title="Noxus-Ionia LLM Service",
    description="LLM-powered post-game analysis, strategy generation, and agent communication",
    version="0.1.0",
)

# Initialize analyzers
analyzer = PostGameAnalyzer()
strategy_gen = StrategyGenerator()

# Feature flags
ENABLE_COMMS = os.getenv("ENABLE_LLM_COMMS", "false").lower() == "true"


@app.get("/")
async def root():
    """Health check endpoint"""
    return {
        "status": "ok",
        "service": "llm",
        "comms_enabled": ENABLE_COMMS,
    }


@app.post("/analyze")
async def analyze_match(
    request: AnalysisRequest = Body(...),
):
    """
    Generate natural-language post-game analysis from match logs.
    
    Args:
        request: AnalysisRequest with events and optional LLM provider
    """
    try:
        analysis = analyzer.analyze(
            events=request.events,
            provider=request.provider,
            model=request.model,
        )
        
        return JSONResponse(content={
            "status": "success",
            "analysis": analysis,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/strategy")
async def generate_strategy(
    request: StrategyRequest = Body(...),
):
    """
    Generate JSON playbook (roles, priorities, tactics) based on performance.
    
    Args:
        request: StrategyRequest with performance data
    """
    try:
        playbook = strategy_gen.generate(
            performance_data=request.performance_data,
            recent_matches=request.recent_matches,
            provider=request.provider,
        )
        
        return JSONResponse(content={
            "status": "success",
            "playbook": playbook,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/comms")
async def generate_communication(
    request: CommsRequest = Body(...),
):
    """
    Generate compact agent communication messages (optional, feature-flagged).
    
    Args:
        request: CommsRequest with agent state and intent
    """
    if not ENABLE_COMMS:
        raise HTTPException(
            status_code=403,
            detail="LLM communication is disabled. Set ENABLE_LLM_COMMS=true to enable.",
        )

    try:
        message = strategy_gen.generate_comm(
            agent_state=request.agent_state,
            intent=request.intent,
            context=request.context,
            provider=request.provider,
        )
        
        return JSONResponse(content={
            "status": "success",
            "message": message,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8002)

