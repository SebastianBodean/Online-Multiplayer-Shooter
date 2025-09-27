# Online Multiplayer Shooter with Anti‑Cheat Strategies — README


## Project overview

This project is a small, local multiplayer game prototype made to showcase secure client‑server design and anti‑cheat strategies for a cybersecurity portfolio. The goal was not to build a full game, but to demonstrate how to never trust the client and how to enforce game rules server‑side to prevent common cheating techniques (speed hacks, fake state updates, packet replay, etc.).


Core technologies:

* Unity

* Netcode for GameObjects

* C# for both client and server logic


## Why I chose to work on this project

* Security relevance: Tackles real-world problems like client tampering, replay attacks, unauthorized state updates, issues found in many web & mobile systems.

* Demonstrates principles: Shows Zero Trust, server‑side enforcement, input validation, and anti‑tampering considerations.

* Complexity: Cheating mitigation forces thinking about attack vectors, validation rules, rate limits, and tradeoffs between UX and security.


## Security design & decisions
### Core principle

Zero Trust: The server does not accept authoritative game state from the client. The client only submits raw inputs (e.g. "holding W key", "pressed right click"), and the server is responsible for applying game logic and updating authoritative state (e.g. "move forward", "shoot gun").

### Input model (what was implemented)

* Server‑requested input batches: At fixed intervals (server tick), the server requests or pulls a list of inputs from each client rather than passively accepting unsolicited state updates.

* Deterministic processing: Server processes inputs in tick order and decides which inputs to accept, drop, or delay (e.g., duplicates are ignored).

* No client‑authoritative position updates: Clients are not allowed to send position/state updates. Any client attempt to force‑send state is ignored by the server.

* Server validation rules: Movement speed, health changes, ability cooldowns and other game invariants are enforced server‑side.

### Example blocked exploits

* Repeated inputs to move faster: A malicious client repeats the "holding W key" input 5× per tick. Server checks sequence numbers and only processes the input once per tick (or enforces a per‑tick max) performing the "move forward" action once and preventing speed hacks.

* Aim assist: A malicious client reports they have damaged another player. The server calculates the trajectory of the projectile and determines whether it has hit any player.


## Architecture & how it works (high level)

* Client: Collects local player inputs, prepares them into a list where each active input is an enum, and responds to server polling by sending the list for a specific tick.

* Server: On each authoritative tick:

    * Requests inputs from connected clients.

    * Validates number of occurances for each input.

    * Applies the validated inputs to the authoritative simulation (movement, damage, actions).

    * Sends updated world state deltas to clients (positions, events).

* Netcode layer: Handles transport, basic connection, and message framing. Security logic sits above Netcode.


## What’s included in this repo

```
/README.md                <- this file
/Scripts/                 <- Woh

/Server/                  <- server code (authoritative logic, validation)
  /src
  /tests
/Client/                  <- Unity client code (input collection + UI)
  /Assets/                <- minimal assets or placeholders (assets not required)
  /Scripts
/Docs/
  /demo_instructions.md
  /threat_model.md
/demo_video_link.txt      <- link to hosted demo
/LICENSE
```

> [!NOTE]
> The full Unity project assets and the game build are not included in the repo. See demo_video_link.txt for an annotated demo video. Source code for the security‑relevant modules (server validation, input handling) is included.


## Demo video (Not Finished)

* A recorded walkthrough is provided because the project is not portable with all assets. The video includes:

* A walkthrough of core gameplay mechanics

* A recorded cheating attempt (client repeats inputs to move faster) and server response

* Commentary/annotations describing the security design choices

Demo link: <INSERT_YOUR_VIDEO_LINK_HERE>


## Limitations & what I’d improve

* Local demo only / no WAN testing: The recorded demo runs both clients on the same machine (zero latency). Real network conditions require:

    * Client‑side prediction + reconciliation to smooth movement

    * Tighter timestamp and tick synchronization

* Input list size checks: Currently the server requests input lists but needs strict upper bounds on list size + per‑connection quotas to prevent ping‑of‑death or large‑packet attacks.

* Stronger authentication & encryption: Replace the demo handshake with TLS and signed tokens to prevent MITM and session spoofing.

* Anti‑tamper / obfuscation: Consider code obfuscation or signing for production clients to raise the barrier for reverse engineering.

* Advanced detection: Add heuristic anomaly detection and server‑side sanity checks (e.g., impossible movement patterns, sudden extreme resource usage).

* Unit & fuzz tests: Increase automated tests (fuzz inputs, simulate malicious clients) to verify server resilience.

* Logging & audit trail: Security‑relevant events (invalid inputs, excessive submissions, auth failures) are logged for review and debugging.

* Server hosting: The hosting client can bypass the server-side security checks using a modified client. Server hosting would ensure all clients are treated equally.


<!-- 
Short resume bullet you can copy/paste:

Developed an authoritative server for a Unity multiplayer demo that enforces Zero Trust and prevents client‑side cheating via server‑requested input batches, sequence/replay protection, rate limiting, and server‑side validation (C#, Netcode).