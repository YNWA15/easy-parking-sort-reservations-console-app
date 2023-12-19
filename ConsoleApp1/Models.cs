using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class ParkingSpot
    {
        public int Id { get; set; }
        public int? NumberInParking { get; set; }
        public int ParkingId { get; set; }
        //  [JsonIgnore]
        public Parking? Parking { get; set; }
        public bool IsFree { get; set; } = true;
        public DateTime? IsPaidTill { get; set; }
        public bool? IsReserved { get; set; } = false;
        public DateTime? ReservedFrom { get; set; } = null;
        public DateTime? ReservedTo { get; set; } = null;
        public bool? HasRoof { get; set; } = false;
        public double? CostPerHour { get; set; } = 2;
        public double? CostPer4h { get; set; } = 5;
        public double? CostPer8h { get; set; } = 8;
        public double? CostPerDay { get; set; } = 20;
        public double? CostPerWeek { get; set; } = 100;
        public string? CarRegNumber { get; set; } = null;
        public List<Reservation>? Reservations { get; set; }

    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public double? ParkCredits { get; set; } = null;
        public int? FailedTimes { get; set; } = null;
        public virtual List<string>? Vehicles { get; set; }
        public virtual List<Reservation>? Reservations { get; set; }
    }
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime StartReservationPeriod { get; set; }
        public DateTime EndReservationPeriod { get; set; }
        public DateTime CreatedResrvationTime { get; set; }
        public int ParkingId { get; set; }
        //[JsonIgnore]
        public Parking? Parking { get; set; }
        public int SpotId { get; set; }
        //[ForeignKey("SpotId")]
        //[JsonIgnore]

        public ParkingSpot? Spot { get; set; }
        //[ForeignKey(nameof(User))]
        public int UserId { get; set; }
        // [JsonIgnore]
        public User? User { get; set; }
        public double Price { get; set; }

        public double? UpdatedPeriodPrice { get; set; } = null;

        public bool IsPaid { get; set; } = false;
        public bool IsStarted { get; set; } = false;
        public bool IsEnded { get; set; } = false;
        public bool CanStartEarly { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public bool IsFailed { get; set; } = false;
        public bool isUserCreditsUpdated { get; set; } = false;
        public bool Is15MinOver { get; set; } = false;
        public double MinOver { get; set; } = 0;
        public string CarRegNumber { get; set; }
        public bool is1hrAdded { get; set; } = false;
    }
    public class Parking
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<ParkingSpot>? ParkingSpots { get; set; }
        public int? Capacity { get { return ParkingSpots?.ToList().Count; } set { } }
        public bool IsAvailable { get; set; } = true;
        public bool HasFreeSpot { get; set; } = true;
        public double? Rating { get; set; } = 3;
        public bool IsSecured { get; set; } = true;
        public ICollection<Employee>? Employees { get; set; }
        public int? EmployeesCount { get { return Employees?.ToList().Count; } }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
        public double CostPerHour { get; set; } = 2;
        public double CostPer4h { get; set; } = 5;
        public double CostPer8h { get; set; } = 8;
        public double CostPerDay { get; set; } = 20;
        public double CostPerWeek { get; set; } = 100;
    }
    public class Employee : User
    {
        //   public int Id { get; set; }
        public int ParkingId { get; set; }
        public Parking? Parking { get; set; }
        public int EmployeeId { get; set; }
        public double SalaryPerHour { get; set; } = 6;
        public double HoursPerMonth { get; set; } = 144;
        public DateTime SignDate { get; set; } = DateTime.Now;
    }
}
