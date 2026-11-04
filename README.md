# eVRyone can play 🎮👓

**eVRyone can play** is an accessible virtual reality maze game built with Unity and Meta Quest 3. It explores full multimodal interaction loops to make VR gaming accessible to players with disabilities—specifically tailored for **visually impaired players** and **players with upper-limb motor impairments (hands-free)**.

---

## 🎯 Game Modes

The game offers three distinct game modes:

| Mode | Target Audience | Interaction Loop & Mechanics |
| :--- | :--- | :--- |
| **Normal Mode** | Standard VR players | Classical dual-controller locomotion (joysticks) and manual wall climbing (`GrabMovement`). |
| **Blind Mode** | Visually impaired / Blind | **Sensory substitution & 3D Audio**: Navigation without vision using active acoustic sonar (launching a 3D sound probe against walls), footstep velocity feedback, body collision thuds, and haptic wall vibrations. |
| **Hands-Free Mode** | Players without arms / Tetraparesis | **Alternative input capture (Head tracking)**: Full 6-DoF head navigation (Pitch forward to walk, Roll/Lean to strafe, Yaw to turn) without requiring any controllers. |

---

## 🚀 How to Launch & Play

### Requirements
* **Hardware**: Meta Quest headset (Quest 2, 3, or Pro) connected to PC via Quest Link or Air Link.
* **Software**: Unity Editor (configured with Oculus XR Plugin).

### Launch Steps
1. Open the project in **Unity Editor**.
2. In the `Project` tab, open the starter hub scene:  
   📂 `Assets/Scenes/modeSelection.unity`
3. Put on your Meta Quest headset (with Link active) and click **Play** in Unity.

> **💡 Quick Tip**: You can also launch any mode directly without passing through the hub by opening and playing its scene directly from `Assets/Scenes/`:
> * `NormalMazeScene.unity`
> * `BlindMazeScene.unity`
> * `HandsFreeMazeScene.unity`

---

## 🕹️ Selecting Your Game Mode in the Hub

In the **Mode Selection** scene (`modeSelection.unity`), three interactive spheres represent the three game modes:

* **With Controllers (Standard / Blind)**:  
  Bring your left or right controller inside a sphere and **press the trigger button** to load the mode.
* **Hands-Free (Gaze-based Selection)**:  
  Simply **look directly at the mode sphere for 3 seconds**. The sphere will smoothly transition from white to green and launch the scene automatically—no controllers or hands required!

---

## ⌨️ Controls Cheat Sheet

* **Normal Mode**:
  * **Left Joystick**: Walk forward / backward and strafe.
  * **Side Grip Button**: Grab and climb maze walls.
* **Blind Mode**:
  * **Index Trigger (or Left Mouse Click on PC)**: Shoot the acoustic ball to ping wall distances via spatial 3D audio.
  * **Controllers**: Vibrate continuously when touching walls or reaching the exit.
  * **Side Grip Button**: Grab and climb maze walls.
* **Hands-Free Mode**:
  * **Head Tilt Down (Pitch)**: Move forward.
  * **Head Lean (Roll)**: Strafe left / right.
  * **Head Turn (Yaw)**: Rotate camera view.
