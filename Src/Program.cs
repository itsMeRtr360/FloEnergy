using FloEnergyMeterReadings.Parsing;
using FloEnergyMeterReadings.Output;


namespace FloEnergyMeterReadings
{
    class FloEnergyMeterReadings
    {
        public static async Task<int> Main(string[] args)
        {
            CliOptions options;
            try
            {
                options = CliOptions.Parse(args);
            }
            catch (CliArgumentException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine();
                Console.Error.WriteLine(CliOptions.UsageText);
                return 1;
            }
            if (options.ShowHelp)
            {
                Console.WriteLine(CliOptions.UsageText);
                return 0;
            }

            if (!File.Exists(options.InputPath))
            {
                Console.Error.WriteLine($"Error: input file not found: {options.InputPath}");
                return 1;
            }

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            var parser = new Nem12Parser(options.InputPath);
            var readings = parser.ParseAsync(cts.Token);
            var writer = new SqlInsertWriter(outputFilePath: options.OutputPath, batchSize: options.BatchSize);
            await writer.WriteAsync(readings, cts.Token);
            return 0;
        }
    }
}