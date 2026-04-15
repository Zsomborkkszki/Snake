using System;
using System.Collections.Generic;
using Snake.Models;
using MySql.Data.MySqlClient;

namespace Snake.Controllers
{
    internal class LeaderboardController
    {
        private const string ConnStr = "server=localhost;database=snake;uid=root;pwd=;";

        public static bool UserExists(string nev)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnStr))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(
                    "SELECT COUNT(*) FROM user WHERE Nev = @nev", connection);
                command.Parameters.AddWithValue("@nev", nev);
                return (long)command.ExecuteScalar() > 0;
            }
        }

        // Inserts new user, or updates score only if the new score is higher
        public static void SaveScore(string nev, int pontszam)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnStr))
            {
                connection.Open();
                if (UserExists(nev))
                {
                    MySqlCommand command = new MySqlCommand(
                        "UPDATE user SET Pontszam = @pontszam WHERE Nev = @nev AND Pontszam < @pontszam",
                        connection);
                    command.Parameters.AddWithValue("@nev", nev);
                    command.Parameters.AddWithValue("@pontszam", pontszam);
                    command.ExecuteNonQuery();
                }
                else
                {
                    MySqlCommand command = new MySqlCommand(
                        "INSERT INTO user (Nev, Pontszam) VALUES (@nev, @pontszam)",
                        connection);
                    command.Parameters.AddWithValue("@nev", nev);
                    command.Parameters.AddWithValue("@pontszam", pontszam);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<User> GetTopScores()
        {
            List<User> users = new List<User>();
            using (MySqlConnection connection = new MySqlConnection(ConnStr))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(
                    "SELECT * FROM user ORDER BY Pontszam DESC", connection);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    users.Add(new User(reader.GetInt32("Id"), reader.GetString("Nev"), reader.GetInt32("Pontszam")));
                reader.Close();
            }
            return users;
        }
    }
}