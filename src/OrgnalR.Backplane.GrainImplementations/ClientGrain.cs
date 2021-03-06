using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;
using OrgnalR.Backplane.GrainInterfaces;
using OrgnalR.Core.Provider;
using Orleans;

namespace OrgnalR.Backplane.GrainImplementations
{
    public class ClientGrain : Grain, IClientGrain
    {
        private readonly GrainObserverManager<IClientMessageObserver> observers = new GrainObserverManager<IClientMessageObserver>
        {
            ExpirationDuration = TimeSpan.FromMinutes(5),
            OnFailBeforeDefunct = x => x.SubscriptionEnded()
        };

        private IRewindableMessageGrain<HubInvocationMessage> rewoundMessagesGrain = null!;

        public override Task OnActivateAsync()
        {
            rewoundMessagesGrain = GrainFactory.GetGrain<IRewindableMessageGrain<HubInvocationMessage>>(this.GetPrimaryKeyString());
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            foreach (var observer in observers)
            {
                observer.SubscriptionEnded();
            }
            return base.OnDeactivateAsync();
        }

        public async Task AcceptMessageAsync(HubInvocationMessage message, GrainCancellationToken cancellationToken)
        {
            var handle = await rewoundMessagesGrain.PushMessageAsync(message);
            observers.Notify(x => x.ReceiveMessage(message, handle));
        }

        public async Task SubscribeToMessages(IClientMessageObserver observer, MessageHandle since)
        {
            observers.Subscribe(observer);
            if (since != default)
            {
                foreach (var (message, handle) in await rewoundMessagesGrain.GetMessagesSinceAsync(since))
                {
                    observer.ReceiveMessage(message, handle);
                }
            }
        }

        public Task UnsubscribeFromMessages(IClientMessageObserver observer)
        {
            observers.Unsubscribe(observer);
            return Task.CompletedTask;
        }

    }
}
