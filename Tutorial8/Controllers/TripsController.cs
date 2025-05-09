using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var result = await _tripsService.GetClientTrips(id);
            if (result == null)
                return NotFound();
            return Ok();
        }
        
        [HttpPost("api/clients")]
        public async Task<IActionResult> PostClient(ClientDTO client)
        {
            var result = await _tripsService.AddClient(client);
            if (result == 0)
                return BadRequest();
            return Ok();    
        }
        
        [HttpPut("/api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
        {
            await _tripsService.RegisterClientToTrip(id, tripId);
            return Ok();
        }

        [HttpDelete("api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> RemoveClientFromTrip(int id, int tripId)
        {
            await _tripsService.RemoveClientFromTrip(id, tripId);
            return Ok();
        }
    }
}
