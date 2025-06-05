using Database_First.Data;
using Database_First.DTO;
using Database_First.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Database_First.Services;

public interface ITripService
{
    Task<TripResponseDto> GetTripsAsync(int page, int pageSize);
    Task<IActionResult> ClientWithTripDto(int idTrip, ClientWithTripDto dto);

}

public class TripService : ITripService
{
    private readonly Apbd10Context _context;

    public TripService(Apbd10Context context)
    {
        _context = context;
    }
    
    public async Task<TripResponseDto> GetTripsAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        
        var totalTrips = await _context.Trips.CountAsync();
        var totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);

        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.Countries.Select(c => new CountryDto
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            }).ToListAsync();
        
        return new TripResponseDto
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = totalPages,
            Trips = trips
        };
    }
    
    public async Task<IActionResult> ClientWithTripDto(int idTrip, ClientWithTripDto dto)
    {
        var trip = await _context.Trips.FindAsync(idTrip);
        if (trip == null || trip.DateFrom <= DateTime.Now)
            return new BadRequestObjectResult("Trip not found or already occurred.");

        var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);
        if (existingClient == null)
        {
            existingClient = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Pesel = dto.Pesel
            };
            _context.Clients.Add(existingClient);
            await _context.SaveChangesAsync();
        }

        var alreadyAssigned = await _context.ClientTrips
            .AnyAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);
        if (alreadyAssigned)
            return new BadRequestObjectResult("Client already assigned to this trip!");

        var clientTrip = new ClientTrip
        {
            IdClient = existingClient.IdClient,
            IdTrip = idTrip,
            PaymentDate = dto.PaymentDate,
            RegisteredAt = DateTime.Now
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return new OkObjectResult("Client successfully assigned to the trip!");
    }
}