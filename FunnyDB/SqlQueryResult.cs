using System;
using System.Collections.Generic;
using System.Data;

namespace FunnyDB
{
    public sealed class SqlQueryResult : ISqlResultSet
    {
        private readonly IDataReader _reader;

        public SqlQueryResult(IDataReader reader)
        {
            _reader = reader;
        }

        public IEnumerable<T> Map<T>(Func<ISqlResultSet, T> f)
        {
            while (_reader.Read())
            {
                yield return f(this);
            }

            _reader.Close();
        }

        public IDataReader AsDataReader() => _reader;

        int ISqlResultSet.Int(string name)
        {
            return Get(name, _reader.GetInt32);
        }

        int ISqlResultSet.Int(string name, int defaultValue)
        {
            return Get(name, _reader.GetInt32, defaultValue);
        }

        int ISqlResultSet.Int(int ordinal)
        {
            return _reader.GetInt32(ordinal);
        }

        int ISqlResultSet.Int(int ordinal, int defaultValue)
        {
            return Get(ordinal, _reader.GetInt32, defaultValue);
        }

        long ISqlResultSet.Long(string name)
        {
            return Get(name, _reader.GetInt64);
        }

        long ISqlResultSet.Long(string name, long defaultValue)
        {
            return Get(name, _reader.GetInt64, defaultValue);
        }

        long ISqlResultSet.Long(int ordinal)
        {
            return _reader.GetInt64(ordinal);
        }

        long ISqlResultSet.Long(int ordinal, long defaultValue)
        {
            return Get(ordinal, _reader.GetInt64, defaultValue);
        }

        float ISqlResultSet.Float(string name)
        {
            return Get(name, _reader.GetFloat);
        }

        float ISqlResultSet.Float(string name, float defaultValue)
        {
            return Get(name, _reader.GetFloat, defaultValue);
        }

        float ISqlResultSet.Float(int ordinal)
        {
            return _reader.GetFloat(ordinal);
        }

        float ISqlResultSet.Float(int ordinal, float defaultValue)
        {
            return Get(ordinal, _reader.GetFloat, defaultValue);
        }

        double ISqlResultSet.Double(string name)
        {
            return Get(name, _reader.GetDouble);
        }

        double ISqlResultSet.Double(string name, double defaultValue)
        {
            return Get(name, _reader.GetDouble, defaultValue);
        }

        double ISqlResultSet.Double(int ordinal)
        {
            return _reader.GetDouble(ordinal);
        }

        double ISqlResultSet.Double(int ordinal, double defaultValue)
        {
            return Get(ordinal, _reader.GetDouble, defaultValue);
        }

        decimal ISqlResultSet.Decimal(string name)
        {
            return Get(name, _reader.GetDecimal);
        }

        decimal ISqlResultSet.Decimal(string name, decimal defaultValue)
        {
            return Get(name, _reader.GetDecimal, defaultValue);
        }

        decimal ISqlResultSet.Decimal(int ordinal)
        {
            return _reader.GetDecimal(ordinal);
        }

        decimal ISqlResultSet.Decimal(int ordinal, decimal defaultValue)
        {
            return Get(ordinal, _reader.GetDecimal, defaultValue);
        }

        string ISqlResultSet.String(string name)
        {
            return Get(name, _reader.GetString);
        }

        string ISqlResultSet.String(string name, string defaultValue)
        {
            return Get(name, _reader.GetString, defaultValue);
        }

        string ISqlResultSet.String(int ordinal)
        {
            return _reader.GetString(ordinal);
        }

        string ISqlResultSet.String(int ordinal, string defaultValue)
        {
            return Get(ordinal, _reader.GetString, defaultValue);
        }

        bool ISqlResultSet.Bool(string name)
        {
            return Get(name, _reader.GetBoolean);
        }

        bool ISqlResultSet.Bool(string name, bool defaultValue)
        {
            return Get(name, _reader.GetBoolean, defaultValue);
        }

        bool ISqlResultSet.Bool(int ordinal)
        {
            return _reader.GetBoolean(ordinal);
        }

        bool ISqlResultSet.Bool(int ordinal, bool defaultValue)
        {
            return Get(ordinal, _reader.GetBoolean, defaultValue);
        }

        DateTime ISqlResultSet.DateTime(string name)
        {
            return Get(name, _reader.GetDateTime);
        }

        DateTime ISqlResultSet.DateTime(string name, DateTime defaultValue)
        {
            return Get(name, _reader.GetDateTime, defaultValue);
        }

        DateTime ISqlResultSet.DateTime(int ordinal)
        {
            return _reader.GetDateTime(ordinal);
        }

        DateTime ISqlResultSet.DateTime(int ordinal, DateTime defaultValue)
        {
            return Get(ordinal, _reader.GetDateTime, defaultValue);
        }

        Guid ISqlResultSet.Guid(string name)
        {
            return Get(name, _reader.GetGuid);
        }

        Guid ISqlResultSet.Guid(string name, Guid defaultValue)
        {
            return Get(name, _reader.GetGuid, defaultValue);
        }

        Guid ISqlResultSet.Guid(int ordinal)
        {
            return _reader.GetGuid(ordinal);
        }

        Guid ISqlResultSet.Guid(int ordinal, Guid defaultValue)
        {
            return Get(ordinal, _reader.GetGuid, defaultValue);
        }

        private T Get<T>(string name, Func<int, T> f)
        {
            var ordinal = _reader.GetOrdinal(name);
            return f(ordinal);
        }

        private T Get<T>(string name, Func<int, T> f, T defaultValue)
        {
            var ordinal = _reader.GetOrdinal(name);
            return Get(ordinal, f, defaultValue);
        }

        private T Get<T>(int ordinal, Func<int, T> f, T defaultValue)
        {
            return _reader.IsDBNull(ordinal) ? defaultValue : f(ordinal);
        }
    }
}