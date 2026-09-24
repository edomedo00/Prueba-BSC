using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSC.DataAccess.Models
{
    public class ProductoInventario
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ClaveProducto { get; set; } = string.Empty;
        public int Inventario { get; set; }
    }
}
