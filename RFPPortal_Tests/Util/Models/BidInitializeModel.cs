/// <summary>
/// The model holds bid environment variables necessary after creating a fake bidding environment to call user ids, usernames and RFP id in RFPController and BidController tests.
/// </summary>
public class BidInitializeModel{

        /// Holds the AdminUserId of the bidding environment
        public int AdminUserId { get; set; } = 0;
        /// Holds the AdminUserName of the bidding environment
        public string AdminUserName {get; set; }
        /// Holds the AdminUserName of the bidding environment
        public int PublicUserId { get;set; } = 0;
        /// Holds the PublicUserId of the bidding environment
        public string PublicUserName { get; set; }
        /// Holds the PublicUserName of the bidding environment
        public int InternalUserId { get; set; } = 0;
        /// Holds the InternalUserName of the bidding environment
        public string InternalUserName { get; set; }
        /// Holds the RfpId of the bidding environment
        public int RfpId { get; set; } = 0;
    }