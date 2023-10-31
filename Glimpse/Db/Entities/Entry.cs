using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Glimpse.Db.Entities
{
    [Index(nameof(SnapshotId), nameof(SubjectId), IsUnique = true)]
    public class Entry : Entity
    {
        private Entry() { }

        public Entry(Snapshot snapshot, EntryType type, Subject subject, Category category, decimal quantity, decimal price)
        {
            Ensure(snapshot != null, "Snapshot cannot be null");
            
            Snapshot = snapshot;
            SnapshotId = snapshot.Id;
            Type = type;

            SetSubject(subject);
            SetCategory(category);
            SetQuantity(quantity);
            SetPrice(price);
        }

        public void SetSubject(Subject subject)
        {
            Ensure(subject != null, "Subject cannot be null");
            Ensure(Snapshot != null, "Snapshot cannot be null");
            Ensure(subject.ProfileId == Snapshot.ProfileId, "Subject is not from snapshot profile");

            Subject = subject;
            SubjectId = subject.Id;
        }

        public void SetCategory(Category category)
        {
            if (category == null)
            {
                Category = null;
                CategoryId = null;
                return;
            }
            
            Ensure(Snapshot != null, "Snapshot cannot be null");
            Ensure(category.ProfileId == Snapshot.ProfileId, "Category is not from snapshot profile");

            Category = category;
            CategoryId = category.Id;
        }

        public void SetQuantity(decimal quantity)
        {
            Ensure(quantity > 0, "Quantity must be greater than 0");
            Quantity = quantity;
        }

        public void SetPrice(decimal price)
        {
            Ensure(price != 0, "Price cannot be 0");
            Price = price;
        }

        public EntryType Type { get; set; }

        public int SnapshotId { get; private set; }
        public Snapshot Snapshot { get; private set; }

        public int SubjectId { get; private set; }
        public Subject Subject { get; private set; }

        public int? CategoryId { get; private set; }
        public Category Category { get; private set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public decimal Quantity { get; private set; }
       
        public decimal Price { get; private set; }

        public decimal DollarValue => Quantity * Price;
    }
}