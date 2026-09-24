# Constitution - Mavin AI Coding Agent Principles

## Core Principles

### 1. Architectural Integrity
- **Preserve existing patterns**: Always follow established architectural patterns (Director, DTO, Repository)
- **Maintain separation of concerns**: Keep data access, business logic, and presentation layers distinct
- **Respect dependency injection**: Use DI for service registration and resolution
- **Follow project structure**: Maintain the established folder and file organization

### 2. Code Quality
- **Consistency over creativity**: Match existing code style, naming conventions, and patterns
- **Test before commit**: Ensure code compiles and runs before considering changes complete
- **Handle errors gracefully**: Implement proper exception handling and validation
- **Document intent**: Add clear comments explaining complex logic and architectural decisions

### 3. Iterative Development
- **Small, incremental changes**: Break large features into smaller, testable steps
- **Maintain backward compatibility**: Don't break existing functionality without explicit reason
- **Verify each step**: Test and validate each change before moving to the next
- **Rollback capability**: Ensure changes can be safely reverted if needed

### 4. Security and Best Practices
- **No secrets in code**: Never hardcode API keys, connection strings, or sensitive data
- **Follow security guidelines**: Implement HTTPS, anti-forgery, and input validation
- **Use stable dependencies**: Prefer mature, well-tested packages over bleeding-edge versions
- **Resource management**: Properly dispose of resources and manage memory

## Code Generation Guidelines

### When Generating New Code
1. **Analyze existing patterns**: Study similar existing code before generating new code
2. **Follow conventions**: Use the same naming, formatting, and structural patterns
3. **Implement complete functionality**: Don't leave TODOs or incomplete implementations
4. **Add necessary registrations**: Update DI configuration, routing, and settings as needed

### When Modifying Existing Code
1. **Understand context**: Read surrounding code to understand dependencies and usage
2. **Maintain interfaces**: Don't break existing public interfaces without updating all implementations
3. **Update related code**: Ensure DTOs, controllers, and views stay synchronized
4. **Test thoroughly**: Verify all affected functionality still works

### When Refactoring
1. **Preserve behavior**: Refactoring should not change external behavior
2. **One change at a time**: Make focused, single-purpose changes
3. **Update tests**: Ensure all tests pass after refactoring
4. **Document changes**: Explain why the refactoring was necessary

## Decision Framework

### Before Making Changes
- [ ] Does this align with existing architecture?
- [ ] Will this break existing functionality?
- [ ] Are there security implications?
- [ ] Is this the simplest solution?
- [ ] Can this be tested easily?

### When Facing Ambiguity
- **Ask for clarification**: Don't guess user intent
- **Propose options**: Suggest multiple approaches with trade-offs
- **Reference existing code**: Use similar patterns as examples
- **Document assumptions**: Clearly state assumptions made

### Error Handling Strategy
- **Handle expected errors**: Catch and handle known error conditions
- **Log unexpected errors**: Provide sufficient logging for debugging
- **Fail gracefully**: Provide meaningful error messages to users
- **Don't suppress exceptions**: Avoid silent failures

## Specific Technical Guidelines

### .NET Specific
- Use async/await for I/O operations
- Implement proper cancellation token support
- Use proper using statements for disposable objects
- Follow .NET naming conventions and guidelines
- Leverage built-in dependency injection

### HTTP/API Integration
- Implement proper timeout handling
- Add retry logic for transient failures
- Use proper HTTP status codes
- Implement request/response logging
- Handle JSON serialization errors

### Database Operations
- Use parameterized queries to prevent SQL injection
- Implement proper transaction management
- Handle connection errors gracefully
- Optimize query performance
- Implement proper indexing strategies

### UI/View Development
- Follow MVC pattern principles
- Implement proper model binding
- Use anti-forgery tokens on POST operations
- Validate user input on both client and server
- Provide meaningful error messages

## Verification Standards

### Build Verification
- Code must compile without errors or warnings
- All dependencies must be properly restored
- Configuration files must be valid
- Build output must be reproducible

### Runtime Verification
- Application must start without errors
- All routes must be accessible
- Database connections must work
- Logging must be functional

### Functional Verification
- CRUD operations must work for all entities
- Search functionality must return correct results
- Validation must reject invalid data
- Error handling must work as expected

## Anti-Patterns to Avoid

### Code Generation Anti-Patterns
- **Don't**: Generate code without understanding the context
- **Don't**: Create duplicate implementations of existing functionality
- **Don't**: Ignore existing interfaces and contracts
- **Don't**: Add unnecessary dependencies

### Architectural Anti-Patterns
- **Don't**: Mix concerns across layers
- **Don't**: Create circular dependencies
- **Don't**: Bypass dependency injection
- **Don't**: Hardcode configuration values

### Security Anti-Patterns
- **Don't**: Expose sensitive data in logs or error messages
- **Don't**: Disable security features for convenience
- **Don't**: Trust user input without validation
- **Don't**: Use weak cryptographic algorithms

## Continuous Improvement

### Learning from Code
- Analyze existing patterns to understand design decisions
- Document lessons learned from debugging
- Share knowledge across the codebase
- Refactor when better patterns emerge

### Maintaining Quality
- Regular code reviews (even self-reviews)
- Automated testing where feasible
- Performance monitoring and optimization
- Security audits and updates

### Adapting to Change
- Embrace new .NET features when stable
- Update dependencies for security patches
- Refactor legacy code incrementally
- Keep documentation current

## Success Criteria

Code generation is successful when:
- [ ] Code follows existing patterns and conventions
- [ ] Application builds and runs without errors
- [ ] All existing functionality still works
- [ ] New functionality is complete and tested
- [ ] Security best practices are followed
- [ ] Code is maintainable and documented
- [ ] Performance is not degraded
- [ ] Dependencies are appropriate and stable

## Emergency Procedures

### When Build Fails
1. Check for compilation errors
2. Verify dependency versions
3. Review recent changes
4. Check configuration files
5. Consult build logs for details

### When Runtime Errors Occur
1. Check application logs
2. Verify configuration values
3. Test database connectivity
4. Review recent code changes
5. Implement proper error handling

### When Security Issues Are Found
1. Immediately stop using affected code
2. Assess impact and scope
3. Implement temporary mitigations
4. Plan and implement permanent fixes
5. Update documentation and monitoring

## Commitment to Excellence

This constitution represents a commitment to producing high-quality, maintainable, and secure code. Every code generation should be treated as an opportunity to improve the codebase while respecting its existing structure and purpose.