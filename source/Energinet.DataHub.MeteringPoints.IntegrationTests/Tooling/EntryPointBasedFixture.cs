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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class EntryPointBasedFixture<TEntryPoint> : IDisposable
        where TEntryPoint : EntryPoint, new()
    {
        private readonly Container _container;
        private readonly Scope _scope;
        private bool _disposed;

        protected EntryPointBasedFixture(
            DatabaseFixture databaseFixture)
        {
            if (databaseFixture == null) throw new ArgumentNullException(nameof(databaseFixture));
            databaseFixture.DatabaseManager.UpgradeDatabase();
            DatabaseConnectionString = databaseFixture.DatabaseManager.ConnectionString;

            SetEnvironmentVariables();

            _container = new Container();
            InitializeContainer(_container);

            OverrideRegistrations();

            _container.Verify();
            _scope = AsyncScopedLifestyle.BeginScope(_container);
            _container
                .GetInstance<ICorrelationContext>()
                .SetId(Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.Ordinal));

            Arrange = new ArrangeHelper(_container);
            Act = new ActHelper(_container);
            Assert = new AssertHelper(_container);
        }

        protected string DatabaseConnectionString { get; }

        protected ArrangeHelper Arrange { get; }

        protected ActHelper Act { get; }

        protected AssertHelper Assert { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected internal TService GetService<TService>()
            where TService : class
        {
            return _container.GetInstance<TService>();
        }

        protected virtual void AddEnvironmentVariables([NotNull] Dictionary<string, string> variables) { }

        protected virtual void OverrideRegistrations([NotNull] Container container) { }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || disposing)
            {
                return;
            }

            CleanupDatabase();
            _scope.Dispose();
            _container.Dispose();
            _disposed = true;
        }

        private static void InitializeContainer(Container container)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var entryPoint = new TEntryPoint();
            var containerField = typeof(EntryPoint).GetField(
                "_container",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            containerField?.SetValue(entryPoint, container);
            entryPoint.ConfigureApplication();

            container.Options.AllowOverridingRegistrations = true;
        }

        private void SetEnvironmentVariables()
        {
            var variables = new Dictionary<string, string>();
            AddEnvironmentVariables(variables);
            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
            }
        }

        private void OverrideRegistrations()
        {
            OverrideRegistrations(_container);
        }

        private void CleanupDatabase()
        {
            var cleanupStatement = new StringBuilder();

            cleanupStatement.AppendLine("DELETE FROM EnergySuppliers");
            cleanupStatement.AppendLine("DELETE FROM MeteringPoints");
            cleanupStatement.AppendLine("DELETE FROM OutboxMessages");
            cleanupStatement.AppendLine("DELETE FROM QueuedInternalCommands");
            cleanupStatement.AppendLine("DELETE FROM GridAreaLinks");
            cleanupStatement.AppendLine("DELETE FROM GridAreas");
            cleanupStatement.AppendLine("DELETE FROM MessageHubMessages");
            cleanupStatement.AppendLine("DELETE FROM BusinessProcesses");

            _container.GetInstance<MeteringPointContext>()
                .Database.ExecuteSqlRaw(cleanupStatement.ToString());
        }
    }
}
