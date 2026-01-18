# 🥊 Sudo Fighter  
*A Unity-based 3D fighting game with adaptive AI*

---

## 🎮 Game Overview

**Sudo Fighter** is a **3D fighting game** developed in the **Unity Engine**, inspired by classic titles like **Tekken** and **Street Fighter**, with a modern twist:  
an **AI opponent that learns and adapts to the player’s fighting style** in real time.

Unlike traditional fighting games where enemies rely on predefined patterns, the AI in Sudo Fighter evolves by analyzing the player’s actions, adjusting its defense, and intelligently blocking repeated attacks.

---

## 🧠 Core Gameplay Concept

> *Defeat an opponent that learns how you fight.*

- Player fights a **learning AI opponent**
- AI adapts based on player behavior
- Repeating the same attacks becomes less effective
- Players must continuously change strategy to win

---

## 🕹️ Gameplay

- 1v1 player vs AI combat
- Fast-paced, close-range fighting
- AI blocks and counters predictable moves
- Each match becomes more challenging as the AI learns

---

## ✨ Key Features

### 🎮 PS5 DualSense Controller Integration
Sudo Fighter integrates advanced **PlayStation 5 controller features** to enhance immersion:

- **Haptic Feedback** – Controller vibrates on attacks and impacts  
- **Light Bar Feedback** – Light bar color changes when player health is low  
- **Voice-to-Attack** – Player can trigger attacks using voice input  
- **Gyroscope & Accelerometer Movement** – Player movement controlled via controller motion  

---

### 🤖 Adaptive Enemy AI (Reinforcement Learning)
- Enemy AI implemented using **Reinforcement Learning**
- Specifically uses **Q-Learning with Q-Tables**
- AI learns optimal actions through trial and error
- Adjusts blocking and defensive behavior dynamically
- No hardcoded attack patterns

#### AI Behavior Includes:
- Learning player attack frequency
- Blocking commonly used moves
- Adapting strategy as fights progress

---

## 🧩 AI System Overview

- **Learning Method:** Reinforcement Learning  
- **Algorithm:** Q-Learning (Q-Tables)  

**States may include:**
- Player action type
- Distance from player
- Previous outcome (hit / block)

**Actions:**
- Block
- Counter-attack
- Idle / reposition

**Rewards:**
- Successful blocks
- Avoided damage
- Punishing predictable behavior

---

## 🛠️ Tech Stack

- **Engine:** Unity Engine 6 (59f2)
- **Programming Language:** C#
- **AI Technique:** Reinforcement Learning (Q-Learning)
- **Controller Support:** PlayStation 5 DualSense
- **Third-Party Library:** Uni-Sense
- **Animations:** Adobe Mixamo
- **UI Design:** Adobe Photoshop
- **Development Methodology:** Agile
- **Project Management Tool:** Notion

---

## 🎨 Theme & Design Philosophy

Sudo Fighter explores themes of:

- **Adaptation**
- **Strategy**
- **Growth**

The game challenges players to evolve their playstyle in response to an AI that continuously learns. Success depends on flexibility, unpredictability, and strategic thinking rather than button-mashing.

---

🎥 Gameplay Video: [https://youtu.be/oTqGt5oOQ2Y](https://youtu.be/oTqGt5oOQ2Y)

