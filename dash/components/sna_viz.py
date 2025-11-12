"""SNA visualization components"""

import streamlit as st
import pandas as pd
import networkx as nx
import plotly.graph_objects as go
import json
from typing import Optional
import boto3
from io import BytesIO


def render_sna_visualization(
    s3_client: Optional[boto3.client],
    s3_bucket: str,
    s3_prefix: str,
):
    """Render SNA visualization"""
    
    # File upload
    uploaded_file = st.file_uploader("Upload event log (JSONL)", type=["jsonl"])
    
    if uploaded_file is not None:
        # Load events
        events = []
        for line in uploaded_file:
            if line.strip():
                events.append(json.loads(line))
        
        st.success(f"Loaded {len(events)} events")
        
        # Build graph
        from services.analytics.app.sna import SocialNetworkAnalyzer
        
        sna = SocialNetworkAnalyzer()
        graph = sna.build_graph(events)
        
        # Compute metrics
        metrics = sna.compute_metrics(graph)
        
        # Display metrics
        st.subheader("Centrality Metrics")
        
        centrality_df = pd.DataFrame({
            "Agent": list(metrics["degree_centrality"].keys()),
            "Degree": [f"{v:.3f}" for v in metrics["degree_centrality"].values()],
            "Betweenness": [f"{v:.3f}" for v in metrics["betweenness_centrality"].values()],
            "Closeness": [f"{v:.3f}" for v in metrics.get("closeness_centrality", {}).values()] if "closeness_centrality" in metrics else ["N/A"] * len(metrics["degree_centrality"]),
        })
        
        st.dataframe(centrality_df, use_container_width=True)
        
        # Network visualization
        st.subheader("Network Graph")
        
        # Use Plotly for interactive graph
        pos = nx.spring_layout(graph, k=1, iterations=50)
        
        edge_x = []
        edge_y = []
        for edge in graph.edges():
            x0, y0 = pos[edge[0]]
            x1, y1 = pos[edge[1]]
            edge_x.extend([x0, x1, None])
            edge_y.extend([y0, y1, None])
        
        edge_trace = go.Scatter(
            x=edge_x, y=edge_y,
            line=dict(width=0.5, color='#888'),
            hoverinfo='none',
            mode='lines'
        )
        
        node_x = []
        node_y = []
        node_text = []
        node_size = []
        for node in graph.nodes():
            x, y = pos[node]
            node_x.append(x)
            node_y.append(y)
            node_text.append(node)
            node_size.append(metrics["degree_centrality"][node] * 20)
        
        node_trace = go.Scatter(
            x=node_x, y=node_y,
            mode='markers+text',
            hoverinfo='text',
            text=node_text,
            textposition="middle center",
            marker=dict(
                size=node_size,
                color=node_size,
                colorscale='Viridis',
                showscale=True,
            )
        )
        
        fig = go.Figure(data=[edge_trace, node_trace],
                       layout=go.Layout(
                           title='Agent Interaction Network',
                           showlegend=False,
                           hovermode='closest',
                           margin=dict(b=20,l=5,r=5,t=40),
                           annotations=[ dict(
                               text="Node size = Degree centrality",
                               showarrow=False,
                               xref="paper", yref="paper",
                               x=0.005, y=-0.002,
                               xanchor="left", yanchor="bottom",
                           )],
                           xaxis=dict(showgrid=False, zeroline=False, showticklabels=False),
                           yaxis=dict(showgrid=False, zeroline=False, showticklabels=False)))
        
        st.plotly_chart(fig, use_container_width=True)
        
        # Community detection
        st.subheader("Community Detection")
        communities = sna.detect_communities(graph)
        st.write(f"Number of communities: {communities['num_communities']}")
        st.write(f"Modularity: {communities.get('modularity', 'N/A')}")
        
        # Display communities
        for comm_id, nodes in communities["communities"].items():
            st.write(f"**Community {comm_id}**: {', '.join(nodes)}")
    
    else:
        st.info("Upload an event log file to visualize SNA")

