using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Utilidades.Extensoes;

namespace InfraData
{
    public static class InfraConfig
    {
        public static void Iniciar()
        {
            var uow = BootstrapInfraData.Container.GetInstance<IUnitOfWork>();

            // Teste
            if(uow.Connection.ExistemRegistros<Categoria>() == false)
            {
                uow.Connection.CriarTabela<Categoria>();

                uow.Connection.Adicionar(new Categoria { Nome = "Mercado", Descricao = "Compras de alimentos e produtos para casa" });
                uow.Connection.Adicionar(new Categoria { Nome = "Restaurante", Descricao = "Despesas com refeições em restaurantes" });
                uow.Connection.Adicionar(new Categoria { Nome = "Farmácia", Descricao = "Despesas com medicamentos e produtos farmacêuticos" });
                uow.Connection.Adicionar(new Categoria { Nome = "Emprego", Descricao = "Renda proveniente do trabalho" });
                uow.Connection.Adicionar(new Categoria { Nome = "Outros", Descricao = "Outras categorias não especificadas" });
            }

            var ret = uow.Connection.ObterLista<Categoria>();
        }
    }
}
