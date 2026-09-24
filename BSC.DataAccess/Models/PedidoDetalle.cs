using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSC.DataAccess.Models
{
    public class PedidoDetalle
    {
        public int PedidoID { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string ClaveProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
