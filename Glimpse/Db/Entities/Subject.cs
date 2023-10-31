using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Glimpse.Db.Entities
{
    [Index(nameof(ProfileId), nameof(Name), IsUnique = true)]
    public class Subject : Entity
    {
        private Subject() { }

        public Subject(Profile profile, string name, string description = "") 
        {
            Ensure(profile != null, "Profile is required");
            Ensure(!string.IsNullOrWhiteSpace(name), "Name is required");

            Profile = profile;
            ProfileId = profile.Id;
            Name = name;
            Description = description;
        }

        public int ProfileId { get; private set; }

        public Profile Profile { get; private set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }

    
}