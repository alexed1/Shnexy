using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Enumerations
{
    public class EventCreateType
    {
        [Key]
        public int Id { get; set; }
        public String Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}