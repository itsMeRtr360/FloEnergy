namespace FloEnergyMeterReadings.Models
{
    public class MeterReading
    {
        public string? Nmi { get; set; }
        public DateTime IntervalTimeStamp { get; set; }
        public decimal Consumption { get; set; }

        public MeterReading(string? nmi, DateTime intervalTimeStamp, decimal value)
        {
            Nmi = nmi;
            IntervalTimeStamp = intervalTimeStamp;
            Consumption = value;
        }

    }



}
