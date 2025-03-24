using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractTagWorkerScriptableObject : ScriptableObject
    {
        [Header("Tag Worker")] 
        
        public TagWorkerRequirements Requirements;
        [Tooltip("Allows this worker to handle tags that have already been worked on by other workers.")]
        public bool AllowWorkedTags;
        [Tooltip("Allow multiple instances of this worker")]
        public bool AllowMultipleInstances;
        
        [Space(5)]
        
        [Tooltip("Ticks between calls to Tick.")]
        public int TickPause;
        
        public abstract void Initialize(GASComponent component);
        public abstract void Tick(GASComponent component);
        public abstract void Resolve(GASComponent component);

        public abstract AbstractTagWorker Generate(GASComponent system);
        
        public bool ValidateWorkFor(GASComponent system)
        {
            var appliedTags = system.TagCache.GetAppliedTags();
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
                
                if (system.TagCache.GetWeight(packet.Tag) < packet.RequiredWeight) return false;
            }

            return true;
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
        private GASComponent System;

        public int TicksRemaining;

        protected AbstractTagWorker(AbstractTagWorkerScriptableObject workerBase, GASComponent system)
        {
            Base = workerBase;
            System = system;

            TicksRemaining = 0;
        }

        public void Initialize() => Base.Initialize(System);
        
        public void Tick()
        {
            if (TicksRemaining <= 0)
            {
                Base.Tick(System);
                TicksRemaining = Base.TickPause;
            }
            else TicksRemaining -= 1;
        }
        
        public void Resolve() => Base.Resolve(System);
    }

    public class TagCache
    {
        private GASComponent System;

        // List of tag worker datas
        private List<AbstractTagWorkerScriptableObject> TagWorkers;

        private Dictionary<GameplayTagScriptableObject, int> TagWeights;
        private Dictionary<AbstractTagWorkerScriptableObject, List<AbstractTagWorker>> ActiveWorkers;

        public List<GameplayTagScriptableObject> GetAppliedTags() => TagWeights.Keys.ToList();

        public TagCache(GASComponent system, List<AbstractTagWorkerScriptableObject> workers)
        {
            System = system;

            TagWeights = new Dictionary<GameplayTagScriptableObject, int>();
            TagWorkers = workers;

            ActiveWorkers = new Dictionary<AbstractTagWorkerScriptableObject, List<AbstractTagWorker>>();

            Initialize();
        }

        private void Initialize()
        {

        }

        public void AddTagWorker(AbstractTagWorkerScriptableObject worker)
        {
            if (!TagWorkers.Contains(worker)) TagWorkers.Add(worker);
        }

        public void RemoveTagWorker(AbstractTagWorkerScriptableObject worker)
        {
            if (TagWorkers.Contains(worker)) TagWorkers.Remove(worker);
        }

        private void HandleTagWorkers()
        {
            // Handle deactivating active workers if applicable
            IEnumerable<AbstractTagWorkerScriptableObject> activeWorkers = ActiveWorkers.Keys;
            foreach (AbstractTagWorkerScriptableObject workerData in activeWorkers)
            {
                if (workerData.ValidateWorkFor(System)) continue;

                foreach (AbstractTagWorker worker in ActiveWorkers[workerData]) worker.Resolve();
                ActiveWorkers.Remove(workerData);
            }

            // Handle activating new workers if applicable
            foreach (AbstractTagWorkerScriptableObject workerData in TagWorkers)
            {
                if (!workerData.ValidateWorkFor(System)) continue;

                if (ActiveWorkers.ContainsKey(workerData)) ActiveWorkers[workerData].Add(workerData.Generate(System));
                else ActiveWorkers[workerData] = new List<AbstractTagWorker>() { workerData.Generate(System) };
                
                ActiveWorkers[workerData][^1].Initialize();
            }
        }

        public void TickTagWorkers()
        {
            foreach (AbstractTagWorkerScriptableObject workerData in ActiveWorkers.Keys)
            {
                foreach (AbstractTagWorker worker in ActiveWorkers[workerData]) worker.Tick();
            }
        }

        public void AddTaggable(ITaggable taggable)
        {
            // Handle tag weight resolution
            foreach (GameplayTagScriptableObject tag in taggable.GetTags())
            {
                if (TagWeights.ContainsKey(tag)) TagWeights[tag] += 1;
                else TagWeights[tag] = 1;
            }

            HandleTagWorkers();
        }

        public void RemoveTaggable(ITaggable taggable)
        {
            // Handle tag weight resolution
            foreach (GameplayTagScriptableObject tag in taggable.GetTags())
            {
                if (!TagWeights.ContainsKey(tag)) continue;
                
                TagWeights[tag] -= 1;
                if (GetWeight(tag) <= 0) TagWeights.Remove(tag);
            }

            HandleTagWorkers();
        }



        public int GetWeight(GameplayTagScriptableObject tag) => TagWeights.TryGetValue(tag, out int weight) ? weight : 0;
        
    }

    [Serializable]
    public class TagWorkerRequirements
    {
        [Header("Requirements")]
        
        public List<TagWorkerRequirementPacket> TagPackets;
    }
    
    [Serializable]
    public struct TagWorkerRequirementPacket
    {
        public GameplayTagScriptableObject Tag;
        public ERequireAvoidPolicy Policy;
        public int RequiredWeight;

        public TagWorkerRequirementPacket(GameplayTagScriptableObject tag, ERequireAvoidPolicy policy, int requiredWeight)
        {
            Tag = tag;
            Policy = policy;
            RequiredWeight = requiredWeight;
        }
    }

    public enum ERequireAvoidPolicy
    {
        Require,
        Avoid
    }

    public interface ITaggable
    {
        public IEnumerable<GameplayTagScriptableObject> GetTags();
    }
    
}
