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
            playerData.AddPayload(GameRoot.PositionTag, ESourceTargetData.Data, PlayerPosition);
            playerData.AddPayload(GameRoot.GenericTag, ESourceTargetData.Data, GameRoot.Instance.AllyTag);

            ProcessControl.Instance.Register(PlayerPrefab, playerData, out var playerRelay);
            if (playerRelay.TryGetProcess(out GASComponentBase player))
            {
                FindObjectOfType<DemoInputHandler>().System = player;
            }
            
            var enemyData = new ProcessDataPacket();
            enemyData.AddPayload(GameRoot.PositionTag, ESourceTargetData.Data, EnemyPosition);
            enemyData.AddPayload(GameRoot.GenericTag, ESourceTargetData.Data, GameRoot.Instance.EnemyTag);
            
            ProcessControl.Instance.Register(EnemyPrefab, enemyData, out _);
        }
    }
}
