using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
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
            bool changesMade = false;
            
            // Handle deactivating active workers if applicable
            IEnumerable<AbstractTagWorkerScriptableObject> activeWorkers = ActiveWorkers.Keys;
            foreach (AbstractTagWorkerScriptableObject workerData in activeWorkers)
            {
                if (workerData.ValidateWorkFor(System)) continue;
                
                // Debug.Log($"\t[ TAG-WORKER ] Deactivate {workerData}");

                foreach (AbstractTagWorker worker in ActiveWorkers[workerData]) worker.Resolve();
                ActiveWorkers.Remove(workerData);

                changesMade = true;
            }

            // Handle activating new workers if applicable
            foreach (AbstractTagWorkerScriptableObject workerData in TagWorkers)
            {
                if (!workerData.ValidateWorkFor(System)) continue;

                if (ActiveWorkers.ContainsKey(workerData) && workerData.AllowMultipleInstances)
                {
                    ActiveWorkers[workerData].Add(workerData.Generate(System));
                    // Debug.Log($"\t[ TAG-WORKER ] Activate duplicate {workerData}");
                    ActiveWorkers[workerData][^1].Initialize();
                }
                else
                {
                    ActiveWorkers[workerData] = new List<AbstractTagWorker>() { workerData.Generate(System) };
                    // Debug.Log($"\t[ TAG-WORKER ] Activate {workerData}");
                    ActiveWorkers[workerData][^1].Initialize();
                }

                changesMade = true;
            }
            
            if (changesMade) HandleTagWorkers();
            
            // LogWeights();
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
                AddTag(tag);
            }

            // Debug.Log($"[ TAG-C ] Add {taggable}");

            HandleTagWorkers();
        }

        public void RemoveTaggable(ITaggable taggable)
        {
            // Debug.Log($"[ TAG-C ] Remove {taggable}");
            
            // Handle tag weight resolution
            foreach (GameplayTagScriptableObject tag in taggable.GetTags())
            {
                RemoveTag(tag);
            }
            
            HandleTagWorkers();
        }

        public void AddTag(GameplayTagScriptableObject tag)
        {
            if (TagWeights.ContainsKey(tag)) TagWeights[tag] += 1;
            else TagWeights[tag] = 1;
            
            LogWeights();
        }

        public void RemoveTag(GameplayTagScriptableObject tag)
        {
            if (!TagWeights.ContainsKey(tag)) return;
                
            TagWeights[tag] -= 1;
            if (GetWeight(tag) <= 0) TagWeights.Remove(tag);
            
            LogWeights();
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

    public interface ITaggable
    {
        public IEnumerable<GameplayTagScriptableObject> GetTags();
        public bool PersistentTags();
    }
}
