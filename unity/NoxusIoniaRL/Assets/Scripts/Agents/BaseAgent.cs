using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Linq;
using NoxusIoniaRL.Events;
using NoxusIoniaRL.Environment;

namespace NoxusIoniaRL.Agents
{
    /// <summary>
    /// Base agent class for Noxus and Ionia teams.
    /// Handles observation collection, action execution, and reward computation.
    /// </summary>
    public class BaseAgent : Agent
    {
        [Header("Agent Configuration")]
        public TeamType teamType;
        public int agentId;
        public float moveSpeed = 5f;
        public float rotationSpeed = 180f;
        public float interactionRange = 2f;
        public float attackRange = 3f;
        public float attackCooldown = 1f;
        public int maxHealth = 100;
        public int maxManaCarried = 3;

        [Header("Observation Settings")]
        public int kNearestEntities = 5;
        public float observationRadius = 20f;

        [Header("Reward Weights")]
        public float rewardDeposit = 1.0f;
        public float rewardPickup = 0.1f;
        public float rewardElimination = 2.0f;
        public float rewardDeath = -1.0f;
        public float rewardWin = 10.0f;
        public float rewardLoss = -5.0f;
        public float rewardIdle = -0.01f;
        public float rewardFriendlyBlock = -0.1f;
        public float rewardManaShaping = 0.05f;

        // Agent state
        private int currentHealth;
        private int manaCarried;
        private float lastAttackTime;
        private Vector3 lastPosition;
        private float idleTime;
        private Rigidbody rb;
        private GameManager gameManager;
        private EventLogger eventLogger;

        // Observation cache
        private List<GameObject> nearbyEntities = new List<GameObject>();
        private List<GameObject> nearbyMana = new List<GameObject>();
        private List<GameObject> nearbyObstacles = new List<GameObject>();

        public enum TeamType
        {
            Noxus,
            Ionia
        }

        public override void Initialize()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            gameManager = FindObjectOfType<GameManager>();
            eventLogger = FindObjectOfType<EventLogger>();

            currentHealth = maxHealth;
            manaCarried = 0;
            lastPosition = transform.position;
            idleTime = 0f;
        }

