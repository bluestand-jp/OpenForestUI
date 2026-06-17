> 🌐 **English** ・ [日本語](README-ja.md)

# Objective (Baron / Dragon) models

The data behind the Baron and Dragon "power play" panels — the panel that appears after a major objective is taken and tracks the gold the team earns while it is up. `BackEndObjective` is the internal accumulator the state machine maintains; `FrontEndObjective` is the trimmed shape pushed to the overlay.

## Contents

- [`Objective.cs`](Objective.cs) — `ObjectiveType` enum (`None = -1`, `Baron = 0`, `Dragon = 1`).
- [`BackEndObjective.cs`](BackEndObjective.cs) — Backend bookkeeping for an active objective: per-team start gold (`BlueStartGold`/`RedStartGold`), `DurationRemaining`, and `TakeGameTime`. Used to compute the power-play gold difference over the objective's lifetime.
- [`FrontEndObjective.cs`](FrontEndObjective.cs) — Overlay payload: formatted `DurationRemaining` ("MM:SS"), `Type`, `GoldDifference`, and `SpawnTimer`.
- [`UpcomingObjective.cs`](UpcomingObjective.cs) — A pending spawn (`Element` name + `SpawnTimer`) for the next Dragon/Baron countdown.
