using Azure.Identity;
using Microsoft.Graph;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;

namespace Frends.Community.Email.Tests
{
    [TestFixture]
    public class ReadExchangeEmailTests
    {
        private readonly string _mailfolder = "Inbox";
        private readonly string _username = Environment.GetEnvironmentVariable("Exchange_User") ?? "frends_exchange_test_user@frends.com";
        private readonly string _otherUser = Environment.GetEnvironmentVariable("Exchange_Other_User") ?? "frends_exchange_test_user_2@frends.com";
        private readonly string _password = Environment.GetEnvironmentVariable("Exchange_User_Password");
        private readonly string _applicationID = Environment.GetEnvironmentVariable("Exchange_Application_ID");
        private readonly string _tenantID = Environment.GetEnvironmentVariable("Exchange_Tenant_ID");
        private static ExchangeSettings _settings;
        private static ExchangeServer _server;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (string.IsNullOrEmpty(_applicationID) ||
                string.IsNullOrEmpty(_password) ||
                string.IsNullOrEmpty(_tenantID))
            {
                throw new ArgumentException("Password, Application ID or Tenant ID is missing. Please check environment variables.");
            }
            _settings = new ExchangeSettings
            {
                TenantId = _tenantID,
                AppId = _applicationID,
                Username = _username,
                Password = _password,
                MailFolder = _mailfolder
            };

            _server = new ExchangeServer
            {
                Username = _username,
                Password = _password,
                AppId = _applicationID,
                TenantId = _tenantID
            };
        }

