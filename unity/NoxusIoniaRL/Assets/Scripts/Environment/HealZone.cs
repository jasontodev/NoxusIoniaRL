using UnityEngine;
using NoxusIoniaRL.Agents;

namespace NoxusIoniaRL.Environment
{
    /// <summary>
    /// Team-specific heal zone where agents can deposit mana and heal.
    /// </summary>
    public class HealZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        public BaseAgent.TeamType teamType;
        public float healRate = 10f; // HP per second
        public float zoneRadius = 5f;

        private int manaDeposited = 0;
        private GameManager gameManager;

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        private void OnTriggerStay(Collider other)
        {
            var agent = other.GetComponent<BaseAgent>();
            if (agent != null && agent.teamType == teamType)
            {
                // Heal agent while in zone
                // This would require exposing health in BaseAgent or using a health component
            }
        }

        public bool DepositMana(BaseAgent agent, int amount)
        {
            if (agent.teamType != teamType)
                return false;

            if (gameManager != null)
            {
                bool success = gameManager.DepositMana(teamType, amount);
                if (success)
                {
                    manaDeposited += amount;
                }
                return success;
            }
            return false;
        }

        public void Reset()
        {
            manaDeposited = 0;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = teamType == BaseAgent.TeamType.Noxus ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(transform.position, zoneRadius);
        }
    }
}

