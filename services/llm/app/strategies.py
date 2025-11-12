"""Strategy generation using LLMs"""

from typing import Dict, Any, Optional, List
import json


class StrategyGenerator:
    """Generates strategy playbooks and agent communications"""

    def __init__(self):
        self.default_providers = {
            "openai": "gpt-3.5-turbo",
            "huggingface": "microsoft/DialoGPT-medium",
        }

    def generate(
        self,
        performance_data: Dict[str, Any],
        recent_matches: Optional[List[Dict[str, Any]]] = None,
        provider: str = "openai",
    ) -> Dict[str, Any]:
        """
        Generate JSON playbook with roles, priorities, and tactics.
        
        Args:
            performance_data: Agent/team performance metrics
            recent_matches: Recent match results
            provider: LLM provider
        """
        # Build prompt
        prompt = self._build_strategy_prompt(performance_data, recent_matches)

        # Generate strategy
        if provider == "openai":
            strategy_json = self._generate_openai(prompt, provider)
        elif provider == "huggingface":
            strategy_json = self._generate_huggingface(prompt, provider)
        else:
            strategy_json = self._template_strategy(performance_data)

        # Parse and validate
        try:
            if isinstance(strategy_json, str):
                strategy = json.loads(strategy_json)
            else:
                strategy = strategy_json
        except:
            strategy = self._template_strategy(performance_data)

        return strategy

    def _build_strategy_prompt(
        self,
        performance_data: Dict[str, Any],
        recent_matches: Optional[List[Dict[str, Any]]],
    ) -> str:
        """Build prompt for strategy generation"""
        prompt = f"""Generate a strategy playbook (JSON format) for the Noxus-Ionia game based on performance data.

Performance Data:
{json.dumps(performance_data, indent=2)}

Recent Matches:
{json.dumps(recent_matches or [], indent=2)}

Return a JSON object with:
{{
  "roles": {{
    "agent_0": "role_name",
    "agent_1": "role_name"
  }},
  "priorities": ["priority1", "priority2", "priority3"],
  "tactics": {{
    "early_game": "strategy description",
    "mid_game": "strategy description",
    "late_game": "strategy description"
  }},
  "team_coordination": "coordination strategy"
}}
"""
        return prompt

    def _generate_openai(self, prompt: str, provider: str) -> str:
        """Generate strategy using OpenAI"""
        try:
            import openai

            response = openai.ChatCompletion.create(
                model=self.default_providers["openai"],
                messages=[
                    {"role": "system", "content": "You are a strategic game coach. Return only valid JSON."},
                    {"role": "user", "content": prompt},
                ],
                max_tokens=500,
                temperature=0.5,
            )

            return response.choices[0].message.content.strip()

        except ImportError:
            return json.dumps(self._template_strategy({}))
        except Exception as e:
            print(f"OpenAI API error: {e}")
            return json.dumps(self._template_strategy({}))

    def _generate_huggingface(self, prompt: str, provider: str) -> str:
        """Generate strategy using HuggingFace"""
        try:
            from transformers import pipeline

            generator = pipeline("text-generation", model=self.default_providers["huggingface"])
            result = generator(prompt, max_length=500, num_return_sequences=1)

            return result[0]["generated_text"]

        except ImportError:
            return json.dumps(self._template_strategy({}))
        except Exception as e:
            print(f"HuggingFace error: {e}")
            return json.dumps(self._template_strategy({}))

    def _template_strategy(self, performance_data: Dict[str, Any]) -> Dict[str, Any]:
        """Fallback template strategy"""
        return {
            "roles": {
                "agent_0": "collector",
                "agent_1": "defender",
            },
            "priorities": [
                "Collect mana items",
                "Deposit in heal zone",
                "Defend teammates",
            ],
            "tactics": {
                "early_game": "Focus on mana collection",
                "mid_game": "Coordinate deposits",
                "late_game": "Defend lead or push for elimination",
            },
            "team_coordination": "Maintain proximity for support",
        }

    def generate_comm(
        self,
        agent_state: Dict[str, Any],
        intent: str,
        context: Optional[Dict[str, Any]] = None,
        provider: str = "openai",
    ) -> str:
        """
        Generate compact communication message for agent.
        
        Args:
            agent_state: Current agent state
            intent: Intent code
            context: Additional context
        """
        # Map intent codes to messages (can be enhanced with LLM)
        intent_map = {
            "0": "collecting_mana",
            "1": "depositing",
            "2": "attacking",
            "3": "defending",
            "4": "retreating",
        }

        # Simple template-based communication
        intent_name = intent_map.get(intent, "unknown")
        
        message = f"{intent_name}:{agent_state.get('position', 'unknown')}"

        # Optionally use LLM for more nuanced messages
        if provider in ["openai", "huggingface"]:
            try:
                prompt = f"Generate a compact agent communication message. Intent: {intent_name}, State: {json.dumps(agent_state)}"
                if provider == "openai":
                    return self._generate_openai(prompt, provider)[:50]  # Limit length
                else:
                    return self._generate_huggingface(prompt, provider)[:50]
            except:
                pass

        return message

