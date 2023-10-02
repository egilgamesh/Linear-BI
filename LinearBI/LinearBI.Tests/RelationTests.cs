using Newtonsoft.Json;
using LinearBI.Metadata;

namespace LinearBI.Tests;

public class RelationTests
{
	[Test]
	public void GenerateSqlQueryOfRelationBWTwoTables_ShouldGenerateValidSqlQuery()
	{
		// Arrange
		const string DataModel = @"[
            {
                ""table_name"": ""employee"",
                ""columns"": [
                    {
                        ""name"": ""employee_id"",
                        ""type"": ""INT""
                    },
                    {
                        ""name"": ""first_name"",
                        ""type"": ""VARCHAR""
                    }
                ],
                ""relationships"": [
                    {
                        ""related_table"": ""department"",
                        ""type"": ""many-to-one"",
                        ""column"": ""department_id""
                    }
                ],
                ""aggregate_columns"": []
            },
            {
                ""table_name"": ""department"",
                ""columns"": [
                    {
                        ""name"": ""department_id"",
                        ""type"": ""INT""
                    },
                    {
                        ""name"": ""department_name"",
                        ""type"": ""VARCHAR""
                    }
                ],
                ""relationships"": [],
                ""aggregate_columns"": []
            }
        ]";
		var tables = JsonConvert.DeserializeObject<List<Table>>(DataModel);
		var selectedTables = new List<string> { "employee", "department" };
		var selectedColumnsByTable = new Dictionary<string, List<string>>
		{
			{ "employee", new List<string> { "employee_id", "first_name" } },
			{ "department", new List<string> { "department_name" } }
		};
		var sqlQuery =
			ReportGenerator.GenerateSqlQuery(tables!, selectedTables, selectedColumnsByTable);
		const string ExpectedSqlQuery =
			"SELECT employee.employee_id, employee.first_name, department.department_name FROM employee JOIN department ON employee.department_id = department.department_id";
		Assert.That(sqlQuery, Is.EqualTo(ExpectedSqlQuery), sqlQuery);
	}

	[Test]
	public void GenerateSqlQuery_ShouldGenerateValidSqlQuery_WithAggregateColumns()
	{
		// Arrange
		const string DataModel = @"[
            {
                ""table_name"": ""employee"",
                ""columns"": [
                    {
                        ""name"": ""employee_id"",
                        ""type"": ""INT""
                    },
                    {
                        ""name"": ""first_name"",
                        ""type"": ""VARCHAR""
                    },
                    {
                        ""name"": ""salary"",
                        ""type"": ""DECIMAL""
                    }
                ],
                ""relationships"": [],
                ""aggregate_columns"": [
                    {
                        ""name"": ""max_salary"",
                        ""expression"": ""MAX(salary)"",
                        ""type"": ""DECIMAL""
                    }
                ]
            }
        ]";
		var tables = JsonConvert.DeserializeObject<List<Table>>(DataModel);
		var selectedTables = new List<string> { "employee" };
		var selectedColumnsByTable = new Dictionary<string, List<string>>
		{
			{ "employee", new List<string> { "employee_id", "first_name" } }
		};

		// Act
		var sqlQuery =
			ReportGenerator.GenerateSqlQuery(tables, selectedTables, selectedColumnsByTable);

		// Assert
		var expectedSqlQuery =
			"SELECT employee.employee_id, employee.first_name, MAX(employee.salary) AS employee_max_salary FROM employee group by employee.employee_id, employee.first_name";
		Assert.That(sqlQuery, Is.EqualTo(expectedSqlQuery), sqlQuery);
	}

	[Test]
	public void GenerateSqlQuery_ShouldGenerateValidSqlQuery_MaxSalaryPerDepartment()
	{
		const string DataModel = @"[
            {
                ""table_name"": ""employee"",
                ""columns"": [
                    {
                        ""name"": ""employee_id"",
                        ""type"": ""INT""
                    },
                    {
                        ""name"": ""first_name"",
                        ""type"": ""VARCHAR""
                    },
                    {
                        ""name"": ""salary"",
                        ""type"": ""DECIMAL""
                    },
                    {
                        ""name"": ""department_id"",
                        ""type"": ""INT""
                    }
                ],
                ""relationships"": [
                    {
                        ""related_table"": ""department"",
                        ""type"": ""many-to-one"",
                        ""column"": ""department_id""
                    }
                ],
                ""aggregate_columns"": [
                    {
                        ""name"": ""max_salary"",
                        ""expression"": ""MAX(salary)"",
                        ""type"": ""DECIMAL""
                    }
                ]
            },
            {
                ""table_name"": ""department"",
                ""columns"": [
                    {
                        ""name"": ""department_id"",
                        ""type"": ""INT""
                    },
                    {
                        ""name"": ""department_name"",
                        ""type"": ""VARCHAR""
                    }
                ],
                ""relationships"": [],
                ""aggregate_columns"": []
            }
        ]";
		var tables = JsonConvert.DeserializeObject<List<Table>>(DataModel);
		var selectedTables = new List<string> { "employee", "department" };
		var selectedColumnsByTable = new Dictionary<string, List<string>>
		{
			{ "employee", new List<string> { "department_id" } },
			{ "department", new List<string> { "department_name" } }
		};
		var sqlQuery =
			ReportGenerator.GenerateSqlQuery(tables, selectedTables, selectedColumnsByTable);

		// Assert
		const string ExpectedSqlQuery = "SELECT employee.department_id, department.department_name, MAX(employee.salary) AS employee_max_salary FROM employee JOIN department ON employee.department_id = department.department_id GROUP BY department.department_id, department.department_name";
		Assert.That(sqlQuery, Is.EqualTo(ExpectedSqlQuery));
	}
}