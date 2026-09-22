namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Converts a markdown pipe table into an OpenXML document table.</summary>
internal interface ITableConverter
{
    /// <summary>Converts <paramref name="table"/> into an OpenXML document table.</summary>
    /// <param name="table">The markdown table to convert.</param>
    /// <param name="inlineRunWriter">The writer used to convert each cell's inline content.</param>
    /// <returns>The converted document table.</returns>
    Table Convert(Markdig.Extensions.Tables.Table table, IInlineRunWriter inlineRunWriter);
}

/// <inheritdoc cref="ITableConverter" />
internal sealed class TableConverter : ITableConverter
{
    /// <inheritdoc />
    public Table Convert(Markdig.Extensions.Tables.Table table, IInlineRunWriter inlineRunWriter)
    {
        Table wordTable = new(
            new TableProperties(
                new TableWidth { Type = TableWidthUnitValues.Auto },
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 })),
            new TableGrid(table.ColumnDefinitions.Select(_ => new GridColumn())));

        bool isHeaderRow = true;
        foreach (Markdig.Extensions.Tables.TableRow row in table)
        {
            TableRow wordRow = new();
            foreach (Markdig.Extensions.Tables.TableCell cell in row)
            {
                wordRow.AppendChild(BuildCell(cell, inlineRunWriter, bold: isHeaderRow && row.IsHeader));
            }

            wordTable.AppendChild(wordRow);
            isHeaderRow = false;
        }

        return wordTable;
    }

    private TableCell BuildCell(Markdig.Extensions.Tables.TableCell cell, IInlineRunWriter inlineRunWriter, bool bold)
    {
        TableCell wordCell = new(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
        bool appendedParagraph = false;

        foreach (Block block in cell)
        {
            if (block is not ParagraphBlock paragraph)
            {
                continue;
            }

            Paragraph wordParagraph = new();
            wordParagraph.Append(inlineRunWriter.Write(paragraph.Inline));
            if (bold)
            {
                foreach (Run run in wordParagraph.Elements<Run>())
                {
                    (run.RunProperties ??= new RunProperties()).Bold = new Bold();
                }
            }

            wordCell.AppendChild(wordParagraph);
            appendedParagraph = true;
        }

        if (!appendedParagraph)
        {
            wordCell.AppendChild(new Paragraph());
        }

        return wordCell;
    }
}
