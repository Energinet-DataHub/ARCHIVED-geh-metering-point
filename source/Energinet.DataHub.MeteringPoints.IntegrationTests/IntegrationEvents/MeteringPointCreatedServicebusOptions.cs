// Copyright 2020 Energinet DataHub A/S
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
using Squadron;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.IntegrationEvents
{
    public class ServiceBusOptions : AzureCloudServiceBusOptions
    {
        public const string ServiceBusTopic = "metering-point-created";
        public const string ServiceBusTopicSubscriber = "subscriber";

        public override void Configure(ServiceBusOptionsBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.AddTopic(ServiceBusTopic)
                .AddSubscription(ServiceBusTopicSubscriber);
        }
    }
}
