﻿namespace ChillChaser.Models.Response
{
    public class AppUsageAnalysis
    {
        public required string Name { get; set; }
        public required double AverageStress { get; set; }
        public required double ReferenceStress { get; set; }
        public required TimeSpan Usage { get; set; }
    }
}
