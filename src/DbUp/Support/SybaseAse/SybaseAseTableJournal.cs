﻿using System;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Sybase.Data.AseClient;

namespace DbUp.Support.SybaseAse
{
    /// <summary>
    /// An implementation of the <see cref="IJournal"/> interface which tracks version numbers for a 
    /// SybaseAse database using a table called SchemaVersions.
    /// </summary>
    public sealed class SybaseAseTableJournal : IJournal
    {
        private readonly string schemaTableName;
        private readonly Func<IConnectionManager> connectionManager;
        private readonly Func<IUpgradeLog> log;

        private static string QuoteIdentifier(string identifier)
        {
            return "[" + identifier + "]";
        }

        /// <summary>
        /// Creates a new SybaseAse table journal.
        /// </summary>
        /// <param name="connectionManager">The SybaseAse connection manager.</param>
        /// <param name="logger">The upgrade logger.</param>
        /// <param name="schema">The name of the schema the journal is stored in.</param>
        /// <param name="table">The name of the journal table.</param>
        public SybaseAseTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema, string table)
        {
            schemaTableName = string.IsNullOrEmpty(schema)
                ? QuoteIdentifier(table)
                : QuoteIdentifier(schema) + "." + QuoteIdentifier(table);
            this.connectionManager = connectionManager;
            log = logger;        
        }

        private static string CreateTableSql(string tableName)
        {
            return string.Format(
                            @"CREATE TABLE {0}
                              (
                                [Id] int identity not null constraint PK_SchemaVersions_Id primary key,
                                [ScriptName] nvarchar(255) not null,
                                [Applied] datetime not null
                              )", tableName);
        }

        public string[] GetExecutedScripts()
        {
            log().WriteInformation("Fetching list of already executed scripts.");
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(string.Format("The {0} table could not be found. The database is assumed to be at version 0.", schemaTableName));
                return new string[0];
            }

            var scripts = new List<string>();
            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText = GetExecutedScriptsSql(schemaTableName);
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            scripts.Add((string)reader[0]);
                    }
                }
            });

            return scripts.ToArray();
        }

        /// <summary>
        /// Records an upgrade script for a database.
        /// </summary>
        /// <param name="script">The script.</param>
        public void StoreExecutedScript(SqlScript script)
        {
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(string.Format("Creating the {0} table", schemaTableName));

                connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
                {
                    using (var command = dbCommandFactory())
                    {
                        command.CommandText = CreateTableSql(schemaTableName);

                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }

                    log().WriteInformation(string.Format("The {0} table has been created", schemaTableName));
                });
            }

            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText = string.Format("insert into {0} ([ScriptName], [Applied]) values (@scriptName, @applied)", schemaTableName);

                    var scriptNameParam = command.CreateParameter();
                    scriptNameParam.ParameterName = "scriptName";
                    scriptNameParam.Value = script.Name;
                    command.Parameters.Add(scriptNameParam);

                    var appliedParam = command.CreateParameter();
                    appliedParam.ParameterName = "applied";
                    appliedParam.Value = DateTime.Now;
                    command.Parameters.Add(appliedParam);

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            });
        }

        private static string GetExecutedScriptsSql(string table)
        {
            return string.Format("select [ScriptName] from {0} order by [ScriptName]", table);
        }

        private bool DoesTableExist()
        {
            return connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                try
                {
                    using (var command = dbCommandFactory())
                    {
                        command.CommandText = string.Format("select count(*) from {0}", schemaTableName);
                        command.CommandType = CommandType.Text;
                        command.ExecuteScalar();
                        return true;
                    }
                }
                catch (AseException)
                {
                    return false;
                }
            });
        }
    }
}