using CoffeeShopManagementSystem.Areas.Product.Models;
using CoffeeShopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data;
using System.Data.SqlClient;
using OfficeOpenXml;
using Newtonsoft.Json;  // Import EPPlus namespace for Excel operations

namespace CoffeeShopManagementSystem.Areas.Product.Controllers
{
    [CheckAccess]
    [Area("Product")]
    public class ProductController : Controller
    {
        private readonly IConfiguration _configuration;
        Uri baseAddress = new Uri("https://localhost:7273/api");
        private readonly HttpClient _httpClient;
        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        #region ProductList-Index
        [HttpGet]
        public IActionResult Index()
        {
            List<ProductModel> products = new List<ProductModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/Product/GetAllProduct").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                products = JsonConvert.DeserializeObject<List<ProductModel>>(data);
            }
            return View(products);
        }
        #endregion

        #region Product AddEdit-Form
        public async Task<IActionResult> Form(int? ProductID)
        {
            if (ProductID <= 0)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            await UserDropDown();
            if (ProductID.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Product/GetProductByID/{ProductID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    ProductModel product = JsonConvert.DeserializeObject<ProductModel>(data);
                    return View(product);
                }
            }
            return View(new ProductModel());
        }
        #endregion

        #region ProductSave
        [HttpPost]
        public async Task<IActionResult> ProductSave(ProductModel productModel)
        {
            if (productModel.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "A valid User is required.");
            }

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(productModel);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (productModel.ProductID == null || productModel.ProductID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Product/InsertProduct", content);
                    TempData["AlertMessage"] = "Product Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Product/UpdateProduct/{productModel.ProductID}", content);
                    TempData["AlertMessage"] = "Product Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            await UserDropDown();
            return View("Form", productModel);
        }
        #endregion

        #region ProductDelete
        public IActionResult ProductDelete(int ProductID)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Product/DeleteProduct/{ProductID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "Product Deleted Successfully";
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
            command.CommandText = "PR_Product_SelectAll";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);

            // Create the Excel file in memory
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");

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

                Response.Headers.Add("Content-Disposition", "inline; filename=ProductList.xlsx");

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
    }
}
