using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TagCache
    {
        private GASComponentBase System;

        // List of tag worker datas
        private List<AbstractTagWorkerScriptableObject> TagWorkers;

        private Dictionary<GameplayTagScriptableObject, int> TagWeights;
        private Dictionary<AbstractTagWorkerScriptableObject, List<AbstractTagWorker>> ActiveWorkers;

        public List<GameplayTagScriptableObject> GetAppliedTags() => TagWeights.Keys.ToList();

        public TagCache(GASComponentBase system, List<AbstractTagWorkerScriptableObject> workers)
        {
            System = system;

            TagWeights = new Dictionary<GameplayTagScriptableObject, int>();
            TagWorkers = workers;

            ActiveWorkers = new Dictionary<AbstractTagWorkerScriptableObject, List<AbstractTagWorker>>();
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

                if (ActiveWorkers.ContainsKey(workerData) && workerData.AllowMultipleInstances) ActiveWorkers[workerData].Add(workerData.Generate(System));
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

        public void AddTag(GameplayTagScriptableObject tag, bool noDuplicates = false, bool handle = true)
        {
            if (TagWeights.ContainsKey(tag))
            {
                if (!noDuplicates) TagWeights[tag] += 1;
            }
            else TagWeights[tag] = 1;
            
            if (handle) HandleTagWorkers();
        }

        public void AddTags(IEnumerable<GameplayTagScriptableObject> tags, bool noDuplicates = false)
        {
            foreach (var tag in tags)
            {
                AddTag(tag, noDuplicates, false);
            }
            
            HandleTagWorkers();
        }

        public void RemoveTag(GameplayTagScriptableObject tag, bool handle = true)
        {
            if (!TagWeights.ContainsKey(tag)) return;
                
            TagWeights[tag] -= 1;
            if (GetWeight(tag) <= 0) TagWeights.Remove(tag);
            
            if (handle) HandleTagWorkers();
        }

        public void RemoveTags(IEnumerable<GameplayTagScriptableObject> tags)
        {
            foreach (var tag in tags)
            {
                RemoveTag(tag, false);
            }
            
            HandleTagWorkers();
        }
        
        public int GetWeight(GameplayTagScriptableObject tag) => TagWeights.TryGetValue(tag, out int weight) ? weight : 0;

        public bool HasTag(GameplayTagScriptableObject tag) => TagWeights.ContainsKey(tag);

        public void LogWeights()
        {
            Debug.Log($"[ LOG-WEIGHTS ]");
            foreach (GameplayTagScriptableObject tag in TagWeights.Keys)
            {
                Debug.Log($"\t{tag} => {TagWeights[tag]}");
            }
        }
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
}
