namespace NServiceBus.AcceptanceTests.Routing.NativePublishSubscribe
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    // copied over from core and excluded the core test to be able to apply the retry attribute the reason is this test is sometimes subjected to policy propagation delays on the queues which can make published to be dead lettered
    public class When_multi_subscribing_to_a_polymorphic_event : NServiceBusAcceptanceTest
    {
        [Test]
        [Retry(5)] // due to policy propagation problems
        public void Both_events_should_be_delivered()
        {
            Requires.NativePubSubSupport();

            Context context = null;

            Assert.DoesNotThrowAsync(async () =>
            {
                context = await Scenario.Define<Context>()
                    .WithEndpoint<Publisher1>(b => b.When(c => c.EndpointsStarted, (session, c) =>
                    {
                        c.AddTrace("Publishing MyEvent1");
                        return session.Publish(new MyEvent1());
                    }))
                    .WithEndpoint<Publisher2>(b => b.When(c => c.EndpointsStarted, (session, c) =>
                    {
                        c.AddTrace("Publishing MyEvent2");
                        return session.Publish(new MyEvent2());
                    }))
                    .WithEndpoint<Subscriber>()
                    .Done(c => c.SubscriberGotIMyEvent && c.SubscriberGotMyEvent2)
                    .Run();
            });

            Assert.True(context.SubscriberGotIMyEvent);
            Assert.True(context.SubscriberGotMyEvent2);
        }

        public class Context : ScenarioContext
        {
            public bool SubscriberGotIMyEvent { get; set; }
            public bool SubscriberGotMyEvent2 { get; set; }
        }

        public class Publisher1 : EndpointConfigurationBuilder
        {
            public Publisher1()
            {
                EndpointSetup<DefaultPublisher>();
            }
        }

        public class Publisher2 : EndpointConfigurationBuilder
        {
            public Publisher2()
            {
                EndpointSetup<DefaultPublisher>();
            }
        }

        public class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>();
            }

            public class MyEventHandler : IHandleMessages<IMyEvent>
            {
                public Context Context { get; set; }

                public Task Handle(IMyEvent messageThatIsEnlisted, IMessageHandlerContext context)
                {
                    Context.AddTrace($"Got event '{messageThatIsEnlisted}'");
                    if (messageThatIsEnlisted is MyEvent2)
                    {
                        Context.SubscriberGotMyEvent2 = true;
                    }
                    else
                    {
                        Context.SubscriberGotIMyEvent = true;
                    }

                    return Task.FromResult(0);
                }
            }
        }

        public class MyEvent1 : IMyEvent
        {
        }

        public class MyEvent2 : IMyEvent
        {
        }

        public interface IMyEvent : IEvent
        {
        }
    }
}