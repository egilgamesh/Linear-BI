namespace LinearBI.Metadata;

public class Column
{
	public string name { get; set; }
	public string type { get; set; }
}

public class Relationship
{
	public string related_table { get; set; }
	public string type { get; set; }
	public string column { get; set; }
}

public class AggregateColumn
{
	public string name { get; set; }
	public string expression { get; set; }
	public object type { get; set; } // Use object type for "type" property
}

// ReSharper disable once HollowTypeName
public class Table
{
	public string table_name { get; set; }
	public List<Column> columns { get; set; }
	public List<Relationship> relationships { get; set; } // Include relationships
	public List<AggregateColumn> aggregate_columns { get; set; } // Include aggregate columns
}