namespace MiniPie.Controls.CircularProgressButton
{
    public interface IProgressReporter
    {
        double Maximum { get; set; }
        double Minimum { get; set; }
        double Value { get; set; }
    }
}
