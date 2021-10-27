using Helpers.Models.SharedModels;
using RFPPortalWebsite.Models.Constants;
using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Timers;

namespace RFPPortalWebsite.Utility
{
    /// <summary>
    ///  Basic healthcheck and information class for applications
    /// </summary>
    public class Monitizer
    {
        /// <summary>
        ///  Service start status
        ///  <value>-1: Application error in startup</value>
        ///  <value>0: Application loading</value>
        ///  <value>1: Application started successfully</value>
        /// </summary>
        public int startSuccesful = 0;

        /// <summary>
        ///  Number of fatal exceptions occured in the application. (Fatal exception: Exceptions which prevents service to work properly.)
        /// </summary>
        public int fatalCounter = 0;

        /// <summary>
        ///  Number of exceptions occured in the application.
        /// </summary>
        public int exceptionCounter = 0;

        /// <summary>
        ///  List of exceptions occured in the application
        /// </summary>
        public List<ErrorLog> exceptions = new List<ErrorLog>();

        /// <summary>
        ///  List of application type logs in the application
        /// </summary>
        public List<ApplicationLog> logs = new List<ApplicationLog>();

        /// <summary>
        ///  Fast logs for diagnose on the application start page. (Not stored in DB, similar to Console.WriteLine())
        /// </summary>
        public List<string> console = new List<string>();

        /// <summary>
        ///  Application name
        /// </summary>
        public string appName = "";

        /// <summary>
        ///  Application hosting ip
        /// </summary>
        public string ipAddress = "";

        /// <summary>
        ///  Initializes application Monitizer
        /// </summary>
        /// <param name="RabbitMQUrl">RabbitMQ Server Url</param>
        /// <param name="RabbitMQUsername">RabbitMQ Server Username</param>
        /// <param name="RabbitMQPassword">RabbitMQ Server Password</param>
        public Monitizer()
        {
            appName = Assembly.GetEntryAssembly().GetName().Name;

            var dns = Dns.GetHostAddresses(Dns.GetHostName());
            if (dns.Length > 0)
            {
                ipAddress = dns[dns.Length - 1].ToString();
                if (dns.Length > 1 && ipAddress.Length > 20)
                {
                    ipAddress = dns[0].ToString();
                }
            }
        }

        /// <summary>
        ///  Generic application exception logger
        /// </summary>
        /// <param name="ex">Exception object</param>
        /// <param name="LogType">Valid log types:  PublicUserLog, UserLog, AdminLog, ApplicationLog, ApplicationError</param>
        /// <param name="isfatal">Is exception considered as a fatal exception</param>
        /// <param name="IdFieldName">Exception source db field name</param>
        /// <param name="IdField">Exception source primary key id</param>
        public void AddException(Exception ex, Enums.LogTypes LogType, bool isfatal = false, string IdFieldName = "", int IdField = 0)
        {
            exceptionCounter++;

            if (isfatal)
            {
                fatalCounter++;
            }

            ErrorLog log = new ErrorLog();
            log.Application = appName;
            log.Server = ipAddress;
            log.Date = DateTime.Now;
            log.IdField = IdField;
            log.IdFieldName = IdFieldName;
            log.Message = GetExceptionMessages(ex);
            MethodBase site = ex.TargetSite;
            string methodName = site == null ? "" : site.Name;
            log.Target = methodName;
            string trace = ex.StackTrace == null ? "" : ex.StackTrace;
            log.Trace = trace;
            log.Type = LogType.ToString();

            exceptions.Add(log);

            if (exceptions.Count > 1000)
            {
                exceptions.RemoveAt(0);
            }

        }

        /// <summary>
        ///  Generic application logger
        /// </summary>
        /// <param name="type">Valid log types:  PublicUserLog, UserLog, AdminLog, ApplicationLog, ApplicationError</param>
        /// <param name="explanation">Explanation</param>
        /// <param name="IdFieldName">Log source db field name</param>
        /// <param name="IdField">Log source primary key id</param>
        public void AddApplicationLog(Enums.LogTypes type, string explanation, string IdFieldName = "", int IdField = 0)
        {
            ApplicationLog log = new ApplicationLog();
            log.Application = appName;
            log.Server = ipAddress;
            log.IdField = IdField;
            log.IdFieldName = IdFieldName;
            log.Explanation = explanation;
            log.Type = type.ToString();
            log.Date = DateTime.Now;
            logs.Add(log);

            if (logs.Count > 1000)
            {
                logs.RemoveAt(0);
            }

        }

        /// <summary>
        ///  Generic user logger
        /// </summary>
        /// <param name="userId">User identity</param>
        /// <param name="type">Valid log types:  Auth, Request, Agreement</param>
        /// <param name="explanation">Explanation</param>
        /// <param name="ip">User request IP</param>
        /// <param name="port">User request port</param>
        public void AddUserLog(int userId, Enums.UserLogType type, string explanation, string ip = "", string port = "")
        {
            UserLog log = new UserLog();
            log.Application = appName;
            log.Ip = ip;
            log.Port = port;
            log.UserId = userId;
            log.Explanation = explanation;
            log.Type = type.ToString();
            log.Date = DateTime.Now;

            if (logs.Count > 1000)
            {
                logs.RemoveAt(0);
            }

        }

        /// <summary>
        ///  Adds string to console list
        /// </summary>
        /// <param name="text">Text to write in application startup page console</param>
        public void AddConsole(string text)
        {
            console.Add(DateTime.Now.ToString() + " :: " + text);
            if (console.Count > 1000)
            {
                console.RemoveAt(0);
            }
        }

        /// <summary>
        ///  Get inner exceptions from exception object and combine all into a string
        /// </summary>
        /// <param name="e">Exception object</param>
        /// <param name="msgs">Custom message</param>
        /// <returns>Returns full exception as string</returns>
        public string GetExceptionMessages(Exception e, string msgs = "")
        {
            if (e == null) return string.Empty;
            if (msgs == "") msgs = e.Message;
            if (e.InnerException != null)
                msgs += "\r\nInnerException: " + GetExceptionMessages(e.InnerException);
            return msgs;
        }

        /// <summary>
        ///  Get application startup page data (Exceptions, logs, console, healthcheck etc.)
        /// </summary>
        /// <returns>MonitizerResult</returns>
        public MonitizerResult GetMonitizerResult()
        {
            MonitizerResult res = new MonitizerResult();

            res.ExceptionCounter = exceptionCounter;

            if (exceptions.Count > 10)
            {
                res.Exceptions = exceptions.OrderByDescending(x => x.Date).Take(10).ToList();
            }
            else
            {
                res.Exceptions = exceptions;
            }

            res.FatalCounter = fatalCounter;

            if (logs.Count > 10)
            {
                res.Logs = logs.OrderByDescending(x => x.Date).Take(10).ToList();
            }
            else
            {
                res.Logs = logs;
            }
            res.StartSuccesful = startSuccesful;
            res.AppName = appName;
            res.IpAddress = ipAddress;
            res.Console = console.ToList();
            res.Console.Reverse();

            return res;
        }

    }
}
