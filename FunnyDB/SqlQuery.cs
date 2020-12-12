using System;
using System.Collections.Generic;
using System.Linq;

namespace FunnyDB
{
    /// <summary>
    /// Represents an SQL query with it text and parameters.
    /// </summary>
    public class SqlQuery
    {
        private readonly string _sql;
        private readonly SqlQueryParameter[] _parameters;

        /// <summary>
        /// Initializes a new instance of SQL query.
        /// </summary>
        /// <param name="sql">An SQL query text.</param>
        /// <param name="parameters">A collection of this SQL query parameters.</param>
        public SqlQuery(string sql, IEnumerable<SqlQueryParameter> parameters)
        {
            _sql = sql;
            _parameters = parameters == null ? Array.Empty<SqlQueryParameter>() : parameters.ToArray();
        }

        /// <summary>
        /// A text of this SQL query.
        /// </summary>
        public string Sql => _sql;

        /// <summary>
        /// A collection of parameters of this SQL query.
        /// </summary>
        public IReadOnlyCollection<SqlQueryParameter> Parameters => _parameters;

        /// <summary>
        /// Represents this SQL query as string. 
        /// </summary>
        public override string ToString()
        {
            return _sql;
        }

        /// <summary>
        /// Concatenates SQL queries with line break.
        /// For example, let:
        ///     q1:
        ///         SELECT id FROM A
        ///          WHERE 1 = 1 
        ///     q2:
        ///           AND id >= 1000
        /// Then:
        ///     q1 + q2:
        ///         SELECT id FROM A
        ///          WHERE 1 = 1 
        ///           AND id >= 1000
        /// </summary>
        /// <param name="q1">A first query to concatenation.</param>
        /// <param name="q2">A second query to concatenation</param>
        /// <returns>Returns a new SQL query as concatenation result of two.</returns>
        public static SqlQuery operator +(SqlQuery q1, SqlQuery q2) => Concat(q1, q2, "\r\n");

        /// <summary>
        /// Concatenates SQL queries without line break.
        /// For example, let:
        ///     q1:
        ///         SELECT id FROM A
        ///          WHERE 1 = 1 
        ///     q2:
        ///           AND id >= 1000
        /// Then:
        ///     q1 / q2:
        ///         SELECT id FROM A
        ///         WHERE 1 = 1 AND id >= 1000
        /// </summary>
        /// <param name="q1">A first query to concatenation.</param>
        /// <param name="q2">A second query to concatenation</param>
        /// <returns>Returns a new SQL query as concatenation result of two.</returns>
        public static SqlQuery operator /(SqlQuery q1, SqlQuery q2) => Concat(q1, q2, "");

        private static SqlQuery Concat(SqlQuery q1, SqlQuery q2, string delimiter)
        {
            var generatedParameters = q1._parameters
                .Where(_ => _.Index != SqlQueryParameter.PinnedNameIndex)
                .OrderByDescending(_ => _.Index);

            var (q2Sql, q2Params) = ReindexParameters(generatedParameters.Count(), q2._sql, q2._parameters);
            var sql = q1._sql + delimiter + q2Sql;
            var parameters = new List<SqlQueryParameter>(q1._parameters);
            parameters.AddRange(q2Params);

            return new SqlQuery(sql, parameters);
        }

        private static (string, SqlQueryParameter[]) ReindexParameters(
            int startIndex,
            string sql,
            SqlQueryParameter[] parameters)
        {
            var newSql = sql;
            var newParameters = new List<SqlQueryParameter>();
            var generatedParameters = parameters
                .Where(_ => _.Index != SqlQueryParameter.PinnedNameIndex)
                .OrderByDescending(_ => _.Index);

            foreach (var parameter in generatedParameters)
            {
                var newParameter = parameter.ChangeIndex(startIndex + parameter.Index);
                newSql = newSql.Replace(parameter.Name, newParameter.Name);
                newParameters.Add(newParameter);
            }

            return (newSql, newParameters.ToArray());
        }
    }
}