        public override void OnEpisodeBegin()
        {
            currentHealth = maxHealth;
            manaCarried = 0;
            lastAttackTime = 0f;
            lastPosition = transform.position;
            idleTime = 0f;

            // Reset position (handled by GameManager)
            if (gameManager != null)
            {
                gameManager.ResetAgent(this);
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Self state (8 features)
            sensor.AddObservation(currentHealth / (float)maxHealth); // Normalized health
            sensor.AddObservation(manaCarried / (float)maxManaCarried); // Normalized mana
            sensor.AddObservation((Time.time - lastAttackTime) / attackCooldown); // Attack cooldown
            sensor.AddObservation(teamType == TeamType.Noxus ? 1f : 0f); // Team ID
            sensor.AddObservation(transform.position.x / 50f); // Normalized position X
            sensor.AddObservation(transform.position.z / 50f); // Normalized position Z
            sensor.AddObservation(transform.rotation.eulerAngles.y / 360f); // Normalized rotation
            sensor.AddObservation(rb.velocity.magnitude / moveSpeed); // Normalized velocity

            // Find nearby entities
            UpdateNearbyEntities();

            // k-NN entities (teammates and opponents)
            var sortedEntities = nearbyEntities
                .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
                .Take(kNearestEntities)
                .ToList();

            // Pad to kNearestEntities
            for (int i = 0; i < kNearestEntities; i++)
            {
                if (i < sortedEntities.Count)
                {
                    var entity = sortedEntities[i];
                    var distance = Vector3.Distance(transform.position, entity.transform.position);
                    var agent = entity.GetComponent<BaseAgent>();
                    
                    sensor.AddObservation(distance / observationRadius); // Normalized distance
                    sensor.AddObservation(agent != null && agent.teamType == teamType ? 1f : 0f); // Is teammate
                    sensor.AddObservation(agent != null ? agent.currentHealth / (float)agent.maxHealth : 0f); // Health
                    sensor.AddObservation(agent != null && agent.manaCarried > 0 ? 1f : 0f); // Carrying mana
                }
                else
                {
                    sensor.AddObservation(1f); // Max distance (no entity)
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                }
            }

            // Nearest mana items
            UpdateNearbyMana();
            var nearestMana = nearbyMana
                .OrderBy(m => Vector3.Distance(transform.position, m.transform.position))
                .Take(3)
                .ToList();

            for (int i = 0; i < 3; i++)
            {
                if (i < nearestMana.Count)
                {
                    var mana = nearestMana[i];
                    var distance = Vector3.Distance(transform.position, mana.transform.position);
                    var direction = (mana.transform.position - transform.position).normalized;
                    
                    sensor.AddObservation(distance / observationRadius);
                    sensor.AddObservation(direction.x);
                    sensor.AddObservation(direction.z);
                }
                else
                {
                    sensor.AddObservation(1f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                }
            }

            // Nearest obstacles
            UpdateNearbyObstacles();
            var nearestObstacles = nearbyObstacles
                .OrderBy(o => Vector3.Distance(transform.position, o.transform.position))
                .Take(3)
                .ToList();

            for (int i = 0; i < 3; i++)
            {
                if (i < nearestObstacles.Count)
                {
                    var obstacle = nearestObstacles[i];
                    var distance = Vector3.Distance(transform.position, obstacle.transform.position);
                    var rb = obstacle.GetComponent<Rigidbody>();
                    var velocity = rb != null ? rb.velocity.magnitude : 0f;
                    
                    sensor.AddObservation(distance / observationRadius);
                    sensor.AddObservation(velocity / 5f); // Normalized velocity
                }
                else
                {
                    sensor.AddObservation(1f);
                    sensor.AddObservation(0f);
                }
            }

            // Distance to home heal zone and enemy heal zone
            if (gameManager != null)
            {
                var homeZone = gameManager.GetHealZone(teamType);
                var enemyZone = gameManager.GetHealZone(teamType == TeamType.Noxus ? TeamType.Ionia : TeamType.Noxus);

                if (homeZone != null)
                {
                    var homeDist = Vector3.Distance(transform.position, homeZone.transform.position);
                    sensor.AddObservation(homeDist / 50f);
                }
                else
                {
                    sensor.AddObservation(1f);
                }

                if (enemyZone != null)
                {
                    var enemyDist = Vector3.Distance(transform.position, enemyZone.transform.position);
                    sensor.AddObservation(enemyDist / 50f);
                }
                else
                {
                    sensor.AddObservation(1f);
                }
            }
            else
            {
                sensor.AddObservation(1f);
                sensor.AddObservation(1f);
            }

            // Global summary (team-level features)
            if (gameManager != null)
            {
                sensor.AddObservation(gameManager.GetTeamManaBanked(teamType) / 20f); // Normalized
                sensor.AddObservation(gameManager.GetTeamManaBanked(teamType == TeamType.Noxus ? TeamType.Ionia : TeamType.Noxus) / 20f);
                sensor.AddObservation(gameManager.GetTimeRemaining() / 300f); // Normalized (5 min max)
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // Continuous actions: movement and rotation
            var moveX = actions.ContinuousActions[0];
            var moveZ = actions.ContinuousActions[1];
            var rotate = actions.ContinuousActions[2];

            // Move agent
            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
            rb.velocity = moveDirection * moveSpeed;

            // Rotate agent
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }

            // Discrete actions
            int actionType = actions.DiscreteActions[0]; // 0: none, 1: interact, 2: attack, 3: defend, 4: signal

            switch (actionType)
            {
                case 1: // Interact (pickup/drop/push)
                    HandleInteract();
                    break;
                case 2: // Attack
                    HandleAttack();
                    break;
                case 3: // Defend/Block
                    HandleDefend();
                    break;
                case 4: // Signal intent
                    HandleSignal(actions.DiscreteActions[1]); // Intent code
                    break;
            }

            // Reward shaping and penalties
            UpdateRewards();
        }

        private void HandleInteract()
        {
            // Try to pickup mana
            if (manaCarried < maxManaCarried)
            {
                var nearestMana = FindNearestMana();
                if (nearestMana != null)
                {
                    var manaItem = nearestMana.GetComponent<ManaItem>();
                    if (manaItem != null && manaItem.Pickup(this))
                    {
                        manaCarried++;
                        AddReward(rewardPickup);
                        eventLogger?.LogEvent("pickup", agentId, teamType, new Dictionary<string, object> { { "mana_id", nearestMana.GetInstanceID() } });
                    }
                }
            }

            // Try to deposit mana at heal zone
            if (manaCarried > 0)
            {
                var healZone = gameManager?.GetHealZone(teamType);
                if (healZone != null && Vector3.Distance(transform.position, healZone.transform.position) < interactionRange)
                {
                    int amountToDeposit = manaCarried;
                    if (healZone.DepositMana(this, amountToDeposit))
                    {
                        var depositReward = rewardDeposit * amountToDeposit;
                        AddReward(depositReward);
                        eventLogger?.LogEvent("deposit", agentId, teamType, new Dictionary<string, object> { { "amount", amountToDeposit } });
                        manaCarried = 0;
                    }
                }
            }

            // Try to push obstacle
            var nearestObstacle = FindNearestObstacle();
            if (nearestObstacle != null && Vector3.Distance(transform.position, nearestObstacle.transform.position) < interactionRange)
            {
                var obstacle = nearestObstacle.GetComponent<Obstacle>();
                if (obstacle != null)
                {
                    obstacle.Push(rb.velocity.normalized);
                }
            }
        }

        private void HandleAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
                return;

            var nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null && Vector3.Distance(transform.position, nearestEnemy.transform.position) < attackRange)
            {
                var enemyAgent = nearestEnemy.GetComponent<BaseAgent>();
                if (enemyAgent != null)
                {
                    int damage = 25;
                    enemyAgent.TakeDamage(damage, this);
                    lastAttackTime = Time.time;
                    eventLogger?.LogEvent("attack", agentId, teamType, new Dictionary<string, object> { { "target", enemyAgent.agentId }, { "damage", damage } });
                }
            }
        }

