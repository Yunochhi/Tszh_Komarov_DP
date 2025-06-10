namespace TSZH_Komarov.Viewmodels.Readings
{
    public class MeterReadingHistViewModel
    {
        public List<ReadingHistItem> readingHistItems { get; set; }

        public DateTime ReadingDate { get; set; }
    }

    public class ReadingHistItem
    {
        public int MeterTypeId { get; set; }
        public string MeterName { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }
}
