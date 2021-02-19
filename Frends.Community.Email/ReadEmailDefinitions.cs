using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace Frends.Community.Email
{
    /// <summary>
    /// Settings for IMAP and POP3 servers
    /// </summary>
    public class ImapSettings
    {
        /// <summary>
        /// Host address
        /// </summary>
        [DefaultValue("imap.frends.com")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Host { get; set; }

        /// <summary>
        /// Host port
        /// </summary>
        [DefaultValue(993)]
        public int Port { get; set; }

        /// <summary>
        /// Use SSL or not
        /// </summary>
        [DefaultValue(true)]
        public bool UseSSL { get; set; }

        /// <summary>
        /// Should the task accept all certificates from IMAP server, including invalid ones?
        /// </summary>
        [DefaultValue(false)]
        public bool AcceptAllCerts { get; set; }

        /// <summary>
        /// Account name to login with
        /// </summary>
        [DefaultValue("accountName")]
        [DisplayFormat(DataFormatString = "Text")]
        public string UserName { get; set; }

        /// <summary>
        /// Account password
        /// </summary>
        [PasswordPropertyText]
        public string Password { get; set; }
    }

    /// <summary>
    /// Exchange server specific options
    /// </summary>
    public class ExchangeSettings
    {
        /// <summary>
        /// Which exchange server to target
        /// </summary>
        public ExchangeServerVersion ExchangeServerVersion { get; set; }

        /// <summary>
        /// If true, will try to auto discover server address from user name. In this cae Host and Port values are not used.
        /// </summary>
        public bool UseAutoDiscover { get; set; }

        /// <summary>
        /// Exchange server address
        /// </summary>
        [DefaultValue("exchange.frends.com")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(UseAutoDiscover), "", false)]
        public string ServerAddress { get; set; }

        /// <summary>
        /// Try to login with agent account?
        /// </summary>
        [DefaultValue(false)]
        [Description("Authorize with agent account")]
        public bool UseAgentAccount { get; set; }

        /// <summary>
        /// Email account to use
        /// </summary>
        [DefaultValue("agent@frends.com")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Username { get; set; }

        /// <summary>
        /// Account password
        /// </summary>
        [PasswordPropertyText]
        [UIHint(nameof(UseAgentAccount), "", false)]
        public string Password { get; set; }


        /// <summary>
        /// Inbox to read emails from. If empty reads from default mailbox.
        /// </summary>
        [DefaultValue("agentinbox@frends.com")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Mailbox { get; set; }
        /// <summary>
        /// Read mail from a custom exchange folder instead of the inbox
        /// </summary>
        [DefaultValue(false)]
        public bool ReadFromCustomExchangeFolder { get; set; }
        /// <summary>
        /// Root of the custom folder. Uses ExchangeWellKnownFolderNames
        /// </summary>
        [UIHint(nameof(ReadFromCustomExchangeFolder), "", true)]
        [DefaultValue(ExchangeWellKnownFolders.MsgFolderRoot)]
        public ExchangeWellKnownFolders CustomExchangeFolderRoot { get; set; }
        /// <summary>
        /// Name of the custom exchange folder. If value is not set, task will try to search messages from custom exchange folder root 
        /// </summary>
        [UIHint(nameof(ReadFromCustomExchangeFolder), "", true)]
        [DefaultValue("Inbox")]
        [DisplayFormat(DataFormatString = "Text")]
        public string CustomExchangeFolderName { get; set; }
    }

    /// <summary>
    /// Root folder when using custom exchange folder name
    /// </summary>
    public enum ExchangeWellKnownFolders 
    { 
        MsgFolderRoot, 
        Inbox, 
        SentItems, 
        Drafts, 
        DeletedItems, 
        JunkEmail, 
        ConversationHistory 
    }

    /// <summary>
    /// Which exchange version to target
    /// </summary>
    public enum ExchangeServerVersion
    {
        Exchange2007_SP1,
        Exchange2010,
        Exchange2010_SP1,
        Exchange2010_SP2,
        Exchange2013,
        Exchange2013_SP1,
        Office365
    }

    /// <summary>
    /// Options related to IMAP reading
    /// </summary>
    public class ImapOptions
    {
        /// <summary>
        /// Maximum number of emails to retrieve
        /// </summary>
        [DefaultValue(10)]
        public int MaxEmails { get; set; }

        /// <summary>
        /// Should get only unread emails?
        /// </summary>
        public bool GetOnlyUnreadEmails { get; set; }

        /// <summary>
        /// If true, then marks queried emails as read
        /// </summary>
        public bool MarkEmailsAsRead { get; set; }

        /// <summary>
        /// If true, then received emails will be hard deleted
        /// </summary>
        public bool DeleteReadEmails { get; set; }
    }

    /// <summary>
    /// Options related to Exchange reading
    /// </summary>
    public class ExchangeOptions
    {
        /// <summary>
        /// Maximum number of emails to retrieve
        /// </summary>
        [DefaultValue(10)]
        public int MaxEmails { get; set; }

        /// <summary>
        /// Should get only unread emails?
        /// </summary>
        public bool GetOnlyUnreadEmails { get; set; }

        /// <summary>
        /// If true, then marks queried emails as read
        /// </summary>
        public bool MarkEmailsAsRead { get; set; }

        /// <summary>
        /// If true, then received emails will be hard deleted
        /// </summary>
        public bool DeleteReadEmails { get; set; }

        /// <summary>
        /// Optional. If a sender is given, it will be used to filter emails.
        /// </summary>
        [DefaultValue("")]
        [DisplayFormat(DataFormatString = "Text")]
        public string EmailSenderFilter { get; set; }

        /// <summary>
        /// Optional. If a subject is given, it will be used to filter emails.
        /// </summary>
        [DefaultValue("")]
        [DisplayFormat(DataFormatString = "Text")]
        public string EmailSubjectFilter { get; set; }

        /// <summary>
        /// If true, the task throws an error if no messages matching
        /// search criteria were found.
        /// </summary>
        public bool ThrowErrorIfNoMessagesFound { get; set; }

        /// <summary>
        /// If true, the task doesn't handle emails attachments.
        /// </summary>
        public bool IgnoreAttachments { get; set; }

        /// <summary>
        /// If true, the task fetches only emails with attachments.
        /// </summary>
        [UIHint(nameof(IgnoreAttachments), "", false)]
        public bool GetOnlyEmailsWithAttachments { get; set; }

        /// <summary>
        /// Directory where attachments will be saved to.
        /// </summary>
        [DefaultValue("")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(IgnoreAttachments), "", false)]
        public string AttachmentSaveDirectory { get; set; }

        /// <summary>
        /// Should the attachment be overwritten, if the save directory
        /// already contains an attachment with the same name? If no,
        /// a GUID will be added to the filename.
        /// </summary>
        [UIHint(nameof(IgnoreAttachments), "", false)]
        public bool OverwriteAttachment { get; set; }
    }

    /// <summary>
    /// Output result for read operation
    /// </summary>
    public class EmailMessageResult
    {
        /// <summary>
        /// Email id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// To-field from email
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Cc-field from email
        /// </summary>
        public string Cc { get; set; }

        /// <summary>
        /// From-field from email
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Email received date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Title of the email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body of the email as text
        /// </summary>
        public string BodyText { get; set; }

        /// <summary>
        /// Body html is available
        /// </summary>
        public string BodyHtml { get; set; }

        /// <summary>
        /// Attachment download path
        /// </summary>
        public List<string> AttachmentSaveDirs { get; set; }
    }
}
