# **Talking Coin (2024)**

Experimental AI-Driven Trading Simulation • LLM + TTS Integration Prototype

*Steam Page:* https://store.steampowered.com/app/3247860/Talking_Coin/

*Youtube videos:* https://www.youtube.com/@ArchDawnDev/shorts

# **⭐ Project Overview**

Talking Coin was an ambitious experiment designed to push the limits of real-time interaction between players and AI-driven NPCs. This project emerged during the global surge of interest in LLMs and AI voice technologies, with the goal of creating a game where the player converses naturally with characters that respond intelligently, emotionally, and immediately within the Unity engine.

## Building on two earlier prototypes, Talking Coin combined:

-OpenAI’s ChatGPT API (LLM + dialog decision logic)

-ElevenLabs Speech-to-Text API (highly accurate, expressive voice recognition)

-Unity-based TTS/STT glue systems

-Custom LLM instruction pipelines for mood, emotion, memory, and logic

-A lightweight IK-driven animated medieval world

*The result was a uniquely immersive experience in which NPCs felt alive, emotional, reactive, and fully conversational. It quickly became one of your most popular convention demos, especially the “Alien Judge” showcase used at public events.*

*Although the game progressed well, deeper production revealed the limitations of LLM reliability for high-complexity gameplay (summaries, multi-step decisions, long-term memory tracking). Development was paused until future LLM advancements allow the full vision to be realized.*

# **🎮 Gameplay Summary**

### Talking Coin featured a real-time medieval trading simulation, where the entire game loop was driven by live conversation:

*World & Structure*

The player wakes at sunrise in a medieval roadside marketplace, standing at a small vendor stall. 

### The only interactions available are:

-Look around to choose whom you speak to (highlighted by a custom shader)

-Hold Spacebar to talk (microphone input via STT)

-Type manually if preferred

### AI-Driven Trading Loop

-A merchant approaches your stand

-Negotiations begin through real speech

### The AI dynamically:

-Interprets tone, intent, and bargaining

-Tracks mood and trust levels

-Evaluates whether the deal benefits them

-Acts out animations based on emotional state

### If a deal is reached:

-Merchant picks items from his cart

-Uses IK animations to place them on your stall

-Takes payment

-A new customer approaches

### The cycle continues until:

-You run out of items

-You’ve seen enough customers for the day

-The in-game time system transitions to the next day

## *Crime & Consequences System*

### If the player threatens any NPC:

-The NPC flees

-A guard arrives and receives a conversation summary from the LLM

-The guard questions the player

### The guard decides if:

-The case is dismissed

-The player is taken to court

### If taken to court:

The Queen presides over a trial

### Based on LLM interpretation, she may:

-Execute the player

-Impose a fine

-Declare innocence

This system demonstrated impressive LLM-driven branching state logic, integrated into world interactions.

## *Side Stories*

### One prototype included a murder investigation where:

-One villager is secretly a murderer

-Each NPC may provide true, false, or partial information

-The player can accuse someone by calling the guards

-A wrong accusation leads to trial and consequences

# **🧩 Key Features**

-Full AI-driven conversation system using OpenAI ChatGPT

-Natural language bargaining and emotional modeling

-High-quality STT via ElevenLabs

-Real-time IK character animation

-Procedural emotional states affecting tone, animations, and logic

-NPCs with personalities, money values, goals, and memory

-Guard/Queen judicial system based entirely on LLM summaries

-Dynamic day/night cycle

-Minimal UI for VR compatibility

-Side-story logic driven completely by LLM-derived clues

-Unity NavMesh pathfinding for medieval villagers

-Prototype designed for VR expansion

# **🏗️ Architecture Overview**

*Talking Coin represents one of my most advanced coding stages before my 2025 breakthroughs. It marked the transition to fully modular systems with strict bans on god scripts.*

## *Architectural Advancements*

-Extensive use of interfaces, abstract classes, and LLM instruction objects

-Rich data pipelines to translate:

-Player speech → STT → LLM context → game state

-Modular AI emotion/state controllers

### Clean, separated logic for:

-Conversation handling

-Personality traits

-Deal-making evaluation

-Legal system interactions

-Item trading logic

-Modular animation layers for IK + baked movements

## **LLM Control System**

### NPC personalities were driven by a set of embedded variables:

-Trust level

-Anger level

-Interest level

-Conversation clarity

-Memory of past statements

-Conversation length/timeouts

This allowed the LLM to behave convincingly during extended conversations.

## **Why Production Paused**

### As the complexity increased, LLM reliability began to fail in areas such as:

-Multi-step reasoning

-Consistent memory summarization

-Following branching logic

-Keeping internal variables stable

The project remains on hold awaiting more robust future AI/LLM consistency.

# **🗂️ Key Scripts to Review**

### *Core*

LLMConversationController

STTInputManager

AudioResponseSystem

NPCEmotionStateController

NPCTradeLogic

CrimeAndLegalSystem

GameLoopController

### *Systems*

MerchantSystem

VillagerPersonalitySystem

GuardInquirySystem

QueenTrialSystem

ItemTradeSystem

PlayerSpeechRouting

### *AI*

NPCStateMachineBase

EmotionModel

MemoryTracker

ConversationSummarizer

### *Animation*

IKHandController

GazeTrackingSystem

ItemPickupAnimationHandler

### *Utilities*

ChatGPTRequestBuilder

ConversationHistoryCompressor

STTConfidenceInterpreter

# **🧪 Development Notes**
*Convention Demo Success*
### The “Alien Judge” version of Talking Coin became my most crowd-pleasing live demo to date:

-Large crowds gathered around the booth

-People lined up to speak with the alien

-The character’s personality, humor, and animations created memorable experiences

### *Coding Milestones*

**This project marked the first time I:**

-Fully banned god scripts

-Embraced interfaces + abstract base classes

-Designed modular emotional models

-Integrated multiple real-world APIs into Unity

-Created a fully speech-driven gameplay loop

### *Animation & Immersion*

-NPCs make eye contact

-Look at objects they pick up

-React to threats, kindness, or bargains

-Use blended IK + baked animations

This helped sell the illusion of a living world.

# **🚧 Why This Project Matters**

### Talking Coin demonstrates:

-My ability to integrate advanced AI technologies into gameplay

-Skill in designing LLM-driven systems with emotional and logical behavior

-Complex narrative branching without traditional scripting

-Strong engineering adaptability in emerging tech

-Clean architectural evolution

-My capacity to wow players in public events

-My pioneering approach to real-time conversational gameplay

It stands as one of my most unique, experimental, and technically forward-looking prototypes.

# **📚 Lessons Learned**

-LLMs struggle with multi-step decisions under long sessions

-Speech-driven gameplay requires strict guardrails and reinforcement

-Emotional modeling improves believability dramatically

-Clear separation between STT/LLM/Gameplay is crucial

-Summary-based memory systems must be resilient

-LLMs can create deeply immersive experiences but need stability for full games

-IK + emotional cues dramatically increase perceived AI intelligence

# **🛠️ Tech Stack**

-Unity 2022.3

-C#

-OpenAI ChatGPT APIs (GPT-4o mini)

-ElevenLabs Speech-to-Text

-Unity NavMesh AI

-IK Rig systems
