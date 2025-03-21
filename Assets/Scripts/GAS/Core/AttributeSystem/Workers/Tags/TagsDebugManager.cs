using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TagsDebugManager : MonoBehaviour
    {
        public static TagsDebugManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public TagsDebugWorker DebugPrefab;

        public void CreateDebugFor(ref AbstractGameplayEffectShelfContainer container)
        {
            foreach (GameplayTagScriptableObject _tag in container.Spec.Base.GrantedTags)
            {
                TagsDebugWorker worker = Instantiate(DebugPrefab, transform);
                worker.Set(ref container, _tag);
            }
        }

        public void BackCommunicate(TagsDebugWorker worker)
        {
            StartCoroutine(DoSlowFadeOut(worker, .75f));
        }
        
        private IEnumerator DoSlowFadeOut(TagsDebugWorker worker, float duration)
        {
            float elapsedDuration = 0f;
            while (elapsedDuration < duration)
            {
                worker.Canvas.alpha = Mathf.Lerp(1, 0, elapsedDuration / duration);
                elapsedDuration += Time.deltaTime;
                yield return null;
            }

            Destroy(worker.gameObject);
        }
    }
}
