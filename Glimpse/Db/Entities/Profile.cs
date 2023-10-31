using System.ComponentModel.DataAnnotations;

namespace Glimpse.Db.Entities
{
    public class Profile : Entity
    {
        private Profile() { }

        public Profile(string name)
        {
            Name = name;
        }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public string UserId { get; private set; }
        public User User { get; private set; }
        
        public IEnumerable<Category> Categories { get; private set; }        
        public IEnumerable<Subject> Subjects { get; private set; }                
        public IEnumerable<Snapshot> Snapshots { get; private set; }
    }
}