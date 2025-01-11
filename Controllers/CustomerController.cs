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
    public class CustomerController : Controller
    {
        private IConfiguration _configuration;
        Uri baseAddress = new Uri("https://localhost:7273/api");
        private readonly HttpClient _httpClient;

        public CustomerController(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        #region CustomerList-Index
        [HttpGet]
        public IActionResult Index()
        {
            List<CustomerModel> customers = new List<CustomerModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/Customer/GetAllCustomer").Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                customers = JsonConvert.DeserializeObject<List<CustomerModel>>(data);
            }
            return View(customers);
        }
        #endregion

        #region Customer AddEdit-Form
        [HttpPost]
        public async Task<IActionResult> Form(int? CustomerID)
        {
            if (CustomerID <= 0)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            await UserDropDown();
            if (CustomerID.HasValue)
            {
                HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/Customer/GetCustomerByID/{CustomerID}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var customer = JsonConvert.DeserializeObject<CustomerModel>(data);
                    return View(customer);
                }
            }
            return View(new CustomerModel());
        }
        #endregion

        #region CustomerSave
        public async Task<IActionResult> CustomerSave(CustomerModel customerModel)
        {
            if (customerModel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid User is required.");
            }

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(customerModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (customerModel.CustomerID == null || customerModel.CustomerID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Customer/InsertCustomer", content);
                    TempData["AlertMessage"] = "Customer Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Customer/UpdateCustomer/{customerModel.CustomerID}", content);
                    TempData["AlertMessage"] = "Customer Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            await UserDropDown();
            return View("Form", customerModel);
        }
        #endregion

        #region CustomerDelete
        [HttpPost]
        public IActionResult CustomerDelete(int CustomerID)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Customer/DeleteCustomer/{CustomerID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "Customer Deleted Successfully";
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
            command.CommandText = "PR_Customer_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);

            // Create the Excel file in memory
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Customers");

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

                Response.Headers.Add("Content-Disposition", "inline; filename=CustomerList.xlsx");

                // Return the Excel file as a download
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }
        #endregion

        #region User Drop Down
        public async Task UserDropDown()
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetUserDropDown");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserDropDownModel>>(data);
                ViewBag.UserList = users;
            }
        }
        #endregion
    }
}
