"""Social Network Analysis using NetworkX"""

import networkx as nx
from typing import List, Dict, Any, Optional
from collections import defaultdict
import numpy as np


class SocialNetworkAnalyzer:
    """Performs social network analysis on agent interactions"""

    def __init__(self):
        self.event_weights = {
            "heal": 2.0,
            "assist": 1.5,
            "joint_attack": 2.0,
            "pass_mana": 1.0,
            "follow": 0.5,
            "proximity": 0.3,
            "block": -0.5,
            "ping": 0.2,
        }

    def build_graph(
        self,
        events: List[Dict[str, Any]],
        window_size: Optional[int] = None,
    ) -> nx.Graph:
        """
        Build NetworkX graph from events.
        
        Args:
            events: List of event dictionaries
            window_size: Optional time window for temporal analysis
        """
        G = nx.Graph()

        # Aggregate interactions
        interactions = defaultdict(lambda: defaultdict(float))

        for event in events:
            agent_id = event.get("agent_id", "")
            event_type = event.get("event_type", "")
            tick = event.get("tick", 0)

            # Skip if windowed and outside window
            if window_size and tick > window_size:
                continue

            weight = self.event_weights.get(event_type, 1.0)

            # Extract target agents
            target = event.get("target")
            nearby_agents = event.get("nearby_agents", [])

            if target:
                if isinstance(target, (list, tuple)):
                    for target_agent in target:
                        if target_agent and target_agent != agent_id:
                            pair = tuple(sorted([agent_id, target_agent]))
                            interactions[pair][event_type] += weight
                else:
                    if target != agent_id:
                        pair = tuple(sorted([agent_id, target]))
                        interactions[pair][event_type] += weight

            if nearby_agents:
                for nearby_agent in nearby_agents:
                    if nearby_agent and nearby_agent != agent_id:
                        pair = tuple(sorted([agent_id, nearby_agent]))
                        interactions[pair]["proximity"] += weight

        # Add nodes and edges
        for (agent1, agent2), event_weights in interactions.items():
            total_weight = sum(event_weights.values())
            
            if not G.has_node(agent1):
                G.add_node(agent1)
            if not G.has_node(agent2):
                G.add_node(agent2)
            
            if G.has_edge(agent1, agent2):
                G[agent1][agent2]["weight"] += total_weight
                G[agent1][agent2]["events"].update(event_weights)
            else:
                G.add_edge(agent1, agent2, weight=total_weight, events=event_weights)

        return G

    def compute_metrics(self, graph: nx.Graph) -> Dict[str, Any]:
        """Compute SNA metrics for the graph"""
        if len(graph.nodes()) == 0:
            return {}

        metrics = {}

        # Degree centrality
        degree_centrality = nx.degree_centrality(graph)
        metrics["degree_centrality"] = degree_centrality

        # Betweenness centrality
        betweenness_centrality = nx.betweenness_centrality(graph, weight="weight")
        metrics["betweenness_centrality"] = betweenness_centrality

        # Closeness centrality
        try:
            closeness_centrality = nx.closeness_centrality(graph, distance="weight")
            metrics["closeness_centrality"] = closeness_centrality
        except:
            # Graph might not be connected
            metrics["closeness_centrality"] = {}

        # Clustering coefficient
        clustering = nx.clustering(graph, weight="weight")
        metrics["clustering"] = clustering
        metrics["average_clustering"] = nx.average_clustering(graph, weight="weight")

        # Assortativity (homophily)
        try:
            assortativity = nx.assortativity.degree_assortativity_coefficient(graph)
            metrics["assortativity"] = assortativity
        except:
            metrics["assortativity"] = None

        # Density
        metrics["density"] = nx.density(graph)

        # Average path length (if connected)
        if nx.is_connected(graph):
            metrics["average_path_length"] = nx.average_shortest_path_length(graph, weight="weight")
        else:
            metrics["average_path_length"] = None

        return metrics

    def compute_centrality(
        self,
        graph: nx.Graph,
        metric: str = "all",
    ) -> Dict[str, Dict[str, float]]:
        """
        Compute centrality metrics.
        
        Args:
            graph: NetworkX graph
            metric: 'degree', 'betweenness', 'closeness', or 'all'
        """
        centrality = {}

        if metric in ["degree", "all"]:
            centrality["degree"] = nx.degree_centrality(graph)

        if metric in ["betweenness", "all"]:
            centrality["betweenness"] = nx.betweenness_centrality(graph, weight="weight")

        if metric in ["closeness", "all"]:
            try:
                centrality["closeness"] = nx.closeness_centrality(graph, distance="weight")
            except:
                centrality["closeness"] = {}

        return centrality

    def detect_communities(self, graph: nx.Graph) -> Dict[str, Any]:
        """Detect communities using Louvain algorithm"""
        try:
            import community.community_louvain as community_louvain
            
            partition = community_louvain.best_partition(graph, weight="weight")
            
            # Group nodes by community
            communities = defaultdict(list)
            for node, comm_id in partition.items():
                communities[comm_id].append(node)

            # Compute modularity
            modularity = community_louvain.modularity(partition, graph, weight="weight")

            return {
                "communities": dict(communities),
                "modularity": modularity,
                "num_communities": len(communities),
            }
        except ImportError:
            # Fallback to simple connected components
            communities = list(nx.connected_components(graph))
            return {
                "communities": {i: list(comm) for i, comm in enumerate(communities)},
                "modularity": None,
                "num_communities": len(communities),
            }

