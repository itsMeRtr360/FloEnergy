using FloEnergyMeterReadings.Models;
using FloEnergyMeterReadings.Output;
using FloEnergyMeterReadings.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MeterReadingTests
{
    public class SqlInsertWriterTests
    {
        private readonly string _outputFolder = @"../../../SampleTestOutputs/";
        private readonly string _inputFolder = @"../../../SampleTestInputs/";

        [Fact]
        public async Task WriteAsyncRecordCount()
        {
            try
            {
                if(File.Exists(@$"{_outputFolder}/output.sql"))
                {
                    File.Delete(@$"{_outputFolder}/output.sql");
                }
                var parser = new Nem12Parser(@$"{_inputFolder}/sampleDataMultiple200.txt");
                var writer = new SqlInsertWriter(1000,@$"{_outputFolder}/output.sql");

                var results = parser.ParseAsync();

                var count = await writer.WriteAsync(results);
                Assert.Equal(192, count);
                Assert.True(File.Exists($@"{_outputFolder}/output.sql"));

                //cleanup
                File.Delete(@$"{_outputFolder}/output.sql");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in WriteAsyncRecordCount test: {ex.Message}", ex);
            }
        }
    }
}
