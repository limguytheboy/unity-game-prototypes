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

### 🌲 1. Hybrid Procedural World & Infinite Chunk Loader (`ProceduralWorldGenerator`)
* **Gameplay Overview:** An infinite open-world terrain generation sandbox designed for seamless player exploration. As the player moves, the world automatically instantiates, shapes, and smooths continuous mathematical landmasses (chunks) on the fly, guaranteeing a limitless map without manual editing or static boundary limits.
* **Technical Implementation & Optimization:**
  * **Infinite Spiral Chunk Loader (Object Pooling):** Designed an automated `ChunkLoader()` that loops on a dynamic spatial queue. Instead of instantiating new GameObjects and triggering heavy garbage collection, it caches deactivated entities inside a `PlaneList`, positions them using a spiral calculation pattern based on `PlayerPos`, and stores live vertex coordinates inside a generic `Dictionary<Vector2Int, (Mesh, float[,])>` lookup cache.
  * **Stochastic Pathing & Perlin Noise Hybridization:** * Terrain shape seeds are determined by a recursive `NextBlock()` random-walk algorithm that computes directional coordinate expansion layers while analyzing neighborhood state densities (`count < 2`) to ensure natural distribution.
    * The structural layout is translated into heights via `BoolMapToFloat()`, scaled using stacked raw mathematical configurations (`Mathf.PerlinNoise`), and recursively smoothed via an optimized dual-pass **`BlurringMap()` box filter matrix**.
  * **Seam-Free Boundary Synchronization:** Solved the classic procedural terrain issue of detached mesh gaps between adjacent nodes. The **`SmoothingBTWChunks()`** pipeline automatically samples cross-border array rows and columns between independent chunks, calculating localized average values (`(CM + dict) * 0.5f`) to form seamless physics bridges and uniform mesh alignment during runtime.

### 🌲 2. ProceduralTreeGenerator (`Dynamic 3D Branching Algorithm`)
  * **Gameplay Overview:** A pure programmatic approach to organic asset creation. The tool algorithmic-generates 3D tree structures on the fly by combining 2D grid cell expansion loops with 3D procedural mesh generation, ensuring every tree has a completely unique growth pattern.
  * **Dynamic Cellular Expansion Architecture:** * Implemented a centralized **`controlTower()` state machine** that coordinates branching layers. As the tree grows taller (`Xheight`), the algorithm dynamic-scales the generation grid bounds (`mapRoot`) to accommodate expanding branches.
    * Developed a recursive **`NextBlock()` pipeline** that executes 2D pathfinding vectors. It analyzes adjacent coordinate densities to prevent branch overlapping (`count < 2`), and implements a stochastic logic loop that randomly decides whether to continue a single growth vector or split into multi-branch junctions (`splitAmount`).
  * **Bone Structure & Procedural Mesh Pipeline:**
    * Translates dynamic 2D array coordinates into 3D bone vectors (`Bone`) and indices (`Joints`), adding dynamic offsets using trigonometry (`Mathf.Cos/Sin`) and a localized height parameter.
    * Generates raw 3D mesh data (`MakeMeshData`) by constructing custom cylinder-like vertex segments around each bone joint according to a defined thickness and subdivision metric (`detail`), capping the terminal endpoints with dynamically generated pyramid-cone primitives.
 
---

## 💻 Technical Stack Summary
* **Engine:** Unity Engine
* **Language:** C# (Object-Oriented Programming, Advanced Data Structures)
* **Core APIs:** `Physics.Boxcast`, `Rigidbody`, LayerMasks, Dictionary-based Object Pooling, Custom Vertex Rendering

---
*For a comprehensive high-level breakdown of my full portfolio, quantitative data architectures, and cloud web applications, please visit my main profile: [github.com/limguytheboy](https://github.com/limguytheboy)*
