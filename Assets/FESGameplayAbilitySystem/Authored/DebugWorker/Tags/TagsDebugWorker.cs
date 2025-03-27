using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TagsDebugWorker : MonoBehaviour
    {
        public TMP_Text TagText;
        public TMP_Text SourceText;

        public CanvasGroup Canvas;
        
        private AbstractGameplayEffectShelfContainer Container;
        private bool initialized;

        public void Set(ref AbstractGameplayEffectShelfContainer container, GameplayTagScriptableObject _tag)
        {
            Container = container;
            initialized = true;

            TagText.text = _tag.Name;
            SourceText.text = container.Spec.Base.Identifier.Name;

            Canvas.alpha = 1f;
        }

        private void Update()
        {
            if (!initialized) return;
            if (Container.DurationRemaining > 0f && Container.Valid) return;
            
            initialized = false;
            TagsDebugManager.Instance.BackCommunicate(this);
        }
    }
}
