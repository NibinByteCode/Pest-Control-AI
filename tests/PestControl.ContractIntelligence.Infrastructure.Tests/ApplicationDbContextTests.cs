using Microsoft.EntityFrameworkCore;
using PestControl.ContractIntelligence.Infrastructure.Persistence;

namespace PestControl.ContractIntelligence.Infrastructure.Tests;

public sealed class ApplicationDbContextTests
{
    [Fact]
    public void ModelContainsRequiredTablesAndIndexes()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new ApplicationDbContext(options);
        var model = context.Model;

        Assert.NotNull(model.FindEntityType("PestControl.ContractIntelligence.Infrastructure.Persistence.Models.DocumentRecord"));
        Assert.NotNull(model.FindEntityType("PestControl.ContractIntelligence.Infrastructure.Persistence.Models.AuditLogRecord"));

        var user = model.FindEntityType("PestControl.ContractIntelligence.Infrastructure.Persistence.Models.UserRecord");
        Assert.NotNull(user?.FindIndex(user.FindProperty("NormalizedEmail")!));

        var job = model.FindEntityType("PestControl.ContractIntelligence.Infrastructure.Persistence.Models.IngestionJobRecord");
        Assert.NotNull(job?.FindProperty("RowVersion")?.IsConcurrencyToken);
    }
}
