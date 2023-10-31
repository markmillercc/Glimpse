using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Glimpse.Db.Entities
{
    public class User : IdentityUser
    {
        private User() { }
        public User(string firstName, string middleName, string lastName)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
        }

        [MaxLength(255)]
        public string FirstName { get; set; }

        [MaxLength(255)]
        public string MiddleName { get; set; }

        [MaxLength(255)]
        public string LastName { get; set; }
        public IEnumerable<Profile> Profiles { get; private set; }
    }
}
