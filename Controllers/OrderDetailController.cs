using CoffeeShopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using CoffeeShopManagementSystem.Areas.Product.Models;
using OfficeOpenXml;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Text;

namespace CoffeeShopManagementSystem.Controllers
{
    [CheckAccess]
    public class OrderDetailController : Controller
    {
        private IConfiguration _configuration;
        Uri baseAddress = new Uri("https://localhost:7273/api");
        HttpClient _httpClient;
        public OrderDetailController(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        #region OrderDetailList-Index
        [HttpGet]
        public IActionResult Index()
        {
            List<OrderDetailModel> orderDetails = new List<OrderDetailModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/OrderDetail/GetAllOrderDetails").Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                orderDetails = JsonConvert.DeserializeObject<List<OrderDetailModel>>(data);
            }
            return View(orderDetails);
        }
        #endregion

        #region OrderDetail AddEdit-Form
        public async Task<IActionResult> Form(int? OrderDetailID)
        {
            if (OrderDetailID <= 0)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            await OrderDropDown();
            await UserDropDown();
            await ProductDropDown();
            if (OrderDetailID.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/OrderDetail/GetOrderDetailByID/{OrderDetailID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    OrderDetailModel orderDetail = JsonConvert.DeserializeObject<OrderDetailModel>(data);
                    return View(orderDetail);
                }
            }
            return View(new OrderDetailModel());
        }
        #endregion

        #region OrderDetailSave
        [HttpPost]
        public async Task<IActionResult> OrderDetailSave(OrderDetailModel orderDetailModel)
        {
            if (orderDetailModel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid User is required.");
            }
            if (orderDetailModel.ProductID <= 0)
            {
                ModelState.AddModelError("ProductID", "A valid Product is required.");
            }
            if (orderDetailModel.OrderID <= 0)
            {
                ModelState.AddModelError("OrderID", "A valid Order ID is required.");
            }

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(orderDetailModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (orderDetailModel.OrderDetailID == null || orderDetailModel.OrderDetailID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/OrderDetail/InsertOrderDetail", content);
                    TempData["AlertMessage"] = "Order Detail Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/OrderDetail/UpdateOrderDetail/{orderDetailModel.OrderDetailID}", content);
                    TempData["AlertMessage"] = "Order Detail Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            await ProductDropDown();
            await UserDropDown();
            await OrderDropDown();

            return View("Form", orderDetailModel);
        }
        #endregion

        #region OrderDetailDelete
        [HttpPost]
        public IActionResult OrderDetailDelete(int OrderDetailID)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/OrderDetail/DeleteOrderDetail/{OrderDetailID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "Order Detail Deleted Successfully";
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
            command.CommandText = "PR_OrderDetail_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);

            // Create the Excel file in memory
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Order Details");

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

                Response.Headers.Add("Content-Disposition", "inline; filename=OrderDetailsList.xlsx");

                // Return the Excel file as a download
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }
        #endregion

        #region User Drop Down
        public async Task UserDropDown()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetUserDropDown");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserDropDownModel>>(data);
                ViewBag.UserList = users;
            }
        }
        #endregion

        #region Product Drop Down
        public async Task ProductDropDown()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetProductDropDown");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var productList = JsonConvert.DeserializeObject<List<ProductDropDownModel>>(data);
                ViewBag.ProductList = productList;
            }
        }
        #endregion

        #region Order Drop Down
        public async Task OrderDropDown()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetOrderDropDown");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var orderList = JsonConvert.DeserializeObject<List<OrderDropDownModel>>(data);
                    ViewBag.OrderList = orderList;
                }
                else
                {
                    ViewBag.OrderList = new List<OrderDropDownModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.OrderList = new List<OrderDropDownModel>();
            }
        }
        #endregion
    }
}
