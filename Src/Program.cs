using FloEnergyMeterReadings.Output;
using FloEnergyMeterReadings.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FloEnergyMeterReadings;

internal static class Program
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

        // No args passed: your CLI parser owns the command line, not IConfiguration
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddSingleton(options);
        builder.Services.AddTransient(sp =>
            new Nem12Parser(sp.GetRequiredService<CliOptions>().InputPath));
        builder.Services.AddTransient(sp =>
        {
            var o = sp.GetRequiredService<CliOptions>();
            return new SqlInsertWriter(outputFilePath: o.OutputPath, batchSize: o.BatchSize);
        });
        builder.Services.AddTransient<MeterReadingImporter>();

        using var host = builder.Build();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var importer = host.Services.GetRequiredService<MeterReadingImporter>();
        return await importer.ImportAsync(cts.Token);
    }
}