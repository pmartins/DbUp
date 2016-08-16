using DbUp.Engine.Transactions;
using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DbUp.Support.SqlServer;

namespace DbUp.SybaseAse
{
    public class SybaseAseConnectionManager : DatabaseConnectionManager
    {
        public SybaseAseConnectionManager(string connectionString)
            : base(new DelegateConnectionFactory(l => new AseConnection(connectionString)))
        { 
        
        }

        public override IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            var commandSplitter = new SqlCommandSplitter(); //new SybaseAseCommandSplitter();
            var scriptStatements = commandSplitter.SplitScriptIntoCommands(scriptContents);
            return scriptStatements;
        }
    }
}
