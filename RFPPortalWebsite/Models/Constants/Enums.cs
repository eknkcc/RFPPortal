namespace RFPPortalWebsite.Models.Constants
{
    public class Enums
    {
       
        /// <summary>
        ///  Enum of log types in the system
        /// </summary>
        public enum LogTypes
        {
            PublicUserLog,
            UserLog,
            AdminLog,
            ApplicationLog,
            ApplicationError
        }


        /// <summary>
        ///  Enum of user authorization types
        /// </summary>
        public enum UserIdentityType
        {
            Public,
            Internal,
            Admin
        }

        /// <summary>
        ///  Enum of user log types in the system
        /// </summary>
        public enum UserLogType
        {
            Auth,
            Request,
            Agreement
        }

        /// <summary>
        ///  Enum of current status of a RFP
        /// </summary>
        public enum RfpStatusTypes
        {
            AdminApproval,
            Internal,
            Public,
            Completed,
            Expired
        }

    }
}
