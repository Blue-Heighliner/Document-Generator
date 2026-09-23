# Architecture

This document explains the high-level design decisions behind Document Generator's
implementation - *why* it's built the way it is, not the class-by-class mechanics of *how*.

## Build the OpenXML tree directly, rather than through a higher-level docx library

The alternatives were a higher-level wrapper library (e.g. one of the `docx`-style NuGet packages)
or driving Word itself via interop. Interop is Windows-only and requires Word installed, which
rules it out for a cross-platform, self-contained tool. Higher-level wrapper libraries trade away
exactly the control this app needs - custom per-level heading numbering, dual header/footer
variants per section, a vertically-centered cover page, and precise TOC field construction all
require dropping to raw OpenXML elements anyway, so a wrapper would only add an indirection layer
without removing any of that complexity. Building directly against `DocumentFormat.OpenXml` costs
more boilerplate per feature but avoids fighting a wrapper's opinions about document structure.

## Render mermaid diagrams via a hosted service instead of bundling a renderer

Rendering a mermaid diagram to an image requires either a headless browser (to run the real
mermaid.js) or a from-scratch diagram layout engine. Both are far too heavy to bundle into a
single self-contained executable, and a headless browser isn't realistically self-contained at
all. Delegating to the hosted `mermaid.ink` rendering service keeps the executable small and the
rendering logic trivial (an HTTP GET), at the cost of requiring network access and an external
dependency's uptime. Given this app already isn't usable offline for anything but the simplest
inputs (no diagrams), that trade-off was judged acceptable; `IMermaidImageRenderer` isolates it
behind an interface so a future local-rendering implementation could swap in without touching any
other component.

## Track heading numbering as ordered document-order state, not a tree

Heading numbers (`1`, `1.1`, `1.2`, `2`, ...) look like they'd naturally fall out of a heading
tree, but the markdown source is only ever walked once, in document order, to build the OpenXML
body incrementally. Building a tree first would mean two full passes (once to compute numbers,
once to render) for no benefit, since the numbering rule itself - increment this level, reset
every deeper level - only ever needs to look at the *current* level and its already-computed
ancestors, both of which a simple `Dictionary<int, int>` of running counters captures directly.
`HeadingNumberer` keeps that state and is handed each heading's level, in order, as the body
writer encounters it.

## Per-level heading and title/subtitle defaults are merged, not replaced, on partial customization

`heading1`-`heading6` and `title`/`subtitle` each have their own distinct default styling (e.g.
`heading1` defaults to size 24 bold, `heading6` to size 11 bold) rather than sharing one generic
default - see `Docs/Api.md`. `System.Text.Json`'s normal behavior for a nested object property,
though, is to construct a *fresh* instance of its declared type (running that type's own field
initializers) whenever the JSON supplies any value for it, discarding whatever default instance
the containing property was initialized with - so a config that only overrode `heading1.italic`
would silently lose `heading1`'s size-24-bold default and fall back to `HeadingConfiguration`'s
generic size-12 default for every other field. `Heading1`-`Heading6` route around this with
`[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]`, which tells the deserializer
to populate directly into the property's existing default instance instead of replacing it.
`Title`/`Subtitle` can't use that same attribute, since they also need the plain-array shorthand
(`"title": ["My Document"]`) that a custom `JsonConverter` provides, and a converter fully owns
deserialization for its type - `Populate` has no effect once one is registered.
`CoverTextConfigurationConverter` implements the same merge manually instead: it starts from a
caller-supplied default value and overlays only the fields present in the JSON.
`TitleConfigurationConverter` and `SubtitleConfigurationConverter` are two trivial subclasses that
each supply their own default instance, since a single shared converter has no way to know which
property it's being used for.

## `App.csproj` declares both publish RIDs up front, even though no single build needs more than one

`RuntimeIdentifiers` (plural) lists `win-x64;linux-x64`, even though no single `dotnet
build`/`dotnet test` run - or even a single `dotnet publish` - needs more than one of them at a
time. With `RestorePackagesWithLockFile` on, `packages.lock.json` records a separate dependency
graph per RID, and NuGet only considers the file internally consistent if the current restore's
RID set exactly matches every RID section already present in it - so once a second RID's section
exists (added the first time `build.yml`'s `publish` job runs `dotnet publish -r <rid>` for a
platform not yet in the file), a later single-RID restore for *either* platform fails
`RestoreLockedMode=true` with `NU1004`, regardless of publish order. Declaring every published RID
here up front makes one plain `dotnet restore` capture all of their graphs atomically in a single
consistent file, so each subsequent single-RID publish succeeds against it.
`Directory.Build.props` covers hash reproducibility for what's *in* the lock file; this covers
which RID sections it needs to have.

## Third-party license notices are a static file, checked by hand, not generated at build time

`Markdig` is BSD-2-Clause and `DocumentFormat.OpenXml`/`DocumentFormat.OpenXml.Framework` (and
their transitive `System.IO.Packaging` dependency) are MIT - all three require, for binary
redistribution, that their copyright notice and license text accompany the distributed binary
(BSD-2-Clause says so explicitly for binary form; MIT's single condition is conventionally read
the same way). Since `docgen.exe`/`docgen` are self-contained executables that statically link
these libraries' compiled IL, and are distributed as GitHub Release assets, that requirement
applies directly. `THIRD-PARTY-NOTICES.txt` at the repo root satisfies it by reproducing each
dependency's exact license text, and `build.yml`'s publish job uploads it alongside the two
executables on every release. It isn't generated from `packages.lock.json` at build time - the
dependency set changes rarely enough that hand-maintaining it when a `PackageReference` changes is
simpler than adding a license-scanning step to CI, and a generated file would still need a human to
verify the license expression before trusting it.

## Table of contents and page numbers are real Word fields, not pre-computed text

The alternative would be computing the table of contents (and running page numbers) ourselves and
writing plain text into the document. That's fragile - it requires this app to reimplement Word's
own pagination and line-breaking to know what page a heading lands on, which is neither practical
nor reliably reproducible outside Word's own layout engine. Instead, the app writes real `TOC` and
`PAGE` field codes (with `UpdateFieldsOnOpen` set), and lets Word itself compute and cache the
result the first time the document is opened. The cost is a document that shows placeholder text
until Word updates the fields (standard behavior for any programmatically generated field, and
exactly what happens when Word itself inserts one), rather than always-correct numbers with no
dependency on Word to resolve them.
