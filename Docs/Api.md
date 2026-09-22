# API

Document Generator has no library API - it's a single-file executable. Its "surface" is the
command line invocation and the JSON configuration schema it reads. This document covers the
*shape and flow* of that surface, the same way a library's `Api.md` would cover its public types.

## Shape

The executable takes exactly three positional arguments:

```
BlueHeighliner.DocumentGenerator <config.json> <input.md> <output.docx>
```

`config.json` is a single JSON object with four top-level concerns: per-level heading styling
(`heading1`-`heading6`, including whether each level appears in the table of contents and gets a
page break after its content), the page header and footer (`header`/`footer`, each with
left/center/right-stacked content - individual lines can be restricted to only the cover page or
only non-cover pages - plus whether and where to show a page number), and whole-document layout
(`title`/`subtitle` styled cover page text and whether to trim the source markdown to only the
content between its outer horizontal bars). There is deliberately one config file rather than
separate style/layout files - every setting in it affects how the same markdown source is laid out
on the page, so splitting it up would just force a caller to keep two files in sync for one
concern.

`input.md` is the markdown source. `output.docx` is overwritten with the generated document.

## Flow

1. The configuration file is deserialized into a `DocumentConfiguration`.
2. The markdown file is read as text and, if `exclusion` is set, trimmed down to the content
   between its first and last horizontal bar line.
3. The trimmed markdown is parsed into a Markdig document.
4. A new `.docx` package is created with three sections: a cover page (`title`/`subtitle`,
   vertically centered), an optional table of contents page (only if any heading level has
   `tableOfContents` set), and the body.
5. The body is built by walking the parsed markdown in document order, converting each block to
   its OpenXML equivalent (see `Docs/Components/Rendering.md` for how each block kind is handled)
   and inserting page breaks after headings at any level with `pageBreakAfter` set.
6. The package is saved to `output.docx` and the process exits 0, or exits 1 with an error message
   on stderr if any step fails.

## Configuration Schema

Every key is optional. An omitted key falls back to its default below, so a config file only needs
to specify the keys whose value differs from the default - `{}` is itself a valid (if minimal)
configuration.

### Document (root object)

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `title` | `CoverText` | size 36, bold | Cover page title. |
| `subtitle` | `CoverText` | size 20 | Cover page subtitle. |
| `exclusion` | `bool` | `false` | When `true`, only the markdown between the first and last horizontal bar (`---`/`***`/`___`) in the source is rendered; the bars themselves and everything outside them are dropped. |
| `heading1` ... `heading6` | `Heading` | see Heading below | That level's heading styling and numbering. |
| `header` | `PageSection` | `{}` | Page header content. |
| `footer` | `PageSection` | `{}` | Page footer content. |

### CoverText (`title` / `subtitle`)

In JSON, a plain array of strings is shorthand for `{ "lines": [...] }`, leaving every other
property at its default. A partial object (e.g. just `{ "italic": true }`) also keeps the rest of
that field's default styling - it doesn't reset to this table's generic per-property defaults.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `lines` | `string[]` | `[]` | The text, one entry per line. |
| `size` | `number` | `36` for `title`, `20` for `subtitle` | Font size, in points. |
| `bold` | `bool` | `true` for `title`, `false` for `subtitle` | Whether the text is bold. |
| `italic` | `bool` | `false` | Whether the text is italic. |
| `color` | `string` | `"#000000"` | Font color, as a hex RGB string (`"1F2937"` or `"#1F2937"`). |

### Heading (`heading1` ... `heading6`)

