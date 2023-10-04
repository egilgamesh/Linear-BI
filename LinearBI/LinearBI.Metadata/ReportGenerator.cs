using System.Text;
using Newtonsoft.Json;

namespace LinearBI.Metadata;

public class ReportGenerator
{
	// ReSharper disable once MethodTooLong
	// ReSharper disable once CyclomaticComplexity
	public static string GenerateSqlQuery(List<Table> tables, List<string> selectedTables,
		Dictionary<string, List<string>> selectedColumnsByTable)
	{
		var query = new StringBuilder();
		query.Append("SELECT ");
		foreach (var selectedTable in selectedTables)
		{
			var table = tables.Find(t => t.table_name == selectedTable);
			if (table == null)
				return "Table not found.";
			GenerateSelectColumns(selectedColumnsByTable, selectedTable, table, query);
		}
		//Remove the trailing comma and space
		if (query.Length > 7)
			query.Remove(query.Length - 2, 2);
		AddAggregateColumns(tables, selectedTables, query, selectedColumnsByTable);

		// Add "FROM" keyword after the list of selected columns
		query.Append(" FROM ");
		query.Append(selectedTables[0]);
		// Append the list of selected tables for the "FROM" clause
		//for (var i = 0; i < selectedTables.Count; i++)
		//{
		//	query.Append(selectedTables[i]);
		//	if (i < selectedTables.Count - 1)
		//		query.Append(", ");
		//}
		JoinTablesGeneration(tables, selectedTables, query);

		// Group by department for max salary per department
		//TODO: this should be only if there is aggregate columns
		//query.Append(" GROUP BY department.department_id, department.department_name");
		return query.ToString();
	}

	private static void GenerateSelectColumns(
		IReadOnlyDictionary<string, List<string>> selectedColumnsByTable, string selectedTable,
		Table table, StringBuilder query)
	{ // Add selected columns for the current table
		foreach (var column in selectedColumnsByTable[selectedTable].
			Where(column => table.columns.Any(c => c.name == column)))
			query.Append($"{selectedTable}.{column}, ");
	}

	private static void JoinTablesGeneration(List<Table> tables, List<string> selectedTables,
		StringBuilder query)
	{ // Add JOIN clauses for related tables
		foreach (var selectedTable in selectedTables)
		{
			var table = tables.Find(t => t.table_name == selectedTable);
			foreach (var relationship in table!.relationships.Where(relationship =>
					selectedTables.Contains(relationship.related_table)))
				// Add the JOIN clauses for related tables
				query.Append($" JOIN {
					relationship.related_table
				} ON {
					selectedTable
				}.{
					relationship.column
				} = {
					relationship.related_table
				}.{
					relationship.column
				}");
		}
	}

	// ReSharper disable once TooManyDeclarations
	private static void AddAggregateColumns(List<Table> tables, List<string> selectedTables,
		StringBuilder query, Dictionary<string, List<string>> selectedColumnsByTable)
	{
		if (selectedColumnsByTable.Count > 0)
			query?.Append(", ");
		if (tables.All(table => table.aggregate_columns.Count == 0))
			return;
		foreach (var selectedTable in selectedTables)
		{
			var table = tables.Find(t => t.table_name == selectedTable);
			foreach (var aggregateColumn in table!.aggregate_columns)
				query.Append($"{
					aggregateColumn.expression
				} AS {
					selectedTable
				}_{
					aggregateColumn.name
				}, ");
			query.Remove(query.Length - 2, 2);
		}
	}
}