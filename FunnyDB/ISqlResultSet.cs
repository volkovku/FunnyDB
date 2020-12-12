using System;
using System.Data;

namespace FunnyDB
{
    public interface ISqlResultSet
    {
        IDataReader AsDataReader();
        int Int(string name);
        int Int(string name, int defaultValue);
        int Int(int ordinal);
        int Int(int ordinal, int defaultValue);
        long Long(string name);
        long Long(string name, long defaultValue);
        long Long(int ordinal);
        long Long(int ordinal, long defaultValue);
        float Float(string name);
        float Float(string name, float defaultValue);
        float Float(int ordinal);
        float Float(int ordinal, float defaultValue);
        double Double(string name);
        double Double(string name, double defaultValue);
        double Double(int ordinal);
        double Double(int ordinal, double defaultValue);
        decimal Decimal(string name);
        decimal Decimal(string name, decimal defaultValue);
        decimal Decimal(int ordinal);
        decimal Decimal(int ordinal, decimal defaultValue);
        string String(string name);
        string String(string name, string defaultValue);
        string String(int ordinal);
        string String(int ordinal, string defaultValue);
        bool Bool(string name);
        bool Bool(string name, bool defaultValue);
        bool Bool(int ordinal);
        bool Bool(int ordinal, bool defaultValue);
        DateTime DateTime(string name);
        DateTime DateTime(string name, DateTime defaultValue);
        DateTime DateTime(int ordinal);
        DateTime DateTime(int ordinal, DateTime defaultValue);
        Guid Guid(string name);
        Guid Guid(string name, Guid defaultValue);
        Guid Guid(int ordinal);
        Guid Guid(int ordinal, Guid defaultValue);
    }
}