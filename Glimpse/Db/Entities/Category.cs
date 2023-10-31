using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Glimpse.Db.Entities
{
    [Index(nameof(ProfileId), nameof(Name), IsUnique = true)]
    public class Category : Entity
    {
        private Category() { }

        public Category(Profile profile, string name)
        {
            Ensure(profile != null, "Profile is required");
            Ensure(!string.IsNullOrWhiteSpace(name), "Name is required");

            Profile = profile;
            ProfileId = profile.Id;
            Name = name;
        }
        public int ProfileId { get; private set; }
        public Profile Profile { get; private set; }

        [Required]
        [MaxLength(50)]        
        public string Name { get; set; }

    }
}