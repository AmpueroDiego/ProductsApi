using System.Data;

namespace Products.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
