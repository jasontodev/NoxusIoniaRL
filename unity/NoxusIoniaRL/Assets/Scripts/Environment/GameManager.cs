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
        public int winConditionMana = 20;

        [Header("Team Spawn Points")]
        public Transform[] noxusSpawnPoints;
        public Transform[] ioniaSpawnPoints;

        [Header("References")]
        public HealZone noxusHealZone;
        public HealZone ioniaHealZone;
        public GameObject agentPrefabNoxus;
        public GameObject agentPrefabIonia;
        public GameObject manaItemPrefab;
        public int manaItemsCount = 15;

        // Game state
        private float episodeStartTime;
        private bool episodeActive = false;
        private List<BaseAgent> noxusAgents = new List<BaseAgent>();
        private List<BaseAgent> ioniaAgents = new List<BaseAgent>();
        private List<GameObject> manaItems = new List<GameObject>();

        // Team scores
        private int noxusManaBanked = 0;
        private int ioniaManaBanked = 0;

        private EventLogger eventLogger;

        private void Start()
        {
            eventLogger = FindObjectOfType<EventLogger>();
            StartEpisode();
        }

        public void StartEpisode()
        {
            episodeStartTime = Time.time;
            episodeActive = true;
            noxusManaBanked = 0;
            ioniaManaBanked = 0;

            // Clear existing agents
            ClearAgents();

            // Spawn agents
            SpawnAgents();

            // Spawn mana items
            SpawnManaItems();

            // Reset heal zones
            if (noxusHealZone != null) noxusHealZone.Reset();
            if (ioniaHealZone != null) ioniaHealZone.Reset();
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

        private void SpawnManaItems()
        {
            // Clear existing mana
            foreach (var mana in manaItems)
            {
                if (mana != null) Destroy(mana);
            }
            manaItems.Clear();

            // Spawn mana items randomly in forest area
            // This assumes a forest area exists - adjust bounds as needed
            Bounds forestBounds = new Bounds(Vector3.zero, new Vector3(40f, 0f, 40f));
            
            for (int i = 0; i < manaItemsCount; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(forestBounds.min.x, forestBounds.max.x),
                    0.5f,
                    Random.Range(forestBounds.min.z, forestBounds.max.z)
                );

                if (manaItemPrefab != null)
                {
                    var manaObj = Instantiate(manaItemPrefab, randomPos, Quaternion.identity);
                    manaItems.Add(manaObj);
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

        private void Update()
        {
            if (!episodeActive) return;

            // Check win conditions
            CheckWinConditions();

            // Check timeout
            if (Time.time - episodeStartTime > episodeMaxTime)
            {
                EndEpisodeByTimeout();
            }
        }

        private void CheckWinConditions()
        {
            // Condition 1: Eliminate all enemies
            bool noxusEliminated = noxusAgents.Count == 0 || noxusAgents.All(a => a == null);
            bool ioniaEliminated = ioniaAgents.Count == 0 || ioniaAgents.All(a => a == null);

            if (noxusEliminated)
            {
                EndEpisode(winner: BaseAgent.TeamType.Ionia);
                return;
            }
            if (ioniaEliminated)
            {
                EndEpisode(winner: BaseAgent.TeamType.Noxus);
                return;
            }

            // Condition 2: More mana banked
            if (noxusManaBanked >= winConditionMana && noxusManaBanked > ioniaManaBanked)
            {
                EndEpisode(winner: BaseAgent.TeamType.Noxus);
                return;
            }
            if (ioniaManaBanked >= winConditionMana && ioniaManaBanked > noxusManaBanked)
            {
                EndEpisode(winner: BaseAgent.TeamType.Ionia);
                return;
            }
        }

        private void EndEpisode(BaseAgent.TeamType? winner = null)
        {
            episodeActive = false;

            if (winner.HasValue)
            {
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
                    { "noxus_mana", noxusManaBanked },
                    { "ionia_mana", ioniaManaBanked },
                    { "duration", Time.time - episodeStartTime }
                });
            }

            // Reset after short delay
            Invoke(nameof(StartEpisode), 1f);
        }

        private void EndEpisodeByTimeout()
        {
            // Determine winner by mana count
            BaseAgent.TeamType? winner = null;
            if (noxusManaBanked > ioniaManaBanked)
                winner = BaseAgent.TeamType.Noxus;
            else if (ioniaManaBanked > noxusManaBanked)
                winner = BaseAgent.TeamType.Ionia;

            EndEpisode(winner);
        }

        public void OnAgentDeath(BaseAgent.TeamType team)
        {
            // Remove dead agent from list
            if (team == BaseAgent.TeamType.Noxus)
            {
                noxusAgents.RemoveAll(a => a == null);
            }
            else
            {
                ioniaAgents.RemoveAll(a => a == null);
            }
        }

        public bool DepositMana(BaseAgent.TeamType team, int amount)
        {
            if (team == BaseAgent.TeamType.Noxus)
            {
                noxusManaBanked += amount;
                return true;
            }
            else if (team == BaseAgent.TeamType.Ionia)
            {
                ioniaManaBanked += amount;
                return true;
            }
            return false;
        }

        public int GetTeamManaBanked(BaseAgent.TeamType team)
        {
            return team == BaseAgent.TeamType.Noxus ? noxusManaBanked : ioniaManaBanked;
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

        public void DropMana(Vector3 position, int amount)
        {
            // Spawn dropped mana items
            for (int i = 0; i < amount; i++)
            {
                Vector3 offset = Random.insideUnitCircle * 2f;
                Vector3 dropPos = position + new Vector3(offset.x, 0.5f, offset.y);
                
                if (manaItemPrefab != null)
                {
                    var manaObj = Instantiate(manaItemPrefab, dropPos, Quaternion.identity);
                    manaItems.Add(manaObj);
                }
            }
        }
    }
}

