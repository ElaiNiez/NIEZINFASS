using Microsoft.Data.SqlClient;

namespace NIEZ.Service;

public class Db
{
    private readonly IConfiguration _configuration;

    public Db(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SqlConnection Connection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }
}