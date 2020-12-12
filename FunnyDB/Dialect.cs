using System;
using System.Collections.Generic;
using System.Text;

namespace FunnyDB
{
    /// <summary>
    /// Provides a set of methods which allows to build parametrised plain SQL query.
    /// </summary>
    public static class Dialect
    {
        [ThreadStatic] private static Dictionary<string, SqlQueryParameter> _queryParameters;
        [ThreadStatic] private static StringBuilder _parameterNameBuilder;

        public static int NextParameterIndex => _queryParameters.Count;
        
        // ReSharper disable once InconsistentNaming
        public static SqlQuery sql(Func<string> query, char? margin = '|')
        {
            var parameters = _queryParameters;
            if (parameters == null)
            {
                parameters = new Dictionary<string, SqlQueryParameter>();
                _queryParameters = parameters;
            }

            try
            {
                var body = query();
                if (margin.HasValue)
                {
                    body = Strip(body, margin.Value);
                }

                return new SqlQuery(body, parameters.Values);
            }
            finally
            {
                parameters.Clear();
            }
        }

        public static string p((SqlQueryValue, string) namedValue)
        {
            var (value, name) = namedValue;
            var parameter = new DbSqlQueryParameter(name, value.DbType, value.Value);
            return p(parameter);
        }

        // ReSharper disable once InconsistentNaming
        public static string p(SqlQueryValue value, params SqlQueryValue[] values)
        {
            var sb = _parameterNameBuilder;
            if (sb == null)
            {
                sb = new StringBuilder();
                _parameterNameBuilder = sb;
            }

            try
            {
                sb.Append(p(value));

                foreach (var vn in values)
                {
                    sb.Append(", ");
                    sb.Append(p(vn));
                }

                return sb.ToString();
            }
            finally
            {
                sb.Clear();
            }
        }

        public static string p(SqlQueryValue value)
        {
            var index = NextParameterIndex;
            var parameter = new DbSqlQueryParameter(index, value.DbType, value.Value);
            return p(parameter);
        }

        public static string p(SqlQueryParameter parameter, params SqlQueryParameter[] parameters)
        {
            var sb = _parameterNameBuilder;
            if (sb == null)
            {
                sb = new StringBuilder();
                _parameterNameBuilder = sb;
            }

            try
            {
                sb.Append(p(parameter));

                foreach (var pn in parameters)
                {
                    sb.Append(", ");
                    sb.Append(p(pn));
                }

                return sb.ToString();
            }
            finally
            {
                sb.Clear();
            }
        }

        public static string p(SqlQueryParameter parameter)
        {
            var parameters = _queryParameters;
            var parameterName = parameter.Name;
            if (parameter.Index == SqlQueryParameter.PinnedNameIndex
                && parameters.TryGetValue(parameterName, out var existParameter))
            {
                if (Equals(existParameter.Value, parameter.Value))
                {
                    return GetParameterNameForQuery(parameterName);
                }

                throw new InvalidOperationException(
                    "Named parameter unexpectedly used with different values (" +
                    $"parameter_name={parameterName}," +
                    $"expected_value={existParameter.Value}," +
                    $"actual_value={parameter.Value})");
            }

            parameters.Add(parameterName, parameter);
            return GetParameterNameForQuery(parameterName);
        }

        private static string GetParameterNameForQuery(string parameterName)
        {
            return parameterName.StartsWith("@") ? parameterName : "@" + parameterName;
        }

        private static string Strip(string text, char margin)
        {
            var sb = new StringBuilder();
            var skip = true;
            foreach (var ch in text)
            {
                if (char.IsWhiteSpace(ch) && skip)
                {
                    continue;
                }

                if (ch == margin && skip)
                {
                    skip = false;
                    continue;
                }

                sb.Append(ch);
                skip = ch == '\n';
            }

            return sb.ToString();
        }
    }
}