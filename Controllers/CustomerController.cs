using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CustomerController : Controller
    {

        private readonly string? _conexion;

        public CustomerController(IConfiguration? configuration)
        {
            _conexion = configuration?.GetConnectionString("conexion");
        }

        public List<Customer> listCustomers()
        { 
            List<Customer> temporal = new List<Customer>();
            using (SqlConnection con = new SqlConnection(_conexion))
            {
                SqlCommand command = new SqlCommand("sp_list_customers", con);
                command.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Customer customer = new Customer()
                    {
                        CustomerID = reader.GetString(0),
                        CompanyName = reader.GetString(1),
                        ContactName = reader.GetString(2),
                        Address = reader.GetString(3),
                        City = reader.GetString(4),
                        Country = reader.GetString(5),
                        Phone = reader.GetString(6),

                    };
                    temporal.Add(customer);
                }
                
            }
            return temporal;
        }
        
        
        
        public async Task<IActionResult> Index()
        {
            List<Customer> lista = listCustomers();
            return await Task.Run (()=> View(lista));
        }
    }
}
