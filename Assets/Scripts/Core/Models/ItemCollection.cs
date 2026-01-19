using System.Collections.Generic;

public class ItemCollection
{
    protected readonly Dictionary<int, int> items = new();

    public int Get(int id)
        => items.GetValueOrDefault(id, 0);

    public int Add(int id, int amount)
    {
        items.TryAdd(id, 0);
        items[id] += amount;
        return items[id];
    }

    public void Clear() => items.Clear();
    
    public List<ItemAmountEntry> ToSerializable()
    {
        var list = new List<ItemAmountEntry>();
        foreach (var kv in items)
        {
            list.Add(new ItemAmountEntry
            {
                ItemId = kv.Key,
                Amount = kv.Value
            });
        }
        return list;
    }

    // 🔹 LOAD
    public void FromSerializable(List<ItemAmountEntry> data)
    {
        items.Clear();
        foreach (var entry in data)
            items[entry.ItemId] = entry.Amount;
    }
}