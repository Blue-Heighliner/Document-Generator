# Document Generator

[![Release](https://img.shields.io/github/v/release/Blue-Heighliner/Document-Generator?label=Release)](https://github.com/Blue-Heighliner/Document-Generator/releases)
[![License: MIT](https://img.shields.io/github/license/Blue-Heighliner/Document-Generator.svg)](LICENSE)
[![Build](https://github.com/Blue-Heighliner/Document-Generator/actions/workflows/build.yml/badge.svg)](https://github.com/Blue-Heighliner/Document-Generator/actions/workflows/build.yml)
[![Coverage](https://raw.githubusercontent.com/Blue-Heighliner/Document-Generator/main/.github/badges/badge_linecoverage.svg)](https://github.com/Blue-Heighliner/Document-Generator/actions/workflows/build.yml)

A command-line tool that renders a markdown file into a styled Word (`.docx`) document, driven by
a JSON configuration file: per-heading-level font/numbering/spacing, page headers and footers
(with distinct first-page content and optional page numbers), a vertically-centered cover page, an
optional table of contents, configurable page breaks after chosen heading levels, and markdown
tables, code blocks, and mermaid diagrams converted to their document equivalents. Built on
[Markdig](https://github.com/xoofx/markdig) for markdown parsing and the
[Open XML SDK](https://github.com/dotnet/Open-XML-SDK) for document generation.

## Requirements

- Windows 10/11, 64-bit, or Linux, x86-64 (glibc-based distro). No separate .NET runtime install
  needed - the executable is self-contained.
- Network access, to render `mermaid` diagram blocks via the hosted mermaid.ink service.

## Installing

Download the executable for your platform from the latest
[Release](https://github.com/Blue-Heighliner/Document-Generator/releases):

- **Windows** - download `docgen.exe` and run it.
- **Linux** - download `docgen`, mark it executable (`chmod +x docgen`), and run it.

```sh
docgen <config.json> <input.md> <output.docx>
```

## Documentation

| File | Covers |
| --- | --- |
| [`Docs/Api.md`](Docs/Api.md) | The command-line interface and configuration file schema |
| [`Docs/Usage.md`](Docs/Usage.md) | Runnable examples, from a minimal document to numbering, trimming, and diagrams |
| [`Docs/Architecture.md`](Docs/Architecture.md) | High-level design decisions and their trade-offs |
| [`Docs/Project.md`](Docs/Project.md) | This repo's own tooling, publishing, and CI workflow |
| [`Docs/Components/`](Docs/Components/) | Internal design of complex components, one file per component |
