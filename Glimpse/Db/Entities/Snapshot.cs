namespace Glimpse.Db.Entities
{
    public class Snapshot : Entity
    {
        private Snapshot() { }

        public Snapshot(Profile profile, DateTime start, DateTime end)
        {
            Ensure(profile != null, "Profile cannot be null");            

            Profile = profile;
            ProfileId = profile.Id;                        

            SetDates(start, end);
        }

        public int ProfileId { get; private set; }

        public Profile Profile { get; private set; }

        public DateTime Start { get; private set; }

        public DateTime End { get; private set; }
        
        public IEnumerable<Entry> Entries { get; private set; }

        public void SetDates(DateTime start, DateTime end)
        {
            Ensure(start <= end, "Cannot end before start");
            Start = start;
            End = end;
        }

    }
}
