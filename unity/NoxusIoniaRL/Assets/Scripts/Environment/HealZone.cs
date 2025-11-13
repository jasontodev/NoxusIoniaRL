using UnityEngine;
using System.Collections.Generic;
using NoxusIoniaRL.Agents;

namespace NoxusIoniaRL.Environment
{
    /// <summary>
    /// Team-specific heal zone where agents can heal.
    /// </summary>
    public class HealZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        public BaseAgent.TeamType teamType;
        public float healRate = 10f; // HP per second
        public float zoneRadius = 5f;

        private GameManager gameManager;
        private SphereCollider zoneCollider;
        
        [Header("Debug")]
        public bool debugHealing = false; // Set to true to see healing debug logs

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            zoneCollider = GetComponent<SphereCollider>();
            
            // Ensure collider is set up correctly
            if (zoneCollider == null)
            {
                zoneCollider = gameObject.AddComponent<SphereCollider>();
                if (debugHealing)
                {
                    Debug.Log($"[HEAL ZONE] {teamType} - Added SphereCollider component");
                }
            }
            zoneCollider.isTrigger = true;
            zoneCollider.radius = zoneRadius;
            
            if (debugHealing)
            {
                Debug.Log($"[HEAL ZONE] {teamType} - Initialized at position {transform.position}, radius: {zoneRadius}, healRate: {healRate} HP/s");
                Debug.Log($"[HEAL ZONE] {teamType} - Collider isTrigger: {zoneCollider.isTrigger}, radius: {zoneCollider.radius}");
            }
        }

        private void FixedUpdate()
        {
            // Check all agents and heal those in range (more reliable than OnTriggerStay)
            if (gameManager == null)
            {
                if (debugHealing && Time.frameCount % 100 == 0) // Log every 100 frames to avoid spam
                {
                    Debug.LogWarning($"[HEAL ZONE] {teamType} - GameManager is null!");
                }
                return;
            }
            
            // Find all agents in the scene
            BaseAgent[] allAgents = FindObjectsOfType<BaseAgent>();
            
            foreach (var agent in allAgents)
            {
                if (agent == null)
                {
                    continue;
                }
                
                if (agent.IsDead())
                {
                    continue;
                }
                
                if (agent.teamType != teamType)
                {
                    // Skip agents from other teams (expected behavior, no need to log)
                    continue;
                }
                
                // Check if agent is within heal zone radius
                float distance = Vector3.Distance(transform.position, agent.transform.position);
                
                if (distance <= zoneRadius)
                {
                    // Heal agent while in zone (healRate is HP per second)
                    float healAmount = healRate * Time.fixedDeltaTime;
                    agent.Heal(healAmount);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            // Keep this as backup, but FixedUpdate is more reliable
            var agent = other.GetComponent<BaseAgent>();
            if (agent != null && agent.teamType == teamType && !agent.IsDead())
            {
                // Heal agent while in zone (healRate is HP per second)
                float healAmount = healRate * Time.fixedDeltaTime;
                agent.Heal(healAmount);
            }
        }

        public void Reset()
        {
            // Reset heal zone (no mana to clear)
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = teamType == BaseAgent.TeamType.Noxus ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(transform.position, zoneRadius);
        }
    }
}

