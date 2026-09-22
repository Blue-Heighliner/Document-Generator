# Rendering

Covers the types under `App/src/Rendering`: `DocumentBuilder` (the orchestrator),
`MarkdownBodyWriter` (the markdown-to-OpenXML block converter), and their supporting writers
(`CoverPageWriter`, `TableOfContentsWriter`, `HeaderFooterWriter`, `InlineRunWriter`,
`TableConverter`, `MermaidImageRenderer`, `DrawingElementBuilder`, `PngDimensionReader`).

## Section structure

A generated document always has three OOXML sections, in order: cover, table of contents (omitted
when no heading level has `tableOfContents` set), and body. Per the OOXML section model, every
section except the last is represented by an otherwise-empty paragraph whose `pPr` carries the
`sectPr` that closes it; the last section's `sectPr` is a direct child of `body` instead.
`DocumentBuilder` owns this wiring centrally - the individual writers (`CoverPageWriter`,
`TableOfContentsWriter`, `MarkdownBodyWriter`) only ever append content, never a `sectPr`, so
there's exactly one place that has to get section boundaries right.

Only the cover section has `TitlePage` set and first-page header/footer references - it's the only
section whose first page is the *document's* first page. The table of contents and body sections
only ever use the "default" header/footer variant, even on their own first page.

## Document settings

`DocumentBuilder` writes a `DocumentSettingsPart` alongside the body: `HideSpellingErrors` and
`HideGrammaticalErrors` suppress Word's wavy spelling/grammar underlines throughout the generated
document (content like heading numbers, code blocks, and mermaid source is expected to trip
proofing false-positives), and `UpdateFieldsOnOpen` makes Word resolve the TOC and page-number
fields (below) the moment the document is opened, instead of requiring a manual update.

## Header and footer content

`HeaderFooterWriter` zips a section's `left`/`center`/`right` line lists together by index into
rows (e.g. `left[0]` and `right[0]` share a row) after filtering each line to the variant being
built (default/"body" or first-page/"cover", per its `mode`). A row with only one position present
is just a plain left/center/right-justified paragraph; a row with more than one position uses a
paragraph-level center and right `w:tabs` stop (positioned from the page's printable width) with
`w:tab` runs between segments - the standard OOXML technique for placing independent text at fixed
horizontal zones on one line, and the only way to put two positions on the same row at all, since
paragraphs don't otherwise support side-by-side text.

The page number paragraph, when enabled, is inserted as row 0 - ahead of every content row - before
the content rows are built into the row list. For a header, rows are then emitted in that order
(index 0 first, i.e. topmost - nearest the page's top edge, so the page number lands there). For a
footer, the whole row list - page number included - is reversed before being written, so index 0
still ends up nearest its own page edge (the bottom, here) despite paragraphs always flowing
top-to-bottom within the part. Prepending before the reversal, rather than always appending, is
what keeps "row 0" meaning the same thing (nearest that part's own page edge) in both.

## Heading numbering and page breaks

`MarkdownBodyWriter` walks the parsed markdown once, in document order. It tracks a stack of
currently "open" heading levels; encountering a heading at level `L` closes every open level `>=
L` (since a new heading at `L` starts a fresh section at that depth), and if any of the closed
levels has `pageBreakAfter` set, a single page break is inserted before the new heading's content
- even if closing multiple nested levels at once would otherwise call for more than one. Numbering
itself is delegated to `HeadingNumberer` per level, in the same walk.

Every heading paragraph gets an explicit `w:outlineLvl` (heading level minus one), regardless of
whether it has custom font styling. This is what lets the `TOC` field's `\o` level-range switch
find it - Word's TOC field scans by outline level, not by a named `Heading N` paragraph style, and
since heading styling here is fully custom (font/size/color from config), relying on built-in
`Heading N` styles wasn't an option.

## Table of contents and page number fields

Both are real, updatable OOXML fields (`fldChar`/`instrText` runs), not pre-computed text -
`Docs/Architecture.md` covers why. `TableOfContentsWriter` builds the `\o` switch's level ranges by
collapsing the configured levels into contiguous runs (e.g. `[1, 3, 4]` becomes `"1-1;3-4"`), since
the field syntax only supports contiguous ranges per group.

## Mermaid diagrams

`MarkdownBodyWriter` recognizes a fenced code block by its `mermaid` info string, renders it via
`IMermaidImageRenderer` (a `mermaid.ink` HTTP call), and embeds the resulting PNG using
`DrawingElementBuilder`. The image is read for its pixel dimensions via `PngDimensionReader`
(a minimal `IHDR`-chunk parser - no decoding), then scaled down (preserving aspect ratio) if its
native width would exceed the configured maximum display width.

## Inline formatting

`InlineRunWriter` walks a Markdig inline tree recursively, threading `bold`/`italic` state down
through nested `EmphasisInline` nodes (odd delimiter counts contribute italic, counts of 2 or more
contribute bold, so `***text***` correctly produces both). A markdown code span becomes a run with
the same monospace font and gray shading as a fenced code block. Heading text reuses this same
writer for its inline content, then has the heading's configured font size/color/bold/italic
applied on top of (not instead of) whatever inline formatting was already present.
