using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NoxusIoniaRL.Agents;
using NoxusIoniaRL.Events;

namespace NoxusIoniaRL.Environment
{
    /// <summary>
    /// Manages game state, win conditions, team scoring, and episode management.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Configuration")]
        public int agentsPerTeam = 2;
        public float episodeMaxTime = 300f; // 5 minutes
        
        [Header("Time Control")]
        [Tooltip("Time scale for episode playback. 1.0 = normal speed, 0.5 = half speed, 2.0 = double speed")]
        [Range(0.1f, 2.0f)]
        public float timeScale = 1.0f;

        [Header("Team Spawn Points")]
        public Transform[] noxusSpawnPoints;
        public Transform[] ioniaSpawnPoints;

        [Header("References")]
        public HealZone noxusHealZone;
        public HealZone ioniaHealZone;
        public GameObject agentPrefabNoxus;
        public GameObject agentPrefabIonia;
        
        [Header("Debug")]
        public bool debugWinConditions = false; // Enable debug logging for win conditions

        // Game state
        private float episodeStartTime;
        private bool episodeActive = false;
        private List<BaseAgent> noxusAgents = new List<BaseAgent>();
        private List<BaseAgent> ioniaAgents = new List<BaseAgent>();

        private EventLogger eventLogger;

        private void Start()
        {
            eventLogger = FindObjectOfType<EventLogger>();
            Time.timeScale = timeScale; // Set initial time scale
            StartEpisode();
        }
        
        private void Update()
        {
            // Update time scale if changed in inspector during runtime
            if (Time.timeScale != timeScale)
            {
                Time.timeScale = timeScale;
            }
            
            if (!episodeActive) return;

            // Check win conditions
            CheckWinConditions();

            // Check timeout
            if (Time.time - episodeStartTime > episodeMaxTime)
            {
                EndEpisodeByTimeout();
            }
        }

        public void StartEpisode()
        {
            episodeStartTime = Time.time;
            episodeActive = true;

            // Clear existing agents
            ClearAgents();

            // Reset obstacles to their starting positions
            ResetObstacles();

            // Spawn agents
            SpawnAgents();

            // Reset heal zones
            if (noxusHealZone != null) noxusHealZone.Reset();
            if (ioniaHealZone != null) ioniaHealZone.Reset();
            
            // Small delay before allowing win condition checks to prevent immediate wins
            // This ensures agents are fully spawned and initialized
        }

        private void SpawnAgents()
        {
            // Spawn Noxus agents
            for (int i = 0; i < agentsPerTeam && i < noxusSpawnPoints.Length; i++)
            {
                if (agentPrefabNoxus != null && noxusSpawnPoints[i] != null)
                {
                    var agentObj = Instantiate(agentPrefabNoxus, noxusSpawnPoints[i].position, noxusSpawnPoints[i].rotation);
                    var agent = agentObj.GetComponent<BaseAgent>();
                    if (agent != null)
                    {
                        agent.agentId = i;
                        noxusAgents.Add(agent);
                    }
                }
            }

            // Spawn Ionia agents
            for (int i = 0; i < agentsPerTeam && i < ioniaSpawnPoints.Length; i++)
            {
                if (agentPrefabIonia != null && ioniaSpawnPoints[i] != null)
                {
                    var agentObj = Instantiate(agentPrefabIonia, ioniaSpawnPoints[i].position, ioniaSpawnPoints[i].rotation);
                    var agent = agentObj.GetComponent<BaseAgent>();
                    if (agent != null)
                    {
                        agent.agentId = i;
                        ioniaAgents.Add(agent);
                    }
                }
            }
        }


        private void ClearAgents()
        {
            foreach (var agent in noxusAgents)
            {
                if (agent != null) Destroy(agent.gameObject);
            }
            foreach (var agent in ioniaAgents)
            {
                if (agent != null) Destroy(agent.gameObject);
            }
            noxusAgents.Clear();
            ioniaAgents.Clear();
        }

        private void ResetObstacles()
        {
            // Find all obstacles in the scene and reset them to starting positions
            Obstacle[] allObstacles = FindObjectsOfType<Obstacle>();
            foreach (var obstacle in allObstacles)
            {
                if (obstacle != null)
                {
                    obstacle.ResetPosition();
                }
            }
        }


        public void CheckWinConditions()
        {
            // Only check elimination-based wins
            
            // Prevent checking win conditions immediately after episode start
            // Give agents time to spawn and initialize (at least 0.5 seconds)
            float timeSinceStart = Time.time - episodeStartTime;
            if (timeSinceStart < 0.5f)
            {
                return; // Too early, don't check win conditions yet
            }
            
            // Ensure we have agents spawned before checking elimination
            // If no agents are spawned yet, don't check (shouldn't happen, but safety check)
            if (noxusAgents.Count == 0 && ioniaAgents.Count == 0)
            {
                return; // Agents not spawned yet, wait
            }
            
            // Check elimination status - team is eliminated if all agents are dead
            // Use a helper method to check if agents are dead (since they're not removed from list)
            bool noxusEliminated = noxusAgents.Count > 0 && noxusAgents.All(a => a == null || IsAgentDead(a));
            bool ioniaEliminated = ioniaAgents.Count > 0 && ioniaAgents.All(a => a == null || IsAgentDead(a));

            // Win Condition: Elimination
            // Noxus wins if all Ionia are eliminated
            if (ioniaEliminated && ioniaAgents.Count > 0)
            {
                Debug.Log($"[WIN CONDITION] Noxus wins - All Ionia eliminated!");
                EndEpisode(winner: BaseAgent.TeamType.Noxus);
                return;
            }
            // Ionia wins if all Noxus are eliminated OR if they survive until timeout
            if (noxusEliminated && noxusAgents.Count > 0)
            {
                Debug.Log($"[WIN CONDITION] Ionia wins - All Noxus eliminated!");
                EndEpisode(winner: BaseAgent.TeamType.Ionia);
                return;
            }
        }

