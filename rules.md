# Project Rules

## Mandatory Git Commit & Push

**Rule:** At every meaningful progress milestone, the project **must** be committed and pushed to Git.

### When to Commit & Push
- After completing a feature or module
- After fixing a bug
- After significant refactoring
- Before switching tasks/contexts
- At end of each work session

### Workflow
```bash
git add .
git commit -m "descriptive message: what changed and why"
git push
```

### Commit Message Format
```
<type>: <short summary>

<body with details if needed>
```

Types: `feat`, `fix`, `refactor`, `docs`, `chore`, `test`

---

> **No exceptions.** Unpushed work is at risk of loss.

---

## Multi-Agent Scope Isolation

**Rule:** When multiple agents work on the same codebase, **scopes must be explicitly separated** and the model must be aware of the boundaries.

### Principles
- **No overlap:** Each agent owns a distinct scope (feature, module, file set)
- **Awareness:** The model knows what other agents are working on
- **No interference:** Never modify files outside your assigned scope
- **No fear:** Don't hesitate to work in your scope just because a teammate is active elsewhere — parallel work is safe when scopes are clean

### Workflow
1. Define scopes before spawning agents
2. Communicate scope assignments clearly
3. Respect boundaries — if unsure, ask before crossing
4. Sync via Git (commit/push) at milestone boundaries

> **Trust the scope.** Clean boundaries = fearless parallelism.