using CoffeeShopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using CoffeeShopManagementSystem.Areas.Product.Models;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Text;

namespace CoffeeShopManagementSystem.Controllers
{
    [CheckAccess]
    public class UserController : Controller
    {
        private IConfiguration _configuration;
        Uri baseAddress = new Uri("https://localhost:7273/api");
        private readonly HttpClient _httpClient;
        public UserController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
            _configuration = configuration;
        }

        #region UserList-Index
        [HttpGet]
        public IActionResult Index()
        {
            List<UserModel> users = new List<UserModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/User/GetAllUsers").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                users = JsonConvert.DeserializeObject<List<UserModel>>(data);
            }
            return View("Index", users);
        }
        #endregion

        #region User AddEdit-Form
        public async Task<IActionResult> Form(int? UserID)
        {
            if (UserID <= 0)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            if (UserID.HasValue)
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/User/GetUserByID/{UserID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<UserModel>(data);
                    return View(user);
                }
            }
            return View(new UserModel());
        }
        #endregion

        #region UserSave
        [HttpPost]
        public async Task<IActionResult> UserSave(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(userModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (userModel.UserID == null || userModel.UserID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/User/InsertUser", content);
                    TempData["AlertMessage"] = "User Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/User/UpdateUser/{userModel.UserID}", content);
                    TempData["AlertMessage"] = "User Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            return View("Form", userModel);
        }
        #endregion

        #region UserDelete
        [HttpPost]
        public IActionResult UserDelete(int UserID)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/User/DeleteUser/{UserID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "User Deleted Successfully";
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region ExportToExcel
        public IActionResult ExportToExcel()
        {
            // Fetch the product data
            string connectionString = _configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_User_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);

            // Create the Excel file in memory
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Load the data from the DataTable into the worksheet, starting from cell A1.
                worksheet.Cells["A1"].LoadFromDataTable(table, true);

                // Format the header row
                using (var range = worksheet.Cells["A1:Z1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Convert the package to a byte array
                var excelData = package.GetAsByteArray();

                Response.Headers.Add("Content-Disposition", "inline; filename=UserList.xlsx");

                // Return the Excel file as a download
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }
        #endregion

    }
}
