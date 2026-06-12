# 🎮 Unity Game Mechanics, Proceduralism & Optimization

A centralized sandbox repository compiling my independent research and development in procedural world-building, mechanics architecture, and physics optimization within the Unity Engine.

---

## 📂 Repository Directory & Featured Projects

### 🚜 1. Grass Mower Idle Tycoon (`MowingGame`)
* **Gameplay Overview:** A unique planetary tycoon game inspired by incremental mechanics. The player controls a dynamic lawnmower on a "miniature planet" covered entirely in grass. As you traverse the planet's surface and cut the grass, you earn money to unlock progressive upgrades. The grass continuously regenerates, and the ultimate objective is to maximize financial revenue through efficient planetary grid routing.
* **Technical Highlights:** Implemented high-density entity batching. Replaced poly-heavy 3D geometries with lightweight, batched 2D quad meshes for the grass field to massively scale grass density without increasing GPU draw calls.

### 🔫 2. "NanJang" FPS Sandbox (`NanJangFPS`)
* **Gameplay Overview:** A casual, fast-paced first-person action shooter built with practical progression systems. Instead of traditional firearms, the core combat loop revolves around dynamic environmental interaction: players grab physical objects scattered around the map and hurl them at opponents to inflict damage and secure eliminations. Includes an interactive main menu and a round timer.
* **Technical Highlights:** Utilized precise `Physics.Boxcast` for interactive object grabbing pipelines and kinetic hit detection. Resolved high-speed physical wall-clipping and camera jitter bugs by reconfiguring moving bodies to `RigidbodyInterpolation.Interpolate` and `CollisionDetectionMode.ContinuousDynamic`.

### ⚔️ 3. 2D MMORPG Sandbox (`2DMMORPG`)
* **Gameplay Overview:** A foundational prototype exploring core online-style top-down RPG systems (Work-in-Progress). The sandbox introduces basic PvE survival elements: players navigate a dynamic map, hunt hostile monsters spawned in the surrounding area, and harvest dropped items to progress through an incremental looting and grinding loop.
* **Technical Highlights:** Focused on creating a modular, decoupled state machine to handle character movement logic, responsive enemy AI aggro ranges, and dynamic inventory/item drop tables.

---

## 🌲 Procedural Generation Toolsets (Algorithms Focus)
These directories isolate specific algorithmic generation frameworks built to bypass manual asset pipelines:

* **`V1ProceduralTerrainGenerator` & `V2ProceduralTerrainGenerator`**
  * **Infinite Terrain Architecture:** Generates continuous 2D grid heights by calculating custom vertex displacements on a **16x16 mesh plane**. Uses structural graph-traversal logic (BFS/DFS) to calculate neighboring cell topologies for proximity-based mountain scales.
  * **Memory Optimization:** Uses a `Dictionary`-based cache queue to instantly pool and reuse mesh planes based on player distance thresholds, eliminating continuous runtime garbage collection.
* **`ProceduralTreeGenerator`**
  * **Structural Object Spawning:** A specialized script algorithm that procedurally computes trunk diameters and branch splitting patterns to instantiate randomized, organic tree nodes across runtime coordinates.

---

## 💻 Technical Stack Summary
* **Engine:** Unity Engine
* **Language:** C# (Object-Oriented Programming, Advanced Data Structures)
* **Core APIs:** `Physics.Boxcast`, `Rigidbody`, LayerMasks, Dictionary-based Object Pooling, Custom Vertex Rendering

---
*For a comprehensive high-level breakdown of my full portfolio, quantitative data architectures, and cloud web applications, please visit my main profile: [github.com/limguytheboy](https://github.com/limguytheboy)*
