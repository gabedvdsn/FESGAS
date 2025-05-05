using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace FESGameplayAbilitySystem
{
    public class ProcessDataPacket
    {
        protected Dictionary<ESourceTarget, List<GASComponentBase>> SourceTargetData = new();
        protected Dictionary<GameplayTagScriptableObject, Dictionary<ESourceTargetData, List<object>>> Payload = new();

        public IGameplayProcessHandler Handler;

        public ProcessDataPacket()
        {
            Handler = ProcessControl.Instance;
        }

        public ProcessDataPacket(IGameplayProcessHandler handler)
        {
            Handler = handler;
        }

        #region GAS
        
        public void Add(ESourceTarget sourceTarget, GASComponentBase component, bool noDuplicates = true)
        {
            if (SourceTargetData.ContainsKey(sourceTarget))
            {
                if (noDuplicates && SourceTargetData[sourceTarget].Contains(component)) return;
                SourceTargetData[sourceTarget].Add(component);
            }
            else SourceTargetData[sourceTarget] = new List<GASComponentBase>{ component };
        }

        public bool Remove(ESourceTarget sourceTarget, GASComponentBase component)
        {
            return SourceTargetData.ContainsKey(sourceTarget) && SourceTargetData[sourceTarget].Remove(component);
        }
        
        public void AddRange(ESourceTarget sourceTarget, List<GASComponentBase> components, bool noDuplicates = true)
        {
            if (SourceTargetData.ContainsKey(sourceTarget)) SourceTargetData[sourceTarget].AddRange(noDuplicates ? components.Where(component => !SourceTargetData[sourceTarget].Contains(component)) : components);
            else SourceTargetData[sourceTarget] = components;
        }

        public DataValue<GASComponentBase> Get(ESourceTarget sourceTarget)
        {
            return !SourceTargetData.ContainsKey(sourceTarget) ? default : new DataValue<GASComponentBase>(SourceTargetData[sourceTarget]);
        }

        public DataValue<GASComponentBase> Target() => Get(ESourceTarget.Target);
        public DataValue<GASComponentBase> Source() => Get(ESourceTarget.Source);
        
        #endregion
        
        #region Payload

        public void AddPayload<T>(ESourceTargetData sourceTarget, GameplayTagScriptableObject key, T value)
        {
            if (!Payload.ContainsKey(key)) Payload[key] = new Dictionary<ESourceTargetData, List<object>>();
            Payload[key].SafeAdd(sourceTarget, value);
        }

        public void AddPayloadRange<T>(ESourceTargetData sourceTarget, GameplayTagScriptableObject key, List<T> values)
        {
            if (!Payload.ContainsKey(key)) Payload[key] = new Dictionary<ESourceTargetData, List<object>>();

            if (!Payload[key].ContainsKey(sourceTarget)) Payload[key][sourceTarget] = new List<object>();

            Payload[key][sourceTarget].AddRange(values);
        }

        public bool TryGetPayload<T>(ESourceTargetData sourceTarget, GameplayTagScriptableObject key, out DataValue<T> dataValue)
        {
            if (!Payload.ContainsKey(key) || !Payload[key].ContainsKey(sourceTarget))
            {
                dataValue = default;
                return false;
            }
            
            List<T> tObjects = new List<T>();
            foreach (object o in Payload[key][sourceTarget])
            {
                if (o is T cast) tObjects.Add(cast);
            }

            dataValue = new DataValue<T>(tObjects);
            return true;
        }

        public bool TryPayloadTarget<T>(GameplayTagScriptableObject key, out DataValue<T> dataValue) => TryGetPayload(ESourceTargetData.Target, key, out dataValue);
        public bool TryPayloadSource<T>(GameplayTagScriptableObject key, out DataValue<T> dataValue) => TryGetPayload(ESourceTargetData.Source, key, out dataValue);
        
        #endregion
    }
}
