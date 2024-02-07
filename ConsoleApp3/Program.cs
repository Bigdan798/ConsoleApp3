﻿using System;
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
            xmlDocument.Load("C:\\Users\\bogda\\source\\repos\\ConsoleApp3\\ConsoleApp3\\market.xml");

            // Парсинг XML и вставка данных в базу данных
            XmlNodeList nodes = xmlDocument.SelectNodes("Market/Order");
            foreach (XmlNode node in nodes)
            {
                int idBuy = int.Parse(node.SelectSingleNode("id_buy").InnerText);
                int idUser = int.Parse(node.SelectSingleNode("id_user").InnerText);
                int idProduct = int.Parse(node.SelectSingleNode("id_product").InnerText);
                int quantity = int.Parse(node.SelectSingleNode("quantity").InnerText);
                string dateBuyString = node.SelectSingleNode("date_buy").InnerText;
                DateTime dateBuy = DateTime.ParseExact(dateBuyString, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                int idOrder = int.Parse(node.SelectSingleNode("id_order").InnerText);

                // Выполнение SQL запроса на вставку данных в базу данных
                string insertQuery = $"INSERT INTO [Order] (id_buy, id_user, id_product, quantity, date_buy, id_order) " +
                                     $"VALUES ({idBuy}, {idUser}, {idProduct}, {quantity}, '{dateBuy.ToString("yyyy-MM-dd HH:mm:ss")}', {idOrder})";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine("Данные успешно загружены в базу данных.");
    }
}
