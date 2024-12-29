![image](https://github.com/user-attachments/assets/ea55e71b-cb68-4995-9ef4-fb8e9da299ea)

# Unity Gameplay Ability System Framework (FESGAS)
![Static Badge](https://img.shields.io/badge/Unity-2022.3.37f1-brightgreen)

## 1. Table of Contents
- [Introduction](#2-introduction)
- [Features](#3-features)
- [Getting Started](#4-getting-started)
- [Architecture Overview](#5-architecture-overview)
- [API Documentation](#6-api-documentation)
- [Examples](#7-ability-examples)
- [Customization](#8-customization)
- [Best Practices](#9-best-practices)
- [Contributing](#10-contributing)
- [License](#11-license)
- [Acknowledgements](#12-acknoledgements)

## 2. Introduction
FESGAS is a modular, composition-inspired, and extensible ability-handling system that supports a built-in attribute system. This framework provides the material necessary to build complex abilities and gameplay effects, as well as monitor and manage interactions between systems.

**Goals:**
- Modular, reusable, and extendable code
- Highly customizable abilities and gameplay effects
- Optimized for large pools of actors in real-time gameplay
- Close monitoring and control over every stage of system interaction

## 3. Features
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

## 4. Getting Started

#### 4.1 Prerequisites

- Unity Version 2022.3.37f1
- Dependencies
    - `SerializedDictionary` from [@AYellowPaper]() on the [Unity Asset Store](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-243052)
        - Install via the Unity Asset Store (using the link above) and add to project via the `Project Manager` window in Unity
    - `UniTask` from @Cysharp on [GitHub](https://github.com/Cysharp/UniTask)
        - Follow the instructions [here](https://github.com/Cysharp/UniTask?tab=readme-ov-file#upm-package) under "Instal via GIT URL"
     
#### 4.2 Installation
- Instructions ...

#### 4.3 Basic Setup
- Scene Actors
    - Add the `GASComponent` script to the actor (this will automatically add the `AbilitySystemComponent` and `AttributeSystemComponent` scripts)
    - Within the `AttributeSystemComponent`, assign an `AttributeSet` data and any `AttributeChangeEvent` datas
    - Within the `AbilitySystemComponent`, assign the max number of abilities
    - Within the `GASComponent`, assign starting abilities and starting level
- Create `Ability` datas by right-clicking, then navigating to `FESGAS/Abilities/Ability`
    - Create and implement `ProxyTask` datas (see under [5.2 Ability Proxy Tasks](#5-2-ability-proxy-tasks))
- Create `GameplayEffect` datas by right-clicking, then navigating to `FESGAS/Gameplay Effect`
- Create `MagnitudeModifier` datas by right-clicking, then navigating to `FESGAS/Magnitude Modifier/...`
- Create `Attribute` datas by right-clicking, then navigating to `FESGAS/Attribute`

## 5. Architecture Overview

#### 5.1 Core Components
The GAS component is the heart and soul of the framework. It ties together the `AttributeSystemComponent` and `AbilitySystemComponent` into one cohesive system. What follows is a brief breakdown of how the different components work together and interact:

- `GASComponent`
    - The conduit through which all external systems interact with and impact an Actor
    - Handle `GameplayEffect` application, removal, update, and validation with respect to the `AttributeSystemComponent`
    - Give, revoke, manage, and activate `Abilities` via the `AbilitySystemComponent`
- `AttributeSystemComponent`
    - Monitor and manage frame-by-frame modification of `Attributes`
    - Activate `AttributeChangeEvent` before and after `Attributes` are modified
- `AbilitySystemComponent`
    - Monitor, give & revoke, validate, and activate `Abilities`
 
#### 5.2 Ability Proxy Tasks
The unique behavior of an `Ability` is encapsulated in an `AbilityProxySpecification` data, which is translated to an `AbilityProxy` object at runtime. Within the `AbilityProxySpecification` data, the `Ability's` behavioral lifecycle is defined as a list of semi-independent, sequential `AbilityProxyStages`. These `AbilityProxyStages` define a collection of `AbilityProxyTasks`. An `Ability's` behavioral lifecycle is the execution of any/all `AbilityProxyTasks` within each `AbilityProxyStage`, sequentially. If an `AbilityProxyTask` fails at any point, any actively executing `AbilityProxyTasks` are interrupted, and the `Ability` is deactivated. This lifecycle is described as:

1. `Ability` activation is requested, validated, and relayed to the `AbilitySystemComponent`
2. The `AbilitySystemComponent` activates (and *awaits* the active lifecycle of) the `AbilityProxy`
3. The `AbilityProxy` activates the first, yet-to-be-activated `AbilityProxyStage`\
   a. Each `AbilityProxyTask` receives a `ProxyDataPacket` from/to which it can read/write data that is passed between `AbilityProxyStages`\
   b. Activate and *await* the conclusion of any/all `AbilityProxyTask`\
   c. Continue from **Step 3**

#### 5.3 Gameplay Tags
...

#### 5.4 Extensibility
...

## 6. API Documentation

## 7. Ability Examples

### Purifying Flames (Oracle, Dota 2)
At level 1, when cast on a target, this ability immediately deals 90 damage and then heals the target for 150 health over 10 seconds.

<details>
    <summary>Create Gameplay Effects</summary>
    
1. Instant Damage
This effect will immediately deal 50 damage to the target.
- **Impact Specification**
    - **Attribute Target:** `Attribute.Health`
    - **Value Target:** Current
    - **Impact Operation:** Add
    - **Magnitude:** -90
    - **Magnitude Calculation:** `MagnitudeModifier.Constant`
    - **Magnitude Calculation Operation:** Multiply
- **Duration Specification**
    - **Duration Policy:** Instant
    - ...

2. Heal Over Time
This effect will heal the target by 10 every .5 seconds for 3 seconds, healing for 60 health in total.
- **Impact Specification**
    - **Attribute Target:** `Attribute.Health`
    - **Value Target:** Current
    - **Impact Operation:** Add
    - **Magnitude:** 15
    - **Magnitude Calculation:** `MagnitudeModifier.Constant`
    - **Magnitude Calculation Operation:** Multiply
- **Duration Specification**
    - **Duration Policy:** Durational
    - **TickOnApplication:** false
    - **Duration:** 10
    - **Duration Calculation:** `MagnitudeModifier.Constant`
    - **Duration Calculation Operation:** Multiply
    - **Ticks:** 10
    - **Tick Calculation:** `MagnitudeModifier.Constant`
    - **Tick Calculation Operation:** Multiply
    - ...
</details>
<details>
    <summary>Create Proxy Task</summary>

1. Create `AbstractAbilityProxyTask.ApplyEffectProxyTask` as a subclass of `AbstractAbilityProxyTask`\
   a. This task will take in a `GameplayEffectScriptableObject`\
   b. Within the `ApplyEffectProxyTask.Activate(...)` method, 


1. Fill in identifying information, `Tag` validation requirements, etc...
2. Create the `AbilityProxySpecification`\
   a. **Use Implicit Data:** true (When the `Ability` is activated in the `AbilitySystemComponent` script, it will capture the associated `GASComponent` within a `ProxyDataPacket` object)\
   b. **Owner As:** Target (The captured `GASComponent` will be captured as a `Target`)\
   c. Add an `AbilityProxyStage`\
       i. **Task Policy:** Any\
       ii. ...

</details>
After an activation request is validated and relayed to the `AbilitySystemComponent`, the ...

### This project (and README) is still being developed.
