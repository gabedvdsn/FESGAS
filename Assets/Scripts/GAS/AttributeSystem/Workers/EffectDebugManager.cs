using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class EffectDebugManager : MonoBehaviour
    {
        public static EffectDebugManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public EffectDebugWorker DebugPrefab;

        private List<EffectDebugWorker> Workers;

        private void Start()
        {
            Workers = new List<EffectDebugWorker>();
        }

        public void CreateDebugFor(ref GameplayEffectShelfContainer container)
        {
            EffectDebugWorker worker = Instantiate(DebugPrefab, transform);
            worker.Set(ref container);
        }

        public void BackCommunicate(EffectDebugWorker worker)
        {
            StartCoroutine(DoSlowFadeOut(worker, .75f));
        }

        private IEnumerator DoSlowFadeOut(EffectDebugWorker worker, float duration)
        {
            float elapsedDuration = 0f;
            while (elapsedDuration < duration)
            {
                worker.Canvas.alpha = Mathf.Lerp(1, 0, elapsedDuration / duration);
                elapsedDuration += Time.deltaTime;
                yield return null;
            }

            Workers.Remove(worker);
            Destroy(worker.gameObject);
        }
    }
}
