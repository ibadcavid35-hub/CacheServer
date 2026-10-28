using CacheServer.Models;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;

namespace CacheClient
{
    internal class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var operations = new List<string>
                { "Add", "Update", "GetAll", "GetById", "Delete" };

            while (true)
            {
                int select = Menu.Menu.ManageMenu(operations, "MENU");
                if (select == -1) return;

                try
                {
                    switch (select)
                    {

                        case 0:
                            {
                                Console.Clear();
                                Console.WriteLine("ADD PRODUCT");

                                var p = new Product();

                                Console.Write("Enter Name: ");
                                p.Name = Console.ReadLine() ?? "";

                                Console.Write("Enter Price: ");
                                decimal.TryParse(Console.ReadLine(),out decimal price);
                                   
                                p.Price = price;

                                Console.Write("Enter Stock: ");
                                int.TryParse(Console.ReadLine(), out int stock);
                                p.Stock = stock;

                                var response = await client.PostAsJsonAsync("products", p);

                                Console.WriteLine($"Status: {(int)response.StatusCode}");
                                Console.WriteLine(await response.Content.ReadAsStringAsync());
                                break;
                            }

                        case 1:
                            {
                                Console.Clear();
                                Console.WriteLine("UPDATE PRODUCT");

                                var p = new Product();

                                Console.Write("Enter ID: ");
                                int.TryParse(Console.ReadLine(), out int id);
                                p.Id = id;

                                Console.Write("Enter Name: ");
                                p.Name = Console.ReadLine() ?? "";

                                Console.Write("Enter Price: ");
                                decimal.TryParse(Console.ReadLine(), out decimal price);
                                p.Price = price;

                                Console.Write("Enter Stock: ");
                                int.TryParse(Console.ReadLine(), out int stock);
                                p.Stock = stock;

                                var response = await client.PutAsJsonAsync("products", p);

                                Console.WriteLine($"Status: {(int)response.StatusCode}");
                                Console.WriteLine(await response.Content.ReadAsStringAsync());
                                break;
                            }


                        case 2:
                            {
                                var response = await client.GetAsync("products");

                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                                    break;
                                }

                                var products = await response.Content.ReadFromJsonAsync<List<Product>>();

                                if (products == null || products.Count == 0)
                                    Console.WriteLine("No products found.");
                                else
                                    foreach (var p in products)
                                        Console.WriteLine($"ID: {p.Id} | Name: {p.Name} | Price: {p.Price}$ | Stock: {p.Stock}");

                                break;
                            }

                        case 3:
                            {
                                Console.Write("Enter ID: ");
                                int.TryParse(Console.ReadLine(), out int id);

                                var response = await client.GetAsync($"products/{id}");
                                Console.WriteLine(await response.Content.ReadAsStringAsync());
                                break;
                            }

                        case 4:
                            {
                                Console.Write("Enter ID to Delete: ");
                                int.TryParse(Console.ReadLine(), out int id);

                                var response = await client.DeleteAsync($"products/{id}");
                                Console.WriteLine($"Status: {(int)response.StatusCode}");
                                Console.WriteLine(await response.Content.ReadAsStringAsync());
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }

                Console.ReadKey();
            }
        }
    }
}
