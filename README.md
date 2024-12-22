# FESGAS

## What Is It
FES Gameplay Ability System (FESGAS) is a modular, composition-based ability handling system that supports a built-in attribute system. Interactions between abilities and attribute systems are handled through gameplay effects, which are modular and heavily customizable. 

## What Makes It Special
As opposed to abstracting abilities into sub-classes based on their defining behaviors (e.g., aimed projectiles, channeled, targeted instantiations, etc...), there is one master definition of what an Ability is (encapsulated within a Scriptable Object). An instance of an Ability allows for its behavior to be defined by modular proxy tasks, which are individual definitions of unique behaviors. These tasks are separated into stages, such that any/all tasks in a stage must execute completely before the next stage is activated; an Ability's behavior is the complete (or forcibly incomplete) execution of all tasks in a stage for each stage. During the activation phase of an ability, a data packet is passed around, which stores and relays information between tasks. The most critical aspect of these packets is with respect to assigning target game objects or positions, which are then utilized by future tasks.

### Example: Pugna's Life Drain (Dota 2)
Let's assume that all validation has been accepted and that Pugna is targeting an Enemy hero. 

This ability can be handled in a single stage and with only 3 proxy tasks.

**Stage 1**
1. Channel Proxy Task
    -  This task will activate a channel, which will control any UI elements but will otherwise only manage the progress of the Channel
2. Target Apply->Remove Proxy Task
    - This task will apply an infinite-duration gameplay effect to the target enemy
    - This gameplay effect will apply damage to the enemy hero
    - When the Channel task is ended (concludes or is interrupted), the effect is removed
3. Source Apply->Remove Proxy Task
    - This task will apply an infinite-duration gameplay effect to Pugna (the Source)
    - This gameplay effect will apply healing to Pugna
    - When the Channel task is ended (concludes or is interrupted), the effect is removed

In this example, we can assume that the healing received by Pugna is independent of the damage done to the enemy Hero. If you would like the healing to be dependent on the damage done, you can attach an Impact Listener to the gameplay effect that will coordinate that relationship.

An important note is about animations. Animations can be handled by proxy tasks as well but are left out of the example to help with concision.

### Example: KOTL's Illuminate (Dota 2)
Let's assume that all validation has been accepted.

This ability can be handled in 2 stages.

**Stage 1**
1. Channel Proxy Task
    - This task will activate a channel, which will control any UI elements but will otherwise only manage the progress of the Channel

**Stage 2**
1. Instantiate Behavior Proxy Task
    - This task is responsible for instantiating the Illuminate prefab and supplying it with the necessary information (e.g. cast position)
  
