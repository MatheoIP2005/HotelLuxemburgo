using HotelLux.Stay.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelLux.Stay.DataAccess;

public class StayDbContextFactory : IDesignTimeDbContextFactory<StayDbContext>
{
    public StayDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("StayDb")
            ?? "Host=localhost;Port=5432;Database=HotelLux_Stay;Username=postgres;Password=BD081205;SearchPath=hospedaje";
        var options = new DbContextOptionsBuilder<StayDbContext>().UseNpgsql(cs).Options;
        return new StayDbContext(options);
    }
}
