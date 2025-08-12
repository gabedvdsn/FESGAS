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
            playerData.AddPayload(Tags.PAYLOAD_POSITION, PlayerPosition);
            playerData.AddPayload(Tags.PAYLOAD_AFFILIATION, Tags.AFFILIATION_GREEN);

            ProcessControl.Instance.Register(PlayerPrefab, playerData, out var playerRelay);
            if (playerRelay.TryGetProcess(out GASComponentBase player))
            {
                var inputHandler = FindObjectOfType<DemoInputHandler>();
                if (inputHandler) inputHandler.System = player;
            }
            
            var enemyData = ProcessDataPacket.RootDefault();
            enemyData.AddPayload(Tags.PAYLOAD_POSITION, EnemyPosition);
            enemyData.AddPayload(Tags.PAYLOAD_AFFILIATION, Tags.AFFILIATION_RED);
            
            ProcessControl.Instance.Register(EnemyPrefab, enemyData, out _);
        }
    }
}
