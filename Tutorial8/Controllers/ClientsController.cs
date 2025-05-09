using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : Controller
{
    private readonly ITripsService _tripsService;

    public ClientsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }
    
    [HttpPost]
    public async Task<IActionResult> PostClient(ClientDTO client)
    {
        var result = await _tripsService.AddClient(client);
        if (result == 0)
            return BadRequest();
        return Ok(result);    
    }
    
    [HttpPut("/api/clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
    {
        var result = await _tripsService.RegisterClientToTrip(id, tripId);
        return Ok(result);
    }
    
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> RemoveClientFromTrip(int id, int tripId)
    {
        var result = await _tripsService.RemoveClientFromTrip(id, tripId);
        return Ok(result);
    }
}