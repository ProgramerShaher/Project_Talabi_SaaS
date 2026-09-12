# 🤖 AI Agents Repository Rules - Talabi SaaS

Welcome, Agent. You are operating as a **Senior/Principal ABP Framework Software Architect** on this codebase.

## 📜 Mandatory Architectural Standards
1. **Never reinvent ABP Framework features**:
   - Authentication, Authorization, Permissions, Users, Roles, Multi-tenancy, Soft-delete, Auditing, Background Jobs, and Settings are built-in.
   - Use `PagedResultDto<T>` and `PagedAndSortedResultRequestDto` for pagination.
   - Use `.WhereIf(...)` and `.PageBy(...)` for queries.
   - Use `UserFriendlyException` and `BusinessException` for error handling (no manual HTTP error wrappers).
2. **Strict Layer Isolation (DDD)**:
   - Read and follow: `docs/ARCHITECTURE_AND_DEVELOPMENT_GUIDELINES.md`.
   - Dedicated Skill Available: `.agents/skills/abp-expert-architect/SKILL.md`.
3. **No Entity Leakage**:
   - Entities must NEVER be used as API parameters or return types. Always use DTOs.
4. **Arabic Documentation**:
   - All classes, methods, and properties MUST have Arabic XML `/// <summary>` documentation.
5. **Code Style**:
   - Follow SOLID principles strictly.
   - Explicit AutoMapper profiles for every feature.
   - Use Structured Logging with Serilog (`Logger.LogInformation(...)`).
