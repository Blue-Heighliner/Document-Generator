#:package Markwardt.ScriptUtilities@0.2.0
#:property TreatWarningsAsErrors=true

// Runs the app against the example config/markdown in Scripts/Example and writes Output.docx,
// demonstrating every feature in one document: heading numbering and ancestor-prefixed numbering,
// page breaks, a table of contents, page header/footer content and page numbers, tables, code
// blocks, mermaid diagrams, lists, and source trimming. File-based app (dotnet run) - run from the
// repo root, e.g. `dotnet run Scripts/Run.cs`.

using Markwardt.ScriptUtilities;

string folder = "Scripts/Example";
string output = Path.Combine(folder, "Output.docx");

(await Script.Run(
    "dotnet", "run", "--project", "App/App.csproj", "--",
    Path.Combine(folder, "Config.json"),
    Path.Combine(folder, "Example.md"),
    output)).Verify();
