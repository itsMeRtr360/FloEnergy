using FloEnergyMeterReadings.Models;
using System.Globalization;
using System.Text;

namespace FloEnergyMeterReadings.Output
{
    public class SqlInsertWriter
    {
        private const string TABLE_NAME = "meter_readings";
        private const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";

        private readonly int _batchSize;
        private readonly string? _outputFilePath;

        public SqlInsertWriter(int batchSize = 1000, string? outputFilePath = null)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be positive.");
            }

            _batchSize = batchSize;
            _outputFilePath = outputFilePath;

        }

        public async Task<long> WriteAsync(IAsyncEnumerable<MeterReading> readings,CancellationToken cancellationToken = default)
        {
            try
            {
                await using var output = new StreamWriter(
                   new FileStream(_outputFilePath ?? "output.sql", FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true));
                {
                    var batch = new List<MeterReading>(_batchSize);
                    long totalWritten = 0;

                    await foreach (var reading in readings.WithCancellation(cancellationToken))
                    {
                        batch.Add(reading);
                        if (batch.Count == _batchSize)
                        {
                            await WriteBatchAsync(output, batch, cancellationToken).ConfigureAwait(false);
                            totalWritten += batch.Count;
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await WriteBatchAsync(output, batch, cancellationToken).ConfigureAwait(false);
                        totalWritten += batch.Count;
                        batch.Clear();
                    }
                    await output.FlushAsync(cancellationToken).ConfigureAwait(false);

                    return totalWritten;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while writing SQL insert statements.", ex);
            }
        }

        private static async Task WriteBatchAsync(
            TextWriter output, List<MeterReading> batch, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder(batch.Count * 64);
            sb.Append("INSERT INTO \"").Append(TABLE_NAME).Append("\" (\"nmi\", \"timestamp\", \"consumption\") VALUES\n");

            for (var i = 0; i < batch.Count; i++)
            {
                var reading = batch[i];
                sb.Append('(')
                  .Append('\'').Append(EscapeSqlLiteral(reading.Nmi)).Append('\'').Append(", ")
                  .Append('\'').Append(reading.IntervalTimeStamp.ToString(TIMESTAMP_FORMAT, CultureInfo.InvariantCulture)).Append('\'').Append(", ")
                  .Append(reading.Consumption.ToString(CultureInfo.InvariantCulture))
                  .Append(')');

                sb.Append(i == batch.Count - 1 ? '\n' : ',');
                if (i < batch.Count - 1)
                {
                    sb.Append('\n');
                }
            }


            await output.WriteAsync(sb, cancellationToken).ConfigureAwait(false);
        }

        private static string EscapeSqlLiteral(string? value) => value?.Replace("'", "''") ?? string.Empty;

    }
}
