# Simulation Systems Plan

## Core Gameplay Loops

1) Gather
- Mine ore, cut trees, forage, fish, farm.

2) Process
- Smelt, saw, grind, cook, refine intermediates.

3) Craft/Build
- Tool crafting, structure planning, component assembly, improvement loops.

4) Trade/Social
- Player markets, contracts, settlement specialization.

## Unified Action Pipeline

Every action follows:
- `Validate`: permissions, tools, distance, stance, stamina.
- `Reserve`: lock target resources/containers.
- `Execute`: time-based ticks with interruption conditions.
- `Resolve`: skill roll, quality roll, damage/decay impacts.
- `Commit`: transactional state write.
- `Replicate`: notify involved clients + nearby observers.

## Skill and Quality Model (Initial)

- Skills: float 1-100 with diminishing returns.
- Item quality: float 1-100 influences success, durability, output quality.
- Difficulty model:
  - Effective chance = f(skill, tool quality, difficulty, condition modifiers).
- Failure can produce:
  - No output, damaged output, or lower quality output.

## Terrain and Terraforming

- Wurm-style corner-height terrain model.
- Each tile references four corner heights; gameplay slope is derived from those corners.
- Separate data layers:
  - Surface corners.
  - Surface tile material/flags.
  - Cave/underground corners and reinforcement state.
- Allowed mutations:
  - Dig (corner decrease), drop dirt (corner increase), level, flatten, pave, mine tunnel, reinforce.
- Constraints:
  - Max slope deltas per action, deed/permission checks, collapse checks (for underground), water interactions.
- Persistence:
  - Store base generation seed + per-chunk corner arrays + mutation records.

### Corner Mutation Rules (Initial)

- All terraforming actions resolve into corner deltas.
- Single-corner edits affect up to 4 adjacent tiles and must trigger tile recomputation.
- `Flatten` targets a plane and applies repeated legal corner deltas until tolerance is met.
- `Level` uses player tile as reference and applies bounded corner edits outward.
- Server rejects corner edits that violate slope or structural invariants.

### Terrain Invariants

- Corner heights are the sole source of truth for surface elevation.
- Tile normals/slope values are derived data and can be regenerated.
- No corner mutation without chunk lock ownership and action reservation.
- Border-corner updates across chunk boundaries must be atomic to both chunks.

## Ecology and Creatures

- Creature archetypes with:
  - Needs, aggression profile, habitat preferences.
- Spawning tied to biome and carrying capacity.
- Long-run simulation drivers:
  - Predation pressure.
  - Resource depletion/regeneration.
  - Human settlement influence.

## Decay and Upkeep

- Items/structures decay based on:
  - Material, quality, exposure, upkeep status.
- Deeds/settlements consume upkeep resources/currency.
- Upkeep failure transitions to increased decay and permissions downgrade.

## Invariants to Protect

- No item duplication across inventory/container/trade transitions.
- No action completion without authoritative timer and resource reservation.
- No terrain mutation without chunk lock ownership.
