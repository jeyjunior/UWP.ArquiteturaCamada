﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilidades.Validador
{
    public class ValidaResultado
    {
        private readonly List<string> _erros = new List<string>();

        public bool EhValido
        {
            get
            {
                return _erros.Count == 0;
            }
        }

        public IEnumerable<string> Erros
        {
            get
            {
                return _erros;
            }
        }

        public void Adicionar(string erro)
        {
            _erros.Add(erro);
        }

        public void Remover(string erro)
        {
            if (!_erros.Contains(erro))
                return;

            _erros.Remove(erro);
        }
    }
}
