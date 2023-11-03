namespace Glimpse.Operations.Snapshots;

public class SnapshotModel
{
    public int SnapshotId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public decimal CashFlowNet { get; set; }
    public decimal NetWorth { get; set; }
    public List<EntryModel> CashFlow { get; set; } = new List<EntryModel>();
    public List<EntryModel> BalanceSheet { get; set; } = new List<EntryModel>();

    public decimal? PreviousCashFlowNet { get; set; }
    public decimal? CashFlowNetChange { get; set; }
    public decimal? CashFlowNetChangePercent { get; set; }

    public decimal? PreviousNetWorth { get; set; }
    public decimal? NetWorthChange { get; set; }
    public decimal? NetWorthChangePercent { get; set; }
}

public class EntryModel
{
    public int EntryId { get; set; }
    public string Category { get; set; }
    public string Subject { get; set; }
    public decimal DollarValue { get; set; }
    public decimal? PreviousDollarValue { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePercent { get; set; }
}

public class SnapshotBuilder
{
    public SnapshotModel Build(Db.Entities.Snapshot snapshot, Db.Entities.Snapshot previousSnapshot)
    {
        var result = new SnapshotModel
        {
            SnapshotId = snapshot.Id,
            Start = snapshot.Start,
            End = snapshot.End,
            CashFlow = new List<EntryModel>(),
            BalanceSheet = new List<EntryModel>()
        };

        foreach (var entry in snapshot.Entries)
        {
            var previousEntry = previousSnapshot?.Entries?
                .FirstOrDefault(e => e.SubjectId == entry.SubjectId);

            var change = entry.DollarValue - previousEntry.DollarValue;
            var changePercent = change / Math.Abs(previousEntry.DollarValue) * 100;

            var line = new EntryModel
            {
                EntryId = entry.Id,
                Category = entry.Category?.Name,
                Subject = entry.Subject?.Name,
                DollarValue = entry.DollarValue,
                PreviousDollarValue = previousEntry.DollarValue,
                Change = change,
                ChangePercent = changePercent
            };

            if (entry.Type == Db.Entities.EntryType.CashFlow)
            {
                result.CashFlow.Add(line);
            }
            else if (entry.Type == Db.Entities.EntryType.BalanceSheet)
            {
                result.BalanceSheet.Add(line);
            }
            else throw new NotImplementedException($"EntryType '{entry.Type}' is not valid in this context");
        }

        result.CashFlowNet = result.CashFlow.Sum(a => a.DollarValue);
        result.NetWorth = result.BalanceSheet.Sum(a => a.DollarValue);

        if (previousSnapshot != null)
        {
            var previousCashFlowNet = previousSnapshot.Entries
                .Where(a => a.Type == Db.Entities.EntryType.CashFlow)
                .Sum(a => a.DollarValue);

            var previousNetWorth = previousSnapshot.Entries
                .Where(a => a.Type == Db.Entities.EntryType.BalanceSheet)
                .Sum(a => a.DollarValue);

            result.PreviousCashFlowNet = previousCashFlowNet;
            result.CashFlowNetChange = result.CashFlowNet - previousCashFlowNet;
            result.CashFlowNetChangePercent = previousCashFlowNet == 0 ? 0 :
                result.CashFlowNetChange / Math.Abs(previousCashFlowNet) * 100;

            result.PreviousNetWorth = previousNetWorth;
            result.NetWorthChange = result.NetWorth - previousNetWorth;
            result.NetWorthChangePercent = previousNetWorth == 0 ? 0 : 
                result.NetWorthChange / Math.Abs(previousNetWorth) * 100;
        }

        return result;
    }
}
