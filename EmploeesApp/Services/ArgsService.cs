using EmploeesApp.Interfaces;
using EmploeesApp.Models;
using System.Diagnostics;
using System.Globalization;

namespace EmploeesApp.Services
{
    class ArgsService
    {
        private readonly string[] args;
        private readonly IEmploeeService emploeeService;
        private readonly Dictionary<string, bool> gengers 
            = new Dictionary<string, bool> { { "male", false }, { "female", true } };

        public ArgsService(string[] args)
        {
            this.args = args;
            this.emploeeService = new EmploeesService();
        }

        /// <summary>
        /// Processes an args array and runs the selected task.
        /// </summary>
        /// <returns></returns>
        public async Task RunTask()
        {
            try
            {
                int taskNumber = GetTaskNumber();
                await emploeeService.EnsureCreateDatabaseAsync();

                switch (taskNumber)
                {
                    case 1:
                        await emploeeService.EnsureCreateEmploeeTableAsync();
                        break;
                    case 2:
                        Emploee emploee = GetEmploee();
                        await emploeeService.SaveEmploeeAsync(emploee);
                        break;
                    case 3:
                        await emploeeService.ShowDistinctEmploeeListAsync();
                        break;
                    case 4:
                        emploeeService.FillDbRandomAsync(1_000_000, false);
                        emploeeService.FillDbRandomAsync(100, true);
                        break;
                    case 5:
                        var sw = new Stopwatch();
                        sw.Start();
                        await emploeeService.ShowListOfMaleStartsWithF();
                        sw.Stop();
                        Console.WriteLine($"Total procedure lasted \t{sw.Elapsed} seconds");
                        break;
                    case 6:
                        await emploeeService.OptimizeDbAsync();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Checks first argument and, if successful, returns task number.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">If first argument is not correct</exception>
        private int GetTaskNumber()
        {
            int taskNumber;
            if (!int.TryParse(args[0], out taskNumber) || taskNumber < 1 || taskNumber > 6)
                throw new Exception("First parameter must be an integer (1-6)!");

            return taskNumber;
        }

        /// <summary>
        /// Checks arguments array and, if successful, returns Emploee object.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">If arguments array is not correct</exception>
        private Emploee GetEmploee()
        {
            DateTime birthDate;
            bool gender;
            if (CheckArgsCount() && CheckBirthDate(out birthDate) && CheckGender(out gender))
                return new Emploee(args[1], birthDate, gender);

            throw new Exception("Error creating Emploee object!");
        }

        /// <summary>
        /// Checks arguments count
        /// </summary>
        /// <returns></returns>
        private bool CheckArgsCount()
        {
            return args.Length == 4;
        }

        /// <summary>
        /// Checks third argument and, if successful, sets the output DateTime parameter.
        /// </summary>
        /// <param name="birthdate">Date of birth for Emploee object</param>
        /// <returns></returns>
        /// <exception cref="Exception">If third argument is not correct</exception>
        private bool CheckBirthDate(out DateTime birthdate)
        {
            bool result = DateTime.TryParseExact(args[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthdate);
            if (!result)
                throw new Exception("Please enter the birthdate using the format (yyyy-MM-dd)");
            return result;
        }

        /// <summary>
        /// Checks fourth argument and, if successful, sets the output boolean parameter.
        /// </summary>
        /// <param name="gender">Parameter of gender for Emploee object</param>
        /// <returns></returns>
        /// <exception cref="Exception">If fourth argument is not correct</exception>
        private bool CheckGender(out bool gender)
        {
            bool result = gengers.TryGetValue(args[3].ToLower(), out gender);
            if (!result)
                throw new Exception("Please enter an existing gender (Male or Female)");
            return result;
        }
    }
}
