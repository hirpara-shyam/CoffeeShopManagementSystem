using CoffeeShopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Mono.TextTemplating;
using System.Text;

namespace CoffeeShopManagementSystem.Controllers
{
    [CheckAccess]
    public class CountryController : Controller
    {
        #region configuration
        private readonly IConfiguration _configuration;

        Uri baseAddress = new Uri("https://localhost:7273/api");
        private readonly HttpClient _httpClient;

        public CountryController(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }
        #endregion

        #region Index
        [HttpGet]
        public IActionResult Index()
        {
            /*string connectionstr = this._configuration.GetConnectionString("ConnectionString");
            //PrePare a connection
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(connectionstr);
            conn.Open();

            //Prepare a Command
            SqlCommand objCmd = conn.CreateCommand();
            objCmd.CommandType = CommandType.StoredProcedure;
            objCmd.CommandText = "PR_LOC_Country_SelectAll";

            SqlDataReader objSDR = objCmd.ExecuteReader();
            dt.Load(objSDR);
            conn.Close();
            return View("Index", dt);*/

            List<CountryModel> countries = new List<CountryModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/Country/GetAllCountries").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                countries = JsonConvert.DeserializeObject<List<CountryModel>>(data);
            }
            return View("Index", countries);
        }
        #endregion

        #region AddEdit Form
        public async Task<IActionResult> Form(int? CountryID)
        {
            if (CountryID <= 0)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            if (CountryID.HasValue)
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Country/GetCountryByID/{CountryID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var country = JsonConvert.DeserializeObject<CountryModel>(data);
                    return View(country);
                }
            }
            return View(new CountryModel());

            /*string connectionString = _configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_LOC_Country_SelectByPK";
            command.Parameters.AddWithValue("@CountryID", CountryID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            CountryModel countryModel = new CountryModel();

            foreach (DataRow dataRow in table.Rows)
            {
                countryModel.CountryID = Convert.ToInt32(@dataRow["CountryID"]);
                countryModel.CountryName = @dataRow["CountryName"].ToString();
                countryModel.CountryCode = @dataRow["CountryCode"].ToString();
            }
            return View("Form", countryModel);*/
        }
        #endregion

        #region StateSave
        [HttpPost]
        public async Task<IActionResult> CountrySave(CountryModel countryModel)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(countryModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (countryModel.CountryID == null || countryModel.CountryID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Country/InsertCountry", content);
                    TempData["AlertMessage"] = "Country Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Country/UpdateCountry/{countryModel.CountryID}", content);
                    TempData["AlertMessage"] = "Country Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            return View("Form", countryModel);

            /*if (ModelState.IsValid)
            {
                string connectionString = this._configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (countryModel.CountryID == 0)
                {
                    command.CommandText = "PR_LOC_Country_Insert";
                }
                else
                {
                    command.CommandText = "PR_LOC_Country_Update";
                    command.Parameters.Add("@CountryID", SqlDbType.Int).Value = countryModel.CountryID;
                }
                command.Parameters.Add("@CountryName", SqlDbType.VarChar).Value = countryModel.CountryName;
                command.Parameters.Add("@CountryCode", SqlDbType.VarChar).Value = countryModel.CountryCode;
                command.ExecuteNonQuery();
                return RedirectToAction("Index");
            }

            return View("Form", countryModel);*/
        }
        #endregion

        #region Delete
        [HttpGet]
        public IActionResult Delete(int CountryID)
        {
            /*string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                using (SqlCommand sqlCommand = conn.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_LOC_Country_Delete";
                    sqlCommand.Parameters.AddWithValue("@CountryID", CountryID);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");*/

            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Country/DeleteCountry/{CountryID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "Country Deleted Successfully";
            }
            return RedirectToAction("Index");
        }
        #endregion
    }
}
