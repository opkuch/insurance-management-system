using InsuranceManagementService.Domain.Common;

namespace InsuranceManagementService.Domain.ValueObjects;

/// <summary>
/// A half-open period [Start, End) used for policy terms. Both endpoints are
/// stored in UTC and the end must be strictly after the start.
/// </summary>
public sealed class DateRange : ValueObject
{
    private DateRange()
    {
    }

    public DateRange(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
        {
            throw new DomainException("The term end date must be after the start date.");
        }

        Start = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        End = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
    }

    public DateTime Start { get; private set; }

    public DateTime End { get; private set; }

    public int DurationDays => (End - Start).Days;

    public bool Includes(DateTime instantUtc) => instantUtc >= Start && instantUtc < End;

    public bool HasEnded(DateTime nowUtc) => nowUtc >= End;

    public bool HasStarted(DateTime nowUtc) => nowUtc >= Start;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
}
