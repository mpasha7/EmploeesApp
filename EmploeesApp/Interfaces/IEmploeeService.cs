using EmploeesApp.Models;

namespace EmploeesApp.Interfaces
{
    interface IEmploeeService
    {
        /// <summary>
        /// Ensures database creation.
        /// </summary>
        /// <returns></returns>
        Task EnsureCreateDatabaseAsync();

        /// <summary>
        /// TASK 1
        /// Ensures Emploees table creation.
        /// </summary>
        /// <returns></returns>
        Task EnsureCreateEmploeeTableAsync();

        /// <summary>
        /// TASK 2
        /// Creates an Employee object and saves it to the database.
        /// </summary>
        /// <param name="emploee"></param>
        /// <returns></returns>
        Task SaveEmploeeAsync(Emploee emploee);

        /// <summary>
        /// TASK 3
        /// Shows a list of employees with a unique value of Fullname + Birthdate
        /// </summary>
        /// <returns></returns>
        Task ShowDistinctEmploeeListAsync();

        /// <summary>
        /// TASK 4
        /// Fills 1,000,000 random emploees into the database or fills 100 emploees (male and fullname starts with 'F') into the database.
        /// </summary>
        /// <param name="size">Count of objects created</param>
        /// <param name="isMaleStartsWithF">If emploees is males and fullnames starts with 'F'</param>
        void FillDbRandomAsync(int size, bool isMaleAndBeginFromF);

        /// <summary>
        /// TASK 5
        /// Shows a list of emploees where gender is male and fullname starts with 'F'.
        /// </summary>
        Task ShowListOfMaleStartsWithF();

        /// <summary>
        /// TASK 6
        /// Optimizes the output of database rows according to task 5.
        /// </summary>
        /// <returns></returns>
        Task OptimizeDbAsync();
    }
}
