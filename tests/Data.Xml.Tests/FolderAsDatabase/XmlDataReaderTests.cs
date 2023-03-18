﻿using System.Data.XmlClient;
using Xunit;

namespace Data.Xml.Tests.FolderAsDatabase;

/// <summary>
/// Tests that exercise the <see cref="XmlDataReader"/> class.
/// </summary>
public class XmlDataReaderTests
{
    [Fact]
    public void Reader_ShouldReadData()
    {
        // Arrange
        var connection = new XmlConnection(ConnectionStrings.FolderAsDBConnectionString);
        connection.Open();

        // Act - Query the locations table
        var command = new XmlCommand("SELECT * FROM locations", connection);
        var reader = command.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        var fieldCount = reader.FieldCount;
        Assert.Equal(4, fieldCount);

        // Act - Query the employees table
        command = new XmlCommand("SELECT * FROM employees", connection);
        reader = command.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        fieldCount = reader.FieldCount;
        Assert.Equal(4, fieldCount);

        // Close the connection
        connection.Close();
    }

    [Fact]
    public void Reader_ShouldReturnData()
    {
        // Arrange
        var connection = new XmlConnection(ConnectionStrings.FolderAsDBConnectionString);
        var command = new XmlCommand("SELECT * FROM [employees]", connection);

        // Act
        connection.Open();
        using (var reader = command.ExecuteReader())
        {
            // Assert
            Assert.NotNull(reader);
            Assert.Equal(4, reader.FieldCount);

            //first Row
            Assert.True(reader.Read());
            Assert.Equal("Joe", reader["name"]);
            Assert.IsType<string>(reader["name"]);
            Assert.Equal("Joe@gmail.com", reader["email"]);
            Assert.IsType<string>(reader["email"]);
            Assert.Equal(56000M, reader["salary"]);
            Assert.IsType<decimal>(reader["salary"]);
            Assert.Equal(true, reader["married"]);
            Assert.IsType<bool>(reader["married"]);

            //second row
            Assert.True(reader.Read());
            Assert.Equal("Bob", reader["name"]);
            Assert.IsType<string>(reader["name"]);
            Assert.Equal("bob32@gmail.com", reader["email"]);
            Assert.IsType<string>(reader["email"]);
            Assert.Equal((decimal)95000, reader["salary"]);
            Assert.IsType<decimal>(reader["salary"]);
            Assert.Equal(DBNull.Value, reader["married"]);
            //this will be dbnull not bool?
            Assert.IsType<DBNull>(reader["married"]);
        }

        connection.Close();
    }

    [Fact]
    public void Reader_ShouldReturnData_WithFilter()
    {
        // Arrange
        var connection = new XmlConnection(ConnectionStrings.FolderAsDBConnectionString);
        var command = new XmlCommand("SELECT * FROM [locations] WHERE zip = 78132", connection);

        // Act
        connection.Open();
        using (var reader = command.ExecuteReader())
        {
            // Assert
            Assert.NotNull(reader);
            Assert.Equal(4, reader.FieldCount);

            //first Row
            Assert.True(reader.Read());
            Assert.Equal("New Braunfels", reader["city"]);

            //No second row
            Assert.False(reader.Read());
        }

        connection.Close();
    }

    [Fact]
    public void Reader_ShouldReadDataWithInnerJoin()
    {
        // Arrange
        var connection = new XmlConnection(ConnectionStrings.FolderAsDBConnectionString);
        connection.Open();

        // Act
        var command = new XmlCommand("SELECT [l].[id], [l].[city], [l].[state], [l].[zip], [e].[name], [e].[email], [e].[salary], [e].[married] FROM [locations l] INNER JOIN [employees e] ON [l].[id] = [e].[salary];", connection);


        var reader = command.ExecuteReader();

        // Assert
        Assert.False(reader.Read());
        var fieldCount = reader.FieldCount;
        Assert.Equal(8, fieldCount);

        // Close the connection
        connection.Close();
    }

    [Fact]
    public void Reader_ShouldReadDataWithSelectedColumns()
    {
        // Arrange
        var connection = new XmlConnection(ConnectionStrings.FolderAsDBConnectionString);
        connection.Open();

        // Act - Query two columns from the locations table
        var command = new XmlCommand("SELECT city, state FROM locations", connection);
        var reader = command.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        var fieldCount = reader.FieldCount;
        Assert.Equal(2, fieldCount);

        // Act - Query two columns from the employees table
        command = new XmlCommand("SELECT name, salary FROM employees", connection);
        reader = command.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        fieldCount = reader.FieldCount;
        Assert.Equal(2, fieldCount);

        // Close the connection
        connection.Close();
    }

}