        private void HandleDefend()
        {
            // Reduce incoming damage temporarily
            // Implementation can add defense buff
        }

        private void HandleSignal(int intentCode)
        {
            eventLogger?.LogEvent("ping", agentId, teamType, new Dictionary<string, object> { { "intent", intentCode } });
        }

        private void UpdateRewards()
        {
            // Idle penalty
            if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
            {
                idleTime += Time.fixedDeltaTime;
                if (idleTime > 2f)
                {
                    AddReward(rewardIdle * Time.fixedDeltaTime);
                }
            }
            else
            {
                idleTime = 0f;
            }

            lastPosition = transform.position;

            // Mana shaping reward (moving toward home zone with mana)
            if (manaCarried > 0 && gameManager != null)
            {
                var homeZone = gameManager.GetHealZone(teamType);
                if (homeZone != null)
                {
                    var distToHome = Vector3.Distance(transform.position, homeZone.transform.position);
                    var prevDistToHome = Vector3.Distance(lastPosition, homeZone.transform.position);
                    if (distToHome < prevDistToHome)
                    {
                        AddReward(rewardManaShaping * Time.fixedDeltaTime);
                    }
                }
            }
        }

        public void TakeDamage(int damage, BaseAgent attacker)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die(attacker);
            }
        }

        private void Die(BaseAgent killer)
        {
            // Drop carried mana
            if (manaCarried > 0 && gameManager != null)
            {
                gameManager.DropMana(transform.position, manaCarried);
            }

            AddReward(rewardDeath);
            eventLogger?.LogEvent("death", agentId, teamType, new Dictionary<string, object> { { "killer", killer?.agentId ?? -1 } });

            // Check if team eliminated
            if (gameManager != null)
            {
                gameManager.OnAgentDeath(teamType);
            }

            EndEpisode();
        }

        public void OnWin()
        {
            AddReward(rewardWin);
            EndEpisode();
        }

        public void OnLoss()
        {
            AddReward(rewardLoss);
            EndEpisode();
        }

        private void UpdateNearbyEntities()
        {
            nearbyEntities.Clear();
            var allAgents = FindObjectsOfType<BaseAgent>();
            foreach (var agent in allAgents)
            {
                if (agent != this && Vector3.Distance(transform.position, agent.transform.position) < observationRadius)
                {
                    nearbyEntities.Add(agent.gameObject);
                }
            }
        }

        private void UpdateNearbyMana()
        {
            nearbyMana.Clear();
            var allMana = FindObjectsOfType<ManaItem>();
            foreach (var mana in allMana)
            {
                if (Vector3.Distance(transform.position, mana.transform.position) < observationRadius)
                {
                    nearbyMana.Add(mana.gameObject);
                }
            }
        }

        private void UpdateNearbyObstacles()
        {
            nearbyObstacles.Clear();
            var allObstacles = FindObjectsOfType<Obstacle>();
            foreach (var obstacle in allObstacles)
            {
                if (Vector3.Distance(transform.position, obstacle.transform.position) < observationRadius)
                {
                    nearbyObstacles.Add(obstacle.gameObject);
                }
            }
        }

        private GameObject FindNearestMana()
        {
            return nearbyMana
                .OrderBy(m => Vector3.Distance(transform.position, m.transform.position))
                .FirstOrDefault();
        }

        private GameObject FindNearestEnemy()
        {
            return nearbyEntities
                .Where(e =>
                {
                    var agent = e.GetComponent<BaseAgent>();
                    return agent != null && agent.teamType != teamType;
                })
                .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
                .FirstOrDefault();
        }

        private GameObject FindNearestObstacle()
        {
            return nearbyObstacles
                .OrderBy(o => Vector3.Distance(transform.position, o.transform.position))
                .FirstOrDefault();
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // Manual control for testing
            var continuousActionsOut = actionsOut.ContinuousActions;
            var discreteActionsOut = actionsOut.DiscreteActions;

            // Movement
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
            continuousActionsOut[2] = 0f;

            // Actions
            discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
            discreteActionsOut[1] = 0;
        }
    }
}

