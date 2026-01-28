---
name: commit
description: Review git changes and create conventional commit messages
---

# Commit Changes

Review the current git changes, write a clear commit message following conventional commits format, and commit the changes.

## Process:

1. **Review Changes**
   - Run `git status` to see what's changed
   - Run `git diff` to see the actual changes
   - Understand what was added, modified, or deleted

2. **Write Commit Message**
   Follow the Conventional Commits format:
   
   ```
   <type>(<scope>): <description>
   
   [optional body]
   
   [optional footer]
   ```
   
   **Types:**
   - `feat`: New feature
   - `fix`: Bug fix
   - `docs`: Documentation changes
   - `refactor`: Code refactoring (no functional changes)
   - `style`: Formatting, missing semicolons, etc (no code change)
   - `test`: Adding or updating tests
   - `chore`: Maintenance tasks, dependency updates
   - `perf`: Performance improvements
   
   **Scope (optional):**
   - Area of codebase affected: `ui`, `dialogue`, `mining`, `data`, etc.
   
   **Description:**
   - Clear, concise summary (50 chars or less ideally)
   - Use imperative mood: "add feature" not "added feature"
   - Don't capitalize first letter
   - No period at the end
   
   **Body (optional):**
   - Explain WHY the change was made
   - Provide additional context
   - Wrap at 72 characters
   
   **Footer (optional):**
   - Breaking changes: `BREAKING CHANGE: description`
   - Issue references: `Closes #123`

3. **Stage and Commit**
   - Run `git add -A` (or specific files if appropriate)
   - Run `git commit -m "your message"`
   - Confirm the commit was successful

4. **Ask About Push**
   - Ask if I want to push to remote
   - If yes, run `git push`

## Examples:

**Simple feature:**
```
feat(ui): add progress bar component

Implemented BoundProgressElement with radial and bar variants
```

**Bug fix with context:**
```
fix(dialogue): prevent null reference in character lookup

DialogueManager was not checking for null character data before
accessing properties. Added null check and fallback to default character.

Closes #42
```

**Refactor:**
```
refactor(data): consolidate ScriptableObject organization

Moved all SO scripts to Scripts/ScriptableObjects and instances to Data
folder following project structure guidelines
```

**Breaking change:**
```
feat(mining): redesign yield calculation API

BREAKING CHANGE: CalculateYield now requires MaterialType enum instead
of string. Update all callers to use the new enum.
```

## Style Guidelines:

- Be specific but concise
- Focus on WHAT changed and WHY
- Group related changes in one commit
- Don't commit unrelated changes together
- Make commits atomic (one logical change per commit)

## Pre-Commit Checklist:

Before committing, I'll review the changes and **ask** about potential issues:

**Accidental Leftovers to Check:**
- ❓ Debug print statements (e.g., `Debug.Log("test")`, `console.log("here")`)
- ❓ Temporary test code or experiments
- ❓ IDE-specific files (unless intentional, like `.vscode/settings.json`)
- ❓ Large binary files (suggest using Git LFS)
- ❓ Sensitive data (API keys, passwords, tokens)

**Commented-Out Code:**
I'll notice commented-out code and **ask your intent**:
- Is it temporary? (testing alternatives, debugging)
- Is it intentional? (keeping old implementation for reference, future use)
- Is it accidental? (forgot to delete)

**You decide** - I won't remove anything automatically. Common valid reasons:
```csharp
// ✅ Valid: Alternative implementation kept for reference
// public void OldCalculateYield() { ... }

// ✅ Valid: Temporarily disabled while testing new approach
// UpdateUI();

// ❓ Maybe cleanup: Debugging leftover
// Debug.Log("WHY ISN'T THIS WORKING???");
```

**Staging Strategy:**
If there are unintended changes mixed with intended ones, I'll ask if you want to:
- Stage only specific files: `git add path/to/file`
- Stage specific hunks interactively: `git add -p`
- Commit everything: `git add -A`
