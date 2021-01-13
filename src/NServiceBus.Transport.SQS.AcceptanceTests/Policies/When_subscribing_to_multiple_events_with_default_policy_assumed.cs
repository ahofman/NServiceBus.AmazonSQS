﻿namespace NServiceBus.AcceptanceTests.Policies
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_subscribing_to_multiple_events_with_default_policy_assumed : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_deliver_events()
        {
            // settles policy
            await Scenario.Define<Context>()
                .WithEndpoint<Publisher>()
                .WithEndpoint<Subscriber>()
                .Done(c => c.EndpointsStarted)
                .Run();

            // actual run with publish
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Publisher>(b => b.When(async session =>
                {
                    await session.Publish(new MyEvent());
                    await session.Publish(new MyOtherEvent());
                }))
                .WithEndpoint<Subscriber>(b => b.CustomConfig(c =>
                {
                    var policies = c.ConfigureSqsTransport().Policies();
                    policies.AssumePolicyHasAppropriatePermissions();
                }))
                .Done(c => c.GotEvents)
                .Run();

            Assert.IsTrue(context.GotMyEvent);
            Assert.IsTrue(context.GotMyOtherEvent);
        }

        public class Context : ScenarioContext
        {
            public bool GotMyEvent { get; set; }
            public bool GotMyOtherEvent { get; set; }

            public bool GotEvents => GotMyEvent && GotMyOtherEvent;
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
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

            public class MyEventHandler : IHandleMessages<MyEvent>
            {
                Context testContext;

                public MyEventHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    testContext.GotMyEvent = true;
                    return Task.CompletedTask;
                }
            }

            public class MyEventOtherHandler : IHandleMessages<MyEvent>
            {
                Context testContext;

                public MyEventOtherHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    testContext.GotMyOtherEvent = true;
                    return Task.CompletedTask;
                }
            }
        }

        public class MyEvent : IEvent
        {
        }

        public class MyOtherEvent : IEvent
        {
        }
    }
}