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

### 2.1 Preliminary Acknowledgements
This framework is the culmination of my journey into game development through university and is a project purely born of passion and excitement for the craft. I would not have been able to develop this without the encouragement and support of my mentors, professors, and peers. While it has been a laborious process developing, testing, and deploying this framework, the fruits of labor are immensely exciting, and I cannot wait to integrate this framework into my next Unity project.

## 3. Features
- Ability System
    - Task-Based Ability Definition
    - UniTask
- Attribute System
- Process Management

## 4. Getting Started

### 4.1 Requirements
- Unity 2022.3 LTS or newer
- [UniTask](https://github.com/Cysharp/UniTask) package installed (@Cysharp)
    - Follow the instructions [here](https://github.com/Cysharp/UniTask?tab=readme-ov-file#upm-package) under "_Install via GIT URL_"
 - [SerializedDictionary](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-243052) package installed (@AYellowPaper)

### 4.2 Installation
1. Clone or download the repository
2. Import the project into Unity
3. Install UniTask via UPM or manual package import
4. (Optional) Import _Demo_ directory

#### 4.3 Setting Up
- GAS Actors
    - Add the `GASComponent` component to your actors (this will automatically add the `AttributeSystemComponent` and `AbilitySystemComponent` components)
    - 

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
