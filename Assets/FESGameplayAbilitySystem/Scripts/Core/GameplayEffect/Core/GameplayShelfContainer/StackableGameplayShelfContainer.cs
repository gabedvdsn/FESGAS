using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class StackableGameplayShelfContainer : AbstractGameplayEffectShelfContainer
    {
        private List<StackableContainerPacket> Packets;
        private int stacks;

        public override float DurationRemaining => Packets.Count > 0 ? Packets[^1].DurationRemaining : -1f;
        public override float TimeUntilPeriodTick => Packets.Count > 0 ? Packets[0].TimeUntilPeriodTick : float.MaxValue;
        
        private StackableGameplayShelfContainer(GameplayEffectSpec spec, bool ongoing) : base(spec, ongoing)
        {
            Packets = new List<StackableContainerPacket>();
            Spec.Base.DurationSpecification.ApplyDurationSpecifications(this);
        }

        public static AbstractGameplayEffectShelfContainer Generate(GameplayEffectSpec spec, bool ongoing)
        {
            var container = new StackableGameplayShelfContainer(spec, ongoing);
            container.Stack();
            return container;
        }
        
        public override void SetDurationRemaining(float duration)
        {
            for (int i = 0; i < Packets.Count; i++)
            {
                if (Packets[i].DurationRemaining > duration) Packets[i] = new StackableContainerPacket(duration, periodDuration);
            }
        }
        
        public override void SetTimeUntilPeriodTick(float duration)
        {
            for (int i = 0; i < Packets.Count; i++)
            {
                if (Packets[i].TimeUntilPeriodTick > duration) Packets[i] = new StackableContainerPacket(totalDuration, duration);
            }
        }

        public override int GetStacks()
        {
            return stacks;
        }

        public override void UpdateTimeRemaining(float deltaTime)
        {
            int removeIndex = -1;
            foreach (StackableContainerPacket packet in Packets)
            {
                packet.UpdateTimeRemaining(deltaTime);
                if (packet.DurationRemaining <= 0f) removeIndex += 1;
            }
            
            if (removeIndex >= 0) RemoveStack(removeIndex);
        }

        private void RemoveStack(int rangeIndex)
        {
            for (int i = 0; i <= rangeIndex; i++) Packets.RemoveAt(i);
            stacks -= rangeIndex + 1;
        }
        
        public override void TickPeriodic(float deltaTime, out int executeTicks)
        {
            executeTicks = 0;
            if (Spec.Base.ImpactSpecification.ReApplicationPolicy is GameplayEffectApplicationPolicy.StackExtend or GameplayEffectApplicationPolicy.StackRefresh)
            {
                Packets[0].TickPeriodic(deltaTime, periodDuration, out bool execute);
                if (execute) executeTicks += stacks;
            }
            else
            {
                foreach (StackableContainerPacket packet in Packets)
                {
                    packet.TickPeriodic(deltaTime, periodDuration, out bool execute);
                    if (execute) executeTicks += 1;
                }
            }
        }
        public override void Refresh()
        {
            Packets[0].Refresh(totalDuration, periodDuration);
            stacks += 1;
        }
        public override void Extend(float duration)
        {
            Packets[0].Extend(DurationRemaining, duration);
            stacks += 1;
        }
        public override void Stack()
        {
            Packets.Add(new StackableContainerPacket(totalDuration, periodDuration));
            stacks += 1;
        }
        
        private class StackableContainerPacket
        {
            public float DurationRemaining;
            public float TimeUntilPeriodTick;

            public StackableContainerPacket(float totalDuration, float periodDuration)
            {
                DurationRemaining = totalDuration;
                TimeUntilPeriodTick = periodDuration;
            }

            public void UpdateTimeRemaining(float deltaTime)
            {
                DurationRemaining -= deltaTime;
            }

            public void TickPeriodic(float deltaTime, float periodDuration, out bool executeTick)
            {
                TimeUntilPeriodTick -= deltaTime;
                if (TimeUntilPeriodTick <= 0f)
                {
                    TimeUntilPeriodTick += periodDuration;
                    executeTick = true;
                }
                else
                {
                    executeTick = false;
                }
            }

            public void Refresh(float totalDuration, float periodDuration)
            {
                DurationRemaining = totalDuration;
                TimeUntilPeriodTick = periodDuration;
            }

            public void Extend(float duration, float extendDuration) => DurationRemaining = duration + extendDuration;
        }
    }
}
