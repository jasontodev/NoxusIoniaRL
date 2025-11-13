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
        public float moveSpeed = 3f; // Reduced from 5f for smoother movement
        public float rotationSpeed = 180f;
        public float interactionRange = 2f;
        public float attackRange = 1.5f; // Reduced for smaller 2x2 arena (was 3f)
        public float attackCooldown = 1f;
        public int maxHealth = 100;
        
        [Header("Debug/Visualization")]
        public bool showHealthBar = true;

        [Header("Movement Smoothing")]
        public float acceleration = 10f; // How quickly agent reaches max speed
        public float deceleration = 15f; // How quickly agent stops

        [Header("Observation Settings")]
        public int kNearestEntities = 5;
        public float observationRadius = 20f;

        [Header("Reward Weights")]
        public float rewardElimination = 2.0f;
        public float rewardDeath = -1.0f;
        public float rewardWin = 10.0f;
        public float rewardLoss = -5.0f;
        public float rewardIdle = -0.01f;
        public float rewardFriendlyBlock = -0.1f;

        // Agent state
        private int currentHealth;
        private float lastAttackTime;
        private Vector3 lastPosition;
        private float idleTime;
        private bool isDead = false; // Track if agent is dead (no respawn during episode)
        private float accumulatedHealing = 0f; // Accumulate fractional healing amounts
        private Rigidbody rb;
        private GameManager gameManager;
        private EventLogger eventLogger;
        private Vector3 currentVelocity; // For smooth movement
        private Collider agentCollider; // Cache collider for enabling/disabling
        private TextMesh healthText; // Health display text (deprecated - using visual bar now)
        private GameObject healthBarBackground; // Red background bar
        private GameObject healthBarFill; // Green fill bar

        // Observation cache
        private List<GameObject> nearbyEntities = new List<GameObject>();
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
            }

            // Configure Rigidbody for smooth movement
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection
            rb.drag = 5f; // Air resistance for smoother deceleration
            rb.useGravity = true; // Gravity enabled (walls will prevent falling)

            gameManager = FindObjectOfType<GameManager>();
            eventLogger = FindObjectOfType<EventLogger>();

            currentHealth = maxHealth;
            lastPosition = transform.position;
            idleTime = 0f;
            currentVelocity = Vector3.zero;
            isDead = false;
            
            // Cache collider for enabling/disabling
            agentCollider = GetComponent<Collider>();
            
            // Create visual health bar (replaces text)
            CreateHealthBar();
        }
        
        private void CreateHealthBar()
        {
            // Create health bar container
            Transform existingBar = transform.Find("HealthBar");
            GameObject barContainer;
            
            if (existingBar != null)
            {
                barContainer = existingBar.gameObject;
            }
            else
            {
                barContainer = new GameObject("HealthBar");
                barContainer.transform.SetParent(transform);
                barContainer.transform.localPosition = new Vector3(0f, 2.5f, 0f); // Above agent's head
                barContainer.transform.localRotation = Quaternion.identity;
                barContainer.transform.localScale = Vector3.one;
                
                // Make bar face camera
                if (barContainer.GetComponent<BillboardText>() == null)
                {
                    barContainer.AddComponent<BillboardText>();
                }
            }
            
            // Create background bar (red)
            if (healthBarBackground == null)
            {
                healthBarBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
                healthBarBackground.name = "HealthBarBackground";
                healthBarBackground.transform.SetParent(barContainer.transform);
                healthBarBackground.transform.localPosition = Vector3.zero;
                healthBarBackground.transform.localRotation = Quaternion.identity;
                healthBarBackground.transform.localScale = new Vector3(1f, 0.2f, 1f);
                
                // Remove collider entirely (we don't need physics, and it causes warnings)
                Collider bgCollider = healthBarBackground.GetComponent<Collider>();
                if (bgCollider != null)
                {
                    DestroyImmediate(bgCollider);
                }
                
                // Set dark gray/black background material (so fill is always visible)
                Shader shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Standard"); // Fallback
                Material bgMat = new Material(shader);
                bgMat.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray background
                healthBarBackground.GetComponent<Renderer>().material = bgMat;
            }
            
            // Create fill bar (green)
            if (healthBarFill == null)
            {
                healthBarFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
                healthBarFill.name = "HealthBarFill";
                healthBarFill.transform.SetParent(barContainer.transform);
                healthBarFill.transform.localPosition = new Vector3(0f, 0f, -0.01f); // Slightly in front
                healthBarFill.transform.localRotation = Quaternion.identity;
                healthBarFill.transform.localScale = new Vector3(1f, 0.2f, 1f);
                
                // Remove collider entirely (we don't need physics, and it causes warnings)
                Collider fillCollider = healthBarFill.GetComponent<Collider>();
                if (fillCollider != null)
                {
                    DestroyImmediate(fillCollider);
                }
                
                // Set green material (use Unlit/Color for compatibility across render pipelines)
                Shader shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Standard"); // Fallback
                Material greenMat = new Material(shader);
                greenMat.color = Color.green;
                healthBarFill.GetComponent<Renderer>().material = greenMat;
            }
            
            UpdateHealthBar();
        }

        public override void OnEpisodeBegin()
        {
            // Reset death state - agent is alive again for new episode
            isDead = false;
            currentHealth = maxHealth;
            lastAttackTime = 0f;
            lastPosition = transform.position;
            idleTime = 0f;
            currentVelocity = Vector3.zero;
            accumulatedHealing = 0f; // Reset accumulated healing

            // Re-enable components
            // Re-enable ALL colliders (main collider and any child colliders)
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
            
            // Also re-enable the cached main collider
            if (agentCollider != null)
            {
                agentCollider.enabled = true;
            }
            if (rb != null)
            {
                // Make non-kinematic FIRST (can't set velocity on kinematic rigidbody)
                rb.isKinematic = false;
                // Then reset velocity
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Re-enable renderers (make agent visible again)
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
            
            // Re-enable health bar
            if (healthBarBackground != null)
            {
                healthBarBackground.SetActive(true);
            }
            if (healthBarFill != null)
            {
                healthBarFill.SetActive(true);
            }
            
            // Re-enable health text (if it exists)
            if (healthText != null)
            {
                healthText.gameObject.SetActive(true);
            }

            // Update health bar
            UpdateHealthBar();

            // Reset position (handled by GameManager)
            if (gameManager != null)
            {
                gameManager.ResetAgent(this);
            }
        }
        
        private void Update()
        {
            // Update health bar every frame
            if (showHealthBar)
            {
                if (healthBarBackground != null)
                {
                    healthBarBackground.SetActive(true);
                }
                if (healthBarFill != null)
                {
                    healthBarFill.SetActive(true);
                    UpdateHealthBar();
                }
            }
            else
            {
                if (healthBarBackground != null)
                {
                    healthBarBackground.SetActive(false);
                }
                if (healthBarFill != null)
                {
                    healthBarFill.SetActive(false);
                }
            }
        }
        
        private void UpdateHealthBar()
        {
            if (healthBarFill == null) return;
            
            float healthPercent = GetHealthPercent();
            
            // Update fill bar width based on health
            Vector3 scale = healthBarFill.transform.localScale;
            scale.x = healthPercent; // Scale from 0 to 1 based on health
            healthBarFill.transform.localScale = scale;
            
            // Adjust position so bar fills from left to right
            Vector3 pos = healthBarFill.transform.localPosition;
            pos.x = -(1f - healthPercent) / 2f; // Center the fill bar
            healthBarFill.transform.localPosition = pos;
            
            // Change color based on health percentage
            Renderer fillRenderer = healthBarFill.GetComponent<Renderer>();
            if (fillRenderer != null && fillRenderer.material != null)
            {
                // Create new material instance to avoid shared material issues
                Material fillMat = fillRenderer.material;
                if (healthPercent > 0.6f)
                    fillMat.color = Color.green;
                else if (healthPercent > 0.3f)
                    fillMat.color = Color.yellow;
                else
                    fillMat.color = Color.red;
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Dead agents provide zero observations (33 total)
            if (isDead)
            {
                // Provide 33 zero observations to match expected size
                // Self state (7 features)
                for (int i = 0; i < 7; i++) sensor.AddObservation(0f);
                
                // k-NN entities (5 × 3 = 15 features)
                for (int i = 0; i < 15; i++) sensor.AddObservation(0f);
                
                // Nearest obstacles (3 × 2 = 6 features)
                for (int i = 0; i < 6; i++) sensor.AddObservation(0f);
                
                // Distances to zones (2 features)
                for (int i = 0; i < 2; i++) sensor.AddObservation(0f);
                
                // Global summary (3 features)
                for (int i = 0; i < 3; i++) sensor.AddObservation(0f);
                
                return; // Total: 7 + 15 + 6 + 2 + 3 = 33 observations
            }
            
            // Self state (7 features)
            // k-NN entities (5 × 3): 15 features (distance, is_teammate, health)
            // Nearest obstacles (3 × 2): 6 features
            // Distances to zones: 2 features
            // Global summary: 3 features
            // Total: 7 + 15 + 6 + 2 + 3 = 33
            sensor.AddObservation(currentHealth / (float)maxHealth); // Normalized health
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
                    sensor.AddObservation(agent != null && agent.teamType == teamType ? 1f : 0f); // Is Teammate
                    sensor.AddObservation(agent != null ? agent.currentHealth / (float)agent.maxHealth : 0f); // Health
                }
                else
                {
                    sensor.AddObservation(1f); // Max distance (no entity)
                    sensor.AddObservation(0f); // Is Teammate (no entity)
                    sensor.AddObservation(0f); // Health (no entity)
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
            // Note: No mana system, so we only observe time remaining
            if (gameManager != null)
            {
                sensor.AddObservation(0f); // Team mana banked (no longer used)
                sensor.AddObservation(0f); // Enemy mana banked (no longer used)
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
            // Dead agents don't take actions
            if (isDead)
            {
                return;
            }
            
            // Continuous actions: movement and rotation
            var moveX = actions.ContinuousActions[0];
            var moveZ = actions.ContinuousActions[1];
            var rotate = actions.ContinuousActions[2];

            // Calculate desired movement direction
            Vector3 desiredDirection = new Vector3(moveX, 0f, moveZ);
            Vector3 targetVelocity = desiredDirection.normalized * moveSpeed;

            // Smooth acceleration/deceleration
            if (targetVelocity.magnitude > 0.1f)
            {
                // Accelerate towards target velocity
                currentVelocity = Vector3.MoveTowards(
                    currentVelocity,
                    targetVelocity,
                    acceleration * Time.fixedDeltaTime
                );
            }
            else
            {
                // Decelerate to zero
                currentVelocity = Vector3.MoveTowards(
                    currentVelocity,
                    Vector3.zero,
                    deceleration * Time.fixedDeltaTime
                );
            }

            // Apply smooth velocity (preserve Y velocity for gravity)
            // Only set velocity if rigidbody is not kinematic (dead agents are kinematic)
            if (rb != null && !rb.isKinematic)
            {
                rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
            }

            // Rotate agent smoothly towards movement direction
            if (currentVelocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentVelocity.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }

            // Discrete actions
            int actionType = actions.DiscreteActions[0]; // 0: none, 1: interact (obstacles), 2: attack, 3: defend, 4: signal

            switch (actionType)
            {
                case 1: // Interact (push obstacles)
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
            // Only handle obstacle pushing (no mana system)
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
        
        private void HandleDrop()
        {
            // Drop action removed - no mana system
            // This action is now unused but kept for action space compatibility
        }

        private void HandleAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
                return;

            // Find nearest enemy (teammates cannot be attacked)
            var nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null && Vector3.Distance(transform.position, nearestEnemy.transform.position) < attackRange)
            {
                var targetAgent = nearestEnemy.GetComponent<BaseAgent>();
                if (targetAgent != null)
                {
                    lastAttackTime = Time.time;
                    
                    // Attack enemy - deal damage
                    // NO REWARD HERE - agent must learn through elimination rewards and win/loss
                    // This forces pure RL learning: agent discovers that attacking enemies leads to eliminations
                    // which leads to wins, purely through trial and error
                    int damage = 25;
                    targetAgent.TakeDamage(damage, this);
                    
                    eventLogger?.LogEvent("attack", agentId, teamType, new Dictionary<string, object> 
                    { 
                        { "target", targetAgent.agentId }, 
                        { "damage", damage },
                        { "is_enemy", true }
                    });
                }
            }
        }
        
        private GameObject FindNearestAgent()
        {
            // Find nearest agent (teammate or enemy) - agent must learn which to attack
            return nearbyEntities
                .Where(e =>
                {
                    var agent = e.GetComponent<BaseAgent>();
                    return agent != null;
                })
                .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
                .FirstOrDefault();
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
            // Dead agents don't get rewards/penalties
            if (isDead)
            {
                return;
            }
            
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

            // No mana shaping reward - agents must learn through win/loss that depositing mana leads to resource-based wins
        }

        public void TakeDamage(int damage, BaseAgent attacker)
        {
            if (isDead) return; // Can't take damage if already dead
            
            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;
            
            // Update health bar
            UpdateHealthBar();
            
            // Check for death immediately after taking damage
            if (currentHealth <= 0 && !isDead)
            {
                Die(attacker);
            }
        }

        private void Die(BaseAgent killer)
        {
            // Prevent multiple death calls
            if (isDead)
            {
                return;
            }
            
            isDead = true;
            currentHealth = 0; // Ensure health is exactly 0
            
            Debug.Log($"[DEATH] {teamType} Agent {agentId} died! Health: {currentHealth}, Killer: {(killer != null ? killer.agentId.ToString() : "unknown")}");
            
            AddReward(rewardDeath);
            eventLogger?.LogEvent("death", agentId, teamType, new Dictionary<string, object> { { "killer", killer?.agentId ?? -1 } });

            // Check if team eliminated (this will trigger episode end if all agents of a team are dead)
            if (gameManager != null)
            {
                gameManager.OnAgentDeath(teamType);
                gameManager.CheckWinConditions(); // Immediately check win conditions after death
            }

            // Disable agent instead of respawning - agent stays dead until episode ends
            // Stop movement first (before making kinematic)
            if (rb != null)
            {
                // Set velocity BEFORE making kinematic (kinematic bodies can't have velocity set)
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // Then make kinematic
                rb.isKinematic = true;
            }
            
            // Disable ALL colliders (main collider and any child colliders)
            // This prevents dead agents from blocking movement or being hit
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
            
            // Also disable the cached main collider
            if (agentCollider != null)
            {
                agentCollider.enabled = false;
            }
            
            // Hide the agent by making it invisible (vision-based - other agents can't see dead agents)
            // Deactivating can cause recursion issues with ML-Agents
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
            
            // Hide health bar
            if (healthBarBackground != null)
            {
                healthBarBackground.SetActive(false);
            }
            if (healthBarFill != null)
            {
                healthBarFill.SetActive(false);
            }
            
            // Also hide health text (if it exists)
            if (healthText != null)
            {
                healthText.gameObject.SetActive(false);
            }
            
            // Note: We do NOT call EndEpisode() here - agent stays dead until episode ends naturally
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
                // Only include alive agents in observations (dead agents are invisible to others - vision-based)
                if (agent != null && agent != this && !agent.IsDead() && Vector3.Distance(transform.position, agent.transform.position) < observationRadius)
                {
                    nearbyEntities.Add(agent.gameObject);
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

        // Public getters for debugging
        public int GetCurrentHealth() => currentHealth;
        public float GetHealthPercent() => currentHealth / (float)maxHealth;
        public bool IsDead() => isDead;
        
        // Heal the agent (used by heal zones)
        public void Heal(float amount)
        {
            if (isDead)
            {
                Debug.LogWarning($"[HEAL] {teamType} Agent {agentId} - Cannot heal dead agent");
                return; // Can't heal dead agents
            }
            
            // Accumulate fractional healing amounts
            accumulatedHealing += amount;
            
            // Apply healing when accumulated amount is >= 1 HP
            if (accumulatedHealing >= 1f)
            {
                int healthBefore = currentHealth;
                int healingToApply = Mathf.FloorToInt(accumulatedHealing);
                accumulatedHealing -= healingToApply; // Keep the remainder
                
                currentHealth = Mathf.Min(maxHealth, currentHealth + healingToApply);
                UpdateHealthBar();
                
                if (healthBefore < currentHealth)
                {
                    Debug.Log($"[HEAL] {teamType} Agent {agentId} - Healed {currentHealth - healthBefore} HP ({healthBefore} -> {currentHealth}/{maxHealth}), accumulated: {accumulatedHealing:F2}");
                }
            }
        }

        private void OnDrawGizmos()
        {
            // Draw health bar above agent
            if (showHealthBar && Application.isPlaying)
            {
                float healthPercent = GetHealthPercent();
                Vector3 barPosition = transform.position + Vector3.up * 2.5f;
                
                // Health bar background (red)
                Gizmos.color = Color.red;
                Gizmos.DrawCube(barPosition, new Vector3(1f, 0.2f, 0.1f));
                
                // Health bar fill (green)
                Gizmos.color = Color.green;
                float barWidth = healthPercent;
                Gizmos.DrawCube(
                    barPosition - new Vector3((1f - barWidth) / 2f, 0f, 0f),
                    new Vector3(barWidth, 0.2f, 0.1f)
                );
                
                // Draw attack range
                Gizmos.color = teamType == TeamType.Noxus ? new Color(1f, 0f, 0f, 0.2f) : new Color(0f, 0f, 1f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, attackRange);
                
                // Draw interaction range
                Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                Gizmos.DrawWireSphere(transform.position, interactionRange);
            }
        }


        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // Manual control for testing
            var continuousActionsOut = actionsOut.ContinuousActions;
            var discreteActionsOut = actionsOut.DiscreteActions;

            // Movement (WASD or Arrow Keys)
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
            continuousActionsOut[2] = 0f;

            // Actions
            // 0: none, 1: interact (pickup/deposit), 2: attack, 3: defend, 4: signal, 5: drop
            if (Input.GetKey(KeyCode.E))
            {
                discreteActionsOut[0] = 1; // Interact (pickup if empty, deposit if at heal zone)
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                discreteActionsOut[0] = 5; // Drop mana anywhere
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                discreteActionsOut[0] = 2; // Attack
            }
            else
            {
                discreteActionsOut[0] = 0; // No action
            }
            discreteActionsOut[1] = 0; // Signal intent code (not used in manual control)
        }
    }
}

