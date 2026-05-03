using System.Collections.Generic;

/// <summary>
/// A generic list with deferred add/remove queues.
/// Safely modify during iteration by queuing changes and applying after.
/// </summary>
public class DeferredEntityList<T>
{
    private readonly List<T> _items = new List<T>();
    private readonly List<T> _toAdd = new List<T>();
    private readonly HashSet<T> _toRemove = new HashSet<T>();

    public IReadOnlyList<T> Items => _items;
    public int Count => _items.Count;

    public T this[int index] => _items[index];

    public void QueueAdd(T item)
    {
        _toAdd.Add(item);
    }

    public void QueueRemove(T item)
    {
        _toRemove.Add(item);
    }

    /// <summary>
    /// Apply all queued additions and removals. Call after iteration is complete.
    /// </summary>
    public void ApplyChanges()
    {
        if (_toAdd.Count > 0)
        {
            _items.AddRange(_toAdd);
            _toAdd.Clear();
        }

        if (_toRemove.Count > 0)
        {
            // Single-pass removal with O(1) HashSet lookup per item
            _items.RemoveAll(item => _toRemove.Contains(item));
            _toRemove.Clear();
        }
    }

    /// <summary>
    /// Remove null entries (for Unity objects that have been destroyed).
    /// </summary>
    public void RemoveNullAt(int index)
    {
        _items.RemoveAt(index);
    }

    public int PendingAddCount => _toAdd.Count;
}
