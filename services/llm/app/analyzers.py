"""Post-game analysis using LLMs"""

from typing import List, Dict, Any, Optional
import json


class PostGameAnalyzer:
    """Generates natural-language post-game analysis"""

    def __init__(self):
        self.default_providers = {
            "openai": "gpt-3.5-turbo",
            "huggingface": "microsoft/DialoGPT-medium",
        }

    def analyze(
        self,
        events: List[Dict[str, Any]],
        provider: str = "openai",
        model: Optional[str] = None,
    ) -> str:
        """
        Analyze match events and generate summary.
        
        Args:
            events: List of event dictionaries
            provider: 'openai' or 'huggingface'
            model: Model name (optional)
        """
        # Extract key statistics
        stats = self._extract_statistics(events)

        # Build prompt
        prompt = self._build_analysis_prompt(stats, events)

        # Generate analysis
        if provider == "openai":
            return self._analyze_openai(prompt, model or self.default_providers["openai"])
        elif provider == "huggingface":
            return self._analyze_huggingface(prompt, model or self.default_providers["huggingface"])
        else:
            # Fallback to template-based analysis
            return self._template_analysis(stats)

    def _extract_statistics(self, events: List[Dict[str, Any]]) -> Dict[str, Any]:
        """Extract key statistics from events"""
        stats = {
            "total_events": len(events),
            "team_stats": {"Noxus": {}, "Ionia": {}},
            "key_events": [],
        }

        # Count events by team
        for event in events:
            team = event.get("team", "")
            event_type = event.get("event_type", "")

            if team in stats["team_stats"]:
                if event_type not in stats["team_stats"][team]:
                    stats["team_stats"][team][event_type] = 0
                stats["team_stats"][team][event_type] += 1

            # Track key events
            if event_type in ["episode_end", "death", "deposit"]:
                stats["key_events"].append(event)

        # Find winner
        for event in events:
            if event.get("event_type") == "episode_end":
                winner = event.get("data", {}).get("winner", "")
                stats["winner"] = winner
                stats["duration"] = event.get("data", {}).get("duration", 0)
                break

        return stats

    def _build_analysis_prompt(self, stats: Dict[str, Any], events: List[Dict[str, Any]]) -> str:
        """Build prompt for LLM"""
        prompt = f"""Analyze this Noxus vs Ionia match and provide a natural-language summary.

Match Statistics:
- Winner: {stats.get('winner', 'Unknown')}
- Duration: {stats.get('duration', 0):.1f} seconds
- Total Events: {stats['total_events']}

Team Statistics:
Noxus: {json.dumps(stats['team_stats'].get('Noxus', {}), indent=2)}
Ionia: {json.dumps(stats['team_stats'].get('Ionia', {}), indent=2)}

Key Events:
{json.dumps(stats['key_events'][:10], indent=2)}

Provide a concise analysis (2-3 paragraphs) covering:
1. Overall match outcome and key turning points
2. Team performance highlights (e.g., "Noxus lost skirmishes 12-15 due to split pushes")
3. Notable agent contributions or coordination patterns
"""
        return prompt

    def _analyze_openai(self, prompt: str, model: str) -> str:
        """Generate analysis using OpenAI API"""
        try:
            import openai

            response = openai.ChatCompletion.create(
                model=model,
                messages=[
                    {"role": "system", "content": "You are an expert game analyst specializing in team-based strategy games."},
                    {"role": "user", "content": prompt},
                ],
                max_tokens=500,
                temperature=0.7,
            )

            return response.choices[0].message.content.strip()

        except ImportError:
            return self._template_analysis_from_prompt(prompt)
        except Exception as e:
            print(f"OpenAI API error: {e}")
            return self._template_analysis_from_prompt(prompt)

    def _analyze_huggingface(self, prompt: str, model: str) -> str:
        """Generate analysis using HuggingFace Transformers"""
        try:
            from transformers import pipeline

            generator = pipeline("text-generation", model=model, max_length=500)
            result = generator(prompt, max_length=500, num_return_sequences=1)

            return result[0]["generated_text"].replace(prompt, "").strip()

        except ImportError:
            return self._template_analysis_from_prompt(prompt)
        except Exception as e:
            print(f"HuggingFace error: {e}")
            return self._template_analysis_from_prompt(prompt)

    def _template_analysis(self, stats: Dict[str, Any]) -> str:
        """Fallback template-based analysis"""
        winner = stats.get("winner", "Unknown")
        duration = stats.get("duration", 0)

        noxus_stats = stats["team_stats"].get("Noxus", {})
        ionia_stats = stats["team_stats"].get("Ionia", {})

        analysis = f"""Match Summary: {winner} won after {duration:.1f} seconds.

Team Performance:
- Noxus: {sum(noxus_stats.values())} total events
- Ionia: {sum(ionia_stats.values())} total events

Key Highlights:
- Deposits: Noxus {noxus_stats.get('deposit', 0)}, Ionia {ionia_stats.get('deposit', 0)}
- Eliminations: Noxus {noxus_stats.get('death', 0)} deaths, Ionia {ionia_stats.get('death', 0)} deaths
"""

        return analysis

    def _template_analysis_from_prompt(self, prompt: str) -> str:
        """Template analysis when LLM unavailable"""
        return "LLM analysis unavailable. Using template-based summary. Install openai or transformers package for full analysis."

