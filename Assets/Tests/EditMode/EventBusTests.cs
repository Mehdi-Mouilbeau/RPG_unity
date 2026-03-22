using NUnit.Framework;

public class EventBusTests
{
    [SetUp]
    public void Setup() => EventBus.Clear();

    [Test]
    public void Subscribe_ThenPublish_CallsHandler()
    {
        bool called = false;
        EventBus.Subscribe<string>(msg => called = true);
        EventBus.Publish("hello");
        Assert.IsTrue(called);
    }

    [Test]
    public void Unsubscribe_ThenPublish_DoesNotCallHandler()
    {
        bool called = false;
        System.Action<string> handler = _ => called = true;
        EventBus.Subscribe(handler);
        EventBus.Unsubscribe(handler);
        EventBus.Publish("hello");
        Assert.IsFalse(called);
    }

    [Test]
    public void Publish_WithNoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => EventBus.Publish(42));
    }

    [Test]
    public void MultipleSubscribers_AllReceiveEvent()
    {
        int count = 0;
        EventBus.Subscribe<int>(_ => count++);
        EventBus.Subscribe<int>(_ => count++);
        EventBus.Publish(1);
        Assert.AreEqual(2, count);
    }
}
