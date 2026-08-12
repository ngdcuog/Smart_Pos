namespace SmartPOS.Services;

public sealed class AttendanceSettings
{
    public string WorkStartTime { get; init; } = "08:00";
    public int LateGraceMinutes { get; init; } = 15;

    public TimeSpan GetLateThreshold()
    {
        if (!TimeSpan.TryParse(WorkStartTime, out var workStart)) workStart = new TimeSpan(8, 0, 0);
        return workStart.Add(TimeSpan.FromMinutes(Math.Max(0, LateGraceMinutes)));
    }
}
