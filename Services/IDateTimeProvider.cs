namespace SmartPOS.Services;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}
