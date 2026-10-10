# Repository Guidelines

## Project Structure & Module Organization

- `SourceCode/BackUpManager/`: console entry point, commands, and configuration.
- `SourceCode/BackUp.Library/`: backup services, Google Drive integration,
  account settings, exclusions, and directory traversal.
- `SourceCode/BackUp.Tests/`: NUnit tests for library behavior.
- `SourceCode/LoggingFoundation/`: Git submodule providing shared logging.
- `Support/`: sample backup configuration and development notes.
- `DevelopmentTools/`: Windows build, publish, and cleanup scripts.
- `.github/workflows/dotnet.yml`: Linux build and test pipeline.

## Build, Test, and Development Commands

Install the .NET 10 SDK. From the repository root, initialize dependencies:

```sh
git submodule update --init --recursive
```

Run these commands from `SourceCode/`, which contains `BackUpManager.slnx`:

```sh
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
dotnet run --project BackUpManager -- help
```

These restore packages, build the solution, run tests, and display CLI help.
Rebuild before using `--no-build` after code changes. On Windows,
`DevelopmentTools/Build.cmd publish` creates a self-contained Windows build
under `SourceCode/Release/win-x64/`.

## Coding Style & Naming Conventions

Follow `.editorconfig`: tabs with width four, an 80-character line limit,
UTF-8, final newlines, and no trailing whitespace. C# and project files use
LF endings; batch files use CRLF. Prefer explicit types, required braces,
file-scoped namespaces, and explicit accessibility modifiers.

Use PascalCase for types and methods, camelCase for locals and instance
fields, and `I` prefixes for interfaces. Match existing copyright headers
and XML documentation. Resolve relevant .NET, StyleCop, and Roslynator
analyzer warnings during builds.

## Testing Guidelines

Use NUnit `[Test]` methods and `Assert.That` assertions. Place tests in
`BackUp.Tests`, using `*Tests.cs` files and descriptive method names such as
`NormalizePathReturnsFullyQualifiedPath`. Cover changed exclusion, path,
and settings behavior; isolate filesystem fixtures in temporary directories
and clean them up. CI specifies no coverage threshold. Collect coverage with
`dotnet test --collect:"XPlat Code Coverage"` when useful.

## Commit & Pull Request Guidelines

Follow recent history with concise, imperative subjects such as
`Improve Logging of Excluded Items` or `Resolve Analysis Warnings`.
Keep commits focused. PRs should describe the behavior change, link relevant
issues, and report build/test results. Ensure the Linux CI checks pass.

## Security & Configuration

Use `Support/Sample.BackUp.json` as a configuration reference. Keep service
account credentials and personal backup configurations out of commits.
Use disposable local data and a dedicated Drive destination when exercising
backup operations.
