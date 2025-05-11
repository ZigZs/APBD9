using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;
    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<int> AddToProduct_Warehouse(ProductDTO product)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            //1
            command.CommandText = @"SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            int counter = (int)await command.ExecuteScalarAsync();
            if (!(counter > 0))
            {
                throw new Exception($"There is no product like {product.IdProduct}");
            }
            command.Parameters.Clear();
            command.CommandText = @"SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            counter = (int)await command.ExecuteScalarAsync();
            if (!(counter > 0))
            {
                throw new Exception($"There is no WareHouse like {product.IdWarehouse}");
            }
            
            //2
            command.Parameters.Clear();
            command.CommandText = @"SELECT IdOrder FROM [Order] WHERE [Order].IdProduct = @IdProduct AND [Order].Amount = @Amount AND [Order].CreatedAt < @CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            var idOrder = await command.ExecuteScalarAsync();
            if (idOrder == null)
            {
                throw new Exception($"There is no Order for Product like {product.IdProduct}");
            }
            
            //3
            command.Parameters.Clear();
            command.CommandText = @"SELECT IdOrder FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder",idOrder);
            var realised = await command.ExecuteScalarAsync();
            if (realised != null)
            {
                throw new Exception($"in Product_Warehouse there already is Order for product like {product.IdProduct}");
            }
            
            //4
            command.Parameters.Clear();
            command.CommandText = @"UPDATE [Order] SET FulfilledAt = @FufilledAr WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            command.Parameters.AddWithValue("@IdOrder",idOrder);
            
            //5
            command.Parameters.Clear();
            command.CommandText = @"SELECT [Price] FROM [Product] WHERE [IdProduct] = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            decimal price = (decimal)await command.ExecuteScalarAsync();
            
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO [Product_Warehouse] ([IdWarehouse],[IdProduct],[IdOrder],[Amount],[Price],[CreatedAt]) VALUES (@IdWarehouse,@IdProduct,@IdOrder,@Amount,@Price,@CreatedAt) SELECT CAST(SCOPE_IDENTITY() as int) ";
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@Price", price * product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            
            //6
            int key = (int)await command.ExecuteScalarAsync();
            
            await transaction.CommitAsync();
            return key;
            
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
       
    }
}