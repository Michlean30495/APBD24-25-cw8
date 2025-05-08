using System.Data;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        string command = @"
            SELECT 
                t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                c.Name AS CountryName
            FROM Trip t
                LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
            ORDER BY t.IdTrip;
        ";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdTrip");
                    var current = new TripDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        Name = reader.GetString(1),
                        Opis = reader.GetString(2),
                        DataOd = reader.GetDateTime(3),
                        DataDo = reader.GetDateTime(4),
                        ileOsob = reader.GetInt32(5),
                        Countries = new List<CountryDTO>()
                    };
                    
                    trips.Add(current);
                    
                    if (!reader.IsDBNull(6))
                    {
                        current.Countries.Add(new CountryDTO()
                        {
                            Name = reader.GetString(6)
                        });
                    }
                }
            }
        }


        return trips;
    }

    public async Task<List<ClientTripDTO>> GetClientTrips(int idClient)
    {
        var trips = new List<ClientTripDTO>();

        var query = @"
            SELECT 
                t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                ct.RegisteredAt, ct.PaymentDate,
                c.Name
            FROM Client_Trip ct
                JOIN Trip t ON ct.IdTrip = t.IdTrip
                LEFT JOIN Country_Trip ctr ON t.IdTrip = ctr.IdTrip
                LEFT JOIN Country c ON ctr.IdCountry = c.IdCountry
            WHERE ct.IdClient = @IdClient
            ORDER BY t.IdTrip;
        ";

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@IdClient", idClient);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {
                    int idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));

                    var current = new ClientTripDTO()
                    {
                        IdTrip = idTrip,
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        RegisteredAt = reader.GetDateTime(6),
                        PaymentDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        Countries = new List<CountryDTO>()
                    };
                    trips.Add(current);

                    if (!reader.IsDBNull(8))
                    {
                        current.Countries.Add(new CountryDTO()
                        {
                            Name = reader.GetString(8)
                        });
                    }
                }
            }
        }
        
        return trips;
    }

    public async Task<int> AddClient(ClientDTO client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            string.IsNullOrWhiteSpace(client.Pesel) ||
            string.IsNullOrWhiteSpace(client.Telephone) || 
            string.IsNullOrWhiteSpace(client.Email))
        {
            throw new InvalidDataException();
        }

        var query = @"
                INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                    VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
            ";

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(query, conn))
        {
            await conn.OpenAsync();
            
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);
            
            var result = await cmd.ExecuteNonQueryAsync();
            return result;
        }
    }

    public async Task<bool> RegisterClientToTrip(int idClient, int idTrip)
    {
        using var conn = new SqlConnection(_connectionString) ;
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();
        try
        {
            var query1 = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
            var clientCheck = new SqlCommand(query1, conn, transaction);
            clientCheck.Parameters.AddWithValue("@IdClient", idClient);
            var result = await clientCheck.ExecuteScalarAsync();
            if (result == null)
                throw new InvalidDataException();

            var query2 = "SELECT * FROM Trip WHERE IdTrip = @IdTrip";
            var tripCheck = new SqlCommand(query2, conn, transaction);
            tripCheck.Parameters.AddWithValue("@IdTrip", idTrip);
            var resultTrip = await tripCheck.ExecuteScalarAsync();
            if (resultTrip == null)
                throw new InvalidDataException();
            
            var query3 = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
            var countCheck = new SqlCommand(query3, conn, transaction);
            countCheck.Parameters.AddWithValue("@IdTrip", idTrip);
            var resultCount = await countCheck.ExecuteScalarAsync();
            
            var query4 = "SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
            var maxPeople = new SqlCommand(query4, conn, transaction);
            maxPeople.Parameters.AddWithValue("@IdTrip", idTrip);
            var resultMaxPeople = await maxPeople.ExecuteScalarAsync();
            
            if ((int)resultMaxPeople >= (int) resultMaxPeople)
                throw new Exception("MaxPeople exceeded");

            var query5 = @"
                    INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                        VALUES (@IdClient, @IdTrip, @RegisteredAt);
                "; 
            var finalResult = new SqlCommand(query5, conn, transaction);
            
            finalResult.Parameters.AddWithValue("@IdClient", idClient);
            finalResult.Parameters.AddWithValue("@IdTrip", idTrip);
            finalResult.Parameters.AddWithValue("@RegisteredAt", DateTime.Now);
            
            await finalResult.ExecuteNonQueryAsync();
            
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}