using System.ComponentModel.DataAnnotations;

namespace Glimpse.Db.Entities
{
    public abstract class Entity
    {
        [Key]
        public int Id { get; protected set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public DateTime ModifiedOn { get; set; }

        [Required]
        [StringLength(250)]
        public string CreatedBy { get; set; }

        [Required]
        [StringLength(250)]
        public string ModifiedBy { get; set; }

        protected void Ensure(bool condition, string message = null)
        {
            if (!condition)
                throw new InvalidOperationException($"{GetType().Name}: {message ?? "Unspecified violation"}");
        }
    }

}