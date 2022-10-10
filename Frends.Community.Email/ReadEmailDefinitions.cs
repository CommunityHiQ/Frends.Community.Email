using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Email.ReadEmailDefinition
{
    /// <summary>
    /// Settings for IMAP and POP3 servers.
    /// </summary>
    public class ImapSettings
    {
        /// <summary>
        /// Host address.
        /// </summary>
        [DefaultValue("imap.frends.com")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Host { get; set; }

        /// <summary>
        /// Host port.
        /// </summary>
        [DefaultValue(993)]
        public int Port { get; set; }

        /// <summary>
        /// Use SSL or not?
        /// </summary>
        [DefaultValue(true)]
        public bool UseSSL { get; set; }

        /// <summary>
        /// Should the task accept all certificates from IMAP server, including invalid ones?
        /// </summary>
        [DefaultValue(false)]
        public bool AcceptAllCerts { get; set; }

        /// <summary>
        /// Account name to login with.
        /// </summary>
        [DefaultValue("accountName")]
        [DisplayFormat(DataFormatString = "Text")]
        public string UserName { get; set; }

        /// <summary>
        /// Account password.
        /// </summary>
        [PasswordPropertyText]
        public string Password { get; set; }
    }

    /// <summary>
    /// Exchange server specific options.
    /// </summary>
    public class ExchangeSettings
    {
      /// <summary>
        /// App ID for fetching access token.
        /// </summary>
        [DefaultValue("")]
        public string AppId { get; set; }
        /// <summary>
        /// App ID for fetching access token.
        /// </summary>
        [PasswordPropertyText]
        public string AppSecret { get; set; }

        /// <summary>
        /// Operation scope. Often Microsoft Graph default value is enough. Scopes are delimeted by ;. Example: https://graph.microsoft.com/.default;https://graph.microsoft.com/Mail.Read
        /// </summary>
        [DefaultValue("https://graph.microsoft.com/.default")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Scopes { get; set; }
        /// <summary>
        /// Tenant ID for fetching access token.
        /// </summary>
        [DefaultValue("")]
        public string TenantId { get; set; }

        /// <summary>
        /// Mailbox from where the emails will be read.
        /// </summary>
        [DefaultValue("groupemailbox@nonfunctionalbox.com")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Mailbox { get; set; }

        /// <summary>
        /// Mailbox from where the emails will be read.
        /// </summary>
        [DefaultValue("Inbox")]
        [DisplayFormat(DataFormatString = "Text")]
        public string MailFolder { get; set; }
    }

    /// <summary>
    /// Options related to IMAP reading.
    /// </summary>
    public class ImapOptions
    {
        /// <summary>
        /// Maximum number of emails to retrieve.
        /// </summary>
        [DefaultValue(10)]
        public int MaxEmails { get; set; }

        /// <summary>
        /// Should get only unread emails?
        /// </summary>
        public bool GetOnlyUnreadEmails { get; set; }

        /// <summary>
        /// If true, then marks queried emails as read.
        /// </summary>
        public bool MarkEmailsAsRead { get; set; }

        /// <summary>
        /// If true, then received emails will be hard deleted.
        /// </summary>
        public bool DeleteReadEmails { get; set; }
    }

    /// <summary>
    /// Options related to Exchange reading.
    /// </summary>
    public class ExchangeOptions
    {
        /// <summary>
        /// Maximum number of emails to retrieve.
        /// </summary>
        [DefaultValue(10)]
        public int MaxEmails { get; set; }

        /// <summary>
        /// Should get only unread emails?
        /// </summary>
        public bool GetOnlyUnreadEmails { get; set; }

        /// <summary>
        /// If true, then marks queried emails as read.
        /// </summary>
        public bool MarkEmailsAsRead { get; set; }

        /// <summary>
        /// If true, then received emails will be hard deleted.
        /// </summary>
        public bool DeleteReadEmails { get; set; }

        /// <summary>
        /// Optional.
        /// If a sender is given, it will be used to filter emails.
        /// </summary>
        [DefaultValue("")]
        [DisplayFormat(DataFormatString = "Text")]
        public string EmailSenderFilter { get; set; }

        /// <summary>
        /// Optional.
        /// If a subject is given, it will be used to filter emails.
        /// </summary>
        [DefaultValue("")]
        [DisplayFormat(DataFormatString = "Text")]
        public string EmailSubjectFilter { get; set; }

        /// <summary>
        /// If true, the task throws an error if no messages matching search criteria were found.
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
        /// Should the attachment be overwritten, if the save directory already contains an attachment with the same name?
        /// If no, a GUID will be added to the filename.
        /// </summary>
        [UIHint(nameof(IgnoreAttachments), "", false)]
        public bool OverwriteAttachment { get; set; }
    }

    /// <summary>
    /// Output result for read operation.
    /// </summary>
    public class EmailMessageResult
    {
        /// <summary>
        /// Email ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// To-field from email.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// CC-field from email.
        /// </summary>
        public string Cc { get; set; }

        /// <summary>
        /// BCC-field from email.
        /// </summary>
        public string Bcc { get; set; }

        /// <summary>
        /// From-field from email.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Email received date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Title of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body of the email as text.
        /// </summary>
        public string BodyText { get; set; }

        /// <summary>
        /// Body HTML is available.
        /// </summary>
        public string BodyHtml { get; set; }


        /// <summary>
        /// List of attachments downloaded List of objects {string OriginalFileName, string SaveDirectory, string SaveFilename, string FullPathToSaveFile }
        /// </summary>
        public List<Frends.Community.Email.ReadEmailDefinition.Attachment> Attachments { get; set; }


    }
    /// <summary>
    /// Attachment retrieval object. Details of stored attachment.
    /// </summary>
    public class Attachment
    {
        public string OriginalFilename { get; set; }
        public string SaveDirectory { get; set; }
        public string SaveFilename { get; set; }
        public string FullPathToSaveFile { get; set; }
    }

}
