using CoffeeShopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using CoffeeShopManagementSystem.Areas.Product.Models;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using System.Text;

namespace CoffeeShopManagementSystem.Controllers
{
    [CheckAccess]
    public class BillsController : Controller
    {
        /*public IConfiguration configuration;

        public BillsController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }*/

        Uri baseAddress = new Uri("https://localhost:7273/api");
        private readonly HttpClient _httpClient;
        private IConfiguration _configuration;

        public BillsController(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        #region BillsList-Index
        public IActionResult Index()
        {
            List<BillsModel> bills = new List<BillsModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/Bill/GetAllBills").Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                bills = JsonConvert.DeserializeObject<List<BillsModel>>(data);
            }
            return View(bills);
        }
        #endregion

        #region Bills AddEdit-BillForm

        public async Task<IActionResult> BillForm(int? BillID)
        {
            if (BillID <= 0)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            await UserDropDown();
            await OrderDropDown();

            if (BillID.HasValue)
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Bill/GetBillByID/{BillID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var bill = JsonConvert.DeserializeObject<BillsModel>(data);
                    return View(bill);
                }
            }
            return View(new BillsModel());
        }
        #endregion

        #region BillsSave
        [HttpPost]
        public async Task<IActionResult> BillsSave(BillsModel billsModel)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(billsModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                if (billsModel.BillID == null || billsModel.BillID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Bill/InsertBill", content);
                    TempData["AlertMessage"] = "Bill Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Bill/UpdateBill/{billsModel.BillID}", content);
                    TempData["AlertMessage"] = "Bill Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            await UserDropDown();
            await OrderDropDown();
            return View("BillForm", billsModel);
        }
        #endregion

        #region BillsDelete
        [HttpPost]
        [Route("Bills/BillsDelete/{BillID}")]
        public IActionResult BillsDelete(int BillID)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Bill/DeleteBill/{BillID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "Bill Deleted Successfully";
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
            command.CommandText = "PR_Bills_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);

            // Create the Excel file in memory
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Bills");

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

                Response.Headers.Add("Content-Disposition", "inline; filename=BillsList.xlsx");

                // Return the Excel file as a download
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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

        #region User Drop Down
        public async Task UserDropDown()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetUserDropDown");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var userList = JsonConvert.DeserializeObject<List<UserDropDownModel>>(data);
                    ViewBag.UserList = userList;
                }
                else
                {
                    ViewBag.UserList = new List<UserDropDownModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.UserList = new List<UserDropDownModel>();
            }

        }
        #endregion
    }
}
