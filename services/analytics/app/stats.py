"""Statistical modeling and analysis"""

from typing import List, Dict, Any, Optional
import numpy as np
import pandas as pd
from scipy import stats
from collections import defaultdict


class StatisticalAnalyzer:
    """Statistical analysis and modeling"""

    def correlate_with_performance(
        self,
        sna_metrics: Dict[str, Any],
        events: List[Dict[str, Any]],
        target_metric: str = "win_rate",
    ) -> Dict[str, Any]:
        """
        Correlate SNA metrics with performance metrics.
        
        Args:
            sna_metrics: SNA metrics dictionary
            events: Event list
            target_metric: 'win_rate', 'episode_return', etc.
        """
        # Extract agent performance
        agent_performance = self._extract_agent_performance(events, target_metric)

        # Extract SNA metrics per agent
        correlations = {}
        
        for metric_name, metric_values in sna_metrics.items():
            if isinstance(metric_values, dict):
                # Match agents and compute correlation
                agents = list(set(metric_values.keys()) & set(agent_performance.keys()))
                if len(agents) >= 2:
                    sna_values = [metric_values.get(agent, 0) for agent in agents]
                    perf_values = [agent_performance.get(agent, 0) for agent in agents]
                    
                    if len(sna_values) > 1 and np.std(sna_values) > 0 and np.std(perf_values) > 0:
                        corr, p_value = stats.pearsonr(sna_values, perf_values)
                        correlations[metric_name] = {
                            "correlation": float(corr),
                            "p_value": float(p_value),
                            "n": len(agents),
                        }

        return correlations

    def _extract_agent_performance(
        self,
        events: List[Dict[str, Any]],
        metric: str,
    ) -> Dict[str, float]:
        """Extract performance metrics per agent"""
        agent_stats = defaultdict(lambda: {"wins": 0, "losses": 0, "returns": []})

        # Process episode_end events
        for event in events:
            if event.get("event_type") == "episode_end":
                winner = event.get("data", {}).get("winner", "")
                # Extract agent IDs from winner team
                # This is simplified - adjust based on actual event structure
                pass

        # Compute win rates
        performance = {}
        for agent_id, stats in agent_stats.items():
            total = stats["wins"] + stats["losses"]
            if total > 0:
                if metric == "win_rate":
                    performance[agent_id] = stats["wins"] / total
                elif metric == "episode_return":
                    performance[agent_id] = np.mean(stats["returns"]) if stats["returns"] else 0.0

        return performance

    def analyze_learning_curves(
        self,
        events: List[Dict[str, Any]],
        metric: str = "episode_return",
    ) -> Dict[str, Any]:
        """
        Analyze learning curves and return distributions.
        
        Args:
            events: Event list
            metric: Metric to analyze
        """
        df = pd.DataFrame(events)

        # Extract episode data
        episode_data = []
        current_episode = None

        for _, row in df.iterrows():
            if row.get("event_type") == "episode_end":
                episode_data.append({
                    "episode": row.get("tick", 0) // 1000,  # Approximate episode number
                    "return": row.get("data", {}).get("return", 0),
                    "duration": row.get("data", {}).get("duration", 0),
                })

        if not episode_data:
            return {"error": "No episode data found"}

        episode_df = pd.DataFrame(episode_data)

        # Compute statistics
        analysis = {
            "num_episodes": len(episode_df),
            "mean_return": float(episode_df["return"].mean()),
            "std_return": float(episode_df["return"].std()),
            "min_return": float(episode_df["return"].min()),
            "max_return": float(episode_df["return"].max()),
        }

        # Learning curve (rolling average)
        if len(episode_df) > 10:
            window_size = min(100, len(episode_df) // 10)
            episode_df["rolling_mean"] = episode_df["return"].rolling(window=window_size).mean()
            analysis["learning_curve"] = episode_df[["episode", "rolling_mean"]].to_dict("records")

        # Confidence intervals
        if len(episode_df) > 1:
            ci = stats.t.interval(
                0.95,
                len(episode_df) - 1,
                loc=episode_df["return"].mean(),
                scale=stats.sem(episode_df["return"]),
            )
            analysis["confidence_interval_95"] = [float(ci[0]), float(ci[1])]

        return analysis

    def compute_action_entropy(
        self,
        events: List[Dict[str, Any]],
    ) -> Dict[str, Any]:
        """Compute action entropy over time"""
        action_counts = defaultdict(lambda: defaultdict(int))

        for event in events:
            agent_id = event.get("agent_id", "")
            action = event.get("data", {}).get("action")
            if action is not None:
                action_counts[agent_id][action] += 1

        entropies = {}
        for agent_id, counts in action_counts.items():
            total = sum(counts.values())
            if total > 0:
                probs = [c / total for c in counts.values()]
                entropy = -sum(p * np.log2(p) for p in probs if p > 0)
                entropies[agent_id] = float(entropy)

        return {
            "action_entropy": entropies,
            "mean_entropy": float(np.mean(list(entropies.values()))) if entropies else 0.0,
        }

