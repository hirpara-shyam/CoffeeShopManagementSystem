﻿using Microsoft.AspNetCore.Mvc;
using System.Data;
using CoffeeShopManagementSystem.Models;
using System.Data.SqlClient;

namespace CoffeeShopManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IConfiguration configuration;

        public DashboardController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = new Dashboard
            {
                Counts = new List<DashboardCounts>(),
                RecentOrders = new List<RecentOrder>(),
                RecentProducts = new List<RecentProduct>(),
                TopCustomers = new List<TopCustomer>(),
                TopSellingProducts = new List<TopSellingProduct>(),
                NavigationLinks = new List<QuickLinks>()
            };

            using (var connection = new SqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("usp_GetDashboardData", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            // Fetch counts
                            while (await reader.ReadAsync())
                            {
                                dashboardData.Counts.Add(new DashboardCounts
                                {
                                    Metric = reader["Metric"].ToString(),
                                    Value = Convert.ToInt32(reader["Value"])
                                });
                            }

                            // Fetch recent orders
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.RecentOrders.Add(new RecentOrder
                                    {
                                        OrderID = Convert.ToInt32(reader["OrderID"]),
                                        CustomerName = reader["CustomerName"].ToString(),
                                        OrderDate = Convert.ToDateTime(reader["OrderDate"])
                                    });
                                }
                            }

                            // Fetch recent products
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.RecentProducts.Add(new RecentProduct
                                    {
                                        ProductID = Convert.ToInt32(reader["ProductID"]),
                                        ProductName = reader["ProductName"].ToString(),
                                        ProductCode = reader["ProductCode"].ToString()
                                    });
                                }
                            }

                            // Fetch top customers
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.TopCustomers.Add(new TopCustomer
                                    {
                                        CustomerName = reader["CustomerName"].ToString(),
                                        TotalOrders = Convert.ToInt32(reader["TotalOrders"]),
                                        Email = reader["Email"].ToString()
                                    });
                                }
                            }

                            // Fetch top selling products
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.TopSellingProducts.Add(new TopSellingProduct
                                    {
                                        ProductName = reader["ProductName"].ToString(),
                                        TotalSoldQuantity = Convert.ToInt32(reader["TotalSoldQuantity"]),
                                        ProductCode = reader["ProductCode"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }

            dashboardData.NavigationLinks = new List<QuickLinks> {
                new QuickLinks {ActionMethodName = "Index", ControllerName="Home", LinkName="Dashboard" },
                new QuickLinks {ActionMethodName = "Index", ControllerName="Country", LinkName="Country" },
                new QuickLinks {ActionMethodName = "Index", ControllerName="State", LinkName="State" },
                new QuickLinks {ActionMethodName = "Index", ControllerName="City", LinkName="City" }
            };

            var model = new Dashboard
            {
                Counts = dashboardData.Counts,
                RecentOrders = dashboardData.RecentOrders,
                RecentProducts = dashboardData.RecentProducts,
                TopCustomers = dashboardData.TopCustomers,
                TopSellingProducts = dashboardData.TopSellingProducts,
                NavigationLinks = dashboardData.NavigationLinks
            };

            return View("Index", model);
        }
    }
}
