# Milestone 0: Reproducible Bootstrap

## Objective

A clean development environment can start PostgreSQL, the standalone .NET server, and the Godot .NET client using documented commands. The client completes a versioned hello handshake with the server, and CI reproduces the supported build and test path.

## Ordered Work

1. Establish delegated issue and pull-request workflow.
2. Pin supported .NET and Godot toolchains.
3. Create the standalone executable world-server host.
4. Create the Godot .NET client shell.
5. Add the local PostgreSQL environment.
6. Add the database migration runner.
7. Add continuous integration.
8. Add unified development commands.
9. Implement the client/server hello handshake.

## Exit Gate

- A clean checkout can bootstrap required tools and services using the documented process.
- PostgreSQL becomes healthy before the server reports ready.
- The server starts and shuts down gracefully.
- The Godot project imports and starts with the pinned engine version.
- One client connects and completes the versioned hello handshake.
- Incompatible protocol major versions are rejected deterministically.
- CI runs the supported build, test, migration, and Godot import checks.
- Human engine validation passes for client startup and handshake behavior.

## Scope Limit

Milestone 0 does not implement gameplay, terrain, authentication, character state, AOI, world persistence, or production deployment.
