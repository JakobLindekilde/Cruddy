using System.ComponentModel.DataAnnotations;

namespace Cruddy.Tests.Models
{
    public class DateTimeType
    {
        [Key]
        public int Id { get; set; }                        // int IDENTITY, PK
        public DateTime DateTimeNotNull { get; set; }      // datetime NOT NULL
        public DateTime? DateTimeNullable { get; set; }    // datetime NULL
        public DateTimeOffset DateTimeOffsetNotNull { get; set; }    // datetimeoffset(7) NOT NULL
        public DateTimeOffset? DateTimeOffsetNullable { get; set; }  // datetimeoffset(7) NULL
        public TimeSpan TimeSpanNotNull { get; set; }      // time(7) NOT NULL
        public TimeSpan? TimeSpanNullable { get; set; }    // time(7) NULL
        public TimeOnly TimeOnlyNotNull { get; set; }      // time(7) NOT NULL
        public TimeOnly? TimeOnlyNullable { get; set; }    // time(7) NULL

        //TODO: Support for DateOnly missing
        //public DateOnly DateOnlyNotNull { get; set; }      // datetime NOT NULL  
        //public DateOnly? DateOnlyNullable { get; set; }    // datetime NULL
    }
}
