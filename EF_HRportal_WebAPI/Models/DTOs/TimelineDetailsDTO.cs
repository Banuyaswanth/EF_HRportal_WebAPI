﻿namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class TimelineDetailsDto
    {
        public string Action { get; set; } = null!;

        public DateTime DateOfAction { get; set; }

        public int Id { get; set; }
    }
}
