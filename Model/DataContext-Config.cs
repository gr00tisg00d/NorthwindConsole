using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace NorthwindConsole.Model;

public partial class DataContext : DbContext
{
  public DataContext() { }
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

    var config = configuration.Build();
    optionsBuilder.UseSqlServer(@config["Northwind:ConnectionString"]);
  }
}

/*
This class inherits from DbContext which makes it the main class for interacting with the database using Entity Framework Core.

OnConfiguring Method:
- Reads configuration settings from appsettings.json
- Retrieves the connection string from the Northwind:ConnectionString key.
Configures Entity Gramework to use SQL Server with that connection string.


This allows the application to connect to the Northwind database using settings from your config file.


*/