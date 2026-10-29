using ManConstruc.Api.Services;
using Xunit;

namespace ManConstruc.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_no_guarda_la_contrasena_y_verifica_correctamente()
    {
        var hash = PasswordHasher.Hash("ClaveSegura123");

        Assert.DoesNotContain("ClaveSegura123", hash);
        Assert.True(PasswordHasher.Verify("ClaveSegura123", hash));
        Assert.False(PasswordHasher.Verify("OtraClave123", hash));
    }

    [Fact]
    public void Hash_usa_sal_aleatoria()
    {
        var first = PasswordHasher.Hash("ClaveSegura123");
        var second = PasswordHasher.Hash("ClaveSegura123");

        Assert.NotEqual(first, second);
        Assert.True(PasswordHasher.Verify("ClaveSegura123", first));
        Assert.True(PasswordHasher.Verify("ClaveSegura123", second));
    }
}
