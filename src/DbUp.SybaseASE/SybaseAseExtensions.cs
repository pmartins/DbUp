using DbUp.Builder;
using DbUp.Engine.Transactions;
using DbUp.Support.SqlServer;
using DbUp.Support.SybaseAse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbUp.SybaseAse
{
    public static class SybaseAseExtensions
    {
        /// <summary>
        /// Creates an upgrader for SybaseAse databases.
        /// </summary>
        /// <param name="supported">Fluent helper type.</param>
        /// <param name="connectionString">SybaseAse database connection string.</param>
        /// <returns>
        /// A builder for a database upgrader designed for SybaseAse databases.
        /// </returns>
        public static UpgradeEngineBuilder SybaseAseDatabase(this SupportedDatabases supported, string connectionString)
        {
            return SybaseAseDatabase(new SybaseAseConnectionManager(connectionString));
        }

        /// <summary>
        /// Creates an upgrader for SybaseAse databases.
        /// </summary>
        /// <param name="connectionManager">The <see cref="SybaseAseConnectionManager"/> to be used during a database upgrade.</param>
        /// <returns>
        /// A builder for a database upgrader designed for SybaseAse databases.
        /// </returns>
        public static UpgradeEngineBuilder SybaseAseDatabase(IConnectionManager connectionManager)
        {
            var builder = new UpgradeEngineBuilder();
            builder.Configure(c => c.ConnectionManager = connectionManager);
            builder.Configure(c => c.ScriptExecutor = new SqlScriptExecutor(() => c.ConnectionManager, () => c.Log, null, () => c.VariablesEnabled, c.ScriptPreprocessors));
            builder.Configure(c => c.Journal = new SybaseAseTableJournal(() => c.ConnectionManager, () => c.Log, null, "SchemaVersions"));
            builder.WithPreprocessor(new SybaseAsePreprocessor());
            return builder;
        }
    }
}
