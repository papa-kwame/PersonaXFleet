using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class Schedule
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        [Required]
        [StringLength(50)]
        public string MaintenanceType { get; set; }

        [Required]
        public DateTime LastServiceDate { get; set; }

        [Required]
        public int LastServiceMileage { get; set; }

        [Required]
        public DateTime NextServiceDate { get; set; }

        [Required]
        public int NextServiceMileage { get; set; }

        [Required]
        public int CurrentMileage { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } 
    }
}
