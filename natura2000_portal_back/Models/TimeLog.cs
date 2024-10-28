using Microsoft.EntityFrameworkCore;
using natura2000_portal_back.Data;
using natura2000_portal_back.Models.backbone_db;
using Microsoft.Data.SqlClient;

namespace natura2000_portal_back.Models
{
    public static class TimeLog
    {
        public static void setTime(N2KBackboneContext pDataContext, string pProcessName, string pAction)
        {
            try
            {
                ProcessTimeLog ptl = new()
                {
                    ProcessName = pProcessName,
                    ActionPerformed = pAction,
                    StampTime = DateTime.Now
                };

                pDataContext.Set<ProcessTimeLog>().Add(ptl);
                pDataContext.SaveChanges();
            }
            catch
            {

            }
            finally
            {

            }
        }

        public static void setTimeStamp(string pProcessName, string pAction)
        {
            return;
            /*
            SqlConnection conn=null;
            SqlCommand cmd = null;
            SqlParameter param1 = null;
            SqlParameter param2 = null;
            SqlParameter param3 = null;

            try
            {
                conn = new SqlConnection(WebApplication.CreateBuilder().Configuration.GetConnectionString("N2K_BackboneBackEndContext"));
                conn.Open();
                cmd = conn.CreateCommand();
                param1 = new SqlParameter("@ProcessName", pProcessName);
                param2 = new SqlParameter("@ActionPerformed", pAction);
                param3 = new SqlParameter("@StampTime", DateTime.Now);

                cmd.CommandText = "INSERT INTO ProcessTimeLog  (ProcessName,ActionPerformed,StampTime) VALUES (@ProcessName,@ActionPerformed,@StampTime)";
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ex = null;
            }
            finally
            {
                param1 = null;
                param2 = null;
                param3 = null;
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (conn != null ) { 
                    if(conn.State != System.Data.ConnectionState.Closed) conn.Close();
                    conn.Dispose();
                }
            }
            */
        }
    }

    public static class SystemLog
    {
        public enum errorLevel
        {
            [System.Runtime.Serialization.DataMember]
            Panic,
            [System.Runtime.Serialization.DataMember]
            Fatal,
            [System.Runtime.Serialization.DataMember]
            Error,
            [System.Runtime.Serialization.DataMember]
            Warning,
            [System.Runtime.Serialization.DataMember]
            Info,
            [System.Runtime.Serialization.DataMember]
            Debug
        }

        public static void write(errorLevel pLevel, string pMessage, string pClass, string pSource, string connString = "")
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlParameter param1 = null;
            SqlParameter param2 = null;
            SqlParameter param3 = null;
            SqlParameter param4 = null;
            SqlParameter param5 = null;
            //TODO: Log level configurable on the settings
            try
            {
                if (!string.IsNullOrEmpty(connString))
                    conn = new SqlConnection(connString);
                else
                    conn = new SqlConnection(WebApplication.CreateBuilder().Configuration.GetConnectionString("N2K_BackboneBackEndContext"));

                conn.Open();
                cmd = conn.CreateCommand();
                param1 = new SqlParameter("@Level", pLevel);
                param2 = new SqlParameter("@Message", pMessage);
                param3 = new SqlParameter("@TimeStamp", DateTime.Now);
                param4 = new SqlParameter("@Class", pClass);
                param5 = new SqlParameter("@Source", pSource);

                cmd.CommandText = "INSERT INTO SystemLog ([Level],[Message],[TimeStamp],[Class],[Source]) VALUES (@Level,@Message,@TimeStamp,@Class,@Source)";
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                var aaa = ex.Message;
            }
            finally
            {
                param1 = null;
                param2 = null;
                param3 = null;
                param4 = null;
                param5 = null;
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (conn != null)
                {
                    if (conn.State != System.Data.ConnectionState.Closed) conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static void write(errorLevel pLevel, Exception pException, string pClass, string pSource)
        {
            //TODO: Log level configurable on the settings
            try
            {
                write(pLevel, pException.Message, pClass, pSource);
                Exception exec = pException.InnerException;
                while (exec != null)
                {
                    write(pLevel, exec.Message, pClass, "InnerException");
                    exec = exec.InnerException;
                }
                write(pLevel, pException.StackTrace, pClass, "StackTrace");
            }
            catch
            {

            }
            finally
            {

            }
        }

        public static async Task WriteAsync(errorLevel pLevel, Exception pException, string pClass, string pSource, string? connString)
        {
            //TODO: Log level configurable on the settings
            if (connString == null) return;
            try
            {
                await WriteAsync(pLevel, pException.Message, pClass, pSource, connString);
                Exception exec = pException.InnerException;
                while (exec != null)
                {
                    await WriteAsync(pLevel, exec.Message, pClass, "InnerException", connString);
                    exec = exec.InnerException;
                }
                await WriteAsync(pLevel, pException.StackTrace, pClass, "StackTrace", connString);
            }
            catch
            {

            }
            finally
            {

            }
        }

        public static async Task WriteAsync(SystemLog.errorLevel pLevel, string pMessage, string pClass, string pSource, string connString)
        {
            await Task.Delay(10);

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlParameter param1 = null;
            SqlParameter param2 = null;
            SqlParameter param3 = null;
            SqlParameter param4 = null;
            SqlParameter param5 = null;
            //TODO: Log level configurable on the settings
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();
                cmd = conn.CreateCommand();
                param1 = new SqlParameter("@Level", pLevel);
                param2 = new SqlParameter("@Message", pMessage);
                param3 = new SqlParameter("@TimeStamp", DateTime.Now);
                param4 = new SqlParameter("@Class", pClass);
                param5 = new SqlParameter("@Source", pSource);

                cmd.CommandText = "INSERT INTO SystemLog ([Level],[Message],[TimeStamp],[Class],[Source]) VALUES (@Level,@Message,@TimeStamp,@Class,@Source)";
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                var aaa = ex.Message;
            }
            finally
            {
                param1 = null;
                param2 = null;
                param3 = null;
                param4 = null;
                param5 = null;
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (conn != null)
                {
                    if (conn.State != System.Data.ConnectionState.Closed) conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}