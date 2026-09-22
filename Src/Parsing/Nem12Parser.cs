using FloEnergyMeterReadings.Models;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace FloEnergyMeterReadings.Parsing
{
    public class Nem12Parser
    {
        private readonly string _filePath;

        public Nem12Parser(string filePath)
        {
            _filePath = filePath;
        }


        public async IAsyncEnumerable<MeterReading> ParseAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            const int bufferSize = 64 * 1024;

            await using var input = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: bufferSize,
                useAsync: true);

            using var reader = new StreamReader(input, bufferSize: bufferSize);

            string? currentNmi = null;
            int intervalMinutes = 30;
            string? line;
        
    

            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                int commaIdx = line.IndexOf(',');
                if (commaIdx == -1) continue;

                ReadOnlySpan<char> recordType = line.AsSpan(0, commaIdx);

                if (recordType.SequenceEqual("200"))
                {
                    var fields = line.Split(',');
                    currentNmi = fields[1];
                    intervalMinutes = int.Parse(fields[8], CultureInfo.InvariantCulture);
                }
                else if (recordType.SequenceEqual("300"))
                {
                    if (currentNmi is null)
                        throw new InvalidDataException("300 record encountered before any 200 record.");

                    foreach (var reading in ParseIntervalRecord(line, currentNmi, intervalMinutes))
                    {
                        yield return reading;
                    }
                }
            }
        }

        private static IEnumerable<MeterReading> ParseIntervalRecord(string line, string currentNmi, int intervalMinutes)
        {
            var fields = line.Split(',');
            if (fields.Length < 4)
                throw new InvalidDataException($"Invalid 300 record: {line}");
            int expectedIntervalCount = 1440 / intervalMinutes; //  48 for 30-min
            int minRequiredFields = 1 + 1 + expectedIntervalCount + 5; // RecordIndicator + Date + N values + 5 trailing fields = 55 minimum

            if (fields.Length < minRequiredFields)
            {
                throw new InvalidDataException(
                    $"Corrupted 300 record: Expected at least {minRequiredFields} CSV fields for {intervalMinutes}-min intervals, but found {fields.Length}.");
            }
            // Parse date (Format: YYYYMMDD)
            if (!DateTime.TryParseExact(fields[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var baseDate))
            {
                throw new InvalidDataException($"Invalid interval date format: '{fields[1]}'");
            }

            // Calculate expected intervals (e.g., 24h * 60m / 30m = 48)
            int expectedIntervals = (24 * 60) / intervalMinutes;

            // Interval values start at index 2 and run for expectedIntervals count
            for (int i = 0; i < expectedIntervals; i++)
            {
                int fieldIndex = 2 + i;
                if (fieldIndex >= fields.Length)
                {
                    throw new InvalidDataException($"300 record missing expected interval fields at index {fieldIndex}");
                }

                if (!decimal.TryParse(fields[fieldIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    throw new InvalidDataException($"Invalid interval value '{fields[fieldIndex]}' at position {i + 1}");
                }

                // NEM12 specifies interval timestamps denote the END of the interval (i + 1)
                var intervalTimestamp = baseDate.AddMinutes(intervalMinutes * (i + 1));

                yield return new MeterReading(currentNmi, intervalTimestamp, value);
            }
        }
    }
}