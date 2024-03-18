// SpotifyGPX by Simon Field

using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Table.PivotTable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpotifyGPX.Output;

public partial class Xlsx : ByteSaveable
{
    protected override byte[] Document { get; }

    public Xlsx(IEnumerable<SongPoint> pairs)
    {
        (byte[] doc, int totalPairs) = GetDocument(pairs);

        Document = doc;
        Count = totalPairs;
    }

    private static (byte[], int) GetDocument(IEnumerable<SongPoint> pairs)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        ExcelPackage package = new();

        int totalPairs = 0;

        // Create a worksheet for each track
        pairs.GroupBy(pair => pair.Origin)
            .ToList()
            .ForEach(track =>
            {
                Random random = new();
                int red = random.Next(256);
                int green = random.Next(256);
                int blue = random.Next(256);
                Color tabColor = Color.FromArgb(red, green, blue);

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(track.Key.ToString());
                worksheet.TabColor = tabColor;

                // Headers
                worksheet.Cells[1, 1].Value = "#"; // A
                worksheet.Cells[1, 2].Value = "Index"; // B
                worksheet.Cells[1, 3].Value = "Latitude"; // C
                worksheet.Cells[1, 4].Value = "Longitude"; // D
                worksheet.Cells[1, 5].Value = "Artist"; // E
                worksheet.Cells[1, 6].Value = "Title"; // F
                worksheet.Cells[1, 7].Value = "Album"; // G
                worksheet.Cells[1, 8].Value = "Song Time"; // H
                worksheet.Cells[1, 9].Value = "Point Time"; // I
                worksheet.Cells[1, 10].Value = "Accuracy"; // J
                worksheet.Cells[1, 11].Value = "AbsAccuracy"; // K
                worksheet.Cells[1, 12].Value = "Duration"; // L
                worksheet.Cells[1, 13].Value = "Country"; // M
                worksheet.Cells[1, 14].Value = "IP Address"; // N

                // Data
                int row = 2;
                track.ToList().ForEach(pair =>
                {
                    worksheet.Cells[row, 1].Value = row - 1; // A
                    worksheet.Cells[row, 2].Value = pair.Index; // B
                    worksheet.Cells[row, 3].Value = pair.Point.Location.Latitude; // C
                    worksheet.Cells[row, 4].Value = pair.Point.Location.Longitude; // D
                    worksheet.Cells[row, 5].Value = pair.Song.Song_Artist; // E
                    worksheet.Cells[row, 6].Value = pair.Song.Song_Name; // F
                    worksheet.Cells[row, 7].Value = pair.Song.Song_Album; // G
                    worksheet.Cells[row, 8].Value = pair.PointTime.UtcDateTime; // H
                    worksheet.Cells[row, 8].Style.Numberformat.Format = Options.ISO8601UTC; // Use UTC format for UtcDateTime DateTimeOffset
                    worksheet.Cells[row, 9].Value = pair.SongTime.UtcDateTime; // I
                    worksheet.Cells[row, 9].Style.Numberformat.Format = Options.ISO8601UTC;
                    worksheet.Cells[row, 10].Value = pair.Accuracy; // J
                    worksheet.Cells[row, 11].Formula = $"ABS(J{row})"; // K
                    worksheet.Cells[row, 12].Value = pair.Song.TimePlayed; // L
                    worksheet.Cells[row, 12].Style.Numberformat.Format = "[HH]:MM:SS.000"; // Use TimeSpan format for TimePlayed
                    worksheet.Cells[row, 13].Value = pair.Song.Spotify_Country; // M
                    worksheet.Cells[row, 14].Value = pair.Song.Spotify_IP; // N
                    row++;
                    totalPairs++;
                });

                // Add Excel table
                var range = worksheet.Cells[1, 1, row - 1, 14]; // Select the range for the table
                var tableName = string.Concat("Table_", Guid.NewGuid().ToString().AsSpan(0, 8)); // Generate a unique table name
                var table = worksheet.Tables.Add(range, tableName); // Add table with name "Table1"
                table.TableStyle = TableStyles.Light1; // Apply a table style

                if (CreateTotalRow)
                {
                    table.ShowTotal = true; // Show total row for the table
                    table.Columns[4].TotalsRowFormula = $"INDEX(E2:E{row - 1}, MODE.SNGL(MATCH(E2:E{row - 1}, E2:E{row - 1}, 0)))"; // Top Artist
                    table.Columns[5].TotalsRowFormula = $"INDEX(F2:F{row - 1}, MODE.SNGL(MATCH(F2:F{row - 1}, F2:F{row - 1}, 0)))"; // Top Track
                    table.Columns[6].TotalsRowFormula = $"INDEX(G2:G{row - 1}, MODE.SNGL(MATCH(G2:G{row - 1}, G2:G{row - 1}, 0)))"; // Top Album
                    table.Columns[7].TotalsRowFormula = $"H{row - 1} - H2"; // Duration of point tracking
                    worksheet.Cells[$"H{row}"].Style.Numberformat.Format = "[h]:mm:ss"; // Time span format
                    table.Columns[8].TotalsRowFormula = $"I{row - 1} - I2"; // Duration of song playback
                    worksheet.Cells[$"I{row}"].Style.Numberformat.Format = "[h]:mm:ss"; // Time span format
                    table.Columns[10].TotalsRowFunction = RowFunctions.Average; // Average proximity of song-point difference, relative to zero
                    table.Columns[11].TotalsRowFunction = RowFunctions.Sum;
                    worksheet.Cells[$"L{row}"].Style.Numberformat.Format = "[h]:mm:ss"; // Time span format
                }

                if (CreatePivots)
                {
                    ExcelWorksheet pivotWorksheet = package.Workbook.Worksheets.Add($"{track.Key.ToString()}_PIVOT");
                    pivotWorksheet.TabColor = tabColor;

                    // Create pivot table
                    ExcelPivotTable pivotTable = pivotWorksheet.PivotTables.Add(pivotWorksheet.Cells["A1"], range, track.Key.ToString());

                    pivotTable.PivotTableStyle = PivotTableStyles.Light9;

                    // Set rows
                    //pivotTable.RowFields.Add(pivotTable.Fields["Country"]);
                    //pivotTable.RowFields.Add(pivotTable.Fields["IP Address"]);
                    pivotTable.RowFields.Add(pivotTable.Fields["Artist"]);
                    pivotTable.RowFields.Add(pivotTable.Fields["Album"]);
                    pivotTable.RowFields.Add(pivotTable.Fields["Title"]);

                    // Set values
                    ExcelPivotTableDataField countOfArtistField = pivotTable.DataFields.Add(pivotTable.Fields["Artist"]);
                    countOfArtistField.Function = DataFieldFunctions.Count;
                    countOfArtistField.Name = "Count";

                    ExcelPivotTableDataField sumOfDurationField = pivotTable.DataFields.Add(pivotTable.Fields["Duration"]);
                    sumOfDurationField.Function = DataFieldFunctions.Sum;
                    sumOfDurationField.Name = "Duration";
                    sumOfDurationField.Format = "[HH]:MM:SS.000";

                    pivotTable.DataOnRows = false;
                }
            });

        return (package.GetAsByteArray(), totalPairs);
    }

    public override int Count { get; }

}
