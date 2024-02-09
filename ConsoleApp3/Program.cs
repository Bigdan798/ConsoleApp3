using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;

class Program
{
    static void Main()
    {
        // Подключение к базе данных
        string connectionString = "Data Source=DESKTOP-EB7UKNL;Initial Catalog=Market;Integrated Security=True;Connect Timeout=30";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Чтение XML файла
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("C:\\Users\\bogda\\source\\repos\\ConsoleApp3\\ConsoleApp3\\xml+2.xml");

            // Парсинг XML и вставка данных в базу данных
            XmlNodeList orderNodes = xmlDocument.SelectNodes("orders/order");
            HashSet<int> processedOrders = new HashSet<int>(); // Хранит номера заказов, которые уже были обработаны

            foreach (XmlNode orderNode in orderNodes)
            {
                int orderNo = int.Parse(orderNode.SelectSingleNode("no").InnerText);
                string regDateStr = orderNode.SelectSingleNode("reg_date").InnerText;
                DateTime regDate = DateTime.ParseExact(regDateStr, "yyyy.MM.dd", CultureInfo.InvariantCulture);

                XmlNodeList productNodes = orderNode.SelectNodes("product");
                foreach (XmlNode productNode in productNodes)
                {
                    int quantity = int.Parse(productNode.SelectSingleNode("quantity").InnerText);
                    string name = productNode.SelectSingleNode("name").InnerText;
                    decimal price = decimal.Parse(productNode.SelectSingleNode("price").InnerText, CultureInfo.InvariantCulture);

                    // Поиск узла пользователя для каждого заказа
                    XmlNode userNode = orderNode.SelectSingleNode("user");
                    string fio = userNode.SelectSingleNode("fio").InnerText;
                    string email = userNode.SelectSingleNode("email").InnerText;
                    // Выполнение SQL запроса на вставку данных в базу данных
                    int? userId = GetUserIdFromDatabase(connection, fio, email);
                    int? productId = GetProductIdFromDatabase(connection, name, price);

                    if (userId != null && productId != null)
                    {
                        string insertQuery = "INSERT INTO [Order] (RegDate, Sum, UserId, ProductId, OrderNo, Quantity) " +
                            "VALUES (@RegDate, @Sum, @UserId, @ProductId, @OrderNo, @Quantity)";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@RegDate", regDate);
                            command.Parameters.AddWithValue("@Sum", price);
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@ProductId", productId);
                            command.Parameters.AddWithValue("@OrderNo", orderNo);
                            command.Parameters.AddWithValue("@Quantity", quantity);

                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Пользователь или продукт не найдены. Пропуск заказа под номером: {orderNo}");
                        break;
                    }
                }
                // Добавление обработанного заказа в список
                processedOrders.Add(orderNo);
            }
        }

        Console.WriteLine("Данные успешно загружены в базу данных.");
    }

    static int? GetUserIdFromDatabase(SqlConnection connection, string fio, string email)
    {
        int? userId = null;
        string query = $"SELECT [id_user] FROM Users WHERE FIO = '{fio}'";

        using (SqlCommand command = new SqlCommand(query, connection))
        {

            object result = command.ExecuteScalar();

            if (result != null)
            {
                userId = Convert.ToInt32(result);
            }

            else 
            {
                string insertUserQuery = $"INSERT INTO Users (FIO, Email) VALUES ('{fio}', '{email}'); SELECT SCOPE_IDENTITY();";


                using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                {
                    userId = Convert.ToInt32(insertUserCommand.ExecuteScalar());
                }
            }
        }


        return userId;
    }

    static int? GetProductIdFromDatabase(SqlConnection connection, string productName, decimal price)
    {
        int? productId = null;
        string query = $"SELECT [id_product] FROM Products WHERE [NameProd] = '{productName}'";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            object result = command.ExecuteScalar();

            if (result != null)
            {
                productId = Convert.ToInt32(result);
            }
        }

        if (productId == null)
        {
            string insertProductQuery = "INSERT INTO Products ([NameProd], [Price]) VALUES (@ProductName, @Price); SELECT SCOPE_IDENTITY();";
            using (SqlCommand insertProductCommand = new SqlCommand(insertProductQuery, connection))
            {
                insertProductCommand.Parameters.AddWithValue("@ProductName", productName);
                insertProductCommand.Parameters.AddWithValue("@Price", price);
                productId = Convert.ToInt32(insertProductCommand.ExecuteScalar());
            }
        }

        return productId;
    }
}