using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace EmploeesApp.Models
{
    class Emploee
    {
        public string Fullname { get; set; }
        public DateTime Birthdate { get; set; }
        public bool Gender { get; set; }

        public Emploee(string fullname, DateTime birthdate, bool gender)
        {
            Fullname = fullname;
            Birthdate = birthdate;
            Gender = gender;
        }

        /// <summary>
        /// Saves the current Emploee object into database.
        /// </summary>
        /// <param name="connection">Object of database connection</param>
        /// <returns></returns>
        public async Task SaveToDbAsync(SqlConnection connection)
        {
            string query = "INSERT INTO Emploees (Fullname, Birthdate, Gender) VALUES (@fullname, @birthdate, @gender)";
            await connection.OpenAsync();

            SqlCommand command = new SqlCommand(query, connection);
            SqlParameter fullnameParam = new SqlParameter("@fullname", SqlDbType.NVarChar, 100);
            fullnameParam.Value = Fullname;
            SqlParameter birthdateParam = new SqlParameter("@birthdate", SqlDbType.Date);
            birthdateParam.Value = Birthdate;
            SqlParameter genderParam = new SqlParameter("@gender", SqlDbType.Bit);
            genderParam.Value = Gender;
            command.Parameters.AddRange(new[] { fullnameParam, birthdateParam, genderParam });

            int number = await command.ExecuteNonQueryAsync();
            Console.WriteLine($"{number} objects save to database");
        }

        /// <summary>
        /// Saves a list of Emploee objects into database.
        /// </summary>
        /// <param name="connection">Object of database connection</param>
        /// <param name="emploees">List of Emploee objects</param>
        public static void SaveRangeToDb(SqlConnection connection, List<Emploee> emploees)
        {
            string query = "SELECT TOP 1 * FROM Emploees";

            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            DataTable dataTable = ds.Tables[0];
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.UpdateBatchSize = 10_000;
            int counter = 0;

            foreach (Emploee emp in emploees)
            {
                DataRow newRow = dataTable.NewRow();
                newRow["Fullname"] = emp.Fullname;
                newRow["Birthdate"] = emp.Birthdate;
                newRow["Gender"] = emp.Gender;
                dataTable.Rows.Add(newRow);

                counter++;
                if (counter >= 10_000)
                {
                    adapter.Update(ds);
                    ds.Clear();
                    adapter.Fill(ds);
                    counter = 0;
                }
            }            
            adapter.Update(ds);
        }

        /// <summary>
        /// Returns age of the current Emploee object.
        /// </summary>
        /// <returns></returns>
        public int GetAge()
        {
            DateTime today = DateTime.Today;
            int age = today.Year - Birthdate.Year;
            if (today.Month < Birthdate.Month || (today.Month == Birthdate.Month && today.Day < Birthdate.Day))
                age--;

            return age;
        }

        /// <summary>
        /// Returns a string that represents the current Emploee object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string genderString = Gender ? "Female" : "Male";
            return $"{Fullname, 30}{Birthdate.ToShortDateString(), 15}{genderString, 10}{GetAge(), 5}";
        }
    }
}
