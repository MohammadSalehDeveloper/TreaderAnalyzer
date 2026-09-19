---
name: write-unit-tests
description: Add xUnit unit tests for new Trader Analyzer functionality, features, stories, and infrastructure. Use when implementing handlers, domain behavior, validators, stories, or infra helpers, or when a change lacks tests.
---

# Write unit tests

Stack: **xUnit**, **FluentAssertions**, **Moq**. Project: `tests/Tests.Unit` (Domain + Application). Do not add packages without architecture-guard approval.

## Layout

Mirror production:

`tests/Tests.Unit/Application/Auth/Commands/Register/RegisterCommandHandlerTests.cs`

## Style

```csharp
[Fact]
public async Task Handle_WhenEmailExists_ThrowsConflictException()
{
    userRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

    var act = async () => await sut.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<ConflictException>();
}
```

- Name: `Method_Scenario_Expected`
- Mock **contracts** (`IUserRepository`, `IPasswordHasher`, `IUnitOfWork`) — not EF
- Domain tests: real entities, no mocks; assert `DomainException` and state
- Validators: `validator.Validate(cmd).IsValid.Should().BeFalse()`
- One behavior per test. No empty `UnitTest1` leftovers

## Coverage bar

Happy path + authz/invariant failure + not found/conflict + important validation. For infra I/O, prefer `tests/Tests.Integration` (Testcontainers) rather than new unit packages.

Run: `dotnet test tests/Tests.Unit --filter FullyQualifiedName~{NewTestClass}`
