using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public class ImpactWorkerCache
    {
        private Dictionary<AttributeScriptableObject, List<AbstractImpactWorkerScriptableObject>> Cache;

        public ImpactWorkerCache()
        {
            Cache = new Dictionary<AttributeScriptableObject, List<AbstractImpactWorkerScriptableObject>>();
        }

        public ImpactWorkerCache(List<AbstractImpactWorkerScriptableObject> workers)
        {
            Cache = new Dictionary<AttributeScriptableObject, List<AbstractImpactWorkerScriptableObject>>();
            foreach (var worker in workers) worker.SubscribeToCache(this);
        }

        public void AddWorker(AbstractImpactWorkerScriptableObject worker)
        {
            worker.SubscribeToCache(this);
        }

        public void RemoveWorker(AbstractImpactWorkerScriptableObject worker)
        {
            worker.UnsubscribeFromCache(this);
        }

        public void AddWorker(AttributeScriptableObject attribute, AbstractImpactWorkerScriptableObject worker)
        {
            Cache.SafeAdd(attribute, worker);
        }

        public void RemoveWorker(AttributeScriptableObject attribute, AbstractImpactWorkerScriptableObject worker)
        {
            if (!Cache.ContainsKey(attribute)) return;
            Cache[attribute].Remove(worker);
        }
        
        public void RunImpactData(List<AbilityImpactData> impactData)
        {
            foreach (var data in impactData)
            {
                if (!Cache.ContainsKey(data.Attribute)) continue;
                foreach (var worker in Cache[data.Attribute])
                {
                    if (!worker.ValidateWorkFor(data)) continue;
                    worker.InterpretImpact(data);
                }
            }
        }
    }
}
