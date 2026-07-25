using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Reflection.PortableExecutable;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ProductController : Controller
    {

        private readonly string? conexion;

        public ProductController(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("conexion");
        }

        List<Product> listProducts(string ProductName)
        {
            List<Product> temporal = new List<Product>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand command = new SqlCommand("sp_list_products_by_name", con);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ProductName", ProductName);
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            ProductID = reader.GetInt32(0),
                            ProductName = reader.GetString(1),
                            CompanyName = reader.GetString(2),
                            CategoryName = reader.GetString(3),
                            UnitPrice = reader.GetDecimal(4),
                            UnitsInStock = reader.GetInt16(5)

                        };
                        temporal.Add(product);

                    }
                }
                return temporal;
            }


        }

        public async Task<IActionResult> Index(string ProductName = "", int page = 0)
        {
            IEnumerable<Product> products = listProducts(ProductName);
            int filas = 0;
            int totalRegistros = products.Count();
            int totalPaginas = totalRegistros % filas == 0 ?
                                (totalRegistros / filas) :
                                (totalRegistros / filas + 1);
            ViewBag.totalRegistros = totalRegistros;
            ViewBag.totalPaginas = totalPaginas;
            ViewBag.page = page;
            ViewBag.productName = ProductName;

            return View(await Task.Run(() => products.Skip(filas * (page - 1)).Take(filas)));
        }

        List<Category> listCategories()
        {
            List<Category> temporal = new List<Category>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("sp_list_customers", con))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Category category = new Category
                            {
                                CategoryID = reader.GetInt32(0),
                                CategoryName = reader.GetString(1),
                            };
                            temporal.Add(category);

                        }
                    }

                }
                
            }
            return temporal;
        }

        List<Supplier> listSuppliers()
        {
            List<Supplier> temporal = new List<Supplier>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("sp_list_suppliers", con))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Supplier supplier = new Supplier
                            {
                                SupplierID = reader.GetInt32(0),
                                CompanyName = reader.GetString(1),
                            };
                            temporal.Add(supplier);

                        }
                    }

                }

            }
            return temporal;
        }

        bool insertProduct(Product product)
        { 
            bool resp = false;
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand("sp_insert_product", con))
                    {
                        command.Transaction = transaction;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProductName", product.ProductName);
                        command.Parameters.AddWithValue("@SupplierID", product.SupplierID);
                        command.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                        command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                        command.Parameters.AddWithValue("@UnitsInStock", product.UnitsInStock);
                        int rows = command.ExecuteNonQuery();
                        transaction.Commit();
                        resp = rows > 0;   

                    }
                }
                catch (Exception ex) 
                {
                    transaction.Rollback();
                    resp = false;
                }


            }
            return resp;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<Category> categories = listCategories();
            List<Supplier> suppliers = listSuppliers();
            ViewBag.categories = new SelectList(categories, "CategoryID", "CategoryName");
            ViewBag.suppliers = new SelectList(suppliers, "SupplierID", "CompanyName");
            return View(await Task.Run(()=> new Product()));
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            bool resp = insertProduct(product);
            TempData["mensaje"] = resp ? "Producto Registrado Correctamente!" : "Hubo un error al registrar al producto!";
            return RedirectToAction(await Task.Run(() => "Index"));
        }
    }
}