        [Test]
        public async Task ShouldReadOneItemTest()
        {
            var subject = "One Email Test";
            await SendTestEmail(subject, _username);
            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = true,
                EmailSubjectFilter = subject
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 1);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldReadFiveItemsTest()
        {
            var subject = "Five Emails test";
            for (int i = 0; i < 5; i++)
                await SendTestEmail(subject, _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 3,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = true,
                EmailSubjectFilter = subject
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.That(result.Count, Is.EqualTo(3));
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldGetUnreadMailsTest()
        {
            var subject = "Get Unread Emails Test";
            await SendTestEmail(subject, _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = true,
                MarkEmailsAsRead = true,
                IgnoreAttachments = true,
                EmailSubjectFilter = subject
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 1);
            result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 0);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldMarkEmailAsReadTest()
        {
            var subject = "Mark Email As Read Test";
            await SendTestEmail(subject, _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = true,
                MarkEmailsAsRead = true,
                IgnoreAttachments = true,
                EmailSubjectFilter = subject
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 1);
            result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 0);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldGetAttachmentTest()
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../AttachmentData/");
            var filename = "GetAttachmentTest.txt";
            var fullpath = Path.Combine(dirPath, filename);
            var subject = "Get Attachment Test";
            await SendTestEmailWithAttachment(subject, filename, _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = false,
                AttachmentSaveDirectory = dirPath
            };

            Directory.CreateDirectory(dirPath);
            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result[0].AttachmentSaveDirs.Count, 1);
            Assert.AreEqual(result[0].AttachmentSaveDirs[0], fullpath);
            Assert.IsTrue(File.Exists(fullpath));
            Directory.Delete(dirPath, true);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldFilterBySenderTest()
        {
            var subject = "Filter By Sender Test";
            await SendTestEmail(subject, _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = true,
                EmailSenderFilter = _username
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 1);
            options.EmailSenderFilter = "SomeRandom@foobar.com";
            result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 0);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldFilterBySubjectTest()
        {
            var subject = "Filter By Subject Test";
            await SendTestEmail(subject, _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = true,
                EmailSubjectFilter = subject
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 1);
            options.EmailSubjectFilter = "Some Random Subject";
            result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 0);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldGetOnlyEmailsWithAttachmentsTest()
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../OnlyAttachmentData/");
            var subject = "Get Only Emails With Attachments Test";
            var fullpath = Path.Combine(dirPath, "OnlyAttachment.txt");
            await SendTestEmailWithAttachment(subject, "OnlyAttachment.txt", _username);
            await SendTestEmail(subject, _username);

            // There has been some hicups where the attachment already exists.
            // This step will make sure that it doesn't.
            if (File.Exists(fullpath)) File.Delete(fullpath);

            var options = new ExchangeOptions
            {
                MaxEmails = 2,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = false,
                GetOnlyEmailsWithAttachments = true,
                AttachmentSaveDirectory = dirPath,
                EmailSubjectFilter = subject
            };

            Directory.CreateDirectory(dirPath);
            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(result.Count, 1);
            Directory.Delete(dirPath, true);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ShouldOverwriteAttachmentsTest()
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../OverwriteData/");
            var subject = "Overwrite Attachment Test";
            await SendTestEmailWithAttachment(subject, "OverwriteAttachment.txt", _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = false,
                GetOnlyEmailsWithAttachments = true,
                AttachmentSaveDirectory = dirPath,
                FileExistsAction = FileExists.Overwrite
            };

            Directory.CreateDirectory(dirPath);
            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.IsTrue(File.Exists(result[0].AttachmentSaveDirs[0]));
            Assert.AreEqual(Directory.GetFiles(dirPath).Length, 1);
            result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.IsTrue(File.Exists(result[0].AttachmentSaveDirs[0]));
            Assert.AreEqual(Directory.GetFiles(dirPath).Length, 1);
            Directory.Delete(dirPath, true);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ItemAttachmentWillNotFailTask()
        {
            var subject = "Read Item Attachment Test";
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../ItemAttachment/");
            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = false,
                EmailSubjectFilter = subject,
                AttachmentSaveDirectory = dirPath,
                FileExistsAction = FileExists.Overwrite
            };

            var settings = new ExchangeSettings
            {
                TenantId = _tenantID,
                AppId = _applicationID,
                Username = _username,
                Password = _password,
                MailFolder = "ItemAttachment"
            };

            Directory.CreateDirectory(dirPath);
            var result = await ReadEmailTask.ReadEmailFromExchangeServer(settings, options, new CancellationToken());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].AttachmentSaveDirs.Count);
            Directory.Delete(dirPath, true);
        }

        [Test]
        public async Task ReadOtherUsersInbox()
        {
            var subject = "Read From Other User";
            await SendTestEmail(subject, _otherUser);
            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = true,
                EmailSubjectFilter = subject
            };

            var settings = new ExchangeSettings
            {
                TenantId = _tenantID,
                AppId = _applicationID,
                Username = _username,
                Password = _password,
                MailFolder = _mailfolder,
                Mailbox = _otherUser
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(settings, options, new CancellationToken());
            Assert.AreEqual(1, result.Count);
            await DeleteMessages(subject, _otherUser);
        }

        [Test]
        public async Task ReadAttachmentFromOtherUsersInbox()
        {
            var subject = "Read Attachment From Other User";
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../OtherUserAttachment/");
            await SendTestEmailWithAttachment(subject, "OtherUserAttachment.txt", _otherUser);
            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = true,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = false,
                EmailSubjectFilter = subject,
                AttachmentSaveDirectory = dirPath,
                FileExistsAction = FileExists.Overwrite
            };

            var settings = new ExchangeSettings
            {
                TenantId = _tenantID,
                AppId = _applicationID,
                Username = _username,
                Password = _password,
                MailFolder = _mailfolder,
                Mailbox = _otherUser
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(settings, options, new CancellationToken());
            Assert.IsTrue(File.Exists(result[0].AttachmentSaveDirs[0]));
            Directory.Delete(dirPath, true);
            await DeleteMessages(subject, _otherUser);
        }

        [Test]
        public async Task ReadFromOtherFolder()
        {
            var subject = "Read from other folder";
            await SendTestEmail(subject, _username);
            await MoveEmailToFolder(subject, "OtherFolder", _username);
            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = true,
                EmailSubjectFilter = subject
            };

            var settings = new ExchangeSettings
            {
                TenantId = _tenantID,
                AppId = _applicationID,
                Username = _username,
                Password = _password,
                MailFolder = "OtherFolder"
            };

            var result = await ReadEmailTask.ReadEmailFromExchangeServer(settings, options, new CancellationToken());
            Assert.AreEqual(1, result.Count);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public void WrongMailFolderThrowsError()
        {
            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = false,
                IgnoreAttachments = true
            };

            var settings = new ExchangeSettings
            {
                TenantId = _tenantID,
                AppId = _applicationID,
                Username = _username,
                Password = _password,
                MailFolder = "something"
            };

            Assert.ThrowsAsync<ArgumentException>(async () => await ReadEmailTask.ReadEmailFromExchangeServer(settings, options, new CancellationToken()));
        }

        [Test]
        public async Task GetAttachmentOnlyFromUnreadEmail()
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../OnlyRead/");
            var subject = "Only Read Attachment Test";
            await SendTestEmailWithAttachment(subject, "OnlyReadAttachment.txt", _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = true,
                MarkEmailsAsRead = true,
                IgnoreAttachments = false,
                GetOnlyEmailsWithAttachments = true,
                AttachmentSaveDirectory = dirPath,
                FileExistsAction = FileExists.Overwrite
            };

            Directory.CreateDirectory(dirPath);
            var result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.IsTrue(File.Exists(result[0].AttachmentSaveDirs[0]));
            Assert.AreEqual(Directory.GetFiles(dirPath).Length, 1);
            File.Delete(result[0].AttachmentSaveDirs[0]);
            result = await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(0, Directory.GetFiles(dirPath).Length);
            Directory.Delete(dirPath, true);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task RenameAttachments()
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Rename/");
            var subject = "Rename Attachments Test";
            await SendTestEmailWithAttachment(subject, "RenameAttachment.txt", _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = true,
                IgnoreAttachments = false,
                GetOnlyEmailsWithAttachments = true,
                AttachmentSaveDirectory = dirPath,
                FileExistsAction = FileExists.Rename
            };

            Directory.CreateDirectory(dirPath);
            await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            Assert.IsTrue(File.Exists(Path.Combine(dirPath, "RenameAttachment.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(dirPath, "RenameAttachment(1).txt")));
            Assert.IsTrue(File.Exists(Path.Combine(dirPath, "RenameAttachment(2).txt")));
            Directory.Delete(dirPath, true);
            await DeleteMessages(subject, _username);
        }

        [Test]
        public async Task ErrorIfAttachmentExists()
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Error/");
            var subject = "Error Attachment Test";
            await SendTestEmailWithAttachment(subject, "Error.txt", _username);

            var options = new ExchangeOptions
            {
                MaxEmails = 1,
                DeleteReadEmails = false,
                GetOnlyUnreadEmails = false,
                MarkEmailsAsRead = true,
                IgnoreAttachments = false,
                GetOnlyEmailsWithAttachments = true,
                AttachmentSaveDirectory = dirPath,
                FileExistsAction = FileExists.Error
            };

            Directory.CreateDirectory(dirPath);
            await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken());
            var result = Assert.ThrowsAsync<Exception>(async () => await ReadEmailTask.ReadEmailFromExchangeServer(_settings, options, new CancellationToken()));
            Assert.AreEqual("Attachment file Error.txt already exists in the given directory.", result.Message);
            Directory.Delete(dirPath, true);
            await DeleteMessages(subject, _username);
        }

        #region HelperMethods

        private async Task DeleteMessages(string subject, string user)
        {
            Thread.Sleep(5000); // Give some time for emails to get through before deletion.
            var options = new List<QueryOption>
            {
                new QueryOption("$search", "\"subject:" + subject + "\"")
            };
            var credentials = new UsernamePasswordCredential(_username, _password, _tenantID, _applicationID);
            var graph = new GraphServiceClient(credentials);
            var messages = await graph.Users[user].Messages.Request(options).GetAsync();
            foreach (var message in messages)
                await graph.Users[user].Messages[message.Id].Request().DeleteAsync();
        }

        private async Task SendTestEmail(string subject, string receiver)
        {
            var input = new ExchangeInput
            {
                To = receiver,
                Message = "This is a test message from Frends.Commmunity.Email Unit Tests.",
                IsMessageHtml = false,
                Subject = subject
            };

            await EmailTask.SendEmailToExchangeServer(input, null, _server, new CancellationToken());
            Thread.Sleep(2000); // Give the email some time to get through.
        }

        private async Task SendTestEmailWithAttachment(string subject, string filename, string receiver)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../..", filename);
            File.WriteAllText(filePath, "This is a test attachment file.");
            var input = new ExchangeInput
            {
                To = receiver,
                Message = "This email has a file attachment.",
                IsMessageHtml = false,
                Subject = subject
            };

            var attachment = new Attachment
            {
                AttachmentType = AttachmentType.FileAttachment,
                FilePath = filePath,
                ThrowExceptionIfAttachmentNotFound = false,
                SendIfNoAttachmentsFound = false
            };

            var attachmentArray = new Attachment[] { attachment };

            await EmailTask.SendEmailToExchangeServer(input, attachmentArray, _server, new CancellationToken());
            File.Delete(filePath);
            Thread.Sleep(5000); // Give the email some time to get through.
        }

        private async Task MoveEmailToFolder(string subject, string folder, string user)
        {
            Thread.Sleep(5000); // Give some time for emails to get through before deletion.
            var options = new List<QueryOption>
            {
                new QueryOption("$search", "\"subject:" + subject + "\"")
            };
            var credentials = new UsernamePasswordCredential(_username, _password, _tenantID, _applicationID);
            var graph = new GraphServiceClient(credentials);
            var messages = await graph.Users[user].Messages.Request(options).GetAsync();

            var allFolders = await graph.Users[user].MailFolders.Request().GetAsync();

            var folderID = "";

            foreach (var oneFolder in allFolders)
                if (oneFolder.DisplayName == folder)
                    folderID = oneFolder.Id;

            foreach (var message in messages)
                await graph.Users[user].Messages[message.Id].Move(folderID).Request().PostAsync();

            Thread.Sleep(5000); // Give some time for emails to be moved.
        }

        #endregion
    }
}
