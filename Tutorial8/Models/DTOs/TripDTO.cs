namespace Tutorial8.Models.DTOs;

public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Opis { get; set; }
    public DateTime DataOd { get; set; }
    public DateTime DataDo { get; set; }
    public int ileOsob { get; set; }
    public List<CountryDTO> Countries { get; set; }
}

public class CountryDTO
{
    public string Name { get; set; }
}
