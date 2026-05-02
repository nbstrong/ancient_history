# Trees and Construction Spec

This spec defines Wurm-style persistent object workflows for tree felling and incremental construction.

## Design Goals

- Actions are time-based and server-authoritative.
- World objects persist through intermediate states.
- Construction and harvesting are collaborative and interruptible.
- Every step is visible to nearby players and survives restart.

## Trees: Persistent Felling and Processing

## Tree Lifecycle

`Standing -> Chopped/Felled -> Ground Log Entity -> Processed Outputs -> Stump -> Removed`

Notes:
- `Standing` trees are world entities with growth/quality metadata.
- Successful felling does not instantly grant inventory items.
- Felling creates a physical fallen-tree object with orientation and length.
- Fallen tree is processed through separate actions (segment, strip bark, cut logs, kindling).

## Tree Data Model (Server)

- `tree_instance_id` (UUID)
- `species`
- `age_stage` (sprout, young, mature, old, very_old)
- `quality` (1-100)
- `tile_x`, `tile_y`
- `corner_anchor` (optional for exact placement)
- `health`
- `state` (`standing`, `felled`, `stump`, `removed`)
- `felled_heading_deg` (null unless felled)
- `remaining_wood_units`
- `created_at`, `updated_at`

## Tree Actions

1) `ChopTree`
- Validate: permissions, tool type/quality, stamina, range, deed rules.
- Execute: timed swings with interruption checks.
- Resolve: reduce tree health; when threshold reached, transition to `felled`.
- Commit: write state + orientation + wood budget.
- Replicate: broadcast animation/event and new collision profile.

2) `ProcessFelledTree`
- Validate: target in `felled` state and has remaining units.
- Execute: timed processing action per output type.
- Resolve: roll output quality/quantity by skill + tool + tree quality.
- Commit: decrement `remaining_wood_units`, spawn output entities/items.
- Transition: when exhausted, set `stump`.

3) `RemoveStump`
- Timed action requiring shovel/mattock.
- On completion, state -> `removed` and tile clears.

## Tree Invariants

- No inventory output is created without consuming `remaining_wood_units`.
- A tree cannot be simultaneously `standing` and `felled`.
- Processing and removal actions must hold exclusive lock on `tree_instance_id`.

## Construction: Beam-by-Beam Build System

## Construction Lifecycle

`Planned -> Framed -> Partial -> Complete -> Damaged/Repairable -> Ruined`

`Planned` and `Partial` states are fully interactive and persistent.

## Object Hierarchy

- `Structure` (building envelope and deed ownership)
- `BuildPart` (wall/floor/roof/stair segment)
- `PartRequirement` (material list + counts)
- `PartProgress` (current delivered materials + quality contributions)

Each build part is independently progressed and completed.

## Build Part Data Model

- `build_part_id` (UUID)
- `structure_id`
- `part_type` (wall, floor, roof, stair, doorframe, window)
- `tile_x`, `tile_y`, `layer` (surface/cave)
- `orientation`
- `state` (`planned`, `framed`, `partial`, `complete`, `damaged`, `ruined`)
- `required_materials` (normalized rows)
- `delivered_materials` (normalized rows)
- `progress_percent`
- `final_quality`
- `permission_group`
- `created_by`, `created_at`, `updated_at`

## Construction Actions

1) `CreatePlan`
- Validate permissions and placement constraints.
- Create `build_part` in `planned` state with full BOM.

2) `DeliverMaterial`
- Validate player has required item and is in range.
- Execute timed action and consume one unit (or batch size).
- Update delivered counts; transition `planned -> framed -> partial` as thresholds are met.

3) `CompletePart`
- Allowed only when BOM is satisfied.
- Timed finishing action (hammer/mallet/etc).
- Computes `final_quality` using skill + tool + material quality.
- Transition to `complete`; update collision, pathing, and visuals.

4) `RepairPart` / `ImprovePart`
- Works on `complete` or `damaged` parts.
- Consumes repair materials and raises condition/quality within bounds.

## Multiplayer Collaboration Rules

- Multiple players may deliver materials concurrently.
- Server uses row-level transactional locks on the target part + source inventories.
- Action idempotency key prevents duplicate deliveries on retries.

## Construction Invariants

- No part reaches `complete` unless all required materials are committed.
- No material unit can be consumed twice.
- Permissions are checked at action start and commit.
- Partial progress survives restarts and node handoffs.

## Replication and UX

- Nearby players receive progress updates and model-state changes.
- Visual stages should be discrete and readable:
  - planned ghost
  - framed scaffold
  - partial geometry
  - completed geometry
- Action interruption should preserve partial progress and consumed materials.

## Persistence and Audit Requirements

- Every action writes:
  - actor id
  - target id
  - consumed/created item ids
  - before/after state snapshot
  - timestamp + shard/node id
- Required for rollback analysis, exploit detection, and GM tooling.

## Test Plan (Minimum)

1) Tree Felling
- Fell tree, restart server, verify felled state persists.
- Process outputs until exhausted; verify stump transition.
- Concurrent processing attempts enforce exclusive lock.

2) Construction
- Two players deliver materials concurrently; no duplication.
- Restart during partial state; verify exact progress retained.
- Completion blocked until BOM satisfied.
- Permission revocation mid-build blocks further contributions.
