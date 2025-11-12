using UnityEngine;

namespace NoxusIoniaRL.Agents
{
    /// <summary>
    /// Noxus team agent.
    /// </summary>
    public class NoxusAgent : BaseAgent
    {
        public override void Initialize()
        {
            teamType = TeamType.Noxus;
            base.Initialize();
        }
    }
}

