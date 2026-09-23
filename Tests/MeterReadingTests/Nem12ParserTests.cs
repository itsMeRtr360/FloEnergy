using FloEnergyMeterReadings.Models;
using FloEnergyMeterReadings.Parsing;
using Xunit;

namespace MeterReadingTests
{
    public class Nem12ParserTests
    {
        [Fact]
        public async Task ParseAsync_Standard30MinFile_Parses48RecordsCorrectly()
        {
                var parser = new Nem12Parser(@"../../../SampleTestInputs/sampleDataSimple200Record.txt");
                var results = new List<MeterReading>();

                // Act
                await foreach (var reading in parser.ParseAsync())
                {
                    results.Add(reading);
                }

                // Assert
                Assert.Equal(48, results.Count);
                Assert.Equal("NEM1201009", results[0].Nmi);
                Assert.Equal(new DateTime(2026, 3, 1, 0, 30, 0), results[0].IntervalTimeStamp);
                Assert.Equal(0.0m, results[0].Consumption);

                Assert.Equal(new DateTime(2026, 3, 1, 6, 30, 0), results[12].IntervalTimeStamp);
                Assert.Equal(0.461m, results[14].Consumption);

                Assert.Equal(new DateTime(2026, 3, 2, 0, 0, 0), results[47].IntervalTimeStamp);
                Assert.Equal(0.657m, results[47].Consumption);


        }

        [Fact]
        public async Task ParseAsync_Standard30MinFile_Missing48IntervalValues()
        {
            try
            {
                var parser = new Nem12Parser(@"../../../SampleTestInputs/sampleDataMissingConsumption.txt");
                var results = new List<MeterReading>();

                // Act
                await foreach (var reading in parser.ParseAsync())
                {
                    results.Add(reading);
                }

            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsType<InvalidDataException>(ex);
                Assert.Equal("Corrupted 300 record: Expected at least 55 CSV fields for 30-min intervals, but found 53.", ex.Message);
            }
        }


        [Fact]
        public async Task ParseAsync_Muliple200Records()
        {
            var parser = new Nem12Parser(@"../../../SampleTestInputs/sampleDataMultiple200.txt");
            var results = new List<MeterReading>();

            // Act
            await foreach (var reading in parser.ParseAsync())
            {
                results.Add(reading);
            }
            Assert.Equal(192, results.Count);
            Assert.Equal("NEM1201009", results[0].Nmi);
            Assert.Equal("NEM1201010", results[97].Nmi);
            Assert.Equal(96, results.Where(r => r.Nmi == "NEM1201009").Count());
            Assert.Equal(96, results.Where(r => r.Nmi == "NEM1201010").Count());
            Assert.Equal(47, results.Where(r => r.Nmi == "NEM1201009" && r.IntervalTimeStamp.Date == new DateTime(2026, 9, 21)).Count());
            Assert.Equal(48, results.Where(r => r.Nmi == "NEM1201009" && r.IntervalTimeStamp.Date == new DateTime(2026, 9, 22)).Count());


        }


        [Fact]
        public async Task ParseAsync_300RecordWithout200()
        {
            try
            {
                var parser = new Nem12Parser(@"../../../SampleTestInputs/sampleData300Without200.txt");
                var results = new List<MeterReading>();

                // Act
                await foreach (var reading in parser.ParseAsync())
                {
                    results.Add(reading);
                }
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsType<InvalidDataException>(ex);
                Assert.Equal("300 record encountered before any 200 record.", ex.Message);
            }
        }
    }
}
