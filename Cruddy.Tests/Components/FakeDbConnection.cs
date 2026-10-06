using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Cruddy.Tests.Components;

/// <summary>
/// A command executed against <see cref="FakeDbConnection"/>.
/// </summary>
public sealed record ExecutedCommand(string Kind, string Sql, Dictionary<string, object?> Parameters);

/// <summary>
/// Minimal in-memory DbConnection that records executed commands and returns configured results.
/// </summary>
public sealed class FakeDbConnection : DbConnection
{
    public List<ExecutedCommand> Commands { get; } = [];
    public List<Dictionary<string, object?>> Rows { get; } = [];
    public object? ScalarResult { get; set; } = 42;
    public bool ThrowOnScalar { get; set; }
    public int NonQueryResult { get; set; } = 1;

    [AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;
    public override string Database => "Fake";
    public override string DataSource => "Fake";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => ConnectionState.Open;

    public override void ChangeDatabase(string databaseName) { }
    public override void Close() { }
    public override void Open() { }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();
    protected override DbCommand CreateDbCommand() => new FakeDbCommand(this);
}

internal sealed class FakeDbCommand(FakeDbConnection owner) : DbCommand
{
    private readonly FakeParameterCollection _parameters = new();

    [AllowNull]
    public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; } = CommandType.Text;
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; } = owner;
    protected override DbParameterCollection DbParameterCollection => _parameters;
    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel() { }
    public override void Prepare() { }

    private void Record(string kind)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (DbParameter p in _parameters)
        {
            parameters[p.ParameterName] = p.Value is DBNull ? null : p.Value;
        }
        owner.Commands.Add(new ExecutedCommand(kind, CommandText, parameters));
    }

    public override int ExecuteNonQuery()
    {
        Record("NonQuery");
        return owner.NonQueryResult;
    }

    public override object? ExecuteScalar()
    {
        Record("Scalar");
        if (owner.ThrowOnScalar)
        {
            throw new InvalidOperationException("Scalar failed");
        }
        return owner.ScalarResult;
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        Record("Reader");
        return new FakeDataReader(owner.Rows);
    }

    protected override DbParameter CreateDbParameter() => new FakeDbParameter();
}

internal sealed class FakeDbParameter : DbParameter
{
    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
    public override bool IsNullable { get; set; }
    [AllowNull]
    public override string ParameterName { get; set; } = string.Empty;
    public override int Size { get; set; }
    [AllowNull]
    public override string SourceColumn { get; set; } = string.Empty;
    public override bool SourceColumnNullMapping { get; set; }
    public override object? Value { get; set; }
    public override void ResetDbType() { }
}

internal sealed class FakeParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _items = [];

    public override int Count => _items.Count;
    public override object SyncRoot => ((ICollection)_items).SyncRoot;

    public override int Add(object value) { _items.Add((DbParameter)value); return _items.Count - 1; }
    public override void AddRange(Array values) { foreach (var v in values) Add(v!); }
    public override void Clear() => _items.Clear();
    public override bool Contains(object value) => _items.Contains((DbParameter)value);
    public override bool Contains(string value) => IndexOf(value) >= 0;
    public override void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
    public override IEnumerator GetEnumerator() => _items.GetEnumerator();
    public override int IndexOf(object value) => _items.IndexOf((DbParameter)value);
    public override int IndexOf(string parameterName) =>
        _items.FindIndex(p => string.Equals(p.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase));
    public override void Insert(int index, object value) => _items.Insert(index, (DbParameter)value);
    public override void Remove(object value) => _items.Remove((DbParameter)value);
    public override void RemoveAt(int index) => _items.RemoveAt(index);
    public override void RemoveAt(string parameterName) => _items.RemoveAt(IndexOf(parameterName));
    protected override DbParameter GetParameter(int index) => _items[index];
    protected override DbParameter GetParameter(string parameterName) => _items[IndexOf(parameterName)];
    protected override void SetParameter(int index, DbParameter value) => _items[index] = value;
    protected override void SetParameter(string parameterName, DbParameter value) => _items[IndexOf(parameterName)] = value;
}

internal sealed class FakeDataReader : DbDataReader
{
    private readonly List<Dictionary<string, object?>> _rows;
    private readonly string[] _columns;
    private int _index = -1;
    private bool _closed;

    public FakeDataReader(List<Dictionary<string, object?>> rows)
    {
        _rows = rows;
        _columns = rows.Count > 0 ? [.. rows[0].Keys] : [];
    }

    private object? Current(int ordinal) => _rows[_index][_columns[ordinal]];

    public override int Depth => 0;
    public override int FieldCount => _columns.Length;
    public override bool HasRows => _rows.Count > 0;
    public override bool IsClosed => _closed;
    public override int RecordsAffected => -1;
    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    public override bool Read() => ++_index < _rows.Count;
    public override bool NextResult() => false;
    public override void Close() => _closed = true;

    public override string GetName(int ordinal) => _columns[ordinal];
    public override int GetOrdinal(string name) => Array.FindIndex(_columns, c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;
    public override Type GetFieldType(int ordinal) => _rows.Select(r => r[_columns[ordinal]]).FirstOrDefault(v => v != null)?.GetType() ?? typeof(object);
    public override object GetValue(int ordinal) => Current(ordinal) ?? DBNull.Value;
    public override int GetValues(object[] values)
    {
        var n = Math.Min(values.Length, FieldCount);
        for (var i = 0; i < n; i++) values[i] = GetValue(i);
        return n;
    }
    public override bool IsDBNull(int ordinal) => Current(ordinal) == null;

    public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);
    public override byte GetByte(int ordinal) => (byte)GetValue(ordinal);
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
    public override char GetChar(int ordinal) => (char)GetValue(ordinal);
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
    public override DateTime GetDateTime(int ordinal) => (DateTime)GetValue(ordinal);
    public override decimal GetDecimal(int ordinal) => (decimal)GetValue(ordinal);
    public override double GetDouble(int ordinal) => (double)GetValue(ordinal);
    public override float GetFloat(int ordinal) => (float)GetValue(ordinal);
    public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
    public override short GetInt16(int ordinal) => (short)GetValue(ordinal);
    public override int GetInt32(int ordinal) => (int)GetValue(ordinal);
    public override long GetInt64(int ordinal) => (long)GetValue(ordinal);
    public override string GetString(int ordinal) => (string)GetValue(ordinal);
    public override IEnumerator GetEnumerator() => _rows.GetEnumerator();
}
