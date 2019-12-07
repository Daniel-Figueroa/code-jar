﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;


namespace CodeJar.WebApp
{
    public class SQL
    {
        public SQL(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
        }

        // SQL connection string
        public SqlConnection Connection { get; set; }

        /// <summary>
        /// Stores codes in the database
        /// </summary>
        /// <param name="code"></param>
        /// <param name="offset"></param>
        public void StoreRequestedCodes(long seedValue, long offset)
        {

            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = $@"INSERT INTO [6 Digit Code] (Seedvalue, State, DateActive, DateExpires) VALUES (@Seedvalue, @State, @DateActive, @DateExpires)";

                // Insert values
                command.Parameters.AddWithValue("@Seedvalue", seedValue);
                command.Parameters.AddWithValue("@State", "Active");
                command.Parameters.AddWithValue("@DateActive", DateTime.Now);
                command.Parameters.AddWithValue("@DateExpires", DateTime.Today.AddDays(8));

                command.ExecuteNonQuery();
            }

            Connection.Close();

            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = $@"INSERT INTO Offset OffsetValue VALUES @Seedvalue";

                // Insert offset
                command.Parameters.AddWithValue("@Seedvalue", offset);

                command.ExecuteNonQuery();
            }

            Connection.Close();
        }

        /// <summary>
        /// Gets the next seed value that will be used to generate codes
        /// </summary>
        /// <returns></returns>
        public long GetOffset()
        {
            long seedValue = 0;

            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Offset";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        seedValue = (long)reader["OffsetValue"];
                    }
                }
                command.ExecuteNonQuery();
            }

            Connection.Close();
            return seedValue;
        }

        /// <summary>
        /// Returns a list of all the codes from the database
        /// </summary>
        /// <returns></returns>
        public List<Code> GetCodes()
        {
            // Create list to store codes gathered from the database
            var codes = new List<Code>();

            Connection.Open();

            using(var command = Connection.CreateCommand())
            {
                // Select all codes from the database
                command.CommandText = "SELECT * FROM [6 Digit Code]";

                // Read all the rows
                using(var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        // Store code in a variable
                        var code = new Code()
                        {
                            ID = (int)reader["ID"],
                            SeedValue = (long)reader["SeedValue"],
                            StringValue = ConvertToCode(code.seedValue),
                            State = (string)reader["State"],
                            DateActive = (DateTime)reader["DateActive"],
                            DateExpires = (DateTime)reader["DateExpires"]
                        };

                        // Add code to the list
                        codes.Add(code);
                    }
                }
            }

            Connection.Close();

            // Return the list of codes
            return codes;
        }

        private static string ConvertToCode(long seedvalue)
        {
            public string alphabet { get; } = "2BCD3FGH4JKLMN5PQRST6VWXYZ";

            var result = EncodeToBaseString(seedvalue, alphabet);

            result = result.PadLeft(6, alphabet[0]);

            return result;
        }

        private static string EncodeToBaseString(long seedvalue, string alphabet)
        {
            var encBase = alphabet.Length;

            var digits = "";
            var num = seedvalue;

            if (num == 0)
                return alphabet[0].ToString();

            while (num > 0)
            {
                digits = alphabet[num % encBase] + digits;
                num = num / encBase;
            }

            return digits;
        }

        public void InactiveStatus(int codeID)
        {
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"UPDATE [6 Digit Code] SET [State] = 'Inactive' WHERE ID = @codeID";

                command.Parameters.AddWithValue("@codeID", codeID);

                command.ExecuteNonQuery();
            }

            Connection.Close();
        }

        public void RedeemedStatus(int codeID)
        {
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"UPDATE [6 Digit Code] SET [State] = 'Redeemed'
                                        WHERE ID = @codeID AND [State] = 'Active'";

                command.Parameters.AddWithValue("@codeID", codeID);

                command.ExecuteNonQuery();
            }

            Connection.Close();
        }
    }
}