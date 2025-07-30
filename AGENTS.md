# Hyatlas Game Agent Guide

This repository contains the core of **Hyatlas**, a voxel sandbox game inspired by Hytale. The codebase is written in C# targeting .NET 6 with the MonoGame framework for rendering. The directory layout is:

- `src/` – Engine source code (blocks, chunks, world generation, entities, rendering).
- `Content/` – Game assets and block definitions.
- `tests/` – Rendering and world tests (expand here for new tests).

The goal is to evolve this into a full Hytale‑like experience: procedurally generated biomes and zones, multiple worlds including space, dungeons, NPCs and diverse civilisations, a modding interface (C# and Java), built‑in world editing, and a polished UI similar to Hytale. Fast rendering and stylised graphics with shadows are desired.

## Coding guidelines

- Use modern C# (nullable enabled, latest language version).
- Keep indentation at **four spaces**.
- Place new tests in the `tests/` folder using `dotnet test` to run them.
- Keep an eye on performance: aim for efficient rendering.

Contributors should read `README.md` for the project vision and structure before implementing new features.
