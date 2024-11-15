using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Core.Utilidades.Enums;
using Windows.Storage;
using System.Data.SqlClient;
using System.IO;


namespace Data
{
    public static class Config
    {
        public static IDbConnection ObterConexao(eConexao conexao)
        {
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;

            switch (conexao)
            {
                case eConexao.Nenhum: break;

                case eConexao.SQLite:
                    SQLitePCL.Batteries.Init();

                    string dbPath = Path.Combine(tempFolder.Path, "sqlite_base.db");

                    if (!File.Exists(dbPath))
                    {
                        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                        {
                            connection.Open();
                            connection.Close();
                        }
                    }

                    return new SqliteConnection($"Data Source={dbPath}");

                case eConexao.SQLServer:

                    return new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=ControleFinanceiro;Integrated Security=True;TrustServerCertificate=True;");


                case eConexao.MySQL:
                    // MySql.Data.
                    // Exemplo: return new MySqlConnection("server=localhost;user=root;database=SeuBancoDeDados;password=suaSenha");

                    break;
                case eConexao.PostgreSQL:
                    // Conexão PostgreSQL usando Npgsql.
                    // Exemplo: return new NpgsqlConnection("Host=localhost;Username=postgres;Password=suaSenha;Database=SeuBancoDeDados");

                    break;
                case eConexao.Firebase:
                    // Normalmente, para o Firebase, não se usa uma string de conexão direta, mas sim uma biblioteca Firebase SDK.

                    break;
            }

            return null;
        }
    }
}
