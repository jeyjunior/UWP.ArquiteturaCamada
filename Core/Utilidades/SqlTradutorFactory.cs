using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Utilidades.Atributos;
using Core.Utilidades.Enums;

namespace Core.Utilidades
{
    public static class SqlTradutorFactory
    {
        public static string ObterCreateTablePrefixo(string table)
        {
            if (Config.Conexao == eConexao.SQLite)
                return "CREATE TABLE IF NOT EXISTS"; 
            else if (Config.Conexao == eConexao.SQLServer)
                return $"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{table}' AND schema_id = SCHEMA_ID('dbo')) BEGIN CREATE TABLE ";

            throw new NotSupportedException($"Banco de dados {Config.Conexao} não suportado.");
        }

        public static string ObterTipoColuna(Type propertyType)
        {
            switch (Config.Conexao)
            {
                case eConexao.SQLite:
                    if (propertyType == typeof(int) || propertyType == typeof(long))
                        return "INTEGER";
                    else if (propertyType == typeof(string))
                        return "TEXT";
                    else if (propertyType == typeof(decimal) || propertyType == typeof(double))
                        return "REAL";
                    else if (propertyType == typeof(bool))
                        return "INTEGER";
                    else if (propertyType == typeof(DateTime))
                        return "INTEGER";
                    else
                        return "BLOB"; 
                case eConexao.SQLServer:
                    if (propertyType == typeof(int) || propertyType == typeof(long))
                        return "INT";
                    else if (propertyType == typeof(string))
                        return "VARCHAR(MAX)";
                    else if (propertyType == typeof(decimal) || propertyType == typeof(double))
                        return "DECIMAL";
                    else if (propertyType == typeof(bool))
                        return "BIT";
                    else if (propertyType == typeof(DateTime))
                        return "DATETIME";
                    else
                        return "VARBINARY(MAX)";
            }

            return "";
        }

        public static string ObterAutoincremento()
        {
            if (Config.Conexao == eConexao.SQLite)
                return "AUTOINCREMENT";
            else if (Config.Conexao == eConexao.SQLServer)
                return "IDENTITY";

            throw new NotSupportedException($"Autoincremento não é suportado para o banco de dados {Config.Conexao}.");
        }

        public static string ObterNotNull()
        {
            return "NOT NULL";
        }

        public static string ObterColunaPadrao(string columnName, string columnType)
        {
            return $"{columnName} {columnType}";
        }

        public static string ObterColunaComNotNull(string columnName, string columnType)
        {
            return $"{columnName} {columnType} {ObterNotNull()}";
        }

        public static string ObterColunaComChavePrimaria(string columnName, string columnType)
        {
            return $"{columnName} {columnType} PRIMARY KEY {ObterAutoincremento()}";
        }
    
        public static string ObterUltimoInsert()
        {
            if (Config.Conexao == eConexao.SQLite)
                return "SELECT last_insert_rowid();";
            else if (Config.Conexao == eConexao.SQLServer)
                return "SELECT SCOPE_IDENTITY();";

            return "";
        }

        public static object TratarData(object value)
        {
            DateTime dateTimeValue = (DateTime)value;

            switch (Config.Conexao)
            {
                case eConexao.SQLite:
                case eConexao.MySQL:
                case eConexao.PostgreSQL:
                    return dateTimeValue.ToString("yyyy-MM-dd"); 
                case eConexao.SQLServer:
                    return dateTimeValue;
                case eConexao.Firebase:
                    return dateTimeValue.ToString("yyyy-MM-dd");
                default:
                    return value; 
            }
        }
    }
}
