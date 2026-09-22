# Flo Energy Meter Readings

A .NET 8 console tool that parses NEM12 meter reading files and generates batched SQL `INSERT` statements for loading interval consumption data into a database.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Building

From the repository root:

```powershell
dotnet build FloEnergyMeterReadings.sln
```

## Running

```powershell
dotnet run --project Src\FloEnergyMeterReadings.csproj -- <inputFilePath> <outputFilePath>
```

- `inputFilePath` — path to the NEM12 file to parse (defaults to `C:\Users\RahulTR\Desktop\Flo\sampleData.txt` if omitted).
- `outputFilePath` — path where the generated SQL `INSERT` statements will be written (defaults to `C:\Users\RahulTR\Desktop\Flo\output.sql` if omitted).

Example:

```powershell
dotnet run --project Src\FloEnergyMeterReadings.csproj -- "C:\data\sampleData.txt" "C:\data\output.sql"
```

You can also run the built executable directly:

```powershell
dotnet build FloEnergyMeterReadings.sln -c Release
.\Src\bin\Release\net8.0\FloEnergyMeterReadings.exe "C:\data\sampleData.txt" "C:\data\output.sql"
```

Press `Ctrl+C` at any time to cancel a running parse/write operation gracefully.

## Running tests

```powershell
dotnet test FloEnergyMeterReadings.sln
```

## Project structure

```
Src/
  Program.cs           Entry point: wires the parser to the SQL writer
  Models/              MeterReading data model
  Parsing/             Nem12Parser - streams MeterReading records from a NEM12 file
  Output/              SqlInsertWriter - writes batched SQL INSERT statements
Tests/
  MeterReadingTests/    xUnit tests for Nem12Parser
```
