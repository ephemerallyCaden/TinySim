# TinySim Performance Optimisations

Applied to push the viable agent count beyond the previous ~300 limit.

---

## 1. Neural Network — Zero-Allocation FeedForward

**Problem:** `NeuralNetwork.FeedForward()` allocated a `new double[]` for outputs on every call. At 300 agents × 50Hz = 15,000 short-lived arrays per second, causing constant GC pauses.

**Fix:** Pre-allocated `_outputCache` array in the constructor. `FeedForward` now writes into this persistent array and returns it. No per-frame heap allocation.

**Also fixed:** Pre-resolved `Connection.sourceNode` reference at build time. The inner evaluation loop previously did a `nodeMap[id]` dictionary lookup for every connection — now it's a direct object reference (`c.sourceNode.activation`). This eliminates ~21,000 dictionary reads per simulation step at 300 agents.

**Also fixed:** Topological sort now uses an adjacency list (O(N+E)) instead of scanning all connections for every node (O(N×E)).

**File:** `Assets/Scripts/NeuralNetworks/NeuralNetwork.cs`

---

## 2. Instanced Renderer — Pre-Allocated Arrays

**Problem:** `InstancedRenderer.RenderBatched()` called `matrices.ToArray()`, `colours.ToArray()`, and allocated two sub-arrays per batch every render frame. With two renderers (agents + food), that's 8+ heap allocations per frame.

**Fix:** Replaced `List<Matrix4x4>` / `List<Vector4>` with pre-sized arrays that grow as needed but never shrink. Subclasses call `AddInstance(matrix, colour)` which writes directly into the persistent arrays. `RenderBatched` slices with `Array.Copy` into persistent batch buffers — zero allocations during normal operation.

**Files:** `Assets/Scripts/Utility/InstancedRenderer.cs`, `Assets/Scripts/Agents/AgentRenderer.cs`, `Assets/Scripts/Environment/Food/FoodRenderer.cs`

---

## 3. Vision Loop — Cached Component Lookups

**Problem:** Every agent's `UpdateVision()` called `GetComponent<Agent>()` and `GetComponent<Food>()` on every collider hit. `GetComponent` crosses the managed/native boundary and does a component scan. At 300 agents with ~10 hits each = 6,000+ expensive component lookups per step.

**Fix:** Introduced `AgentComponentCache` — a static dictionary mapping `Collider2D` → `Agent`/`Food`. Components register at `Start()` and unregister at death/despawn. Vision loop now does `AgentComponentCache.GetAgent(hit)` which is a single O(1) dictionary read.

**Files:** `Assets/Scripts/Utility/AgentComponentCache.cs` (new), `Assets/Scripts/Agents/Agent.cs`, `Assets/Scripts/Environment/Food/Food.cs`

---

## 4. Speciation — Reusable Dictionaries

**Problem:** `CompatibilityDistance()` allocated two `Dictionary<int, ConnectionGene>` instances on every call. This method is called once per species per agent birth — potentially thousands of times during reproduction waves, each creating short-lived dictionaries for GC.

**Fix:** Promoted to instance-level `_g1Cache` / `_g2Cache` dictionaries on `SpeciationManager`, cleared at the start of each call. Zero allocation during compatibility checks.

**File:** `Assets/Scripts/Evolution/SpeciationManager.cs`

---

## 5. Deferred Entity List — O(n) Single-Pass Removal

**Problem:** `DeferredEntityList.ApplyChanges()` called `List.Remove(item)` in a loop — each `Remove` is O(n) linear scan. During mass death events (starvation waves), this became O(n × deaths).

**Fix:** Changed `_toRemove` from `List<T>` to `HashSet<T>`. Removal now uses `_items.RemoveAll(x => _toRemove.Contains(x))` — a single O(n) pass with O(1) lookup per item.

**File:** `Assets/Scripts/Core/DeferredEntityList.cs`

---

## 6. Config Access — Cached Per-Frame Reference

**Problem:** `Agent.UpdateAgent()` and its sub-methods (`UpdateEnergyAndHealth`, `UpdateInputs`, `ExecuteOutputs`, `Eat`) each independently fetched `SimulationManager.instance.config` through the property chain. At 300 agents that's ~1,500+ redundant property dereferences per step.

**Fix:** `UpdateAgent()` now caches the config reference once into `_cfg` at the start of each update cycle. All sub-methods read from `_cfg` directly.

**File:** `Assets/Scripts/Agents/Agent.cs`

---

## Expected Impact

| Optimisation | Saves |
|---|---|
| NN output pre-allocation | ~15,000 GC allocations/sec |
| NN source node pre-resolution | ~21,000 dictionary lookups/step |
| Renderer pre-allocation | ~8 GC allocations/frame (large arrays) |
| Component cache | ~6,000 GetComponent calls/step |
| Speciation dict reuse | Thousands of dict allocations during reproduction |
| DeferredEntityList | O(n²) → O(n) during mass death |
| Config caching | ~1,500 property chain dereferences/step |

These changes together should significantly reduce GC stutter and CPU cost per frame, pushing the viable agent count well past 300. The next bottleneck will be the Physics2D broadphase (`OverlapCircleNonAlloc`), which can be addressed with a spatial hash grid if needed.

---

## What Was NOT Changed

- No behavioral changes — all simulation logic is identical
- Physics2D still used for vision (spatial hash grid is the next optimisation if needed)
- No changes to rendering quality or simulation accuracy
- All existing features (speciation, mutation, reproduction, UI) work identically
