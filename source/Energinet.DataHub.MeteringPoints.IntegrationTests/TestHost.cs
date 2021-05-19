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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    [Collection("IntegrationTest")]
    public class TestHost : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private bool _disposed;

        protected TestHost()
        {
            CleanupDatabase();

            var services = new ServiceCollection();

            services.AddDbContext<MeteringPointContext>(x =>
                x.UseSqlServer(ConnectionString, y => y.UseNodaTime()));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMeteringPointRepository, MeteringPointRepository>();

            services.AddMediatR(new[]
            {
                typeof(CreateMeteringPoint).Assembly,
                //typeof(PublishWhenEnergySupplierHasChanged).Assembly,
            });

            // Busines process responders
            //services.AddScoped<IBusinessProcessResponder<RequestChangeOfSupplier>, RequestChangeOfSupplierResponder>();

            // Input validation
            //services.AddScoped<IValidator<RequestChangeOfSupplier>, RequestChangeOfSupplierRuleSet>();

            // Business process pipeline
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InputValidationBehavior<,>));
            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainEventsDispatcherBehavior<,>));
            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(BusinessProcessResponderBehavior<,>));
            _serviceProvider = services.BuildServiceProvider();
        }

        private string ConnectionString =>
            Environment.GetEnvironmentVariable("MeteringPoints_IntegrationTests_ConnectionString");

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == true)
            {
                return;
            }

            CleanupDatabase();
            _serviceProvider.Dispose();
            _disposed = true;
        }

        protected TService GetService<TService>()
        {
            return _serviceProvider.GetRequiredService<TService>();
        }

        private void CleanupDatabase()
        {
            var cleanupStatement = $"";
            //new SqlCommand(cleanupStatement, GetSqlDbConnection()).ExecuteNonQuery();
        }
    }
}
