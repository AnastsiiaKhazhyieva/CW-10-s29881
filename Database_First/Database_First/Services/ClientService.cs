using Database_First.Data;
using Microsoft.EntityFrameworkCore;

namespace Database_First.Services;

public interface IClientService
{
    Task<bool> DeleteClientAsync(int idClient);
}

public class ClientService : IClientService
{
    private readonly Apbd10Context _context;

    public ClientService(Apbd10Context context)
    {
        _context = context;
    }

    public async Task<bool> DeleteClientAsync(int idClient)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
            throw new InvalidOperationException("Cant find client!");

        if (client.ClientTrips.Any())
            throw new InvalidOperationException("Client cannot be deleted (Client is assigned to at least one trip)!");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return true;
    }
}