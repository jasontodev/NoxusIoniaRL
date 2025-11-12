using UnityEngine;
using NoxusIoniaRL.Agents;

namespace NoxusIoniaRL.Environment
{
    /// <summary>
    /// Collectible mana item that can be picked up and deposited.
    /// </summary>
    public class ManaItem : MonoBehaviour
    {
        private bool isPickedUp = false;
        private BaseAgent carrier = null;

        public bool Pickup(BaseAgent agent)
        {
            if (isPickedUp || agent == null)
                return false;

            isPickedUp = true;
            carrier = agent;
            gameObject.SetActive(false); // Hide but don't destroy
            return true;
        }

        public void Drop()
        {
            isPickedUp = false;
            carrier = null;
            gameObject.SetActive(true);
            // Position will be set by GameManager
        }

        public bool IsPickedUp()
        {
            return isPickedUp;
        }
    }
}

