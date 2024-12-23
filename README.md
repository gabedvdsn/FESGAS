![image](https://github.com/user-attachments/assets/ea55e71b-cb68-4995-9ef4-fb8e9da299ea)

# Unity Gameplay Ability System Framework (FESGAS)
![Static Badge](https://img.shields.io/badge/UnityVersion-2022.3.37f1-brightgreen)

1. ## Table of Contents
- [Introduction](##introduction)
- [Features](#features)
- [Getting Started](#getting-started)
- [Architecture Overview](#architecture-overview)
- [API Documentation](#api-documentation)
- [Examples](#examples)
- [Customization](#customization)
- [Best Practices](#best-practices)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgements](#acknoledgements)

2. ## Introduction
FESGAS is a modular, composition-inspired, and extensible ability-handling system that supports a built-in attribute system. This framework provides the material necessary to build complex abilities and gameplay effects, as well as monitor and manage interactions between systems.

**Goals:**
- Modular, reusable, and extendable code
- Highly customizable abilities and gameplay effects
- Optimized for large pools of actors in real-time gameplay
- Close monitoring and control over every stage of system interaction

3. ## Features
This framework is inspired by @sjai013 [Unity Gameplay Ability System](https://github.com/sjai013/unity-gameplay-ability-system), which was itself inspired by Unreal Engine's [GAS](https://dev.epicgames.com/documentation/en-us/unreal-engine/gameplay-ability-system-for-unreal-engine). My initial experience working with GAS came with @sjai013's system, but I found that it didn't meet all the needs of my projects; I wanted a framework that was just as modular, with less redundancy, and greater control over an ability's behavior.

- Ability System
    - Master Ability class (no subclassing)
    - Modular control structures to manage magnitude-related scaling
    - Non-exclusive support for passive, active, toggled, and triggered abilities
    - Proxy Tasks
        - Completely modular, extensible definitions of ability-related behaviors
        - Integration with Unity's animation system
- Ability Handling
    - Ability use validation
    - Cooldown and cost management
    - Implicit instructions to activated abilities
- Attribute System
    - Efficient handling of impacted attributes
    - Pre and post-attribute impact monitoring and management

4. ## Getting Started

**4.1 Prerequisites**

- Unity Version 2022.3.37f1
- Dependencies
    - `SerializedDictionary` from [@AYellowPaper]() on the [Unity Asset Store](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-243052)
        - Install via the Unity Asset Store (using the link above) and add to project via the `Project Manager` window in Unity
    - `UniTask` from @Cysharp on [GitHub](https://github.com/Cysharp/UniTask)
        - Follow the instructions [here](https://github.com/Cysharp/UniTask?tab=readme-ov-file#upm-package) under "Instal via GIT URL"
     
**4.2 Installation**
- Instructions ...

**4.3 Basic Setup**
- Scene Actors
    - Add the `GASComponent` script to the actor (this will automatically add the `AbilitySystemComponent` and `AttributeSystemComponent` scripts)
    - Within the `AttributeSystemComponent`, assign an `AttributeSet` data and any `AttributeChangeEvent` datas
    - Within the `AbilitySystemComponent`, assign the max number of abilities
    - Within the `GASComponent`, assign starting abilities and starting level
- Create `Ability` datas by right-clicking, then navigating to `FESGAS/Abilities/Ability`
    - Create and implement `ProxyTask` datas (see under [Proxy Tasks](#proxy-tasks))
- Create `GameplayEffect` datas by right-clicking, then navigating to `FESGAS/Gameplay Effect`
- Create `MagnitudeModifier` datas by right-clicking, then navigating to `FESGAS/Magnitude Modifier/...`
- Create `Attribute` datas by right-clicking, then navigating to `FESGAS/Attribute`

5. ## Architecture Overview
ASDASD

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

## Another Section
