
namespace DienstDuizend.AuthService.IntegrationTesting.Setup;

[CollectionDefinition(nameof(IntegrationTestCollection), DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<WebAppFactory>
{
}
