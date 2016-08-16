using DbUp.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbUp.SybaseAse
{
    /// <summary>
    /// This preprocessor makes adjustments to your sql to make it compatible with SybaseAse.
    /// </summary>
    public class SybaseAsePreprocessor : IScriptPreprocessor
    {
        /// <summary>
        /// Performs some proprocessing step on a SybaseAse script.
        /// </summary>
        public string Process(string contents)
        {
            return contents;
        }

    }
}
