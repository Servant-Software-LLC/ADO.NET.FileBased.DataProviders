﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.JsonClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Data.Json.Tests.FileAsDatabase
{
    public partial class JsonDataAdapterTests
    {
        [Fact]
        public void DataAdapter_ShouldFillTheDataSet()
        {
            // Arrange
            var connection = new JsonConnection(ConnectionStrings.FileAsDBConnectionString);
            var adapter = new JsonDataAdapter("SELECT * FROM locations", connection);

            // Act
            var dataset = new DataSet();
            adapter.Fill(dataset);

            // Assert
            Assert.True(dataset.Tables[0].Rows.Count > 0);
            Assert.Equal(4, dataset.Tables[0].Columns.Count);

            // Fill the employees table
            adapter.SelectCommand!.CommandText = "SELECT * FROM employees";
            adapter.Fill(dataset);

            // Assert
            Assert.True(dataset.Tables[0].Rows.Count > 0);
            Assert.Equal(4, dataset.Tables[0].Columns.Count);

            // Close the connection
            connection.Close();
        }
        [Fact]
        public void Adapter_ShouldReturnData()
        {
            // Arrange
            var connection = new JsonConnection(ConnectionStrings.FileAsDBConnectionString);
            var adapter = new JsonDataAdapter("SELECT * FROM employees", connection);

            // Act
            var dataset = new DataSet();
            adapter.Fill(dataset);

            // Assert
            Assert.NotNull(dataset);
            Assert.Equal(4, dataset.Tables[0].Columns.Count);

            //first Row
            Assert.Equal("Joe", dataset.Tables[0].Rows[0]["name"]);
            Assert.IsType<string>(dataset.Tables![0].Rows[0]["name"]);
            Assert.Equal("Joe@gmail.com", dataset.Tables![0].Rows[0]["email"]);
            Assert.IsType<string>(dataset.Tables[0].Rows[0]["email"]);
            Assert.Equal(56000M, dataset.Tables[0].Rows[0]["salary"]);
            Assert.IsType<decimal>(dataset.Tables[0].Rows[0]["salary"]);
            Assert.Equal(true, dataset.Tables[0].Rows[0]["married"]);
            Assert.IsType<bool>(dataset.Tables[0].Rows[0]["married"]);

            //second row
            Assert.Equal("Bob", dataset.Tables[0].Rows[1]["name"]);
            Assert.IsType<string>(dataset.Tables[0].Rows[1]["name"]);
            Assert.Equal("bob32@gmail.com", dataset.Tables[0].Rows[1]["email"]);
            Assert.IsType<string>(dataset.Tables[0].Rows[1]["email"]);
            Assert.Equal((decimal)95000, dataset.Tables[0].Rows[1]["salary"]);
            Assert.IsType<decimal>(dataset.Tables[0].Rows[1]["salary"]);
            Assert.Equal(DBNull.Value, dataset.Tables[0].Rows[1]["married"]);
            Assert.IsType<DBNull>(dataset.Tables[0].Rows[1]["married"]);

            connection.Close();
        }

        [Fact]
        public void DataAdapter_ShouldFillTheDataSet_WithFilter()
        {
            // Arrange
            var connection = new JsonConnection(ConnectionStrings.FileAsDBConnectionString);
            var selectCommand = new JsonCommand("SELECT * FROM [locations] WHERE zip = 78132", connection);
            var dataAdapter = new JsonDataAdapter(selectCommand);
            var dataSet = new DataSet();

            // Act
            connection.Open();
            dataAdapter.Fill(dataSet);

            // Assert
            Assert.NotEmpty(dataSet.Tables);
            var locationsTable = dataSet.Tables[0];
            Assert.Equal(4, locationsTable.Columns.Count);
            Assert.Equal(1, locationsTable.Rows.Count);

            var row = locationsTable.Rows[0];
            Assert.Equal("New Braunfels", row["city"]);
            Assert.Equal(78132M, row["zip"]);

            // Close the connection
            connection.Close();
        }

        [Fact]
        public void Adapter_ShouldFillDatasetWithInnerJoin()
        {
            // Arrange
            var connection = new JsonConnection(ConnectionStrings.eComDBConnectionString);
            var command = new JsonCommand("SELECT [c].[CustomerName], [o].[OrderDate], [oi].[Quantity], [p].[Name] FROM [Customers c] INNER JOIN [Orders o] ON [c].[ID] = [o].[CustomerID] INNER JOIN [OrderItems oi] ON [o].[ID] = [oi].[OrderID] INNER JOIN [Products p] ON [p].[ID] = [oi].[ProductID]", connection);
            var adapter = new JsonDataAdapter(command);
            var dataSet = new DataSet();

            // Act
            adapter.Fill(dataSet);

            // Assert
            var table = dataSet.Tables[0];
            Assert.True(table.Rows.Count > 0, "No records were returned in the INNER JOINs");

            foreach (DataRow row in table.Rows)
            {
                Assert.NotNull(row[0]);
                Assert.NotNull(row[1]);
                Assert.NotNull(row[2]);
                Assert.NotNull(row[3]);
            }
        }

        [Fact]
        public void Adapter_ShouldReadDataWithSelectedColumns()
        {
            // Arrange
            var connection = new JsonConnection(ConnectionStrings.FileAsDBConnectionString);
            var dataSet = new DataSet();

            // Act - Query two columns from the locations table
            var command = new JsonCommand("SELECT city, state FROM locations", connection);
            var adapter = new JsonDataAdapter(command);
            adapter.Fill(dataSet);

            // Assert
            var dataTable = dataSet.Tables[0];
            Assert.NotNull(dataTable);
            Assert.True(dataTable.Rows.Count > 0);
            Assert.Equal(2, dataTable.Columns.Count);

            // Act - Query two columns from the employees table
            command = new JsonCommand("SELECT name, salary FROM employees", connection);
            adapter.SelectCommand = command;
            adapter.Fill(dataSet);

            // Assert
            dataTable = dataSet.Tables[0];
            Assert.NotNull(dataTable);
            Assert.True(dataTable.Rows.Count > 0);
            Assert.Equal(2, dataTable.Columns.Count);

            // Close the connection
            connection.Close();
        }


    }
}
