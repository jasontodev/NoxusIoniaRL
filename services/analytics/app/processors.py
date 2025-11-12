"""Event log processing and aggregation"""

from typing import List, Dict, Any, Optional
import pandas as pd
from collections import defaultdict
import json


class EventLogProcessor:
    """Processes and aggregates event logs"""

    def __init__(self):
        self.event_types = [
            "heal",
            "assist",
            "proximity",
            "pass_mana",
            "follow",
            "joint_attack",
            "block",
            "ping",
            "deposit",
            "pickup",
            "attack",
            "death",
            "episode_end",
        ]

    def process_events(
        self,
        events: List[Dict[str, Any]],
        aggregate_by: str = "agent_pair",
    ) -> Dict[str, Any]:
        """
        Process events and aggregate by agent pairs or time windows.
        
        Args:
            events: List of event dictionaries
            aggregate_by: 'agent_pair', 'time_window', or 'event_type'
        """
        df = pd.DataFrame(events)

        if aggregate_by == "agent_pair":
            return self._aggregate_by_agent_pair(df)
        elif aggregate_by == "time_window":
            return self._aggregate_by_time_window(df)
        elif aggregate_by == "event_type":
            return self._aggregate_by_event_type(df)
        else:
            return {"raw_events": events}

    def _aggregate_by_agent_pair(self, df: pd.DataFrame) -> Dict[str, Any]:
        """Aggregate events by agent pairs"""
        interactions = defaultdict(lambda: defaultdict(int))

        for _, row in df.iterrows():
            agent_id = row.get("agent_id", "")
            event_type = row.get("event_type", "")

            # Extract target agent if present
            target = row.get("target", row.get("nearby_agents", []))
            if isinstance(target, list):
                for target_agent in target:
                    if target_agent and target_agent != agent_id:
                        pair = tuple(sorted([agent_id, target_agent]))
                        interactions[pair][event_type] += 1
            elif target and target != agent_id:
                pair = tuple(sorted([agent_id, target]))
                interactions[pair][event_type] += 1

        # Convert to list format
        aggregated = []
        for pair, event_counts in interactions.items():
            aggregated.append({
                "agent_1": pair[0],
                "agent_2": pair[1],
                "interactions": dict(event_counts),
                "total_interactions": sum(event_counts.values()),
            })

        return {
            "aggregation_type": "agent_pair",
            "pairs": aggregated,
        }

    def _aggregate_by_time_window(
        self,
        df: pd.DataFrame,
        window_size: int = 100,
    ) -> Dict[str, Any]:
        """Aggregate events by time windows"""
        if "tick" not in df.columns:
            return {"error": "No 'tick' column found"}

        df["window"] = df["tick"] // window_size
        windowed = df.groupby("window").agg({
            "event_type": "value_counts",
        }).to_dict()

        return {
            "aggregation_type": "time_window",
            "window_size": window_size,
            "data": windowed,
        }

    def _aggregate_by_event_type(self, df: pd.DataFrame) -> Dict[str, Any]:
        """Aggregate events by type"""
        event_counts = df["event_type"].value_counts().to_dict()

        return {
            "aggregation_type": "event_type",
            "counts": event_counts,
        }

    def load_from_file(self, file_path: str, format: str = "jsonl") -> List[Dict[str, Any]]:
        """Load events from file"""
        if format == "jsonl":
            events = []
            with open(file_path, "r") as f:
                for line in f:
                    if line.strip():
                        events.append(json.loads(line))
            return events
        elif format == "parquet":
            df = pd.read_parquet(file_path)
            return df.to_dict("records")
        else:
            raise ValueError(f"Unsupported format: {format}")

