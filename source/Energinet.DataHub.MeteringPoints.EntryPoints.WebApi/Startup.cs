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
using System.Text.Json.Serialization;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Energinet.DataHub.Core.App.WebApp.SimpleInjector;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.GridAreas.Create;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.RequestResponse.Contract;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SimpleInjector;
using JsonSerializer = Energinet.DataHub.MeteringPoints.Infrastructure.Serialization.JsonSerializer;
using MasterDataDocument = Energinet.DataHub.MeteringPoints.Application.MarketDocuments.MasterDataDocument;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.WebApi
{
    public class Startup : IDisposable
    {
        private readonly Container _container;
        private bool _disposed;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _container = new Container();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddSwaggerGen(config =>
            {
                config.SupportNonNullableReferenceTypes();
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "Energinet.DataHub.MeteringPoints.EntryPoints.WebApi", Version = "v1", });

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer", },
                };

                config.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement { { securitySchema, new[] { "Bearer" } }, };

                config.AddSecurityRequirement(securityRequirement);
            });

            var connectionString = Configuration.GetConnectionString("METERINGPOINT_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Metering point db connection string not found.");

            services.AddDbContext<MeteringPointContext>(x =>
            {
                x.UseSqlServer(connectionString, y => y.UseNodaTime());
            });

            services.AddLogging();
            services.AddSimpleInjector(_container, options =>
            {
                options.AddAspNetCore()
                    .AddControllerActivation();
                options.AddLogging();
            });

            services.AddTransient<IMiddlewareFactory>(_ => new SimpleInjectorMiddlewareFactory(_container));

            services.UseSimpleInjectorAspNetRequestScoping(_container);

            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
            _container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            _container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            _container.Register<IGridAreaRepository, GridAreaRepository>(Lifestyle.Scoped);
            _container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            _container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            _container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Singleton);
            _container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            _container.Register<ISystemDateTimeProvider, SystemDateTimeProvider>(Lifestyle.Singleton);
            _container.Register(typeof(IBusinessProcessResultHandler<CreateGridArea>), typeof(CreateGridAreaNullResultHandler), Lifestyle.Scoped);
            _container.Register<IValidator<CreateGridArea>, CreateGridAreaRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<MasterDataDocument>, ValidationRuleSet>(Lifestyle.Scoped);
            _container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            _container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            _container.Register<IDomainEventPublisher, DomainEventPublisher>();
            _container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);
            _container.Register<IBusinessProcessValidationContext, BusinessProcessValidationContext>(Lifestyle.Scoped);
            _container.Register(typeof(ProtobufOutboundMapper<>), typeof(ProtobufOutboundMapper<>).Assembly);
            _container.Register(typeof(ProtobufInboundMapper<>), typeof(ProtobufInboundMapper<>).Assembly);
            _container.Register<ProtobufOutboundMapperFactory>();
            _container.Register<ProtobufInboundMapperFactory>();

            _container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(connectionString), Lifestyle.Scoped);
            _container.Register<DbGridAreaHelper>(Lifestyle.Scoped);

            var openIdUrl = Configuration["FRONTEND_OPEN_ID_URL"] ?? throw new InvalidOperationException(
                "Frontend OpenID URL not found.");

            var audience = Configuration["FRONTEND_SERVICE_APP_ID"] ?? throw new InvalidOperationException(
                "Frontend service app id not found.");

            _container.AddJwtTokenSecurity(openIdUrl, audience);
            _container.AddUserContext<UserProvider>();

            Dapper.SqlMapper.AddTypeHandler(NodaTimeSqlMapper.Instance);

            _container.UseMediatR()
                .WithPipeline(
                    typeof(UnitOfWorkBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    typeof(InternalCommandHandlingBehaviour<,>),
                    typeof(BusinessProcessResultBehavior<,>))
                .WithRequestHandlers(
                    typeof(CreateGridAreaHandler),
                    typeof(MeteringPointProcessesByGsrnQueryHandler),
                    typeof(MeteringPointByGsrnQueryHandler));

            _container.SendProtobuf<MeteringPointEnvelope>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energinet.DataHub.MeteringPoints.EntryPoints.WebApi v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<JwtTokenMiddleware>();
            app.UseMiddleware<UserMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSimpleInjector(_container);
            _container.Verify();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _container.Dispose();
            _disposed = true;
        }
    }
}
