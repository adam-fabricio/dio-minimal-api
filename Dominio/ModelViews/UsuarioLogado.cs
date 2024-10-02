using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dio_minimal_api.Dominio.ModelViews
{
    public class UsuarioLogado
    {
        public string? Email { get; set; }
        public string? Perfil { get; set; }
        public string? Token { get; set; }
    }
}