# Backup reporting

This library inspects backup configuration and explains local path selection.
It contains the report models, diagnostics, local Git checks, and text/JSON
formatting. It does not start backups or contact storage providers.

## Dependencies

`BackUp.Reporting` references `BackUp.Library` and reuses its traversal and
exclusion rules. The backup library grants friend-assembly access to its
internal helpers; it has no project reference to reporting.

The executable owns command parsing, default configuration paths, output
selection, and exit codes. Call `ConfigurationInspector.Review`, optionally
`ConfigurationInspector.Explain`, then `ConfigurationReportWriter.Write`.
The caller owns the supplied `TextWriter`.

## Development

Reporting tests currently live in `../BackUp.Tests/`, including
`ConfigurationInspectorTests`, `ConfigurationInputTests`,
`LocalGitInspectorTests`, and `ConfigurationReportWriterTests`.
Run `dotnet test BackUpManager.slnx` from `SourceCode/`.

The project inherits `Directory.Build.props` and uses the same StyleCop and
Roslynator analyzers as the other projects.

Keep backup-specific inspection here until another consumer provides a
concrete reason to extract a general reporting library.
