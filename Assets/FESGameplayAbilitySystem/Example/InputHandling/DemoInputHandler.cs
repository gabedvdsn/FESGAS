﻿using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class DemoInputHandler : MonoBehaviour
    {
        public GASComponentBase System;
        public List<KeyCode> AbilityKeyMaps;

        private void Update()
        {
            for (int i = 0; i < AbilityKeyMaps.Count; i++)
            {
                if (Input.GetKeyDown(AbilityKeyMaps[i])) System.AbilitySystem.TryActivateAbility(i);
            }
        }
    }
}
