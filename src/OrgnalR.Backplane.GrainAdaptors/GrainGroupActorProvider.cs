﻿using System;
using OrgnalR.Backplane.GrainInterfaces;
using OrgnalR.Core.Provider;
using OrgnalR.Core.State;
using Orleans;

namespace OrgnalR.Backplane.GrainAdaptors
{
    public class GrainActorProvider : IGroupActorProvider, IUserActorProvider
    {
        private readonly string hubName;
        private readonly IGrainFactory grainFactory;

        public GrainActorProvider(
            string hubName,
            IGrainFactory grainFactory
            )
        {
            this.hubName = hubName ?? throw new ArgumentNullException(nameof(hubName));
            this.grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
        }
        public IGroupActor GetGroupActor(string groupName)
        {
            return new GrainGroupActor(grainFactory.GetGrain<IGroupActorGrain>($"{hubName}::{groupName}"));
        }

        public IUserActor GetUserActor(string userId)
        {
            return new GrainUserActor(grainFactory.GetGrain<IUserActorGrain>($"{hubName}::{userId}"));
        }
    }
}