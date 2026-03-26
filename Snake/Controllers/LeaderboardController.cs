using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snake.Models;
using MySql.Data.MySqlClient;

namespace Snake.Controllers
{
    internal class LeaderboardController
    {
        public static int UserCheck(string nev)
        {
            MySqlConnection connection = new MySqlConnection("server=localhost;database=snake;uid=root;pwd=;");
            connection.Open();
            MySqlCommand command = new MySqlCommand("SELECT * FROM users", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString("nev") == nev)
                {
                    reader.Close();
                    connection.Close();
                    return -1;
                }
                else
                {
                    reader.Close();
                    connection.Close();
                    return 1;
                }
            }
            reader.Close();
            connection.Close();
            return 0; // Default return value if no rows are read
        }
        public static void AddUser(string nev, int pontszam)
        {
            if (UserCheck(nev) == -1)
            {
                return;
            }
            else if (UserCheck(nev) == 1)
            {
                MySqlConnection connection = new MySqlConnection("server=localhost;database=snake;uid=root;pwd=;");
                connection.Open();
                MySqlCommand command = new MySqlCommand("INSERT INTO users (id, nev, pontszam) VALUES (NULL, @nev, @pontszam)", connection);
                command.Parameters.AddWithValue("@nev", nev);
                command.Parameters.AddWithValue("@pontszam", pontszam);
                command.ExecuteNonQuery();
                connection.Close();
            }
            else
            {
                return;
            }

        }
        public static List<User> UsersToListByPoints()
        {
            MySqlConnection connection = new MySqlConnection("server=localhost;database=snake;uid=root;pwd=;");
            List<User> users = new List<User>();
            connection.Open();
            MySqlCommand command = new MySqlCommand("SELECT * FROM users ORDER BY pontszam DESC", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) 
            {
                users.Add(new User(reader.GetInt32("id"), reader.GetString("nev"), reader.GetInt32("pontszam")));
            }
            reader.Close();
            connection.Close();
            return users;
        }
    }
}
