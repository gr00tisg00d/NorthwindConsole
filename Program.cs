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
  Console.WriteLine("10) Delete product");
  Console.WriteLine("11) Delete category");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  if (choice == "1")
  {
    try
    {
      logger.Info("Displaying all categories");
      var db = new DataContext();
      var query = db.Categories.OrderBy(p => p.CategoryName);

      Console.WriteLine($"{query.Count()} records returned");
      foreach (var item in query)
      {
        Console.WriteLine($"{item.CategoryName} - {item.Description}");
      }
      logger.Info("{Count} categories displayed", query.Count());
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to display categories.");
      logger.Error(ex, "Error displaying categories");
    }
  }
  else if (choice == "2")
  {
    try
    {
      logger.Info("Adding new category");
      Category category = new();
      Console.WriteLine("Enter Category Name:");
      category.CategoryName = Console.ReadLine()!;
      Console.WriteLine("Enter the Category Description:");
      category.Description = Console.ReadLine();
      ValidationContext context = new ValidationContext(category, null, null);
      List<ValidationResult> results = new List<ValidationResult>();

      var isValid = Validator.TryValidateObject(category, context, results, true);
      if (isValid)
      {
        var db = new DataContext();
        if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
        {
          isValid = false;
          results.Add(new ValidationResult("Name exists", ["CategoryName"]));
        }
        else
        {
          logger.Info("Validation passed");
          db.Categories.Add(category);
          db.SaveChanges();
          logger.Info("Category added: {CategoryName}", category.CategoryName);
          Console.WriteLine("Category added successfully!");
        }
      }
      if (!isValid)
      {
        foreach (var result in results)
        {
          Console.WriteLine($"Error: {result.ErrorMessage}");
          logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to add category.");
      logger.Error(ex, "Error adding category");
    }
  }
  else if (choice == "3")
  {
    try
    {
      logger.Info("Editing category");
      var db = new DataContext();
      var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();

      if (!categories.Any())
      {
        Console.WriteLine("No categories available to edit.");
        logger.Info("No categories available to edit");
      }
      else
      {
        Console.WriteLine("Select a Category to Edit:");
        for (int i = 0; i < categories.Count; i++)
        {
          Console.WriteLine($"{i + 1}.) {categories[i].CategoryName}");
        }

        if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= categories.Count)
        {
          Category categoryToEdit = categories[selection - 1];
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

          ValidationContext context = new ValidationContext(categoryToEdit, null, null);
          List<ValidationResult> results = new List<ValidationResult>();
          if (Validator.TryValidateObject(categoryToEdit, context, results, true))
          {
            db.SaveChanges();
            logger.Info("Category updated: {CategoryName}", categoryToEdit.CategoryName);
            Console.WriteLine("Category updated successfully!");
          }
          else
          {
            foreach (var result in results)
            {
              Console.WriteLine($"Error: {result.ErrorMessage}");
              logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }
          }
        }
        else
        {
          Console.WriteLine("Error: Invalid selection.");
          logger.Error("Invalid category selection: {Selection}", selection);
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to edit category.");
      logger.Error(ex, "Error editing category");
    }
  }
  else if (choice == "4")
  {
    try
    {
      logger.Info("Displaying category with products");
      var db = new DataContext();
      var query = db.Categories.OrderBy(p => p.CategoryId).ToList();

      if (!query.Any())
      {
        Console.WriteLine("No categories available.");
        logger.Info("No categories available");
      }
      else
      {
        Console.WriteLine("Select the category whose products you want to display:");
        foreach (var item in query)
        {
          Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
        }

        if (int.TryParse(Console.ReadLine(), out int id))
        {
          Console.Clear();
          logger.Info($"CategoryId {id} selected");
          Category? category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);

          if (category != null)
          {
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            var activeProducts = category.Products.Where(p => !p.Discontinued).ToList();
            foreach (Product p in activeProducts)
            {
              Console.WriteLine($"\t{p.ProductName}");
            }
            logger.Info("Displayed {Count} active products for category {CategoryName}", activeProducts.Count, category.CategoryName);
          }
          else
          {
            Console.WriteLine("Error: Category not found.");
            logger.Error("Category with ID {Id} not found", id);
          }
        }
        else
        {
          Console.WriteLine("Error: Invalid category ID.");
          logger.Error("Invalid category ID input");
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to display category and products.");
      logger.Error(ex, "Error displaying category with products");
    }
  }
  else if (choice == "5")
  {
    try
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
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to display categories and products.");
      logger.Error(ex, "Error displaying all categories with products");
    }
  }
  else if (choice == "6")
  {
    try
    {
      logger.Info("Creating new product");
      var db = new DataContext();

      Product product = new();
      product.ProductName = GetValidInput<string>("Product Name");

      // Display available suppliers
      Console.WriteLine("\nAvailable Suppliers:");
      var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
      foreach (var s in suppliers)
      {
        Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
      }
      int supplierId = GetValidInput<int>("SupplierId");
      if (suppliers.Any(s => s.SupplierId == supplierId))
      {
        product.SupplierId = supplierId;
      }
      else
      {
        Console.WriteLine("Warning: Invalid supplier ID. Setting to null.");
        logger.Warn("Invalid supplier ID: {SupplierId}", supplierId);
        product.SupplierId = null;
      }

      // Display available categories
      Console.WriteLine("\nAvailable Categories:");
      var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
      foreach (var c in categories)
      {
        Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
      }
      int categoryId = GetValidInput<int>("CategoryId");
      if (categories.Any(c => c.CategoryId == categoryId))
      {
        product.CategoryId = categoryId;
      }
      else
      {
        Console.WriteLine("Warning: Invalid category ID. Setting to null.");
        logger.Warn("Invalid category ID: {CategoryId}", categoryId);
        product.CategoryId = null;
      }

      product.QuantityPerUnit = GetValidInput<string>("QuantityPerUnit");
      product.UnitPrice = GetValidInput<decimal>("UnitPrice");
      product.UnitsInStock = GetValidInput<short>("UnitsInStock");
      product.UnitsOnOrder = GetValidInput<short>("UnitsOnOrder");
      product.ReorderLevel = GetValidInput<short>("ReorderLevel");
      product.Discontinued = GetValidInput<bool>("Discontinued (true/false)");

      ValidationContext context = new ValidationContext(product, null, null);
      List<ValidationResult> results = new List<ValidationResult>();
      if (Validator.TryValidateObject(product, context, results, true))
      {
        db.Products.Add(product);
        db.SaveChanges();
        logger.Info("Product added: {ProductName}", product.ProductName);
        Console.WriteLine("Product added successfully!");
      }
      else
      {
        foreach (var result in results)
        {
          Console.WriteLine($"Error: {result.ErrorMessage}");
          logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to create product.");
      logger.Error(ex, "Error creating product");
    }
  }
  else if (choice == "7")
  {
    try
    {
      logger.Info("Editing product");
      var db = new DataContext();
      var products = db.Products.ToList();

      if (!products.Any())
      {
        Console.WriteLine("No products available to edit.");
        logger.Info("No products available to edit");
      }
      else
      {
        Console.WriteLine("Please Select a Product:");
        for (int i = 0; i < products.Count; i++)
        {
          Console.WriteLine($"{i + 1}.) {products[i].ProductName}");
        }

        if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= products.Count)
        {
          Product productToEdit = products[selection - 1];
          logger.Info("Selected product {ProductName} for editing", productToEdit.ProductName);

          Console.WriteLine($"\nEditing: {productToEdit.ProductName}");
          Console.WriteLine("Press Enter to keep current value\n");

          Console.WriteLine($"Product Name [{productToEdit.ProductName}]: ");
          string input = Console.ReadLine();
          if (!string.IsNullOrWhiteSpace(input)) productToEdit.ProductName = input;

          Console.WriteLine($"UnitPrice [{productToEdit.UnitPrice}]: ");
          input = Console.ReadLine();
          if (!string.IsNullOrWhiteSpace(input))
          {
            if (decimal.TryParse(input, out decimal price))
            {
              productToEdit.UnitPrice = price;
            }
            else
            {
              Console.WriteLine("Error: Invalid price format.");
              logger.Error("Invalid price format: {Input}", input);
            }
          }

          ValidationContext context = new ValidationContext(productToEdit, null, null);
          List<ValidationResult> results = new List<ValidationResult>();
          if (Validator.TryValidateObject(productToEdit, context, results, true))
          {
            db.SaveChanges();
            logger.Info("Product updated: {ProductName}", productToEdit.ProductName);
            Console.WriteLine("Product updated successfully!");
          }
          else
          {
            foreach (var result in results)
            {
              Console.WriteLine($"Error: {result.ErrorMessage}");
              logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }
          }
        }
        else
        {
          Console.WriteLine("Error: Invalid selection.");
          logger.Error("Invalid product selection: {Selection}", selection);
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to edit product.");
      logger.Error(ex, "Error editing product");
    }
  }
  else if (choice == "8")
  {
    try
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
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to display products.");
      logger.Error(ex, "Error displaying products");
    }
  }
  else if (choice == "9")
  {
    try
    {
      logger.Info("Displaying specific product details");
      var db = new DataContext();
      var products = db.Products.Include("Category").Include("Supplier").OrderBy(p => p.ProductName).ToList();

      if (!products.Any())
      {
        Console.WriteLine("No products available.");
        logger.Info("No products available to display");
      }
      else
      {
        Console.WriteLine("Select a Product to Display:");
        for (int i = 0; i < products.Count; i++)
        {
          Console.WriteLine($"{i + 1}.) {products[i].ProductName}");
        }

        if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= products.Count)
        {
          Product product = products[selection - 1];
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
        else
        {
          Console.WriteLine("Error: Invalid selection.");
          logger.Error("Invalid product selection: {Selection}", selection);
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to display product details.");
      logger.Error(ex, "Error displaying specific product");
    }
  }
  else if (choice == "10")
  {
    logger.Info("Deleting product");
    var db = new DataContext();
    var products = db.Products.OrderBy(p => p.ProductName).ToList();

    Console.WriteLine("Select a Product to Delete:");
    for (int i = 0; i < products.Count; i++)
    {
      Console.WriteLine($"{i + 1}.) {products[i].ProductName}");
    }

    int selection = int.Parse(Console.ReadLine()!) - 1;
    Product productToDelete = products[selection];

    var productWithOrders = db.Products.Include("OrderDetails").FirstOrDefault(p => p.ProductId == productToDelete.ProductId);

    if (productWithOrders != null && productWithOrders.OrderDetails.Any())
    {
      Console.WriteLine($"\nWarning: Product '{productToDelete.ProductName}' has {productWithOrders.OrderDetails.Count} related order(s).");
      Console.WriteLine("Deleting this product will also remove all related order details.");
      Console.Write("Are you sure you want to delete? (yes/no): ");
      string confirmation = Console.ReadLine()?.ToLower();

      if (confirmation != "yes")
      {
        Console.WriteLine("Product deletion cancelled.");
        logger.Info("Product deletion cancelled by user for: {ProductName}", productToDelete.ProductName);
      }
      else
      {
        db.OrderDetails.RemoveRange(productWithOrders.OrderDetails);
        db.Products.Remove(productWithOrders);
        db.SaveChanges();
        Console.WriteLine($"Product '{productToDelete.ProductName}' and {productWithOrders.OrderDetails.Count} related order detail(s) deleted successfully.");
        logger.Info("Product deleted with {Count} order details: {ProductName}", productWithOrders.OrderDetails.Count, productToDelete.ProductName);
      }
    }
    else
    {
      db.Products.Remove(productToDelete);
      db.SaveChanges();
      Console.WriteLine($"Product '{productToDelete.ProductName}' deleted successfully.");
      logger.Info("Product deleted: {ProductName}", productToDelete.ProductName);
    }
  }
  else if (choice == "11")
  {
    try
    {
      logger.Info("Deleting category");
      var db = new DataContext();
      var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();

      if (!categories.Any())
      {
        Console.WriteLine("No categories available to delete.");
        logger.Info("No categories available to delete");
      }
      else
      {
        Console.WriteLine("Select a Category to Delete:");
        for (int i = 0; i < categories.Count; i++)
        {
          Console.WriteLine($"{i + 1}.) {categories[i].CategoryName}");
        }

        if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= categories.Count)
        {
          Category categoryToDelete = categories[selection - 1];

          // Load related Products to check for orphans
          var categoryWithProducts = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == categoryToDelete.CategoryId);

          if (categoryWithProducts != null && categoryWithProducts.Products.Any())
          {
            Console.WriteLine($"\nWarning: Category '{categoryToDelete.CategoryName}' has {categoryWithProducts.Products.Count} related product(s).");
            Console.WriteLine("Deleting this category will set all related products' CategoryId to NULL.");
            Console.Write("Are you sure you want to delete? (yes/no): ");
            string confirmation = Console.ReadLine()?.ToLower();

            if (confirmation != "yes")
            {
              Console.WriteLine("Category deletion cancelled.");
              logger.Info("Category deletion cancelled by user for: {CategoryName}", categoryToDelete.CategoryName);
            }
            else
            {
              // Set CategoryId to NULL for orphaned products instead of deleting them
              foreach (var product in categoryWithProducts.Products)
              {
                product.CategoryId = null;
              }
              db.Categories.Remove(categoryWithProducts);
              db.SaveChanges();
              Console.WriteLine($"Category '{categoryToDelete.CategoryName}' deleted successfully.");
              Console.WriteLine($"{categoryWithProducts.Products.Count} product(s) now have no category assigned.");
              logger.Info("Category deleted with {Count} orphaned products: {CategoryName}", categoryWithProducts.Products.Count, categoryToDelete.CategoryName);
            }
          }
          else
          {
            db.Categories.Remove(categoryToDelete);
            db.SaveChanges();
            Console.WriteLine($"Category '{categoryToDelete.CategoryName}' deleted successfully.");
            logger.Info("Category deleted: {CategoryName}", categoryToDelete.CategoryName);
          }
        }
        else
        {
          Console.WriteLine("Error: Invalid selection.");
          logger.Error("Invalid category selection for deletion: {Selection}", selection);
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Unable to delete category.");
      logger.Error(ex, "Error deleting category");
    }
  }
  else if (String.IsNullOrEmpty(choice))
  {
    logger.Info("User chose to exit");
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");

static T GetValidInput<T>(string prompt, string? currentValue = null)
{
  var logger = LogManager.GetCurrentClassLogger();
  while (true)
  {
    try
    {
      if (currentValue != null)
        Console.WriteLine($"{prompt} [{currentValue}]: ");
      else
        Console.WriteLine($"{prompt}: ");

      string? input = Console.ReadLine();

      if (string.IsNullOrWhiteSpace(input) && currentValue != null)
        return default(T)!;

      if (string.IsNullOrWhiteSpace(input))
      {
        Console.WriteLine("Input cannot be empty. Please try again.");
        logger.Warn("Empty input for prompt: {Prompt}", prompt);
        continue;
      }

      return (T)Convert.ChangeType(input, typeof(T));
    }
    catch (FormatException ex)
    {
      Console.WriteLine($"Error: Invalid format for {typeof(T).Name}. Please try again.");
      logger.Error(ex, "Format error for prompt: {Prompt}", prompt);
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: Invalid entry. Please try again.");
      logger.Error(ex, "Error getting input for prompt: {Prompt}", prompt);
    }
  }
}
