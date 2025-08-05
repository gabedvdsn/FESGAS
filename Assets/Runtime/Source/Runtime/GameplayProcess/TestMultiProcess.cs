using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TestMultiProcess : LazyMonoProcess
    {
        private void Start()
        {
            ProcessControl.Instance.Register(this, out _);
        }
    }
}
