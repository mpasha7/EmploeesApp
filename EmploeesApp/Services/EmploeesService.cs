using EmploeesApp.Interfaces;
using EmploeesApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace EmploeesApp.Services
{
    class EmploeesService : IEmploeeService
    {
        private readonly IConfigurationRoot configuration;

        public EmploeesService()
        {
            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }

        /// <summary>
        /// Ensures database creation.
        /// </summary>
        /// <returns></returns>
        public async Task EnsureCreateDatabaseAsync()
        {
            string connectionString = GetConnectionString("createConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EmploeeDB') CREATE DATABASE EmploeeDB;";
                    SqlCommand command = new SqlCommand(query, connection);
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("Database is ready to work!");
                    await command.DisposeAsync();
                }
                catch (Exception)
                {
                    Console.WriteLine("Database connection error!");
                    throw;
                }
            }
        }

        /// <summary>
        /// TASK 1
        /// Ensures Emploees table creation.
        /// </summary>
        /// <returns></returns>
        public async Task EnsureCreateEmploeeTableAsync()
        {
            string connectionString = GetConnectionString("workConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = $"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Emploees') AND type in (N'U'))"
                        + $" CREATE TABLE Emploees (Fullname NVARCHAR(100) NOT NULL, Birthdate DATE NOT NULL, Gender BIT NOT NULL);";
                    SqlCommand command = new SqlCommand(query, connection);
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("Emploee table is ready to work!");
                    await command.DisposeAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// TASK 2
        /// Creates an Employee object and saves it to the database.
        /// </summary>
        /// <param name="emploee"></param>
        /// <returns></returns>
        public async Task SaveEmploeeAsync(Emploee emploee)
        {
            string connectionString = GetConnectionString("workConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await emploee.SaveToDbAsync(connection);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// TASK 3
        /// Shows a list of employees with a unique value of Fullname + Birthdate
        /// </summary>
        /// <returns></returns>
        public async Task ShowDistinctEmploeeListAsync()
        {
            string connectionString = GetConnectionString("workConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = 
                        "SELECT sub.Fullname, sub.Birthdate, sub.Gender FROM " +
                           "(SELECT Fullname, Birthdate, Gender, " +
                               "ROW_NUMBER() OVER (PARTITION BY Fullname, Birthdate ORDER BY Fullname) AS row_num " +
                           "FROM Emploees) AS sub " +
                        "WHERE row_num = 1 ORDER BY sub.Fullname;";
                    SqlCommand command = new SqlCommand(query, connection);
                    var reader =  await command.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        string column1 = reader.GetName(0);
                        string column2 = reader.GetName(1);
                        string column3 = reader.GetName(2);
                        string column4 = "Age";
                        Console.WriteLine($"{column1, 30}{column2, 15}{column3, 10}{column4, 5}");
                        Console.WriteLine(new string('-', 60));

                        while (await reader.ReadAsync())
                        {
                            Emploee emploee = new Emploee(reader.GetString(0), reader.GetDateTime(1), reader.GetBoolean(2));
                            Console.WriteLine(emploee);
                        }
                    }
                    await command.DisposeAsync();
                    await reader.DisposeAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// TASK 4
        /// Fills 1,000,000 random emploees into the database or fills 100 emploees (male and fullname starts with 'F') into the database.
        /// </summary>
        /// <param name="size">Count of objects created</param>
        /// <param name="isMaleStartsWithF">If emploees is males and fullnames starts with 'F'</param>
        public void FillDbRandomAsync(int size, bool isMaleStartsWithF)
        {
            string connectionString = GetConnectionString("workConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    List<Emploee> emploees = new List<Emploee>();
                    for (int i = 0; i < size; i++)
                    {
                        if (isMaleStartsWithF)
                            emploees.Add(GetRandomMaleStartsWithF());
                        else
                            emploees.Add(GetRandomEmploee());
                    }
                    Emploee.SaveRangeToDb(connection, emploees);
                    Console.WriteLine($"Inserted {size} Emploee objects");
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }        

        /// <summary>
        /// TASK 5
        /// Shows a list of emploees where gender is male and fullname starts with 'F'.
        /// </summary>
        /// <returns></returns>
        public async Task ShowListOfMaleStartsWithF()
        {
            string connectionString = GetConnectionString("workConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT Fullname, Birthdate, Gender FROM Emploees WHERE Fullname LIKE 'F%' AND Gender = 0;";

                    var sw = Stopwatch.StartNew();
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, connection);
                    sw.Stop(); var res0 = sw.Elapsed; sw.Restart();

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    sw.Stop();
                    var res1 = sw.Elapsed;
                    sw.Restart();

                    if (reader.HasRows)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(reader.GetName(0));
                        builder.Append(reader.GetName(1));
                        builder.Append(reader.GetName(2));
                        builder.AppendLine("Age");
                        builder.AppendLine(new string('-', 60));

                        int counter = 0;
                        while (await reader.ReadAsync())
                        {
                            Emploee emploee = new Emploee(reader.GetString(0), reader.GetDateTime(1), reader.GetBoolean(2));
                            builder.AppendLine(emploee.ToString());
                            counter++;                            
                        }
                        sw.Stop();
                        var res2 = sw.Elapsed;
                        sw.Restart();

                        string result = builder.ToString();
                        sw.Stop();
                        var res3 = sw.Elapsed;
                        sw.Restart();

                        Console.WriteLine(result);
                        sw.Stop();
                        var res4 = sw.Elapsed;

                        Console.WriteLine($"{counter} Emploee objects received");
                        Console.WriteLine($"Connect opening lasted \t{res0} seconds");
                        Console.WriteLine($"Query execution lasted \t{res1} seconds");
                        Console.WriteLine($"Strings picking lasted \t{res2} seconds");
                        Console.WriteLine($"Table build lasted \t{res3} seconds");
                        Console.WriteLine($"Table show lasted \t{res4} seconds");
                    }
                    await command.DisposeAsync();
                    await reader.DisposeAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// TASK 6
        /// Optimizes the output of database rows according to task 5.
        /// </summary>
        /// <returns></returns>
        public async Task OptimizeDbAsync()
        {
            string connectionString = GetConnectionString("workConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "CREATE UNIQUE CLUSTERED INDEX IX_Fullname_Gender ON Emploees(Fullname, Gender);";

                    SqlCommand command = new SqlCommand(query, connection);
                    await command.ExecuteNonQueryAsync();

                    Console.WriteLine("Database optimized");
                    await command.DisposeAsync();
                }
                catch (Exception)
                {
                    Console.WriteLine("Database has already optimized before");
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets due connection string.
        /// </summary>
        /// <param name="stringKey"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string GetConnectionString(string stringKey)
        {
            string? connectionString = configuration.GetConnectionString(stringKey);
            if (connectionString == null)
                throw new Exception("Connection string missing!");
            return connectionString;
        }

        /// <summary>
        /// Gets random Emploee object.
        /// </summary>
        /// <returns></returns>
        private Emploee GetRandomEmploee()
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            return new Emploee(GetRandomFullname(rnd, false), GetRandomBirthDate(rnd), GetRandomGender(rnd));
        }

        /// <summary>
        /// Gets random Emploee object where gender is male and fullname starts with 'F'.
        /// </summary>
        /// <returns></returns>
        private Emploee GetRandomMaleStartsWithF()
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            return new Emploee(GetRandomFullname(rnd, true), GetRandomBirthDate(rnd), false);
        }

        /// <summary>
        /// Gets random fullname.
        /// </summary>
        /// <param name="rnd">Object of Random class</param>
        /// <param name="isMaleStartsWithF">If fullname starts with 'F'</param>
        /// <returns></returns>
        private string GetRandomFullname(Random rnd, bool isMaleStartsWithF)
        {            
            StringBuilder stringBuilder = new StringBuilder();
            if (isMaleStartsWithF)
                stringBuilder.Append('F');
            for (int j = 0; j < 3; j++)
            {
                if (!isMaleStartsWithF || j > 0)
                    stringBuilder.Append((char)rnd.Next(65, 91));
                for (int i = 0; i < 5; i++)
                {
                    stringBuilder.Append((char)rnd.Next(97, 123));
                }
                if (j < 2)
                    stringBuilder.Append(" ");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets random birthdate.
        /// </summary>
        /// <param name="rnd">Object of Random class</param>
        /// <returns></returns>
        private DateTime GetRandomBirthDate(Random rnd)
        {
            return new DateTime(rnd.Next(1980, 2006), rnd.Next(1, 13), rnd.Next(1, 29));
        }

        /// <summary>
        /// Gets random parameter of gender.
        /// </summary>
        /// <param name="rnd">Object of Random class</param>
        /// <returns></returns>
        private bool GetRandomGender(Random rnd)
        {
            return rnd.Next() % 2 == 0;
        }
    }
}