        private void EndEpisode(BaseAgent.TeamType? winner = null)
        {
            episodeActive = false;

            if (winner.HasValue)
            {
                // Log win/loss to console
                float duration = Time.time - episodeStartTime;
                string winReason = "";
                
                // Determine win reason
                bool noxusEliminated = noxusAgents.Count == 0 || noxusAgents.All(a => a == null || IsAgentDead(a));
                bool ioniaEliminated = ioniaAgents.Count == 0 || ioniaAgents.All(a => a == null || IsAgentDead(a));
                
                if (winner.Value == BaseAgent.TeamType.Noxus)
                {
                    winReason = "Ionia eliminated";
                    Debug.Log($"[EPISODE END] ⚔️ NOXUS WINS! ⚔️\n" +
                             $"Reason: {winReason}\n" +
                             $"Duration: {duration:F2}s");
                }
                else
                {
                    // Check if Noxus was eliminated (already checked in outer scope, but need to check again)
                    bool noxusEliminatedCheck = noxusAgents.Count > 0 && noxusAgents.All(a => a == null || IsAgentDead(a));
                    if (noxusEliminatedCheck)
                        winReason = "Noxus eliminated";
                    else
                        winReason = "Survived until timeout";
                    
                    Debug.Log($"[EPISODE END] ⚔️ IONIA WINS! ⚔️\n" +
                             $"Reason: {winReason}\n" +
                             $"Duration: {duration:F2}s");
                }
                
                // Reward winners and losers
                if (winner.Value == BaseAgent.TeamType.Noxus)
                {
                    foreach (var agent in noxusAgents)
                    {
                        if (agent != null) agent.OnWin();
                    }
                    foreach (var agent in ioniaAgents)
                    {
                        if (agent != null) agent.OnLoss();
                    }
                }
                else
                {
                    foreach (var agent in ioniaAgents)
                    {
                        if (agent != null) agent.OnWin();
                    }
                    foreach (var agent in noxusAgents)
                    {
                        if (agent != null) agent.OnLoss();
                    }
                }

                eventLogger?.LogEvent("episode_end", -1, winner.Value, new Dictionary<string, object>
                {
                    { "winner", winner.Value.ToString() },
                    { "duration", Time.time - episodeStartTime }
                });
            }
            else
            {
                // This should never happen with the new win conditions, but log it if it does
                Debug.LogWarning($"[EPISODE END] ⚠️ NO WINNER DETERMINED (This shouldn't happen!)\n" +
                                 $"Duration: {Time.time - episodeStartTime:F2}s");
            }

            // Reset after short delay
            Invoke(nameof(StartEpisode), 1f);
        }

        private void EndEpisodeByTimeout()
        {
            // On timeout, determine winner by elimination
            // Check elimination first
            bool noxusEliminated = noxusAgents.Count == 0 || noxusAgents.All(a => a == null || IsAgentDead(a));
            bool ioniaEliminated = ioniaAgents.Count == 0 || ioniaAgents.All(a => a == null || IsAgentDead(a));

            BaseAgent.TeamType winner;

            // Elimination takes priority
            if (ioniaEliminated)
            {
                winner = BaseAgent.TeamType.Noxus;
            }
            else if (noxusEliminated)
            {
                winner = BaseAgent.TeamType.Ionia;
            }
            else
            {
                // If no one is eliminated at timeout, Ionia wins (survival victory)
                winner = BaseAgent.TeamType.Ionia;
            }

            EndEpisode(winner);
        }

        public void OnAgentDeath(BaseAgent.TeamType team)
        {
            // Remove null agents from list (agents stay in list but are marked as dead)
            if (team == BaseAgent.TeamType.Noxus)
            {
                noxusAgents.RemoveAll(a => a == null);
            }
            else
            {
                ioniaAgents.RemoveAll(a => a == null);
            }
        }
        
        // Helper method to check if an agent is dead
        private bool IsAgentDead(BaseAgent agent)
        {
            if (agent == null) return true;
            return agent.IsDead();
        }


        public float GetTimeRemaining()
        {
            return Mathf.Max(0f, episodeMaxTime - (Time.time - episodeStartTime));
        }

        public HealZone GetHealZone(BaseAgent.TeamType team)
        {
            return team == BaseAgent.TeamType.Noxus ? noxusHealZone : ioniaHealZone;
        }

        public void ResetAgent(BaseAgent agent)
        {
            // Reset agent to spawn point
            if (agent.teamType == BaseAgent.TeamType.Noxus)
            {
                int idx = noxusAgents.IndexOf(agent);
                if (idx >= 0 && idx < noxusSpawnPoints.Length && noxusSpawnPoints[idx] != null)
                {
                    agent.transform.position = noxusSpawnPoints[idx].position;
                    agent.transform.rotation = noxusSpawnPoints[idx].rotation;
                }
            }
            else
            {
                int idx = ioniaAgents.IndexOf(agent);
                if (idx >= 0 && idx < ioniaSpawnPoints.Length && ioniaSpawnPoints[idx] != null)
                {
                    agent.transform.position = ioniaSpawnPoints[idx].position;
                    agent.transform.rotation = ioniaSpawnPoints[idx].rotation;
                }
            }
        }

    }
}

