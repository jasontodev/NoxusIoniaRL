"""FastAPI service for analytics and SNA"""

from fastapi import FastAPI, HTTPException, UploadFile, File
from fastapi.responses import JSONResponse
from typing import Optional, List, Dict, Any
import pandas as pd
import json
from pathlib import Path

from .processors import EventLogProcessor
from .sna import SocialNetworkAnalyzer
from .stats import StatisticalAnalyzer

app = FastAPI(
    title="Noxus-Ionia Analytics Service",
    description="Social network analysis and statistical modeling for RL game events",
    version="0.1.0",
)

# Initialize analyzers
processor = EventLogProcessor()
sna_analyzer = SocialNetworkAnalyzer()
stats_analyzer = StatisticalAnalyzer()


@app.get("/")
async def root():
    """Health check endpoint"""
    return {"status": "ok", "service": "analytics"}


@app.post("/process-events")
async def process_events(
    file: UploadFile = File(...),
    format: str = "jsonl",
):
    """
    Process event log file and return aggregated data.
    Supports JSONL and Parquet formats.
    """
    try:
        # Read file
        if format == "jsonl":
            content = await file.read()
            lines = content.decode("utf-8").strip().split("\n")
            events = [json.loads(line) for line in lines if line]
        elif format == "parquet":
            df = pd.read_parquet(file.file)
            events = df.to_dict("records")
        else:
            raise HTTPException(status_code=400, detail=f"Unsupported format: {format}")

        # Process events
        processed = processor.process_events(events)
        
        return JSONResponse(content={
            "status": "success",
            "events_processed": len(events),
            "processed_data": processed,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/sna/analyze")
async def analyze_sna(
    events: List[Dict[str, Any]],
    window_size: Optional[int] = None,
):
    """
    Perform social network analysis on event data.
    
    Args:
        events: List of event dictionaries
        window_size: Optional time window for analysis
    """
    try:
        # Build network graph
        graph = sna_analyzer.build_graph(events, window_size=window_size)
        
        # Compute metrics
        metrics = sna_analyzer.compute_metrics(graph)
        
        # Community detection
        communities = sna_analyzer.detect_communities(graph)
        
        return JSONResponse(content={
            "status": "success",
            "nodes": len(graph.nodes()),
            "edges": len(graph.edges()),
            "metrics": metrics,
            "communities": communities,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/sna/centrality")
async def compute_centrality(
    events: List[Dict[str, Any]],
    metric: str = "all",
):
    """
    Compute centrality metrics for agents.
    
    Args:
        events: List of event dictionaries
        metric: 'degree', 'betweenness', 'closeness', 'all'
    """
    try:
        graph = sna_analyzer.build_graph(events)
        centrality = sna_analyzer.compute_centrality(graph, metric=metric)
        
        return JSONResponse(content={
            "status": "success",
            "centrality": centrality,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/stats/correlate")
async def correlate_metrics(
    events: List[Dict[str, Any]],
    target_metric: str = "win_rate",
):
    """
    Correlate SNA metrics with performance metrics.
    
    Args:
        events: List of event dictionaries
        target_metric: 'win_rate', 'episode_return', etc.
    """
    try:
        # Build graph and compute SNA metrics
        graph = sna_analyzer.build_graph(events)
        sna_metrics = sna_analyzer.compute_metrics(graph)
        
        # Correlate with performance
        correlations = stats_analyzer.correlate_with_performance(
            sna_metrics,
            events,
            target_metric=target_metric,
        )
        
        return JSONResponse(content={
            "status": "success",
            "correlations": correlations,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/stats/learning-curves")
async def analyze_learning_curves(
    events: List[Dict[str, Any]],
    metric: str = "episode_return",
):
    """
    Analyze learning curves and return distributions.
    """
    try:
        analysis = stats_analyzer.analyze_learning_curves(events, metric=metric)
        
        return JSONResponse(content={
            "status": "success",
            "analysis": analysis,
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001)

