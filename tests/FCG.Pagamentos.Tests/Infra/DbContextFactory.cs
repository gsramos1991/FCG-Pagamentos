using FCG.Pagamentos.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Pagamentos.Tests.Infra;

public static class DbContextFactory
{
    public static ApplicationDbContext CreateInMemory(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;
        return new ApplicationDbContext(options);
    }
}
