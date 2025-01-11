using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using CoffeeShopManagementSystem.Models;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using CoffeeShopManagementSystem.Helper;

namespace CoffeeShopManagementSystem.Controllers
{
    [CheckAccess]
    public class CityController : Controller
    {
        #region configuration
        private readonly IConfiguration _configuration;

        Uri baseAddress = new Uri("https://localhost:7273/api");
        private readonly HttpClient _httpClient;

        public CityController(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }
        #endregion

        /*#region GetStatesByCountry
        // AJAX handler for loading states dynamically
        [HttpPost]
        public JsonResult GetStatesByCountry(int CountryID)
        {
            List<StateDropDownModel> loc_State = GetStateByCountryID(CountryID); // Fetch states
            return Json(loc_State); // Return JSON response
        }
        #endregion*/

        /*#region GetStateByCountryID
        // Helper method to fetch states by country ID
        public List<StateDropDownModel> GetStateByCountryID(int CountryID)
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            List<StateDropDownModel> loc_State = new List<StateDropDownModel>();

            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                using (SqlCommand objCmd = conn.CreateCommand())
                {
                    objCmd.CommandType = CommandType.StoredProcedure;
                    objCmd.CommandText = "PR_LOC_State_SelectComboBoxByCountryID";
                    objCmd.Parameters.AddWithValue("@CountryID", CountryID);

                    using (SqlDataReader objSDR = objCmd.ExecuteReader())
                    {
                        if (objSDR.HasRows)
                        {
                            while (objSDR.Read())
                            {
                                loc_State.Add(new StateDropDownModel
                                {
                                    StateID = Convert.ToInt32(objSDR["StateID"]),
                                    StateName = objSDR["StateName"].ToString()
                                });
                            }
                        }
                    }
                }
            }

            return loc_State;
        }
        #endregion*/

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index(int? StateID)
        {
            List<CityModel> cities = new List<CityModel>();
            List<CityModel> newCities = new List<CityModel>();

            try
            {
                // Make the HTTP GET request
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/City/GetAllCities");

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string data = await response.Content.ReadAsStringAsync();

                    // Directly deserialize the JSON array into a List<CityModel>
                    newCities = JsonConvert.DeserializeObject<List<CityModel>>(data);
                    if (StateID.HasValue)
                    {
                        foreach (var item in newCities)
                        {
                            if (item.StateID == StateID)
                            {
                                cities.Add(item);
                            }
                        }
                    }
                    else
                    {
                        cities = newCities;
                    }
                }
                else
                {
                    // Handle unsuccessful responses
                    Console.WriteLine($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                Console.WriteLine($"Exception occurred: {ex.Message}");
            }
            return View("Index", cities);
        }
        #endregion

        #region AddEdit Form
        public async Task CountryDropDownMethod()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetCountryDropDown");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var countries = JsonConvert.DeserializeObject<List<CountryDropDownModel>>(data);
                ViewBag.CountryList = countries;
            }
        }

        public async Task<IActionResult> Form(string? CityID)
        {
            int? decryptedCityID = null;

            // Decrypt only if CityID is not null or empty
            if (!string.IsNullOrEmpty(CityID))
            {
                string decryptedCityIDString = UrlEncryptor.Decrypt(CityID); // Decrypt the encrypted CityID
                decryptedCityID = int.Parse(decryptedCityIDString); // Convert decrypted string to integer
            }

            if (decryptedCityID <= 0 || decryptedCityID == null)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            //await StateDropDownMethod();
            await CountryDropDownMethod();

            if (decryptedCityID.HasValue)
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/City/GetCityByID/{decryptedCityID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var city = JsonConvert.DeserializeObject<CityModel>(data);
                    ViewBag.StateList = await GetStatesByCountryID(city.CountryID);
                    return View(city);
                }
            }
            return View(new CityModel());
        }

        #endregion

        #region CitySave
        [HttpPost]
        public async Task<IActionResult> CitySave(CityModel cityModel)
        {
            if (cityModel.StateID <= 0)
            {
                ModelState.AddModelError("StateID", "A valid State is required.");
            }
            if (cityModel.CountryID <= 0)
            {
                ModelState.AddModelError("CountryID", "A valid Country is required.");
            }

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(cityModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (cityModel.CityID == null || cityModel.CityID == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/City/InsertCity", content);
                    TempData["AlertMessage"] = "City Inserted Successfully";
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/City/UpdateCity/{cityModel.CityID}", content);
                    TempData["AlertMessage"] = "City Updated Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            await CountryDropDownMethod();
            //await StateDropDownMethod();

            return View("Form", cityModel);
        }
        #endregion

        #region Delete
        public IActionResult Delete(string CityID)
        {
            int decryptedCityID = Convert.ToInt32(UrlEncryptor.Decrypt(CityID.ToString()));

            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/City/DeleteCity/{decryptedCityID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "City Deleted Successfully";
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region Get State By Country(Drop Down)
        [HttpPost]
        public async Task<JsonResult> GetStatesByCountry(int CountryID)
        {
            var states = await GetStatesByCountryID(CountryID);
            return Json(states);
        }

        private async Task<List<StateDropDownModel>> GetStatesByCountryID(int CountryID)
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/City/GetStatesByCountryID/states/{CountryID}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<StateDropDownModel>>(data);
            }
            return new List<StateDropDownModel>();
        }
        #endregion

        /*#region Get City BY State
        public async Task<IActionResult> CityByState(int? StateID)
        {
            List<CityModel> cities = new List<CityModel>();
            List<CityModel> newCities = new List<CityModel>();

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/City/GetAllCities");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    newCities = JsonConvert.DeserializeObject<List<CityModel>>(data);
                    if (StateID.HasValue)
                    {
                        foreach (var item in newCities)
                        {
                            if (item.StateID == StateID)
                            {
                                cities.Add(item);
                            }
                        }
                    }
                    else
                    {
                        cities = newCities;
                    }
                }
                else
                {
                    Console.WriteLine($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
            }
            return View("GetAllCity", cities);
        }
        #endregion*/
    }
}
