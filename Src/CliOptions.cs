using System.Globalization;

namespace FloEnergyMeterReadings;

public sealed class CliArgumentException(string message) : Exception(message);

public sealed class CliOptions
{
    public required string InputPath { get; init; }
    public required string OutputPath { get; init; }
    public int BatchSize { get; init; } = 1000;
    public bool ShowHelp { get; init; }

    public const string UsageText =
        """
        FloEnergyMeterReadings - NEM12 to meter_readings INSERT statement generator

        Usage:
          FloEnergyMeterReadings --input FILE.csv [--output FILE.sql] [--batch-size N] [--skip-invalid]
          FloEnergyMeterReadings -i FILE.csv -o -   (writes to stdout)

        Options:
          -i, --input FILE       Path to the NEM12 input file. Required.
          -o, --output FILE      Path to write generated SQL to. Defaults to the input
                                  file's path with its extension replaced by .sql.
                                  Pass "-" to write to stdout.
              --batch-size N     Rows per multi-row INSERT statement. Default: 1000.
              --skip-invalid     Skip interval values that fail to parse instead of
                                  aborting the run (a warning is printed for each one).
                                  Default is to fail fast.
          -h, --help              Show this message.
        """;

    public static CliOptions Parse(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;
        var batchSize = 1000;
        var showHelp = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h":
                case "--help":
                    showHelp = true;
                    break;

                case "-i":
                case "--input":
                    inputPath = RequireValue(args, ref i, "--input");
                    break;

                case "-o":
                case "--output":
                    outputPath = RequireValue(args, ref i, "--output");
                    break;

                case "--batch-size":
                    var rawBatchSize = RequireValue(args, ref i, "--batch-size");
                    if (!int.TryParse(rawBatchSize, NumberStyles.Integer, CultureInfo.InvariantCulture, out batchSize)
                        || batchSize <= 0)
                    {
                        throw new CliArgumentException($"--batch-size must be a positive integer, got '{rawBatchSize}'.");
                    }

                    break;

                default:
                    throw new CliArgumentException($"Unrecognized argument: '{args[i]}'.");
            }
        }

        if (showHelp)
        {
            return new CliOptions { InputPath = string.Empty, OutputPath = string.Empty, ShowHelp = true };
        }

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new CliArgumentException("--input is required.");
        }

        outputPath ??= Path.ChangeExtension(inputPath, ".sql");

        return new CliOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            BatchSize = batchSize,
        };
    }

    private static string RequireValue(string[] args, ref int i, string flagName)
    {
        if (i + 1 >= args.Length)
        {
            throw new CliArgumentException($"{flagName} requires a value.");
        }

        return args[++i];
    }
}
