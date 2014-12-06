using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.States.Templates
{
    public class _RemoteCalendarServiceInterfaceTemplate : IStateTemplate<RemoteCalendarServiceInterface>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [MaxLength(16)]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}