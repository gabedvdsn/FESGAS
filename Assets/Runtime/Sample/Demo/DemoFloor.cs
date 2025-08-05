using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem.Demo
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

            var playerData = ProcessDataPacket.RootDefault();
            playerData.AddPayload(GameRoot.PositionTag, ESourceTargetData.Data, PlayerPosition);
            playerData.AddPayload(GameRoot.GenericTag, ESourceTargetData.Data, GameRoot.Instance.AllyTag);

            ProcessControl.Instance.Register(PlayerPrefab, playerData, out var playerRelay);
            if (playerRelay.TryGetProcess(out GASComponentBase player))
            {
                var inputHandler = FindObjectOfType<DemoInputHandler>();
                if (inputHandler) inputHandler.System = player;
            }
            
            var enemyData = ProcessDataPacket.RootDefault();
            enemyData.AddPayload(GameRoot.PositionTag, ESourceTargetData.Data, EnemyPosition);
            enemyData.AddPayload(GameRoot.GenericTag, ESourceTargetData.Data, GameRoot.Instance.EnemyTag);
            
            ProcessControl.Instance.Register(EnemyPrefab, enemyData, out _);
        }
    }
}
