using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Core.Utilidades.Extensoes;
using Core.Utilidades.Enums;
using Core.Interfaces;
using Data;
using SimpleInjector;

namespace InfraData
{
    public class BootstrapInfraData
    {
        public static Container Container { get; private set; }
        public static void Iniciar(Container container)
        {
            Container = container;

            eConexao conexao = eConexao.SQLite;

            IDbConnection connection = Config.ObterConexao(conexao);

            container.Register<IUnitOfWork>(() => new UnitOfWork(connection, conexao), Lifestyle.Singleton);
        }
    }
}
