using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController(IWarehouseService _warehouseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddToProduct_Warehouse([FromBody] ProductDTO product)
    {
        try
        {
            var id = await _warehouseService.AddToProduct_Warehouse(product);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Procedure")]
    public async Task<IActionResult> AddToProduct_WarehouseProcedure([FromBody] ProductDTO product)
    {
        try
        {
            var result = await _warehouseService.AddToProduct_WarehouseProcedure(product);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}