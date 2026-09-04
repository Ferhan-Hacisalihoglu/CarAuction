# Hermes Agent Configuration - CarAuction Project

## Backend Reference

**Backend Documentation:** [`backend.md`](backend.md)  
This file contains the database schema and backend architecture for the CarAuction project.

## Rules Reference

**Project Rules:** [`rules.md`](rules.md)  
**These rules must be followed at all times.** Mandatory Git commit & push at every meaningful progress milestone.

---

## Brainstorming Mode Protocol

> **Trigger:** When the user writes **`ping`**  
> **Response:** Assistant replies **`pong`** and enters **Brainstorming Mode**

### Brainstorming Mode Rules

1. **Mode Entry:** Upon receiving `ping`, respond with `pong` and confirm brainstorming mode is active
2. **Purpose:** Collaborative idea exploration — **no implementation, no file writes, no code execution**
3. **Interaction:** Free-form discussion, concept validation, architecture sketches, feature ideation
4. **Exit Condition:** Only exit when user explicitly says one of:
   - `let's build`
   - `implement`
   - `do it`
   - Explicit agreement to proceed with implementation
5. **Transition:** After exit confirmation, switch to implementation mode with full tool access

### Mode Indicators

| Mode | Trigger | Behavior |
|------|---------|----------|
| **Brainstorming** | User: `ping` → Assistant: `pong` | Discussion only, no actions |
| **Implementation** | User: `let's build` / `implement` / `do it` | Full tool access, execute tasks |

---

## Quick Reference

```markdown
User: ping
Assistant: pong 🧠 Brainstorming mode active — let's explore ideas!

[...collaborative discussion...]

User: let's build
Assistant: 🚀 Implementation mode active — let's build it!
```