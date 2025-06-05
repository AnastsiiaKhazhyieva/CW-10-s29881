using Database_First.Data;
using Database_First.DTO;
using Database_First.Services;
using Microsoft.AspNetCore.Mvc;

namespace Database_First.Controllers;

[ApiController]
[Route("api/trips")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _tripService.GetTripsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> ClientWithTripDto(int idTrip, [FromBody] ClientWithTripDto dto)
    {
        return await _tripService.ClientWithTripDto(idTrip, dto);
    }

}