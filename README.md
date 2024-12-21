# FESGAS

### What Is It
FES Gameplay Ability System (FESGAS) is a modular, composition-based ability handling system that supports a built-in attribute system. Interactions between abilities and attribute systems are handled through gameplay effects, which are modular and heavily customizable. 

### What Makes It Special
As opposed to abstracting abilities into sub-classes based on their defining behaviors (e.g., casting projectile, channeling, instantiating an AOE, etc...), there is one master definition of what an Ability is (encapsulated within a Scriptable Object). An instance of an Ability allows for its behavior to be defined by modular proxy tasks, which are individual definitions of unique behaviors. Groups of proxy tasks are separated into stages, such that any/all tasks in a stage must execute completely before the next stage is activated; an Ability's behavior is the complete (or forcibly incomplete) execution of all tasks in a stage for each stage.
