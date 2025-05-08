![image](https://github.com/user-attachments/assets/ea55e71b-cb68-4995-9ef4-fb8e9da299ea)

# Unity Gameplay Ability System Framework (FESGAS)
![Static Badge](https://img.shields.io/badge/Unity-2022.3.37f1-brightgreen)

## 1. Table of Contents
- [Introduction](#2-introduction)
- [Features](#3-features)
- [Getting Started](#4-getting-started)
- [Architecture Overview](#5-architecture-overview)
- [Customization](#6-customization)
- [Contributing](#7-contributing)
- [License](#8-license)
- [Acknowledgements](#9-acknowledgements)

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

#### Attribute System
- Tracks gameplay attributes dynamically at runtime (e.g. Health, MoveSpeed, Armor)
- Easy extension for any variety of custom attributes and attribute-derived values

#### Modular Events
- Manage and react to changes within the system
    - `AttributeChangeEvent`: Manipulate impacts and attribute values (e.g. clamp health, damage amplification/reduction)
    - `ImpactWorker`: React to attribute impact (e.g. lifesteal, damage reflection)
    - `TagWorker`: Perform actions when/while tags are applied or removed (e.g. apply flame particles while the _burning_ tag is applied)
    - `EffectWorker`: Monitor systems while an effect is applied (e.g. Dota 2's Ancient Apparition's Ice Blast kills when below a certain threshold)

#### Gameplay Effects
- Modify target attributes under a variety of parameters, including duration, tick rate, and reversing impact after removal
- Define as **Instant**, **Durational**, or **Infinite**
- Supports application, ongoing, and removal requirements

#### Gameplay Abilities
- Encapsulate complex gameplay logic through modular, reusable components
- Supports activation conditions, cooldowns/costs, and lifecycle steps
- Designed to separate logic (how the ability executes) from data (what an ability does)

#### Proxy Task System
- Lightweight, modular, async tasks that build ability behaviours as chains of operations
- Allows precise control over the lifecycle of an ability
- Designed to minimize coupling between the ability and what it executes

#### Gameplay Processes
- Runtime systems (e.g. projectiles, AOEs, timers) managed via the centralized `ProcessControl`
- Supports Self-Terminating, RunThenWait, and ExternalControl lifecycles
- Utilizes UniTask for highly scalable and lightweight deployment of hundreds of active processes
- Define custom `MonoProcessInstantiator` classes to handle custom logic behind MonoBehaviour process instantiation (e.g. object pooling)

### 5.1 A Deeper Look
Let's take a deeper look at some of the core systems. 

#### Setting Up Your Attributes
An Actor's set of Attributes is defined in its `AttributeSet`. It is easy to define how the attribute is initialized, as well as its overflow policy. Easily create more complex sets of Attributes by combining multiple `AttributeSet`s with respect to some collision resolution policy.

![image](https://github.com/user-attachments/assets/e9a3fa80-2411-490e-8a72-55867b674107)

#### How Abilities Work
FESGAS distinguishes itself from traditional GAS frameworks by how it approaches Abilities. As opposed to defining abilities by what they do, abilities are simply a collection of tasks separated into distinct stages. These stages, and the tasks therein (called `ProxyTasks`), are what define the behaviour of the Ability.

One of the biggest issues with traditional approaches is that Ability inheritance trees quickly become completely overdeveloped and unnecessarily complicated. Abilities that perform different actions are separated into subclasses, but when a particular Ability calls for their logic to coincide, integration can be downright painful. One of my goals with FESGAS was to solve this issue and avoid subclassing the `Ability` class altogether.

![image](https://github.com/user-attachments/assets/9a1a8b97-81e4-4c5b-803d-5a35b681d374)

Abilities are split into two stages:
1. Targeting\
    a. Some targeting logic is executed (e.g. waiting for user input or automatically finding the nearest enemy)\
    b. Pertinent data is stored in a shared packet passed between the stages and tasks of execution
2. Activation\
    a. Each `ProxyStage` is iterated through, and all `ProxyTask`s within the stage are activated\
    b. The stage is ended when Any/All of the `ProxyTask`s complete

Some examples of `ProxyTask`s are:
- Channeling
- Applying `GameplayEffect`s
- Creating processes

#### Making a Gameplay Effect
Gameplay effects are the heart and soul of any GAS framework. The setup for GEs in FESGAS will be familiar if you have experience with other traditional frameworks. The GE is defined by its Impact and Duration specifications. A set of Source and Target requirements define the parameters for application, ongoing, and removal conditions. To minimize overhead, these requirements are only validated against when pertinent parts of the system are updated, such as other GEs being applied or tags being applied/removed.

![image](https://github.com/user-attachments/assets/e881be2c-7596-4e3e-88a9-5735f2d932bc)

If the situation arises where you must create a GE outside of a ScriptableObject, utilize the `EffectBuilder` factory class to create custom effects during runtime. 

#### Process Handling
There are two types of processes: MonoBehaviour processes and non-MonoBehaviour processes. Any MonoBehaviour process must inherit from the `AbstractMonoProcess` class, and it is recommended to subclass directly from the `LazyMonoProcess` class, which predefines some helpful logic.

The most-used method for registering mono processes is via a `CreateMonoProcessProxyTask`. Non-MonoBehaviour processes can be registered as required by accessing `ProcessControl.Instance.Register(...)` as needed. For MonoBehaviour processes specifically, a `ProcessDataPacket` is passed into the `ProcessControl.Register` call, which should contain any pertinent data the process may require. As you define processes, please be aware of the following critical aspects:
1. Processes define their own lifecycle, from initialization to termination. Processes are excluded from any Unity event loops by default.
2. When a process accesses its `ProcessDataPacket`, it is assuming that the data was categorized correctly before registration. This logic must be clearly outlined when setting up processes and when adding data to the `ProcessDataPacket`. If the process attempts to gather data but cannot find any, a default value will be used. See below for more information on `ProcessDataValues`.

![image](https://github.com/user-attachments/assets/f523481c-0e77-4d18-8653-b0f99d2d02ac)

#### Working With `ProcessDataPackets`
Used to hold and disseminate information across decoupled instances. PDPs store data in a generic dictionary guarded by a classifier `GameplayTag` and an enum target. The PDP itself will never know the scope of its use; instead, the decoupled instances are responsible for providing and procuring information across relevant classifiers. There are two types of `ProcessDataPackets`, being `ProxyDataPacket` and `ProcessDataPacket`. The only difference is that the former always originates from Abilities and contains a reference to the calling Ability.

An instance that provides information is called a _supplier_, and an instance that procures information is called a _consumer_. Instances can be a supplier, consumer, or both. There are 6 distinct classifier tags used by default, which are defined in the `GameRoot` singleton; any instance along the use-path of a PDV can supply and/or procure information. These predefined tags are: **Generic**, **GAS**, **Derivation**, **Position**, **Rotation**, and **Transform**. It is _critical_ to have a solid and consistent strategy about how data is supplied and retrieved. For example, the initial position, rotation, and parent transform of a new MonoBehaviour process should always be stored under the Data target classifier.

The data within a PDP is called its _payload_, and is set up with the following structure: **Payload[Tag][Target] = List<_object_>**. Let's run through an example:

##### Example: Tracking Fireball
Given an Ability that creates a Fireball that follows its target until it collides with it:

1. The Ability is activated and creates a new PDP. Implicit data is added to the PDP.\
        a. PDP.AddPayload<EffectDerivation>(Data, GameRoot.Derivation, derivation: EffectDerivation) => The activated ability\
        b. PDP.AddPayload<GAS>(Source, GameRoot.GAS, source: GAS) => The source GAS system that activated the Ability\
        c. PDP.AddPayload<Transform>(Data, GameRoot.Position, initialPosition: Transform) => The firepoint Transform at which to spawn the Fireball
2. The Ability's targeting is activated\
        a. PDP.GetPayload(Data, GameRoot.Derivation, out EffectDerivation derivation) -> if (derivation.Affiliation != target.Affiliation) ...\
        b. PDP.AddPayload<GAS>(Target, GameRoot.GAS, target: GAS) => The target GAS that the Fireball will track\
        c. PDP.AddPayload<Quaternion>(Data, GameRoot.Rotation, initialRotation: Quaternion) => The initial rotation of the Fireball
3. The Ability's `CreateMonoProcess` task is activated, which stores a prefab of the Fireball prefab\
        a. PDP.GetPayload(Data, GameRoot.Position, out Transform initialPosition) -> Fireball.position = initialPosition.position\
        b. PDP.GetPayload(Data, GameRoot.Rotation, out Quaternion initialRotation) -> Fireball.rotation = initialRotation\
        c. PDP.GetPayload(Target, GameRoot.GAS, out GAS target) -> Fireball.target = target

##### Example: Damaging Pit
Given an Ability that creates a Damaging Pit that damages enemies that walk into it:

1. The Ability is activated and creates a new PDP. Implicit data is added to the PDP.\
        a. PDP.AddPayload<EffectDerivation>(Data, GameRoot.Derivation, derivation: EffectDerivation) => The activated ability\
        b. PDP.AddPayload<GAS>(Source, GameRoot.GAS, source: GAS) => The source GAS system that activated the Ability
2. The Ability's targeting is activated\
        a. PDP.AddPayload<Vector3>(Data, GameRoot.Position, initialPosition: Vector3) => The initial position of the Pit
3. The Ability's `CreateMonoProcess` task is activated, which stores a prefab of the Pit prefab\
        a. PDP.GetPayload(Data, GameRoot.Position, out Vector3 initialPosition) -> Pit.position = initialPosition
4. A GAS actor walks into the Pit\
        a. PDP.GetPaylaod(Data, GameRoot.Derivation, derivation: EffectDerivation) -> if (actor.Affiliation != derivation.Affiliation) ...

## 6. Customization
The FESGAS framework is highly extensible due to its consistent focus on ScriptableObject-based development. Easily extend modular events, proxy tasks, and processes to fit the needs of your project.

## 7. Contributing
Contributions are welcome! If you have suggestions, bug reports, or enhancements, please submit an issue or pull request.

## 8. License
FESGAS is licensed under the MIT License. See the LICENSE file for details.

## 9. Acknowledgements
Special thanks to @sjai013 for kickstarting this adventure by inspiring me to create my own. I utilized his framework for my individual Game Development Capstone project at university. Thank you to my professors and peers for advice and guidance.
