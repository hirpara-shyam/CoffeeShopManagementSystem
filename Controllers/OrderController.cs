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
    public class OrderController : Controller
    {
        private IConfiguration _configuration;
        Uri baseAddress = new Uri("https://localhost:7273/api");
        HttpClient _httpClient;
        public OrderController(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        #region OrderList-Index
        [HttpGet]
        public IActionResult Index()
        {
            List<OrderModel> orders = new List<OrderModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/Order/GetAllOrders").Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                orders = JsonConvert.DeserializeObject<List<OrderModel>>(data);
            }
            return View(orders);
        }
        #endregion

        #region UserDropDown
        public async Task UserDropDown()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetUserDropDown");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                List<UserDropDownModel> userList = JsonConvert.DeserializeObject<List<UserDropDownModel>>(data);
                ViewBag.UserList = userList;
            }
        }
        #endregion

        #region Customer DropDown
        public async Task CustomerDropDown()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetCustomerDropDown");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                List<CustomerDropDownModel> customerList = JsonConvert.DeserializeObject<List<CustomerDropDownModel>>(data);
                ViewBag.CustomerList = customerList;
            }
        }
        #endregion

        #region Order AddEdit-Form
        public async Task<IActionResult> Form(int? OrderID)
        {
            if (OrderID <= 0)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            await CustomerDropDown();
            await UserDropDown();
            if (OrderID.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Order/GetOrderByID/{OrderID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var order = JsonConvert.DeserializeObject<OrderModel>(data);
                    return View(order);
                }
            }
            return View(new OrderModel());
        }
        #endregion

        #region OrderSave
        [HttpPost]
        public async Task<IActionResult> OrderSave(OrderModel orderModel)
        {
            if (orderModel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid User is required.");
            }

            if (orderModel.CustomerID <= 0)
            {
                ModelState.AddModelError("CustomerID", "A valid Customer is required.");
            }

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(orderModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                if (orderModel.OrderID == null || orderModel.OrderID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Order/InsertOrder", content);
                    TempData["AlertMessage"] = "Order Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Order/UpdateOrder/{orderModel.OrderID}", content);
                    TempData["AlertMessage"] = "Order Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            await CustomerDropDown();
            await UserDropDown();
            return View("Form", orderModel);
        }
        #endregion

        #region OrderDelete
        [HttpPost]
        public IActionResult OrderDelete(int OrderID)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Order/DeleteOrder/{OrderID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "Order Deleted Successfully";
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
            command.CommandText = "PR_Order_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);

            // Create the Excel file in memory
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Orders");

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

                Response.Headers.Add("Content-Disposition", "inline; filename=OrderList.xlsx");

                // Return the Excel file as a download
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }
        #endregion
    }
}
