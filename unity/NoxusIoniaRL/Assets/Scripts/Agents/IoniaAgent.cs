using UnityEngine;

namespace NoxusIoniaRL.Agents
{
    /// <summary>
    /// Ionia team agent.
    /// </summary>
    public class IoniaAgent : BaseAgent
    {
        public override void Initialize()
        {
            teamType = TeamType.Ionia;
            base.Initialize();
        }
    }
}

