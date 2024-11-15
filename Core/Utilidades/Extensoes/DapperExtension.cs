using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Core.Utilidades.Enums;
using Microsoft.Data.SqlClient;
using Core.Utilidades.Atributos;

namespace Core.Utilidades.Extensoes
{
    public static class DapperExtension
    {
        public static void DefinirTipoConexao(this IDbConnection connection, eConexao eConexao)
        {
            Config.Conexao = eConexao;
        }

        public static bool ExistemRegistros<T>(this IDbConnection connection)
        {
            try
            {
                Type entityType = typeof(T);
                string tableName = entityType.Name;

                string sql = $"SELECT COUNT(1) FROM {tableName}";

                int count = connection.ExecuteScalar<int>(sql);
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static T Obter<T>(this IDbConnection connection, object id)
        {
            Type entityType = typeof(T);
            string tableName = entityType.Name;

            var keyProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty == null)
                throw new InvalidOperationException($"A entidade {entityType.Name} não possui uma chave primária definida.");

            string keyColumnName = keyProperty.Name;
            string sql = $"SELECT * FROM {tableName} WHERE {keyColumnName} = @Id";

            return connection.QuerySingleOrDefault<T>(sql, new { Id = id });
        }

        public static IEnumerable<T> ObterLista<T>(this IDbConnection connection, string condition = "", object parameters = null)
        {
            Type entityType = typeof(T);
            string tableName = entityType.Name;

            string sql = $"SELECT * FROM {tableName}";

            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += $" WHERE {condition}";
            }

            return parameters == null ? connection.Query<T>(sql) : connection.Query<T>(sql, parameters);
        }

        public static int Adicionar<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null)
        {
            Type entityType = typeof(T);
            string tableName = entityType.Name;

            PropertyInfo[] properties = entityType.GetProperties();
            var columnNames = new List<string>();
            var parameters = new DynamicParameters();

            PropertyInfo keyProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            if (keyProperty == null)
            {
                throw new InvalidOperationException($"A entidade {entityType.Name} não possui uma chave primária definida.");
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<KeyAttribute>() != null)
                    continue;

                if (property.GetCustomAttribute<EditableAttribute>(false) != null)
                    continue;

                if (property.GetCustomAttribute<RequiredAttribute>() != null && property.GetValue(entity) == null)
                {
                    throw new InvalidOperationException($"A propriedade {property.Name} é obrigatória e não foi preenchida.");
                }

                var propertyValue = property.GetValue(entity);
                if (propertyValue is DateTime dateTimeValue)
                {
                    propertyValue = dateTimeValue.Date.ToString("yyyy-MM-dd");
                }

                columnNames.Add($"{property.Name}");
                parameters.Add(property.Name, propertyValue);
            }

            string sql = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", columnNames.Select(p => "@" + p))}); ";
            sql += SqlTradutorFactory.ObterUltimoInsert();

            var result = connection.ExecuteScalar<int>(sql, parameters, transaction);

            if (result == 0)
            {
                throw new InvalidOperationException("Nenhuma linha foi inserida no banco de dados.");
            }

            return result;
        }

        public static int Atualizar<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null)
        {
            Type entityType = typeof(T);
            string tableName = entityType.Name;

            PropertyInfo keyProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty == null)
            {
                throw new InvalidOperationException($"A entidade {entityType.Name} não possui uma chave primária definida.");
            }

            string keyColumnName = keyProperty.Name;
            var columnNames = new List<string>();
            var parameters = new DynamicParameters();

            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (property.GetCustomAttribute<KeyAttribute>() != null)
                    continue;

                if (property.GetCustomAttribute<EditableAttribute>(false) != null)
                    continue;

                if (property.GetCustomAttribute<RequiredAttribute>() != null && property.GetValue(entity) == null)
                {
                    throw new InvalidOperationException($"A propriedade {property.Name} é obrigatória e não foi preenchida.");
                }

                var propertyValue = property.GetValue(entity);
                if (propertyValue is DateTime)
                {
                    propertyValue = SqlTradutorFactory.TratarData(propertyValue);
                }

                columnNames.Add($"{property.Name}");
                parameters.Add(property.Name, propertyValue);
            }

            parameters.Add("Id", keyProperty.GetValue(entity));

            string sql = $"UPDATE {tableName} SET {string.Join(", ", columnNames)} WHERE {keyColumnName} = @Id";

            var rowsAffected = connection.Execute(sql, parameters, transaction);

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Nenhuma linha foi atualizada no banco de dados. Verifique se o registro existe.");
            }

            return rowsAffected;
        }

        public static int Deletar<T>(this IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            Type entityType = typeof(T);
            string tableName = entityType.Name;

            PropertyInfo keyProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty == null)
            {
                throw new InvalidOperationException($"A entidade {entityType.Name} não possui uma chave primária definida.");
            }

            string keyColumnName = keyProperty.Name;

            string checkSql = $"SELECT COUNT(1) FROM {tableName} WHERE {keyColumnName} = @Id";
            var exists = connection.ExecuteScalar<int>(checkSql, new { Id = id }, transaction) > 0;

            if (!exists)
            {
                throw new InvalidOperationException("O registro a ser excluído não existe.");
            }

            string sql = $"DELETE FROM {tableName} WHERE {keyColumnName} = @Id";

            return connection.Execute(sql, new { Id = id }, transaction);
        }


        public static bool CriarTabela<T>(this IDbConnection connection, IDbTransaction transaction = null)
        {
            Type entityType = typeof(T);
            string tableName = entityType.Name;

            StringBuilder createTableSql = new StringBuilder();
            string createTablePrefix = SqlTradutorFactory.ObterCreateTablePrefixo(tableName);
            createTableSql.Append(createTablePrefix); 

            createTableSql.Append($" {tableName} (");

            PropertyInfo[] properties = entityType.GetProperties();
            List<string> columns = new List<string>();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<EditableAttribute>()?.AllowEdit == false)
                    continue;

                Type propertyType = property.PropertyType;
                string columnName = property.Name;
                string columnType = SqlTradutorFactory.ObterTipoColuna(propertyType);  

                if (property.GetCustomAttribute<KeyAttribute>() != null)
                {
                    columns.Add(SqlTradutorFactory.ObterColunaComChavePrimaria(columnName, columnType));  
                }
                else
                {
                    if (property.GetCustomAttribute<RequiredAttribute>() != null)
                    {
                        columns.Add(SqlTradutorFactory.ObterColunaComNotNull(columnName, columnType)); 
                    }
                    else
                    {
                        columns.Add(SqlTradutorFactory.ObterColunaPadrao(columnName, columnType));
                    }
                }
            }

            createTableSql.Append(string.Join(", ", columns));
            createTableSql.Append(");");

            try
            {
                var ret = connection.Execute(createTableSql.ToString(), transaction: transaction);
                return (ret > 0); 
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao criar a tabela '{tableName}': {createTableSql}", ex);
            }
        }
    }
}
