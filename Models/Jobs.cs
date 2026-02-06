using System;

namespace SillowApp.Models
{
    public class Job
    {
        public int Id { get; set; }    
        public string Title { get; set; } 
        public string Description { get; set; } 
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; } 
        public double? Latitude { get; set; }   // optional
        public double? Longitude { get; set; }  // optional

        public string JobAddress { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string JobNotes { get; set; }
        public DateTime? JobDateTime { get; set; }
        public decimal? JobCost { get; set; }
    }
}