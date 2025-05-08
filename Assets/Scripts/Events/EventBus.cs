using System;
using System.Collections.Generic;

public enum UIEventType
{
    StateButton,
    Slider,
    VideoButton,
    Volume,      // add new kinds here
    /* … */
}

public static class EventBus
{
    // maps (UIEventType) → multicast delegate of type Action<object>
    static readonly Dictionary<UIEventType, Delegate> _events = new();

    public static void Subscribe<T>(UIEventType type, Action<T> handler)
    {
        if (_events.TryGetValue(type, out var existing))
            _events[type] = Delegate.Combine(existing, (Action<T>)(o => handler((T)o)));
        else
            _events[type] = new Action<T>(o => handler((T)o));
    }

    public static void Unsubscribe<T>(UIEventType type, Action<T> handler)
    {
        if (!_events.TryGetValue(type, out var existing)) return;
        var toRemove = (Action<T>)(o => handler((T)o));
        var updated = Delegate.Remove(existing, toRemove);
        if (updated == null) _events.Remove(type);
        else                _events[type] = updated;
    }

    public static void Publish<T>(UIEventType type, T payload)
    {
        if (_events.TryGetValue(type, out var d))
            ((Action<T>)d)(payload);
    }
}