# Global Agent Instructions

## Purpose
You are assisting in a professional .NET backend project. Always follow the rules defined in:
- coding.md
- project.md
- models.md

## Behavior Rules
- Never rewrite entire files unless explicitly asked.
- Prefer minimal diffs and focused changes.
- Ask for clarification when requirements are ambiguous.
- Follow clean architecture: Domain → Application → Infrastructure → API.
- Always validate inputs before processing.
- Use async/await everywhere.
- Keep responses concise and avoid unnecessary explanations.

## Output Rules
- When generating code, output ONLY the code block unless asked otherwise.
- When modifying existing code, show only the changed sections.
- Never introduce new dependencies without justification.
- Follow the naming conventions and patterns defined in coding.md.

## Token Efficiency
- Avoid repeating project context; assume it from project.md.
- Avoid restating rules already defined in these .md files.
- Keep reasoning internal; output only final results unless asked.