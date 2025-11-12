"""Pydantic models for LLM service"""

from pydantic import BaseModel, Field
from typing import List, Dict, Any, Optional


class AnalysisRequest(BaseModel):
    """Request for post-game analysis"""
    events: List[Dict[str, Any]] = Field(..., description="Match event logs")
    provider: Optional[str] = Field("openai", description="LLM provider: 'openai' or 'huggingface'")
    model: Optional[str] = Field(None, description="Model name (default based on provider)")


class StrategyRequest(BaseModel):
    """Request for strategy generation"""
    performance_data: Dict[str, Any] = Field(..., description="Agent/team performance metrics")
    recent_matches: Optional[List[Dict[str, Any]]] = Field(None, description="Recent match results")
    provider: Optional[str] = Field("openai", description="LLM provider")


class CommsRequest(BaseModel):
    """Request for agent communication"""
    agent_state: Dict[str, Any] = Field(..., description="Current agent state")
    intent: str = Field(..., description="Agent intent code")
    context: Optional[Dict[str, Any]] = Field(None, description="Additional context")
    provider: Optional[str] = Field("openai", description="LLM provider")

