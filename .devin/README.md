# Mavin AI Coding Agent Specifications

This directory contains comprehensive specifications to empower Mavin AI coding agent for spec-driven development of the CoreMVCClean .NET web application.

## File Structure

```
.devin/
├── README.md              # This file - overview of specifications
├── AGENTS.md              # Project-specific rules and conventions
├── constitution.md        # AI coding principles and decision framework
├── environment.yaml       # Build and development setup configuration
├── project-spec.md        # Detailed project architecture and specification
├── code-conventions.md     # Coding standards and best practices
└── api-standardization-requirements.md  # API response model standardization requirements
```

## File Descriptions

### AGENTS.md
**Purpose**: Project-specific rules and conventions for Mavin AI coding agent.

**Contents**:
- Project overview and architecture
- Coding conventions specific to this project
- Build and development commands
- Configuration details
- Entity management patterns
- Development guidelines
- Error handling strategies
- Security considerations
- Testing strategy
- Code quality standards

**Usage**: This is the primary reference for understanding project-specific patterns and should be consulted first when making any changes.

### constitution.md
**Purpose**: Core principles and decision framework for AI-assisted code generation.

**Contents**:
- Core principles (architectural integrity, code quality, iterative development)
- Code generation guidelines
- Decision framework
- Specific technical guidelines (.NET, HTTP/API, database, UI)
- Verification standards
- Anti-patterns to avoid
- Continuous improvement strategies
- Success criteria
- Emergency procedures

**Usage**: This file defines the "constitution" that all code generation must follow, ensuring architectural consistency and quality.

### environment.yaml
**Purpose**: Build and development setup configuration for the development environment.

**Contents**:
- Initialize commands (one-time setup)
- Maintenance commands (run on session start)
- Knowledge base (build, run, test, package management, etc.)
- Common workflows
- Development commands reference

**Usage**: This file is used by the development environment to set up and maintain the project build system.

### project-spec.md
**Purpose**: Comprehensive project architecture and technical specification.

**Contents**:
- Executive summary
- Technical stack and dependencies
- Architecture overview and layer responsibilities
- Data models and DTO patterns
- Business logic and Director pattern implementation
- API endpoints and routing
- Configuration details
- Implementation status (completed, in progress, planned)
- Data flow diagrams
- Security and performance considerations
- Error handling strategy
- Testing strategy
- Deployment considerations
- Future enhancement roadmap

**Usage**: This is the authoritative technical specification for understanding the complete system architecture and implementation details.

### code-conventions.md
**Purpose**: Detailed coding standards and best practices for the project.

**Contents**:
- General principles and code style philosophy
- C# coding standards (naming conventions, file organization, formatting)
- Code structure guidelines
- Async/await patterns
- Exception handling
- Dependency injection
- LINQ guidelines
- String and collection handling
- Null handling
- Comments and documentation
- ASP.NET MVC specific conventions
- HTTP client usage
- JSON serialization
- Logging
- Testing conventions
- Code review checklist
- Common anti-patterns to avoid
- Tool configuration

**Usage**: This file provides detailed guidelines for writing consistent, high-quality code that follows project standards.

### api-standardization-requirements.md
**Purpose**: Requirements specification for API response model standardization adaptation.

**Contents**:
- Current state analysis of existing response patterns
- New contract requirements (success/error response models)
- Standardized error codes and error handling
- Correlation ID implementation requirements
- Impact analysis on existing components
- Implementation requirements by phase
- Backward compatibility and migration strategy
- Testing, performance, and security considerations
- Timeline estimates and risk mitigations

**Usage**: This file captures the requirements for adapting the application to standardized API response models without making code changes.

## How to Use These Specifications

### For Mavin AI Coding Agent

1. **Initial Setup**: Read `AGENTS.md` first to understand project context
2. **Code Generation**: Follow guidelines in `constitution.md` for decision-making
3. **Architecture Reference**: Consult `project-spec.md` for detailed technical understanding
4. **Code Standards**: Follow `code-conventions.md` for consistent code style
5. **Build Process**: Use commands from `environment.yaml` for development workflow
6. **Feature Requirements**: Review `api-standardization-requirements.md` for specific feature changes

### For Human Developers

1. **Onboarding**: Start with `README.md` (this file) and `AGENTS.md`
2. **Understanding Architecture**: Read `project-spec.md` for system overview
3. **Coding Standards**: Reference `code-conventions.md` when writing code
4. **Decision Making**: Use `constitution.md` principles for architectural decisions
5. **Build Setup**: Follow `environment.yaml` for environment configuration
6. **Feature Planning**: Review `api-standardization-requirements.md` for enhancement planning

## Key Principles

### Architectural Integrity
- Maintain clean architecture separation
- Follow established patterns (Director, DTO, Repository)
- Respect dependency injection principles
- Preserve project structure

### Code Quality
- Consistency over creativity
- Test before commit
- Handle errors gracefully
- Document intent

### Iterative Development
- Small, incremental changes
- Maintain backward compatibility
- Verify each step
- Ensure rollback capability

## Workflow for AI-Assisted Development

### 1. Planning Phase
- Consult `project-spec.md` for architectural context
- Review `AGENTS.md` for project-specific patterns
- Use `constitution.md` decision framework

### 2. Code Generation Phase
- Follow `code-conventions.md` for implementation
- Adhere to patterns in `AGENTS.md`
- Maintain architectural integrity per `constitution.md`

### 3. Verification Phase
- Use build commands from `environment.yaml`
- Follow verification standards in `constitution.md`
- Check against code review checklist in `code-conventions.md`

### 4. Documentation Phase
- Update relevant specification files if architecture changes
- Document new patterns in `AGENTS.md`
- Update `project-spec.md` for new features

## Maintenance

### Updating Specifications
- Keep specifications synchronized with code changes
- Update `project-spec.md` when architecture changes
- Add new patterns to `AGENTS.md` as they emerge
- Revise `code-conventions.md` when standards evolve

### Version Control
- All specification files are under version control
- Changes to specifications should be committed with clear messages
- Review specification changes like code changes

## Success Criteria

Mavin AI coding agent is considered successful when:
- Code follows all specifications consistently
- Architectural integrity is maintained
- Code quality standards are met
- Build and verification processes work smoothly
- Iterative development is supported effectively
- Specifications remain accurate and up-to-date

## Support and Troubleshooting

### Common Issues
- **Build failures**: Check `environment.yaml` commands and dependencies
- **Architecture drift**: Review `constitution.md` principles
- **Code inconsistency**: Consult `code-conventions.md`
- **Pattern confusion**: Reference `AGENTS.md` examples

### Getting Help
- Review relevant specification file for specific issue
- Check `project-spec.md` for architectural context
- Follow decision framework in `constitution.md`
- Use code review checklist in `code-conventions.md`

## Future Enhancements

The specification structure can be extended with:
- API documentation (Swagger/OpenAPI specs)
- Database schema documentation
- Performance benchmarks
- Security audit results
- Test coverage reports
- Deployment guides

---

**Last Updated**: 2024-09-24  
**Project**: CoreMVCClean - .NET 10.0 Web Application  
**Purpose**: Empower Mavin AI coding agent for spec-driven development  
**Latest Addition**: API standardization requirements for response model adaptation