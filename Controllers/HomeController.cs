using CoffeeShopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace CoffeeShopManagementSystem.Controllers
{
    [CheckAccess]
    public class HomeController : Controller
    {
        public IConfiguration configuration;

        public HomeController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        #region UserProfileData
        public IActionResult UserProfile(int UserID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_User_SelectByID";
            command.Parameters.AddWithValue("@UserID", UserID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            UserModel userModel = new UserModel();

            foreach (DataRow dataRow in table.Rows)
            {
                userModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
                userModel.UserName = @dataRow["UserName"].ToString();
                userModel.Email = @dataRow["Email"].ToString();
                userModel.Password = @dataRow["Password"].ToString();
                userModel.MobileNo = @dataRow["MobileNo"].ToString();
                userModel.Address = @dataRow["Address"].ToString();
                userModel.IsActive = Convert.ToBoolean(@dataRow["IsActive"]);
            }
            ViewBag.userData = userModel;
            return View("UserProfile");
        }
        #endregion
    }
}
