using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractTagWorkerScriptableObject : ScriptableObject
    {
        [Header("Tag Worker")] 
        
        public TagWorkerRequirements Requirements;
        [Tooltip("Allow multiple instances of this worker")]
        public bool AllowMultipleInstances;
        
        [Space(5)]
        
        [Tooltip("Ticks between calls to Tick.")]
        public int TickPause;
        
        public abstract void Initialize(GASComponentBase component);
        public abstract void Tick(GASComponentBase component);
        public abstract void Resolve(GASComponentBase component);

        public abstract AbstractTagWorker Generate(ITagHandler system);
        
        public bool ValidateWorkFor(ITagHandler system)
        {
            var appliedTags = system.GetAppliedTags();
            foreach (TagWorkerRequirementPacket packet in Requirements.TagPackets)
            {
                switch (packet.Policy)
                {
                    case ERequireAvoidPolicy.Require:
                        if (!appliedTags.Contains(packet.Tag)) return false;
                        break;
                    case ERequireAvoidPolicy.Avoid:
                        if (appliedTags.Contains(packet.Tag)) return false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (system.GetWeight(packet.Tag) < packet.RequiredWeight) return false;
            }

            return true;
        }

        public override string ToString()
        {
            return name;
        }

        private void OnValidate()
        {
            for (int i = 0; i < Requirements.TagPackets.Count; i++)
            {
                if (Requirements.TagPackets[i].RequiredWeight < 1)
                {
                    Requirements.TagPackets[i] = new TagWorkerRequirementPacket(
                        Requirements.TagPackets[i].Tag,
                        Requirements.TagPackets[i].Policy,
                        1
                    );
                }
            }
        }

    }
    
    public abstract class AbstractTagWorker
    {
        public AbstractTagWorkerScriptableObject Base;
        private GASComponentBase System;

        public int TicksRemaining;

        protected AbstractTagWorker(AbstractTagWorkerScriptableObject workerBase, GASComponentBase system)
        {
            Base = workerBase;
            System = system;

            TicksRemaining = 0;
        }

        public void Initialize()
        {
            Base.Initialize(System);
        }
        
        public void Tick()
        {
            if (TicksRemaining <= 0)
            {
                Base.Tick(System);
                TicksRemaining = Base.TickPause;
            }
            else TicksRemaining -= 1;
        }

        public void Resolve()
        {
            Base.Resolve(System);
        }
    }
}
