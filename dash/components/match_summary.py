"""Match summary components"""

import streamlit as st
import json
import requests
import pandas as pd
from typing import Optional
import boto3
from io import BytesIO


def render_match_summary(
    s3_client: Optional[boto3.client],
    s3_bucket: str,
    s3_prefix: str,
):
    """Render match summary"""
    
    # File upload
    uploaded_file = st.file_uploader("Upload match log (JSONL)", type=["jsonl"])
    
    if uploaded_file is not None:
        # Load events
        events = []
        for line in uploaded_file:
            if line.strip():
                events.append(json.loads(line))
        
        st.success(f"Loaded {len(events)} events")
        
        # Get LLM analysis
        llm_url = st.text_input("LLM Service URL", value="http://localhost:8002")
        
        if st.button("Generate Analysis"):
            try:
                response = requests.post(
                    f"{llm_url}/analyze",
                    json={
                        "events": events,
                        "provider": "openai",
                    },
                    timeout=30,
                )
                
                if response.status_code == 200:
                    result = response.json()
                    st.subheader("LLM Analysis")
                    st.write(result.get("analysis", "No analysis generated"))
                else:
                    st.error(f"LLM service error: {response.status_code}")
            except Exception as e:
                st.error(f"Failed to connect to LLM service: {e}")
        
        # Event timeline
        st.subheader("Event Timeline")
        
        # Filter key events
        key_events = [e for e in events if e.get("event_type") in ["episode_end", "death", "deposit", "attack"]]
        
        event_df = pd.DataFrame(key_events[:100])  # Limit to 100 events
        if not event_df.empty:
            st.dataframe(event_df[["tick", "agent_id", "event_type", "team"]], use_container_width=True)
    
    else:
        st.info("Upload a match log file to view summary")

