using NLog;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
  Console.WriteLine("1) Display categories");
  Console.WriteLine("2) Add category");
  Console.WriteLine("3) Display Category and related products");
  Console.WriteLine("4) Display all Categories and their related products");
  Console.WriteLine("5) Create product");
  Console.WriteLine("6) Edit product");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  if (choice == "1")
  {
    // display categories
    var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

    var config = configuration.Build();

    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryName);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Magenta;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName} - {item.Description}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "2")
  {
    // Add category
    Category category = new();
    Console.WriteLine("Enter Category Name:");
    category.CategoryName = Console.ReadLine()!;
    Console.WriteLine("Enter the Category Description:");
    category.Description = Console.ReadLine();
    ValidationContext context = new ValidationContext(category, null, null); // Creates a context for validating the category object.
    List<ValidationResult> results = new List<ValidationResult>(); // results is a list that will hold any validation errors.

    var isValid = Validator.TryValidateObject(category, context, results, true); // Checks the category object against its validation attributes. (like [Required])
    if (isValid)
    {
      var db = new DataContext();
      if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
      {
        // generate validation error
        isValid = false;
        results.Add(new ValidationResult("Name exists", ["CategoryName"]));
      }
      else
      {
        logger.Info("Validation passed");
        db.Categories.Add(category);
        db.SaveChanges();
        logger.Info("Category added: {CategoryName}", category.CategoryName);

      }
    }
    if (!isValid)
    {
      foreach (var result in results)
      {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
      }
    }
  }
  else if (choice == "3")
  {
    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryId);

    Console.WriteLine("Select the category whose products you want to display:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"CategoryId {id} selected");
    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
    Console.WriteLine($"{category.CategoryName} - {category.Description}");
    foreach (Product p in category.Products)
    {
      Console.WriteLine($"\t{p.ProductName}");
    }
  }
  else if (choice == "4")
  {
    var db = new DataContext();
    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName}");
      foreach (Product p in item.Products)
      {
        Console.WriteLine($"\t{p.ProductName}");
      }
    }
  }
  else if (choice == "5")
  {
    Product product = new();
    Console.WriteLine("Product Name: ");
    product.ProductName = Console.ReadLine();

    Console.WriteLine("Please edit the product information in the 'Product Editor' menu.");

    var db = new DataContext();

    db.Products.Add(product);
    db.SaveChanges();

    Console.WriteLine("SupplierId: ");
    product.SupplierId = int.Parse(Console.ReadLine());
    Console.WriteLine("CategoryId: ");
    product.CategoryId = int.Parse(Console.ReadLine());
    Console.WriteLine("QuantityPerUnity: ");
    product.QuantityPerUnit = Console.ReadLine();
    Console.WriteLine("UnitPrice: ");
    product.UnitPrice = decimal.Parse(Console.ReadLine());
    Console.WriteLine("UnitsInStock: ");
    product.UnitsInStock = short.Parse(Console.ReadLine());
    Console.WriteLine("UnitsOnOrder: ");
    product.UnitsOnOrder = short.Parse(Console.ReadLine());
    Console.WriteLine("ReorderLevel: ");
    product.ReorderLevel = short.Parse(Console.ReadLine());
    Console.WriteLine("Discontinued: ");
    product.Discontinued = bool.Parse(Console.ReadLine());
  }
  else if (choice == "6")
  {
    var db = new DataContext();
    Console.WriteLine("Please Select a Product:");
    int i = 1;
    foreach (Product product in db.Products)
    {
      Console.WriteLine($"{i}.) {product.ProductName}");
      i++;
    }
  }
  else if (String.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");
