using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Exchange.WebServices.Data;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Frends.Community.Email
{
    /// <summary>
    /// Read email operations for Exchange servers
    /// </summary>
    public class ReadEmailTask
    {
        #region Public functions





        /// <summary>
        /// Read emails from IMAP server
        /// </summary>
        /// <param name="settings">IMAP server settings</param>
        /// <param name="options">Email options</param>
        /// <returns>
        /// List of
        /// {
        /// string Id.
        /// string To.
        /// string Cc.
        /// string From.
        /// DateTime Date.
        /// string Subject.
        /// string BodyText.
        /// string BodyHtml.
        /// }
        /// </returns>
        /// 

        public static List<EmailMessageResult> ReadEmailWithIMAP([PropertyTab] ImapSettings settings, [PropertyTab] ImapOptions options)
        {
            var result = new List<EmailMessageResult>();

            using (var client = new ImapClient())
            {
                // accept all certs?
                if (settings.AcceptAllCerts)
                {
                    client.ServerCertificateValidationCallback = (s, x509certificate, x590chain, sslPolicyErrors) => true;
                }
                else
                {
                    client.ServerCertificateValidationCallback = MailService.DefaultServerCertificateValidationCallback;
                }

                // connect to imap server
                client.Connect(settings.Host, settings.Port, settings.UseSSL);

                // authenticate with imap server
                client.Authenticate(settings.UserName, settings.Password);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                // get all or only unread emails?
                IList<UniqueId> messageIds = options.GetOnlyUnreadEmails
                    ? inbox.Search(SearchQuery.NotSeen)
                    : inbox.Search(SearchQuery.All);

                // read as many as there are unread emails or as many as defined in options.MaxEmails
                for (int i = 0; i < messageIds.Count && i < options.MaxEmails; i++)
                {
                    MimeMessage msg = inbox.GetMessage(messageIds[i]);
                    result.Add(new EmailMessageResult
                    {
                        Id = msg.MessageId,
                        Date = msg.Date.DateTime,
                        Subject = msg.Subject,
                        BodyText = msg.TextBody,
                        BodyHtml = msg.HtmlBody,
                        From = string.Join(",", msg.From.Select(j => j.ToString())),
                        To = string.Join(",", msg.To.Select(j => j.ToString())),
                        Cc = string.Join(",", msg.Cc.Select(j => j.ToString()))
                    });

                    // should mark emails as read?
                    if (!options.DeleteReadEmails && options.MarkEmailsAsRead)
                    {
                        inbox.AddFlags(messageIds[i], MessageFlags.Seen, true);
                    }
                }

                // should delete emails?
                if (options.DeleteReadEmails && messageIds.Any())
                {
                    inbox.AddFlags(messageIds, MessageFlags.Deleted, false);
                    inbox.Expunge();
                }

                client.Disconnect(true);
            }

            return result;
        }

        /// <summary>
        /// Reads emails from an Exchange server. Can be used only on legacy (Windows) agents.
        /// </summary>
        /// <param name="settings">Settings to use</param>
        /// <param name="options">Options to use</param>
        /// <returns>
        /// List of
        /// {
        /// string Id.
        /// string To.
        /// string Cc.
        /// string From.
        /// DateTime Date.
        /// string Subject.
        /// string BodyText.
        /// string BodyHtml.
        /// List(string) AttachmentSaveDirs
        /// }
        /// </returns>
        public static List<EmailMessageResult> ReadEmailFromExchangeServer([PropertyTab] ExchangeSettings settings, [PropertyTab] ExchangeOptions options)
        {
            #if NET471


            if (!options.IgnoreAttachments)
            {
                // Check that save directory is given
                if (options.AttachmentSaveDirectory == "")
                    throw new ArgumentException("No save directory given. ",
                        nameof(options.AttachmentSaveDirectory));
                // Check that save directory exists
                if (!Directory.Exists(options.AttachmentSaveDirectory))
                    throw new ArgumentException("Could not find or access attachment save directory. ",
                        nameof(options.AttachmentSaveDirectory));
            }

            // Connect, create view and search filter
            ExchangeService exchangeService = Services.ConnectToExchangeService(settings);
            ItemView view = new ItemView(options.MaxEmails);
            var searchFilter = BuildFilterCollection(options);
            FindItemsResults<Item> exchangeResults;

            
            if (!string.IsNullOrEmpty(settings.Mailbox) || settings.ReadFromCustomExchangeFolder)
            {
                var mb = new Mailbox(settings.Mailbox);
                var fid = settings.ReadFromCustomExchangeFolder && string.IsNullOrEmpty(settings.CustomExchangeFolderName) ?
                    new FolderId(ConvertWellKnownFolderName(settings.CustomExchangeFolderRoot), mb) :
                    new FolderId(WellKnownFolderName.Inbox, mb);

                FolderId customFolderId = null;

                if(settings.ReadFromCustomExchangeFolder && !string.IsNullOrEmpty(settings.CustomExchangeFolderName))
                {
                    customFolderId = FindFolderIdByDisplayName(exchangeService, settings.CustomExchangeFolderName, mb, settings.CustomExchangeFolderRoot);
                    if(customFolderId == null)
                    {
                        throw new ArgumentException($"Could not find Exchange folder {settings.CustomExchangeFolderName} under {settings.CustomExchangeFolderRoot}");
                    }
                }
                       
                var inbox = Folder.Bind(exchangeService, customFolderId ?? fid);
                exchangeResults = searchFilter.Count == 0 ? inbox.FindItems(view) : inbox.FindItems(searchFilter, view);
            }
            else
            {
                exchangeResults = searchFilter.Count == 0 ? exchangeService.FindItems(WellKnownFolderName.Inbox, view) : exchangeService.FindItems(WellKnownFolderName.Inbox, searchFilter, view);
            }
            // Get email items
            List<EmailMessage> emails = exchangeResults.Where(msg => msg is EmailMessage).Cast<EmailMessage>().ToList();

            // Check if list is empty and if an error needs to be thrown.
            if (emails.Count == 0 && options.ThrowErrorIfNoMessagesFound)
            {
                // If not, return a result with a notification of no found messages.
                throw new ArgumentException("No messages matching the search filter found. ",
                    nameof(options.ThrowErrorIfNoMessagesFound));
            }

            // Load properties for each email and process attachments
            var result = ReadEmails(emails, exchangeService, options);

            // should delete mails?
            if (options.DeleteReadEmails)
                emails.ForEach(msg => msg.Delete(DeleteMode.HardDelete));

            // should mark mails as read?
            if (!options.DeleteReadEmails && options.MarkEmailsAsRead)
            {
                foreach (EmailMessage msg in emails)
                {
                    msg.IsRead = true;
                    msg.Update(ConflictResolutionMode.AutoResolve);
                }
            }

            return result;

            #else
            throw new Exception("Only supported on .NET Framework (i.e. on Windows).");
            #endif
        }

        #endregion

        #region Private functions

        private static FolderId FindFolderIdByDisplayName(ExchangeService service, string customFolderName, Mailbox mailbox, ExchangeWellKnownFolders wellKnownFolders)
        {
#if NET471

            WellKnownFolderName wellKnownFolderName = ConvertWellKnownFolderName(wellKnownFolders);
            var folderView = new FolderView(100)
            {
                PropertySet = new PropertySet(BasePropertySet.IdOnly)
            };
            folderView.PropertySet.Add(FolderSchema.DisplayName);
            folderView.Traversal = FolderTraversal.Deep;

            FindFoldersResults findFolderResults = service.FindFolders(new FolderId(wellKnownFolderName, mailbox), folderView);

            return findFolderResults.Cast<Folder>().FirstOrDefault(folder => folder.DisplayName.Equals(customFolderName))?.Id;
#else
            throw new Exception("Only supported on .NET Framework (i.e. on Windows).");
#endif
        }

        private static WellKnownFolderName ConvertWellKnownFolderName(ExchangeWellKnownFolders wellKnownFolders)
        {
            return (WellKnownFolderName)Enum.Parse(typeof(WellKnownFolderName), wellKnownFolders.ToString());
        }
        /// <summary>
        /// Build search filter from options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <returns>Search filter collection.</returns>
        private static SearchFilter.SearchFilterCollection BuildFilterCollection(ExchangeOptions options)
        {
            // Create search filter collection.
            var searchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And);

            // Construct rest of search filter based on options
            if (options.GetOnlyEmailsWithAttachments)
                searchFilter.Add(new SearchFilter.IsEqualTo(ItemSchema.HasAttachments, true));

            if (options.GetOnlyUnreadEmails)
                searchFilter.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));

            if (!string.IsNullOrEmpty(options.EmailSenderFilter))
                searchFilter.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.Sender, options.EmailSenderFilter));

            if (!string.IsNullOrEmpty(options.EmailSubjectFilter))
                searchFilter.Add(new SearchFilter.ContainsSubstring(EmailMessageSchema.Subject, options.EmailSubjectFilter));

            return searchFilter;
        }

        /// <summary>
        /// Convert Email collection t EMailMessageResults.
        /// </summary>
        /// <param name="emails">Emails collection.</param>
        /// <param name="exchangeService">Exchange services.</param>
        /// <param name="options">Options.</param>
        /// <returns>Collection of EmailMessageResult.</returns>
        private static List<EmailMessageResult> ReadEmails(IEnumerable<EmailMessage> emails, ExchangeService exchangeService, ExchangeOptions options)
        {

#if NET471

            List<EmailMessageResult> result = new List<EmailMessageResult>();

    foreach (EmailMessage email in emails)
    {
        // Define property set
        var propSet = new PropertySet(
                BasePropertySet.FirstClassProperties,
                EmailMessageSchema.Body,
                EmailMessageSchema.Attachments);

        // Bind and load email message with desired properties
        var newEmail = EmailMessage.Bind(exchangeService, email.Id, propSet);

        var pathList = new List<string>();
        if (!options.IgnoreAttachments)
        {
                    // Save all attachments to given directory

                pathList = SaveAttachments(newEmail.Attachments, options);
        }

        // Build result for email message

        var emailMessage = new EmailMessageResult
        {
            Id = newEmail.Id.UniqueId,
            Date = newEmail.DateTimeReceived,
            Subject = newEmail.Subject,
            BodyText = "",
            BodyHtml = newEmail.Body.Text,
            To = string.Join(",", newEmail.ToRecipients.Select(j => j.Address)),
            From = newEmail.From.Address,
            Cc = string.Join(",", newEmail.CcRecipients.Select(j => j.Address)),
            AttachmentSaveDirs = pathList
        };


        // Catch exception in case of server version is earlier than Exchange2013
        try { emailMessage.BodyText = newEmail.TextBody.Text; } catch { }

        result.Add(emailMessage);
    }

    return result;

#else
            throw new Exception("Only supported on .NET Framework (i.e. on Windows).");
#endif
        }

        /// <summary>
        /// Save attachments from collection to files.
        /// </summary>
        /// <param name="attachments">Attachments collection.</param>
        /// <param name="options">Options.</param>
        /// <returns>List of full paths to saved file attachments.</returns>
        private static List<string> SaveAttachments(Microsoft.Exchange.WebServices.Data.AttachmentCollection attachments, ExchangeOptions options)
        {
            List<string> pathList = new List<string> { };

            foreach (var attachment in attachments)
            {
                FileAttachment file = attachment as FileAttachment;
                string path = Path.Combine(
                    options.AttachmentSaveDirectory,
                    options.OverwriteAttachment ? file.Name :
                        String.Concat(
                            Path.GetFileNameWithoutExtension(file.Name), "_",
                            Guid.NewGuid().ToString(),
                            Path.GetExtension(file.Name))
                        );
                file.Load(path);
                pathList.Add(path);
            }

            return pathList;
        }

#endregion
    }
}
