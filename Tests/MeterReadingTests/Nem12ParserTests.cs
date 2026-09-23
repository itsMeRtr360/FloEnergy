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
            // Arrange
            var sampleData = @"100,NEM12,202609221400,TESTDP,NEMMCO
200,NEM1201009,E1E2,1,E1,N1,01009,kWh,30,20260922
300,20260301,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.461,0.810,0.568,1.234,1.353,1.507,1.344,1.773,0.848,1.271,0.895,1.327,1.013,1.793,0.988,0.985,0.876,0.555,0.760,0.938,0.566,0.512,0.970,0.760,0.731,0.615,0.886,0.531,0.774,0.712,0.598,0.670,0.587,0.657,A,,,20260302120000,20260302120000
900";

            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, sampleData);

            try
            {
                var parser = new Nem12Parser(tempFilePath);
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
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task ParseAsync_Standard30MinFile_Missing48IntervalValues()
        {
            // Arrange
            var sampleData = @"100,NEM12,202609221400,TESTDP,NEMMCO
200,NEM1201009,E1E2,1,E1,N1,01009,kWh,30,20260922
300,20260301,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.461,0.810,0.568,1.234,1.353,1.507,1.344,1.773,0.848,1.271,0.895,1.327,1.013,1.793,0.988,0.985,0.876,0.555,0.760,0.938,0.566,0.512,0.970,0.760,0.731,0.615,0.886,0.531,0.774,0.712,0.598,0.670,0.587,0.657,A,,,20260302120000,20260302120000
900";

            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, sampleData);

            try
            {
                var parser = new Nem12Parser(tempFilePath);
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
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }


        [Fact]
        public async Task ParseAsync_Muliple200Records()
        {
            var parser = new Nem12Parser(@"../../../SampleTestInputs/sampleData1.txt");
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
    }
}
