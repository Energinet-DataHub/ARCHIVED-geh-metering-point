﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching
{
    public class DomainEventsAccessor : IDomainEventsAccessor
    {
        private readonly MeteringPointContext _context;

        public DomainEventsAccessor(MeteringPointContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IReadOnlyCollection<IDomainEvent> GetAllDomainEvents()
        {
            var domainEvents = _context.ChangeTracker
                .Entries<Entity>()
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            return domainEvents;
        }

        public void ClearAllDomainEvents()
        {
            _context.ChangeTracker
                .Entries<Entity>()
                .ToList()
                .ForEach(e => e.Entity.ClearDomainEvents());
        }
    }
}
