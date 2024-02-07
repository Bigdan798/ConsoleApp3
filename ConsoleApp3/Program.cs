using System;
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

                    // Выполнение SQL запроса на вставку данных в базу данных
                    int userId = GetUserIdFromDatabase(connection, fio);
                    int productId = GetProductIdFromDatabase(connection, name);

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
            }
        }

        Console.WriteLine("Данные успешно загружены в базу данных.");
    }

    static int GetUserIdFromDatabase(SqlConnection connection, string fio)
    {
        // Выполнение SQL запроса для получения значения пользователя
        string query = $"SELECT [id_user] FROM Users WHERE FIO = '{fio}'";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            int UserId = Convert.ToInt32(command.ExecuteScalar());
            return UserId;
        }
    }

    static int GetProductIdFromDatabase(SqlConnection connection, string productName)
    {
        // Выполнение SQL запроса для получения значения продукта
        string query = $"SELECT id_product FROM Products WHERE NameProd = '{productName}'";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            int ProductId = (int)command.ExecuteScalar();
            return ProductId;
        }
    }
}