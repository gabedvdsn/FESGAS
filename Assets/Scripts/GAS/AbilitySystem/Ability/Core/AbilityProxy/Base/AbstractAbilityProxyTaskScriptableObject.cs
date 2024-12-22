using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAbilityProxyTaskScriptableObject : ScriptableObject
    {
        [HideInInspector] public string ReadOnlyDescription = "This is an Ability Proxy Task";
        
        public virtual UniTask Prepare(AbilityProxyData data, CancellationToken token) => UniTask.CompletedTask;

        public abstract UniTask Activate(AbilityProxyData data, CancellationToken token);

        
        public virtual UniTask Clean(AbilityProxyData data, CancellationToken token) => UniTask.CompletedTask;
    }

    public class AbilityProxyData
    {
        public AbilitySpec Spec;
        
        private Dictionary<ESourceTarget, List<GASComponent>> SourceTargetData;

        public AbilityProxyData(AbilitySpec spec)
        {
            Spec = spec;
            SourceTargetData = new Dictionary<ESourceTarget, List<GASComponent>>();
        }

        public void Add(ESourceTarget sourceTarget, GASComponent component, bool noDuplicates = true)
        {
            if (SourceTargetData.ContainsKey(sourceTarget))
            {
                if (noDuplicates && SourceTargetData[sourceTarget].Contains(component)) return;
                SourceTargetData[sourceTarget].Add(component);
            }
            else SourceTargetData[sourceTarget] = new List<GASComponent>{ component };
        }
        
        public void AddRange(ESourceTarget sourceTarget, List<GASComponent> components, bool noDuplicates = true)
        {
            if (SourceTargetData.ContainsKey(sourceTarget))
                SourceTargetData[sourceTarget].AddRange(components.Where(component => noDuplicates && !SourceTargetData[sourceTarget].Contains(component)));
            else SourceTargetData[sourceTarget] = components;
        }

        public ProxyDataValue Get(ESourceTarget sourceTarget)
        {
            return !SourceTargetData.ContainsKey(sourceTarget) ? default : new ProxyDataValue(SourceTargetData[sourceTarget]);
        }

        public ProxyDataValue Target() => Get(ESourceTarget.Target);
        public ProxyDataValue Source() => Get(ESourceTarget.Source);

        public static AbilityProxyData GenerateFrom(AbilitySpec spec, GASComponent component, ESourceTarget sourceTarget)
        {
            AbilityProxyData data = new AbilityProxyData(spec);
            data.Add(sourceTarget, component);

            return data;
        }

        public void CompileWith(AbilityProxyData other)
        {
            foreach (ESourceTarget sourceTarget in other.SourceTargetData.Keys)
            {
                AddRange(sourceTarget, other.SourceTargetData[sourceTarget]);
            }
        }
    }

    public struct ProxyDataValue
    {
        private List<GASComponent> Data;
        private bool Valid => Data is not null && Data.Count > 0;

        public ProxyDataValue(List<GASComponent> data)
        {
            Data = data;
        }

        public GASComponent Primary => Valid ? Data[0] : null;
        public GASComponent Any => Valid ? Data.RandomChoice() : null;
        public List<GASComponent> All => Valid ? Data : null;
        
        public GASComponent Last => Valid ? Data[^1] : null;
    }
}
