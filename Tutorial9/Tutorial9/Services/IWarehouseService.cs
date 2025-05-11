using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<int> AddToProduct_Warehouse(ProductDTO product);
    
    Task<int> AddToProduct_WarehouseProcedure(ProductDTO product);
}