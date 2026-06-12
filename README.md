# 🎮 Unity Game Mechanics, Proceduralism & Optimization

A centralized sandbox repository compiling my independent research and development in procedural world-building, mechanics architecture, and physics optimization within the Unity Engine.

---

## 📂 Repository Directory & Technical Breakdown

### 🌲 Procedural Generation (V1 & V2)
* **`V1ProceduralTerrainGenerator` & `V2ProceduralTerrainGenerator`**
  * **Algorithmic Heightmapping:** Engineered dynamic 2D grid heights by calculating custom vertices on a 16x16 mesh plane. Leveraged structural graph-traversal logic (BFS/DFS) to compute adjacent cell topologies, generating natural mountain ranges based on proximity vectors.
  * **Memory & Performance Optimization:** Implemented a chunk-rendering queue bound to a `Dictionary` lookup data structure. Instead of running continuous garbage-heavy instantiation loops, the system caches and reuses existing mesh planes based on the player’s distance thresholds.

### 🚜 Low-Overhead Micro-Simulations
* **`MowingGame/Script`**
  * **GPU Optimization via Batching:** Developed a grass-cutting idle-tycoon prototype focused on massive asset optimization.
  * **Lightweight Entity Rendering:** Replaced dense, poly-heavy 3D geometries with lightweight, batched 2D quad meshes for the interactive grass field, drastically scaling runtime entity counts while heavily minimizing draw calls and CPU overhead.

### 🔫 First-Person Systems & Physics
* **`NanJangFPS`**
  * **Kinetic Interaction Pipeline:** Programmed core FPS mechanics including responsive player translation, dynamic object grabbing, and physical projectile trajectories.
  * **Collision Detection:** Utilized precise `Physics.Boxcast` sweeping routines for deterministic projectile collisions and hitscan object interaction layer verification.
  * **Rigidbody Optimization:** Reconfigured high-velocity entity parameters to `RigidbodyInterpolation.Interpolate` and `CollisionDetectionMode.ContinuousDynamic`, completely resolving high-speed camera jitter and physical wall-clipping bugs.

### 🌐 Sandbox Experiments
* **`2DMMORPG`**
  * A foundational sandbox environment dedicated to exploring 2D structural frameworks, decoupled state machines, and dynamic top-down character movement logic.

---

## 💻 Technical Stack Overview
* **Engine:** Unity Engine
* **Language:** C# (Object-Oriented Programming, Advanced Data Structures)

---
*For a comprehensive high-level breakdown of my full portfolio, quantitative data architectures, and cloud web applications, please visit my main profile: [github.com/limguytheboy](https://github.com/limguytheboy)*
