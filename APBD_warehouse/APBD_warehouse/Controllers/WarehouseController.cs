using APBD_warehouse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace APBD_warehouse.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarehouseController : ControllerBase
    {

        //If warehouseDbContext is used
        /*
        private readonly WarehouseDbContext _context;
        public WarehouseController(WarehouseDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public IActionResult AddProductToWarehouse([FromBody] ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount has to be greater than zero");
            }

            var productExists = _context.Products.Any(p => p.Id == request.ProductId);
            if (!productExists)
            {
                return NotFound($"No product found with Id {request.ProductId}");
            }

            //check whether warehouseExists

            var order = _context.Orders
                .Where(o => o.ProductId == request.ProductId && o.CreatedAt < request.CreatedAt)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();
        */

        // If dbcontext is not used

        private readonly string connectionString = "YourConnectionStringGoesHere";


        [HttpPost]
        public IActionResult AddProductToWarehouse([FromBody] ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount has to be greater than zero");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // Check if the product with the given ID exists
                    var productExistsCommand = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @ProductId", connection, transaction);
                    productExistsCommand.Parameters.AddWithValue("@ProductId", request.ProductId);
                    var productCount = (int)productExistsCommand.ExecuteScalar();
                    if (productCount == 0)
                    {
                        transaction.Rollback();
                        return NotFound($"No product found with ID {request.ProductId}");
                    }

                    // Check if the warehouse with the given ID exists
                    var warehouseExistsCommand = new SqlCommand("SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @WarehouseId", connection, transaction);
                    warehouseExistsCommand.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
                    var warehouseCount = (int)warehouseExistsCommand.ExecuteScalar();
                    if (warehouseCount == 0)
                    {
                        transaction.Rollback();
                        return NotFound($"No warehouse found with ID {request.WarehouseId}");
                    }

                    // Verify if there is a corresponding product purchase order in the Order table
                    var orderExistsCommand = new SqlCommand("SELECT TOP 1 IdOrder FROM [Order] WHERE IdProduct = @ProductId AND Amount = @Amount AND CreatedAt < @CreatedAt", connection, transaction);
                    orderExistsCommand.Parameters.AddWithValue("@ProductId", request.ProductId);
                    orderExistsCommand.Parameters.AddWithValue("@Amount", request.Amount);
                    orderExistsCommand.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                    var orderId = orderExistsCommand.ExecuteScalar();
                    if (orderId == null)
                    {
                        transaction.Rollback();
                        return NotFound("No matching order found in the Order table");
                    }

                    // Check if the order has been completed
                    var orderCompletedCommand = new SqlCommand("SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @OrderId AND FulfilledAt IS NOT NULL", connection, transaction);
                    orderCompletedCommand.Parameters.AddWithValue("@OrderId", orderId);
                    var orderCompletedCount = (int)orderCompletedCommand.ExecuteScalar();
                    if (orderCompletedCount > 0)
                    {
                        transaction.Rollback();
                        return BadRequest($"Order with ID {orderId} has already been completed");
                    }

                    // Update the FulfilledAt column of the order with the current date and time
                    var updateOrderCommand = new SqlCommand("UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @OrderId", connection, transaction);
                    updateOrderCommand.Parameters.AddWithValue("@OrderId", orderId);
                    updateOrderCommand.ExecuteNonQuery();

                    // Insert a record into the Product_Warehouse table
                    var insertProductWarehouseCommand = new SqlCommand("INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)", connection, transaction);
                    insertProductWarehouseCommand.Parameters.AddWithValue("@IdWarehouse", request.WarehouseId);
                    insertProductWarehouseCommand.Parameters.AddWithValue("@IdProduct", request.ProductId);
                    insertProductWarehouseCommand.Parameters.AddWithValue("@IdOrder", orderId);
                    insertProductWarehouseCommand.Parameters.AddWithValue("@Amount", request.Amount);
                    insertProductWarehouseCommand.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                    // Fetch the product price from the database
                    decimal productPrice = 0; // Assume you fetch the product price from the database
                    decimal totalPrice = productPrice * request.Amount;
                    insertProductWarehouseCommand.Parameters.AddWithValue("@Price", totalPrice);

                    insertProductWarehouseCommand.ExecuteNonQuery();

                    // Return the value of the primary key generated for the inserted record
                    var getProductWarehouseIdCommand = new SqlCommand("SELECT SCOPE_IDENTITY()", connection, transaction);
                    var productWarehouseId = getProductWarehouseIdCommand.ExecuteScalar();

                    transaction.Commit();
                    return Ok($"Product added successfully. Product_Warehouse ID: {productWarehouseId}");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }


        [HttpPost]
        public IActionResult AddProductToWarehouseUsingProcedure([FromBody] ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount has to be greater than zero");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // Execute the stored procedure
                    using (var command = new SqlCommand("AddProductToWarehouseUsingProcedure", connection, transaction))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // Set up the parameters for the stored procedure
                        command.Parameters.AddWithValue("@IdProduct", request.ProductId);
                        command.Parameters.AddWithValue("@IdWarehouse", request.WarehouseId);
                        command.Parameters.AddWithValue("@Amount", request.Amount);
                        command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                        // Execute the stored procedure and get the result
                        var result = command.ExecuteScalar();

                        // Commit the transaction
                        transaction.Commit();

                        // Return the result from the stored procedure to the client
                        return Ok($"Product added successfully. Product_Warehouse ID: {result}");
                    }
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}
