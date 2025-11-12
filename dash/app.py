"""Streamlit dashboard for Noxus-Ionia RL game"""

import streamlit as st
import pandas as pd
import json
import boto3
from pathlib import Path
import sys
from typing import Optional

# Add parent directories to path
sys.path.insert(0, str(Path(__file__).parent.parent / "services" / "analytics" / "app"))
sys.path.insert(0, str(Path(__file__).parent.parent / "services" / "llm" / "app"))

from components.sna_viz import render_sna_visualization
from components.learning_curves import render_learning_curves
from components.match_summary import render_match_summary

# Page config
st.set_page_config(
    page_title="Noxus-Ionia RL Dashboard",
    page_icon="⚔️",
    layout="wide",
)

# Initialize S3 client (if configured)
S3_BUCKET = st.sidebar.text_input("S3 Bucket", value="")
S3_PREFIX = st.sidebar.text_input("S3 Prefix", value="training-runs")

s3_client = None
if S3_BUCKET:
    try:
        s3_client = boto3.client("s3")
    except:
        st.sidebar.warning("AWS credentials not configured")

# Sidebar
st.sidebar.title("⚔️ Noxus-Ionia RL Dashboard")
st.sidebar.markdown("---")

page = st.sidebar.selectbox(
    "Navigation",
    ["Overview", "Learning Curves", "SNA Analysis", "Match Summary", "Settings"],
)

# Main content
if page == "Overview":
    st.title("Noxus-Ionia RL Training Dashboard")
    st.markdown("---")

    col1, col2, col3, col4 = st.columns(4)

    # Load metrics (placeholder)
    with col1:
        st.metric("Total Episodes", "1,234")
    with col2:
        st.metric("Win Rate (Noxus)", "52%")
    with col3:
        st.metric("Win Rate (Ionia)", "48%")
    with col4:
        st.metric("Avg Episode Return", "125.5")

    st.markdown("---")
    st.subheader("Recent Training Runs")
    
    # Placeholder table
    runs_df = pd.DataFrame({
        "Run ID": ["run_001", "run_002", "run_003"],
        "Status": ["Completed", "Running", "Completed"],
        "Episodes": [1000, 500, 2000],
        "Best Return": [150.2, 120.5, 180.3],
    })
    st.dataframe(runs_df, use_container_width=True)

elif page == "Learning Curves":
    st.title("Learning Curves")
    render_learning_curves(s3_client, S3_BUCKET, S3_PREFIX)

elif page == "SNA Analysis":
    st.title("Social Network Analysis")
    render_sna_visualization(s3_client, S3_BUCKET, S3_PREFIX)

elif page == "Match Summary":
    st.title("Match Summary")
    render_match_summary(s3_client, S3_BUCKET, S3_PREFIX)

elif page == "Settings":
    st.title("Settings")
    
    st.subheader("Data Sources")
    data_source = st.radio(
        "Data Source",
        ["Local Files", "S3"],
        help="Choose where to load data from",
    )
    
    if data_source == "Local Files":
        data_dir = st.text_input("Data Directory", value="data")
        runs_dir = st.text_input("Runs Directory", value="runs")
    else:
        st.text_input("S3 Bucket", value=S3_BUCKET)
        st.text_input("S3 Prefix", value=S3_PREFIX)
    
    st.subheader("Services")
    analytics_url = st.text_input("Analytics Service URL", value="http://localhost:8001")
    llm_url = st.text_input("LLM Service URL", value="http://localhost:8002")
    
    if st.button("Save Settings"):
        st.success("Settings saved!")

if __name__ == "__main__":
    pass

