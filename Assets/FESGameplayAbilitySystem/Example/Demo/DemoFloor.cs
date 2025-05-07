using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class DemoFloor : LazyMonoProcess
    {
        [Header("Demo Floor")]
        
        public Vector3 PlayerPosition;
        public GASComponentBase PlayerPrefab;
        
        public Vector3 EnemyPosition;
        public GASComponentBase EnemyPrefab;
        
        public override void WhenInitialize(ProcessRelay relay)
        {
            base.WhenInitialize(relay);

            var playerData = new ProcessDataPacket();
            playerData.AddPayload(ESourceTargetData.Data, GameRoot.PositionTag, PlayerPosition);

            ProcessControl.Instance.Register(PlayerPrefab, playerData, out var playerRelay);
            if (playerRelay.TryGetProcess(out GASComponentBase player))
            {
                FindObjectOfType<DemoInputHandler>().System = player;
            }
            
            var enemyData = new ProcessDataPacket();
            enemyData.AddPayload(ESourceTargetData.Data, GameRoot.PositionTag, EnemyPosition);
            
            ProcessControl.Instance.Register(EnemyPrefab, enemyData, out _);
        }
    }
}
