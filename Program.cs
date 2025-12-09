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
  Console.WriteLine("3) Edit category");
  Console.WriteLine("4) Display Category and related products");
  Console.WriteLine("5) Display all Categories and their related products");
  Console.WriteLine("6) Create product");
  Console.WriteLine("7) Edit product");
  Console.WriteLine("8) Display products");
  Console.WriteLine("9) Display specific product");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  if (choice == "1")
  {
    logger.Info("Displaying all categories");
    // display categories
    var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

    var config = configuration.Build();

    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryName);

    Console.WriteLine($"{query.Count()} records returned");
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName} - {item.Description}");
    }
    logger.Info("{Count} categories displayed", query.Count());
  }
  else if (choice == "2")
  {
    logger.Info("Adding new category");
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
        // validation error
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
    logger.Info("Editing category");
    var db = new DataContext();
    var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();

    Console.WriteLine("Select a Category to Edit:");
    for (int i = 0; i < categories.Count; i++)
    {
      Console.WriteLine($"{i + 1}.) {categories[i].CategoryName}");
    }

    int selection = int.Parse(Console.ReadLine()!) - 1;
    Category categoryToEdit = categories[selection];
    logger.Info("Selected category {CategoryName} for editing", categoryToEdit.CategoryName);

    Console.WriteLine($"\nEditing: {categoryToEdit.CategoryName}");
    Console.WriteLine("Press Enter to keep current value\n");

    Console.WriteLine($"Category Name [{categoryToEdit.CategoryName}]: ");
    string input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
      if (db.Categories.Any(c => c.CategoryName == input && c.CategoryId != categoryToEdit.CategoryId))
      {
        Console.WriteLine("Error: Category name already exists");
        logger.Error("Category name {Name} already exists", input);
      }
      else
      {
        categoryToEdit.CategoryName = input;
      }
    }

    Console.WriteLine($"Description [{categoryToEdit.Description}]: ");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) categoryToEdit.Description = input;

    db.SaveChanges();
    logger.Info("Category updated: {CategoryName}", categoryToEdit.CategoryName);
    Console.WriteLine("Category updated successfully!");
  }
  else if (choice == "4")
  {
    logger.Info("Displaying category with products");
    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryId);

    Console.WriteLine("Select the category whose products you want to display:");
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
    }
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"CategoryId {id} selected");
    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
    Console.WriteLine($"{category.CategoryName} - {category.Description}");
    var activeProducts = category.Products.Where(p => !p.Discontinued).ToList();
    foreach (Product p in activeProducts)
    {
      Console.WriteLine($"\t{p.ProductName}");
    }
    logger.Info("Displayed {Count} active products for category {CategoryName}", activeProducts.Count, category.CategoryName);
  }
  else if (choice == "5")
  {
    logger.Info("Displaying all categories with their active products");
    var db = new DataContext();
    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
    int totalProducts = 0;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName}");
      foreach (Product p in item.Products.Where(p => !p.Discontinued))
      {
        Console.WriteLine($"\t{p.ProductName}");
        totalProducts++;
      }
    }
    logger.Info("Displayed {CategoryCount} categories with {ProductCount} active products", query.Count(), totalProducts);
  }
  else if (choice == "6")
  {
    logger.Info("Creating new product");
    var db = new DataContext();

    Product product = new();
    product.ProductName = GetValidInput<string>("Product Name");

    // Display available suppliers
    Console.WriteLine("\nAvailable Suppliers:");
    foreach (var s in db.Suppliers.OrderBy(s => s.SupplierId))
    {
      Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
    }
    product.SupplierId = GetValidInput<int>("SupplierId");

    // Display available categories
    Console.WriteLine("\nAvailable Categories:");
    foreach (var c in db.Categories.OrderBy(c => c.CategoryId))
    {
      Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
    }
    product.CategoryId = GetValidInput<int>("CategoryId");

    product.QuantityPerUnit = GetValidInput<string>("QuantityPerUnit");
    product.UnitPrice = GetValidInput<decimal>("UnitPrice");
    product.UnitsInStock = GetValidInput<short>("UnitsInStock");
    product.UnitsOnOrder = GetValidInput<short>("UnitsOnOrder");
    product.ReorderLevel = GetValidInput<short>("ReorderLevel");
    product.Discontinued = GetValidInput<bool>("Discontinued (true/false)");

    db.Products.Add(product);
    db.SaveChanges();
    logger.Info("Product added: {ProductName}", product.ProductName);
  }
  else if (choice == "7")
  {
    logger.Info("Editing product");
    var db = new DataContext();
    var products = db.Products.ToList();

    Console.WriteLine("Please Select a Product:");
    for (int i = 0; i < products.Count; i++)
    {
      Console.WriteLine($"{i + 1}.) {products[i].ProductName}");
    }

    int selection = int.Parse(Console.ReadLine()!) - 1;
    Product productToEdit = products[selection];
    logger.Info("Selected product {ProductName} for editing", productToEdit.ProductName);

    Console.WriteLine($"\nEditing: {productToEdit.ProductName}");
    Console.WriteLine("Press Enter to keep current value\n");

    Console.WriteLine($"Product Name [{productToEdit.ProductName}]: ");
    string input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) productToEdit.ProductName = input;

    Console.WriteLine($"UnitPrice [{productToEdit.UnitPrice}]: ");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input)) productToEdit.UnitPrice = decimal.Parse(input);

    db.SaveChanges();
    logger.Info("Product updated: {ProductName}", productToEdit.ProductName);
  }
  else if (choice == "8")
  {
    logger.Info("Displaying products");
    var db = new DataContext();

    Console.WriteLine("Select product display option:");
    Console.WriteLine("1) All products");
    Console.WriteLine("2) Active products only");
    Console.WriteLine("3) Discontinued products only");
    string productChoice = Console.ReadLine();
    logger.Info("Product display option {Choice} selected", productChoice);

    IQueryable<Product> query = db.Products;

    if (productChoice == "2")
    {
      query = query.Where(p => !p.Discontinued);
      Console.WriteLine("\n--- Active Products ---");
    }
    else if (productChoice == "3")
    {
      query = query.Where(p => p.Discontinued);
      Console.WriteLine("\n--- Discontinued Products ---");
    }
    else
    {
      Console.WriteLine("\n--- All Products ---");
    }

    query = query.OrderBy(p => p.ProductName);

    Console.WriteLine($"{query.Count()} records returned\n");

    foreach (var product in query)
    {
      if (product.Discontinued)
      {
        Console.WriteLine($"{product.ProductName} (DISCONTINUED)");
      }
      else
      {
        Console.WriteLine($"{product.ProductName}");
      }
    }
    logger.Info("Displayed {Count} products", query.Count());
  }
  else if (choice == "9")
  {
    logger.Info("Displaying specific product details");
    var db = new DataContext();
    var products = db.Products.Include("Category").Include("Supplier").OrderBy(p => p.ProductName).ToList();

    Console.WriteLine("Select a Product to Display:");
    for (int i = 0; i < products.Count; i++)
    {
      Console.WriteLine($"{i + 1}.) {products[i].ProductName}");
    }

    int selection = int.Parse(Console.ReadLine()!) - 1;
    Product product = products[selection];
    logger.Info("Displaying details for product: {ProductName}", product.ProductName);

    Console.WriteLine("\n--- Product Details ---");
    Console.WriteLine($"Product ID: {product.ProductId}");
    Console.WriteLine($"Product Name: {product.ProductName}");
    Console.WriteLine($"Supplier: {product.Supplier?.CompanyName ?? "N/A"}");
    Console.WriteLine($"Category: {product.Category?.CategoryName ?? "N/A"}");
    Console.WriteLine($"Quantity Per Unit: {product.QuantityPerUnit ?? "N/A"}");
    Console.WriteLine($"Unit Price: {product.UnitPrice:C}");
    Console.WriteLine($"Units In Stock: {product.UnitsInStock}");
    Console.WriteLine($"Units On Order: {product.UnitsOnOrder}");
    Console.WriteLine($"Reorder Level: {product.ReorderLevel}");
    Console.WriteLine($"Discontinued: {(product.Discontinued ? "Yes" : "No")}");
  }
  else if (String.IsNullOrEmpty(choice))
  {
    logger.Info("User chose to exit");
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");

static T GetValidInput<T>(string prompt, string currentValue = null)
{
  while (true)
  {
    if (currentValue != null)
      Console.WriteLine($"{prompt} [{currentValue}]: ");
    else
      Console.WriteLine($"{prompt}: ");

    string input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) && currentValue != null)
      return default(T);

    try
    {
      return (T)Convert.ChangeType(input, typeof(T));
    }
    catch
    {
      Console.WriteLine("Wrong entry type. Please try again.");
    }
  }
}
