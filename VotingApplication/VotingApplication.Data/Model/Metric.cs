using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingApplication.Data.Model
{
    public class Metric
    {
        public Metric(MetricType metricType, Guid pollId)
        {
            Timestamp = DateTime.UtcNow;
            MetricType = metricType;
            StatusCode = 200;
            PollId = pollId;
        }

        public long Id { get; set; }

        [Required]
        [Index]
        public DateTime Timestamp { get; set; }

        [Required]
        public MetricType MetricType { get; set; }

        [Required]
        [Index]
        public Guid PollId { get; set; }

        public int StatusCode { get; set; }

        public string Value { get; set; }
        public string Detail { get; set; }
    }
}
