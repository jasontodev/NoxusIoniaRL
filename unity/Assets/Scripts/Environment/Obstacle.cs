using UnityEngine;

namespace NoxusIoniaRL.Environment
{
    /// <summary>
    /// Movable obstacle (box, wall, ramp) in the forest area.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        [Header("Obstacle Properties")]
        public ObstacleType obstacleType = ObstacleType.Box;
        public float pushForce = 5f;
        public float maxPushDistance = 10f;

        private Rigidbody rb;
        private Vector3 startPosition;

        public enum ObstacleType
        {
            Box,
            Wall,
            Ramp
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            // Configure based on type
            switch (obstacleType)
            {
                case ObstacleType.Box:
                    rb.mass = 10f;
                    break;
                case ObstacleType.Wall:
                    rb.mass = 50f;
                    rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                    break;
                case ObstacleType.Ramp:
                    rb.mass = 20f;
                    break;
            }

            startPosition = transform.position;
        }

        public void Push(Vector3 direction)
        {
            if (rb != null)
            {
                // Check if within max push distance from start
                float distanceFromStart = Vector3.Distance(transform.position, startPosition);
                if (distanceFromStart < maxPushDistance)
                {
                    rb.AddForce(direction * pushForce, ForceMode.Impulse);
                }
            }
        }

        public void ResetPosition()
        {
            transform.position = startPosition;
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}

