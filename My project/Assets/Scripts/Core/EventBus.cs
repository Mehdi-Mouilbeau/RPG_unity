using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, List<object>> _handlers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<object>();
        _handlers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var list))
            list.Remove(handler);
    }

    public static void Publish<T>(T eventData)
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list)) return;
        foreach (var handler in new List<object>(list))
            ((Action<T>)handler)(eventData);
    }

    public static void Clear() => _handlers.Clear();
}