Each level defaults to a plain, unnumbered, bold heading with a decreasing size and spacing
scale, so all six are usable out of the box with no configuration at all; levels 1 and 2 also
default to appearing in the table of contents. A partial object (e.g. just
`{ "numbering": "decimal" }`) keeps the rest of that level's own defaults below - it doesn't reset
to a shared generic default.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `size` | `number` | `24`/`20`/`16`/`14`/`12`/`11` (levels 1-6) | Font size, in points. |
| `bold` | `bool` | `true` | Whether the heading text is bold. |
| `italic` | `bool` | `false` | Whether the heading text is italic. |
| `color` | `string` | `"#000000"` | Font color, as a hex RGB string (`"1F2937"` or `"#1F2937"`). |
| `numbering` | `string` | `"none"` | Numbering style - one of the values in Numbering Styles below. |
| `numberingPrefix` | `bool` | `false` | When `true`, this level's number is prefixed with its ancestor levels' numbers (e.g. `1.2.1`); when `false`, it's numbered independently (e.g. just `1`). |
| `spaceBefore` | `number` | `24`/`18`/`14`/`12`/`10`/`8` (levels 1-6) | Paragraph spacing before the heading, in points. |
| `spaceAfter` | `number` | `12`/`9`/`7`/`6`/`5`/`4` (levels 1-6) | Paragraph spacing after the heading, in points. |
| `tableOfContents` | `bool` | `true` for levels 1-2, `false` for levels 3-6 | Whether this level is included in the table of contents. If no level across `heading1`-`heading6` has this set, the table of contents page is omitted entirely. |
| `pageBreakAfter` | `bool` | `false` | When `true`, a page break is inserted immediately after this level's content ends (right before the next heading at this level or shallower). |

### Numbering Styles (`heading.numbering` values)

| Value | Produces |
| --- | --- |
| `none` | No numbering. |
| `decimal` | Arabic decimal (`1`, `2`, `3`, ...). |
| `upperRoman` | Uppercase Roman numerals (`I`, `II`, `III`, ...). |
| `lowerRoman` | Lowercase Roman numerals (`i`, `ii`, `iii`, ...). |
| `upperLetter` | Uppercase letters (`A`, `B`, `C`, ..., `Z`, `AA`, ...). |
| `lowerLetter` | Lowercase letters (`a`, `b`, `c`, ..., `z`, `aa`, ...). |

### PageSection (`header` / `footer`)

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `left` | `Line[]` | `[]` | Lines stacked at the left edge. |
| `center` | `Line[]` | `[]` | Lines stacked at the center. |
| `right` | `Line[]` | `[]` | Lines stacked at the right edge. |
| `pageNumbers` | `string` | `"none"` | Whether the current page number is shown, and where it's aligned - one of the values in PageNumbers below. |

Lines at the same array index across `left`/`center`/`right` land on the same row (e.g. `left[0]`
and `right[0]` render on the same line, on opposite sides of the page); a position with fewer
lines than another just leaves the remaining rows blank on that side. In a header, `left[0]` etc.
is the topmost row (farthest from the body) and later indices stack downward, toward the body. In
a footer, it's the opposite: `left[0]` etc. is the row closest to the page's bottom edge (also
farthest from the body) and later indices stack upward, toward the body. The page number, if
enabled, is always inserted as row 0 - i.e. the topmost row in a header, or the row closest to the
page's bottom edge in a footer - ahead of any `left`/`center`/`right` rows.

### Line (`pageSection.left` / `pageSection.center` / `pageSection.right` entry)

A line is either a plain JSON string - shorthand for `{ "line": "<string>", "mode": "all" }` - or
an object:

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `line` | `string` | *(required)* | The line's text. |
| `mode` | `string` | `"all"` | Which page(s) this line is shown on - one of the values in Mode below. |

### Mode (`line.mode` values)

| Value | Shown on |
| --- | --- |
| `all` | Every page, including the cover page. |
| `cover` | Only the cover (first) page. |
| `body` | Every page except the cover page. |

### PageNumbers (`pageSection.pageNumbers` values)

| Value | Produces |
| --- | --- |
| `none` | No page number. |
| `left` | A left-aligned page number. |
| `center` | A center-aligned page number. |
| `right` | A right-aligned page number. |
