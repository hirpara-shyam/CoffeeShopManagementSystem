using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using CoffeeShopManagementSystem.Models;
using Newtonsoft.Json;
using System.Text;
using Mono.TextTemplating;

namespace CoffeeShopManagementSystem.Controllers
{
    [CheckAccess]
    public class StateController : Controller
    {
        #region configuration
        private readonly IConfiguration _configuration;

        Uri baseAddress = new Uri("https://localhost:7273/api");
        private readonly HttpClient _httpClient;

        public StateController(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }
        #endregion

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index(int? CountryID)
        {
            List<StateModel> states = new List<StateModel>();
            List<StateModel> newStates = new List<StateModel>();

            try
            {
                // Make the HTTP GET request
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/State/GetAllStates");

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string data = await response.Content.ReadAsStringAsync();

                    // Directly deserialize the JSON array into a List<CityModel>
                    newStates = JsonConvert.DeserializeObject<List<StateModel>>(data);
                    if (CountryID.HasValue)
                    {
                        foreach (var item in newStates)
                        {
                            if (item.CountryID == CountryID)
                            {
                                states.Add(item);
                            }
                        }
                    }
                    else
                    {
                        states = newStates;
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

            return View("Index", states);
        }
        #endregion

        #region AddEdit Form
        public async Task CountryDropDownMethod()
        {
            /*string connectionString = this._configuration.GetConnectionString("ConnectionString");
            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_LOC_Country_SelectComboBox";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<CountryDropDownModel> countryList = new List<CountryDropDownModel>();
            foreach (DataRow data in dataTable1.Rows)
            {
                CountryDropDownModel countryDropDownModel = new CountryDropDownModel();
                countryDropDownModel.CountryID = Convert.ToInt32(data["CountryID"]);
                countryDropDownModel.CountryName = data["CountryName"].ToString();
                countryList.Add(countryDropDownModel);
            }
            ViewBag.CountryList = countryList;*/

            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/DropDown/GetCountryDropDown");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var countries = JsonConvert.DeserializeObject<List<CountryDropDownModel>>(data);
                ViewBag.CountryList = countries;
            }
        }

        public async Task<IActionResult> Form(int? StateID)
        {
            if (StateID <= 0 || StateID == null)
            {
                ViewBag.AddOrEdit = "Add";
            }
            else
            {
                ViewBag.AddOrEdit = "Edit";
            }

            await CountryDropDownMethod();

            if (StateID.HasValue)
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/State/GetStateByID/{StateID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var state = JsonConvert.DeserializeObject<StateModel>(data);
                    return View(state);
                }
            }
            return View(new StateModel());

            /*string connectionString = _configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_LOC_State_SelectByPK";
            command.Parameters.AddWithValue("@StateID", StateID);
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            StateModel stateModel = new StateModel();

            foreach (DataRow dataRow in table.Rows)
            {
                stateModel.StateID = Convert.ToInt32(@dataRow["StateID"]);
                stateModel.StateName = @dataRow["StateName"].ToString();
                stateModel.CountryID = Convert.ToInt32(@dataRow["CountryID"]);
                stateModel.StateCode = @dataRow["StateCode"].ToString();
            }
            return View("Form", stateModel);*/
        }
        #endregion

        #region StateSave
        [HttpPost]
        public async Task<IActionResult> StateSave(StateModel stateModel)
        {
            if (stateModel.CountryID <= 0)
            {
                ModelState.AddModelError("CountryID", "A valid Country is required.");
            }

            if (ModelState.IsValid)
			{
				var json = JsonConvert.SerializeObject(stateModel);
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				HttpResponseMessage response;

				if (stateModel.StateID == null || stateModel.StateID == 0)
				{
					response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/State/InsertState", content);
					TempData["SaveMessage"] = "State Insert Successfully";
				}
				else
				{
					response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/State/UpdateState/{stateModel.StateID}", content);
					TempData["SaveMessage"] = "State Update Successfully";
				}
				if (response.IsSuccessStatusCode)
				{
					return RedirectToAction("Index");
				}
			}
			await CountryDropDownMethod();
			return View("Form", stateModel);

            /*if (ModelState.IsValid)
            {
                string connectionString = this._configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (stateModel.StateID == null)
                {
                    command.CommandText = "PR_LOC_State_Insert";
                }
                else
                {
                    command.CommandText = "PR_LOC_State_Update";
                    command.Parameters.Add("@StateID", SqlDbType.Int).Value = stateModel.StateID;
                }
                command.Parameters.Add("@StateName", SqlDbType.VarChar).Value = stateModel.StateName;
                command.Parameters.Add("@StateCode", SqlDbType.VarChar).Value = stateModel.StateCode;
                command.Parameters.Add("@CountryID", SqlDbType.Int).Value = stateModel.CountryID;
                command.ExecuteNonQuery();
                return RedirectToAction("Index");
            }

            CountryDropDownMethod();
            return View("Form", stateModel);*/
        }
        #endregion

        #region Delete
        [HttpGet]
        public IActionResult Delete(int StateID)
        {
            /*string connectionstr = _configuration.GetConnectionString("ConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                using (SqlCommand sqlCommand = conn.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_LOC_State_Delete";
                    sqlCommand.Parameters.AddWithValue("@StateID", StateID);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");*/

            HttpResponseMessage response = _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/State/DeleteState/{StateID}").Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "State Deleted Successfully";
            }
            return RedirectToAction("Index");
        }
        #endregion
    }
}
