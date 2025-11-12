"""Learning curve visualization components"""

import streamlit as st
import pandas as pd
import plotly.express as px
import plotly.graph_objects as go
from typing import Optional
import boto3
from pathlib import Path
from io import BytesIO
import json


def render_learning_curves(
    s3_client: Optional[boto3.client],
    s3_bucket: str,
    s3_prefix: str,
):
    """Render learning curves"""
    
    # Load TensorBoard data or CSV
    data_source = st.radio("Data Source", ["Local CSV", "S3", "Upload"])
    
    if data_source == "Upload":
        uploaded_file = st.file_uploader("Upload metrics CSV", type=["csv"])
        if uploaded_file:
            df = pd.read_csv(uploaded_file)
        else:
            st.info("Upload a CSV file with columns: step, return, win_rate")
            return
    elif data_source == "Local CSV":
        csv_path = st.text_input("CSV Path", value="data/logs/metrics.csv")
        try:
            df = pd.read_csv(csv_path)
        except:
            st.error(f"Could not load {csv_path}")
            return
    else:  # S3
        if not s3_client:
            st.error("S3 client not configured")
            return
        s3_key = st.text_input("S3 Key", value=f"{s3_prefix}/logs/metrics.csv")
        try:
            obj = s3_client.get_object(Bucket=s3_bucket, Key=s3_key)
            df = pd.read_csv(BytesIO(obj['Body'].read()))
        except Exception as e:
            st.error(f"Could not load from S3: {e}")
            return
    
    # Plot learning curves
    if "step" in df.columns and "return" in df.columns:
        st.subheader("Episode Returns")
        fig = px.line(df, x="step", y="return", title="Learning Curve: Episode Returns")
        st.plotly_chart(fig, use_container_width=True)
    
    if "step" in df.columns and "win_rate" in df.columns:
        st.subheader("Win Rate")
        fig = px.line(df, x="step", y="win_rate", title="Win Rate Over Time")
        st.plotly_chart(fig, use_container_width=True)
    
    # Statistics
    st.subheader("Statistics")
    if "return" in df.columns:
        col1, col2, col3, col4 = st.columns(4)
        with col1:
            st.metric("Mean Return", f"{df['return'].mean():.2f}")
        with col2:
            st.metric("Std Return", f"{df['return'].std():.2f}")
        with col3:
            st.metric("Min Return", f"{df['return'].min():.2f}")
        with col4:
            st.metric("Max Return", f"{df['return'].max():.2f}")
    
    # Return distribution
    if "return" in df.columns:
        st.subheader("Return Distribution")
        fig = px.histogram(df, x="return", nbins=50, title="Distribution of Episode Returns")
        st.plotly_chart(fig, use_container_width=True)

