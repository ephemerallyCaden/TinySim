using System.Collections.Generic;

// A generic list with deferred add/remove queues.
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

    // Apply all queued additions and removals.
    public void ApplyChanges()
    {
        if (_toAdd.Count > 0)
        {
            _items.AddRange(_toAdd);
            _toAdd.Clear();
        }

        if (_toRemove.Count > 0)
        {
            _items.RemoveAll(item => _toRemove.Contains(item));
            _toRemove.Clear();
        }
    }

    // Remove null entries (mostly objects that have been destroyed)).
    public void RemoveNullAt(int index)
    {
        _items.RemoveAt(index);
    }

    public int PendingAddCount => _toAdd.Count;
}
