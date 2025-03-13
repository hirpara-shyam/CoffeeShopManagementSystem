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
        }
        #endregion

        #region CountrySave
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
        }
        #endregion

        #region Delete
        [HttpGet]
        public IActionResult Delete(int CountryID)
        {
            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Country/DeleteCountry/{CountryID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "Country Deleted Successfully";
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region Ajax CRUD
        public async Task<IActionResult> AjaxCrud()
        {
            return View();
        }

        public IEnumerable<CountryModel> GetCountries()
        {
            List<CountryModel> countries = new List<CountryModel>();
            HttpResponseMessage response = _httpClient.GetAsync($"{_httpClient.BaseAddress}/Country/GetAllCountries").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                countries = JsonConvert.DeserializeObject<List<CountryModel>>(data);
            }
            return countries;
        }

        /*[HttpPost]
        public async Task<JsonResult> SaveCountry([FromBody] CountryModel country)
        {
            HttpResponseMessage response;

            if (country.CountryID > 0)
            {
                var jsonContent = JsonConvert.SerializeObject(country);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Country/UpdateCountry/{country.CountryID}", content);
            }
            else
            {
                var jsonContent = JsonConvert.SerializeObject(country);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Country/InsertCountry", content);
            }

            return Json(response.IsSuccessStatusCode);
        }*/

        [HttpPost]
        public async Task<JsonResult> SaveCountry([FromBody] CountryModel country)
        {
            HttpResponseMessage response;
            string message;

            try
            {
                if (country.CountryID > 0)
                {
                    var jsonContent = JsonConvert.SerializeObject(country);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Country/UpdateCountry/{country.CountryID}", content);
                    message = response.IsSuccessStatusCode ? "Country updated successfully!" : $"Failed to update country. Response: {await response.Content.ReadAsStringAsync()}";
                }
                else
                {
                    var jsonContent = JsonConvert.SerializeObject(country);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Country/InsertCountry", content);
                    message = response.IsSuccessStatusCode ? "Country added successfully!" : $"Failed to add country. Response: {await response.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                message = $"An error occurred: {ex.Message}";
                return Json(new { success = false, message });
            }

            return Json(new { success = response.IsSuccessStatusCode, message });
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteCountry(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Country/DeleteCountry/{id}");
            return Json(response.IsSuccessStatusCode);
        }
        #endregion
    }
}
