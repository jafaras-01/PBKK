namespace BookingTempatWPF;

public sealed class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public string Facilities { get; set; } = "";
    public bool IsActive { get; set; }
    public string DisplayName => $"{Name} — {Location} (kap. {Capacity})";
}

public sealed class Booking
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public int RoomId { get; set; }
    public string RoomName { get; set; } = "";
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int ParticipantCount { get; set; }
    public string EventName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? DecisionNote { get; set; }
    public DateTime CreatedAt { get; set; }
}
