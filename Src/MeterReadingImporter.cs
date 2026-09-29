using FloEnergyMeterReadings.Output;
using FloEnergyMeterReadings.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloEnergyMeterReadings
{
    public class MeterReadingImporter
    {
        private readonly Nem12Parser _parser;
        private readonly SqlInsertWriter _writer;

        public MeterReadingImporter(Nem12Parser parser, SqlInsertWriter writer)
        {
            _parser = parser;
            _writer = writer;
        }

        public async Task<int> ImportAsync(CancellationToken cancellationToken)
        {
            var readings = _parser.ParseAsync();
            await _writer.WriteAsync(readings, cancellationToken);
            return 0;
        }

    }
}
