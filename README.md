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
FESGAS is a modular and extensible **Gameplay Ability System** framework for Unity, designed for high-performance, flexibility, and scalability. The framework offers a unique take on traditional GAS features in addition to a powerful process management controller. FESGAS empowers designers and engineers to manage attribute-based systems, create complex abilities, and handle runtime processes performantly. This framework is built with modularity at the forefront of development, such that most aspects of integration are wrapped under derivable logic systems built with extensibility in mind. While it is originally inspired by @sjai013's [Unity Gameplay Ability System](https://github.com/sjai013/unity-gameplay-ability-system) and Unreal Engine's [Gameplay Ability System](https://dev.epicgames.com/documentation/en-us/unreal-engine/gameplay-ability-system-for-unreal-engine), FESGAS differs in some key aspects, which this README will demonstrate in detail. Thank you for checking out my framework!

### About FESGAS
This framework is the combination of two powerful systems folded into one. The first is the **Gameplay Ability System**, which handles abilities, attributes, effects, and various related events. This component is most similar to traditional GAS systems with two key differences: how abilities are defined and the use of ScriptableObject-based events throughout the lifecycle of effects. The second component in the framework is the **ProcessControl** system, which is a powerful management tool for managing processes within your game, utilizing UniTask for zero-allocation, performant deployment of MonoBehaviour and non-MonoBehaviour tasks alike.

### Preliminary Acknowledgements
This framework is the culmination of my journey into game development through university and is a project purely born of passion and excitement for the craft. I would not have been able to develop this without the encouragement and support of my mentors, professors, and peers. While it has been a laborious process developing, testing, and deploying this framework, the fruits of labor are immensely exciting, and I cannot wait to integrate this framework into my next Unity project.

### Definitions
- **_Process_**
    - **Process** refers to MonoBehaviour and non-MonoBehaviour tasks which run via a UniTask async container and/or Unity's Update/LateUpdate/FixedUpdate loop.
    - Example: Projectiles, Actors, Observers

## 3. Features
- Ability System
- Attribute System
- Modular Events
- Process Management

## 4. Getting Started

### Requirements
- Unity 2022.3 LTS or newer
- [UniTask](https://github.com/Cysharp/UniTask) package installed (@Cysharp)
    - Follow the instructions [here](https://github.com/Cysharp/UniTask?tab=readme-ov-file#upm-package) under "_Install via GIT URL_"
 - [SerializedDictionary](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-243052) package installed (@AYellowPaper)

### Installation
1. Clone or download the repository
2. Import the project into Unity
3. Install UniTask via UPM or manual package import
4. (Optional) Import _Demo_ directory

### Setting Up
- Boostrapper (Critical)
    - Create a `Bootstrapper` object in your scene (a default prefab is provided in the _Demo_ directory)
    - Assign the `ProcessControl` and `GameRoot` prefabs (also provided) in the `Bootstrapper` inspector window
- GAS System Data
    - Assign `Ability`s, `ImpactWorker`s, `AttributeSet`, `AttributeChangeEvent`s, and `TagWorker`s
- GAS Actors
    - Add the `GASComponent` component to your actors (this will automatically add the `AttributeSystemComponent` and `AbilitySystemComponent` components)
    - Assign `GASSystemData` to your actors
 
##### Disclaimer
Integrating FESGAS into an existing project well into development can be very tricky because of how FESGAS expects processes to be managed.

## 5. Architecture Overview

### Attribute System
- Tracks gameplay attributes dynamically at runtime (e.g. Health, MoveSpeed, Armor)
- Easy extension for any variety of custom attributes and attribute-derived values

### Modular Events
- Manage and react to changes within the system
    - `AttributeChangeEvent`: Manipulate impacts and attribute values (e.g. clamp health, damage amplification/reduction)
    - `ImpactWorker`: React to attribute impact (e.g. lifesteal, damage reflection)
    - `TagWorker`: Perform actions when/while tags are applied or removed (e.g. apply flame particles while the _burning_ tag is applied)
    - `EffectWorker`: Monitor systems while an effect is applied (e.g. Dota 2's Ancient Apparition's Ice Blast kills when below a certain threshold)

### Gameplay Effects
- Modify target attributes under a variety of parameters, including duration, tick rate, and reversing impact after removal
- Define as **Instant**, **Durational**, or **Infinite**
- Supports application, ongoing, and removal requirements

### Gameplay Abilities
- Encapsulate complex gameplay logic through modular, reusable components
- Supports activation conditions, cooldowns/costs, and lifecycle steps
- Designed to separate logic (how the ability executes) from data (what an ability does)

### Proxy Task System
- Lightweight, modular, async tasks that build ability behaviours as chains of operations
- Allows precise control over the lifecycle of an ability
- Designed to minimize coupling between the ability and what it executes

### Gameplay Processes
- Runtime systems (e.g. projectiles, AOEs, timers) managed via the centralized `ProcessControl`
- Supports Self-Terminating, RunThenWait, and ExternalControl lifecycles
- Utilizes UniTask for highly scalable and lightweight deployment of hundreds of active processes

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
