namespace SCommon.Database
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class MSSQL : IDisposable
    {
        #region Private Properties and Fields

        /// <summary>
        /// The <see cref="SqlConnection"/> handle
        /// </summary>
        private SqlConnection m_Connection;

        #endregion

        #region Constructors & Destructors

        public MSSQL(string connectionStr)
        {
            m_Connection = new SqlConnection(connectionStr);
            m_Connection.Open();
        }

        public MSSQL(string Server, string DBName, string UserID, string Password, bool MultipleActiveResultSets)
            : this(String.Format("Server={0};Database={1};User Id={2};Password={3};MultipleActiveResultSets={4}", Server, DBName, UserID, Password, MultipleActiveResultSets))
        {

        }

        public void Dispose()
        {
            m_Connection.Close();
            m_Connection.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="SQLCommand">The query string.</param>
        /// <param name="args">The args.</param>
        /// <returns>The number of affected rows by query execution.</returns>
        public int ExecuteCommand(string SQLCommand, params object[] args)
        {
            SQLCommand = ReplaceArgs(SQLCommand, args);
            using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the query as async.
        /// </summary>
        /// <param name="SQLCommand">The query string.</param>
        /// <param name="args">The args.</param>
        /// <returns>The number of affected rows by query execution.</returns>
        public Task<int> ExecuteCommandAsync(string SQLCommand, params object[] args)
        {
            SQLCommand = ReplaceArgs(SQLCommand, args);
            using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                return cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes query and returns the query result reader
        /// </summary>
        /// <param name="SQLCommand">The query string.</param>
        /// <param name="args">The args.</param>
        /// <returns><see cref="SqlDataReader"/> returned by the query execution.</returns>
        public SqlDataReader ExecuteReader(string SQLCommand, params object[] args)
        {
            try
            {
                SQLCommand = ReplaceArgs(SQLCommand, args);
                using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                    return cmd.ExecuteReader();
            }
            catch { return null; }
        }

        /// <summary>
        /// Executes query as async and returns the query result reader
        /// </summary>
        /// <param name="SQLCommand">The query string.</param>
        /// <param name="args">The args.</param>
        /// <returns><see cref="SqlDataReader"/> returned by the query execution.</returns>
        public Task<SqlDataReader> ExecuteReaderAsync(string SQLCommand, params object[] args)
        {
            try
            {
                SQLCommand = ReplaceArgs(SQLCommand, args);
                using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                    return cmd.ExecuteReaderAsync();
            }
            catch { return null; }
        }

        /// <summary>
        /// Executes query and gets the result of query (row 0 column 0)
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="SQLCommand">The query string.</param>
        /// <param name="args">The args.</param>
        /// <returns>Data returned by query</returns>
        public T Result<T>(string SQLCommand, params object[] args)
        {
            try
            {
                SQLCommand = ReplaceArgs(SQLCommand, args);
                using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                    return (T)cmd.ExecuteScalar();
            }
            catch { return default(T); }
        }

        /// <summary>
        /// Executes query as async and gets the result of query (row 0 column 0)
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="SQLCommand">The query string.</param>
        /// <param name="args">The args.</param>
        /// <returns>Data returned by query</returns>
        public Task<object> ResultAsync(string SQLCommand, params object[] args)
        {
            try
            {
                SQLCommand = ReplaceArgs(SQLCommand, args);
                using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                    return cmd.ExecuteScalarAsync();
            }
            catch { return null; }
        }

        /// <summary>
        /// Returns the count of rows returned by query
        /// </summary>
        /// <param name="SQLCommand">the query string</param>
        /// <param name="args">the args</param>
        /// <returns>The number of rows returned by query</returns>
        public int Count(string SQLCommand, params object[] args)
        {
            try
            {
                var tmp = ExecuteReader(SQLCommand, args);
                int cnt = 0;
                while (tmp.Read())
                    cnt++;
                return cnt;
            }
            catch { return 0; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Replaces the possible injection args
        /// </summary>
        /// <param name="SQLCommand">the query string.</param>
        /// <param name="args">the args</param>
        /// <returns>The true query string</returns>
        private string ReplaceArgs(string SQLCommand, params object[] args)
        {
            for (int i = 0; i < args.Length; i++) args[i] = args[i].ToString().Replace("'", "''");
#if DEBUG
            Logging.Log()(String.Format(SQLCommand, args), LogLevel.Warning);
#endif
            return String.Format(SQLCommand, args);
        }

        #endregion
    }
}
