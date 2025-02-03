# Frends.Community.Email
Frends task for sending emails. Task sends emails via SMTP protocol and can handle attachments either from file or as raw string input.

[![Actions Status](https://github.com/CommunityHiQ/Frends.Community.Email/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.Email/actions)
![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.Email)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

- [Installing](#installing)
- [Prerequisites for Exchange](#prerequisitesforexchange)
- [Tasks](#tasks)
  - [Send Email](#sendemail)
  - [Send Email To Exchange Server](#sendemailtoexchangeserver)
  - [Read Email With IMAP](#reademailwithimap)
  - [Read Email From Exchange Server](#reademailfromexchangeserver)
- [Building](#building)
- [License](#license)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing
You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-community/api/v3/index.json and in Gallery view in MyGet https://www.myget.org/feed/frends-community/package/nuget/Frends.Community.Email

# Prerequisites for Exchange

Due to deprication of legacy authentication to Exchange, tasks related to Exchange are using Microsoft Graph.<br/>
Authentication provider used in Exchange tasks is [Username/password provider](https://docs.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#usernamepassword-provider).<br/>
To use Exchange tasks you need to have a service account which DOES NOT have MFA enabled and an App Registration with following configurations:<br/>

From authentication, Allow public client flows has to be set on, since Resource Owner Password Credentials Flow is a public client flow.<br/>
![](img/allow_public_client_flows.png)

From API permissions, you need to add delegate permissions for Mail.ReadWrite and Mail.Send from Microsoft Graph. In addition, these permissions need admin consent.<br/>
Without admin consent Graph will require interactive authentication for the user, which is not possible with Frends.<br/>
![](img/api_permissions.png)

Note that if you want to read emails from other users mailbox instead of the users which you use to authenticate (on behalf of other user), you need to use Shared API permissions.<br/>
![](img/api_shared.png)

# Tasks

## SendEmail

### Task Parameters

### Message
Settings for message properties

| Property         | Type   | Description                                         | Example                 |
|------------------|--------|-----------------------------------------------------|-------------------------|
| To               | string | Recipient addresses separated by , or ;.            | jane.doe@somedomain.com |
| Cc               | string | Cc recipient addresses separated by , or ;.         | jane.doe@somedomain.com |
| Bcc              | string | Bcc recipient addresses separated by , or ;.        | jane.doe@somedomain.com |
| From             | string | The email address the message is sent from.         | john.doe@somedomain.com |
| Sender Name      | string | Name of the sender.                                 | Frends errors           |
| Subject          | string | Subject of the message.                             | Hello Jane              |
| Message          | string | Body of the message.                                | You've got mail!        |
| Is message html  | bool   | Indicates whether the mail message body is in Html. | false                   |
| Message encoding | string | Encoding of message body and subject.               | utf-8                   |


### Attachments

Settings for included attachments.

| Property                                | Type                                          | Description                                                                                                                                                                                                                                 | Example         |
|-----------------------------------------|-----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------|
| Attachment type                         | enum { FileAttachment, AttachmentFromString } | Chooses if the attachment file is created from a string or copied from disk.                                                                                                                                                                | FileAttachment  |
| File content                            | string                                        | Usable with Attachment type AttachmentFromString. Sets the content of the attachment file.                                                                                                                                                  | Lorem ipsum...  |
| File name                               | string                                        | Usable with Attachment type AttachmentFromString. Sets the name of the attachment file.                                                                                                                                                     | error.txt       |
| File path                               | string                                        | Usable with Attachment type FileAttachment. Attachment file's path. Uses Directory.GetFiles(string, string) as a pattern matching technique. Exception: If the path ends in a directory, all files in that folder are added as attachments. | C:\\temp\\*.csv |
| Throw exception if attachment not found | bool                                          | Usable with Attachment type FileAttachment. If set true and no files match the given path, an exception is thrown.                                                                                                                          | true            |
| Send if no attachments found            | bool                                          | Usable with Attachment type FileAttachment. If set true and no files match the given path, email will be sent nevertheless.                                                                                                                 | false           |

### SMTP Settings

Settings for connecting to SMTP server.

| Property           | Type     | Description                                                                                                                                                                 | Example                  |
|--------------------|----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------|
| Smtp server        | string   | SMTP server address.                                                                                                                                                        | smtp.somedomain.com      |
| Port               | int      | SMTP server port.                                                                                                                                                           | 25                       |
| SecureSocket       | enum     | Choose what SecureSocketOptions to use, default: SecureSocketOptions.Auto                                                                                                   | SecureSocketOptions.None |
| AcceptAllCerts     | bool     | Accept all certificates when connecting the host, if true, will accept event invalid certificates. If false, will accept self-signed certificates if the root is untrusted. | false                    |
| SkipAuthentication | bool     | If true, task will not authenticate to the SMTP server.                                                                                                                     | false                    |
| Username           | string   | Use this username to log in to the SMTP server.                                                                                                                             | user                     |
| Password           | string   | Use this password to log in to the SMTP server.                                                                                                                             | password                 |
| CustomHeaders      | Header[] | Array of custom headers which will be added to sending of email. Can be left empty if no custom headers are needed. See Header.                                             | Check Header             |

### Header

| Property | Type   | Description                 | Example                   |
|----------|--------|-----------------------------|---------------------------|
| Key      | string | Key of the custom header.   | Content-Transfer-Encoding |
| Value    | string | Value of the custom header. | 7bit                      |

### Result

| Property     | Type   | Description                                   | Example                                                                |
| -------------|--------|-----------------------------------------------|------------------------------------------------------------------------|
| EmailSent    | bool   | Returns true if email message has been sent.  | true                                                                   |
| StatusString | string | Contains information about the task's result. | No attachments found matching path \"C:\\temp\\*.csv\". No email sent. |

## SendEmailToExchangeServer

### Task Parameters

### Message
Settings for message properties

| Property         | Type   | Description                                                                 | Example                 |
|------------------|--------|-----------------------------------------------------------------------------|-------------------------|
| To               | string | Recipient addresses separated by , or ;.                                    | jane.doe@somedomain.com |
| Cc               | string | Cc recipient addresses separated by , or ;.                                 | jane.doe@somedomain.com |
| Bcc              | string | Bcc recipient addresses separated by , or ;.                                | jane.doe@somedomain.com |
| From             | string | Email of user which sends the email. If empty, logged in user will be used. | jane.doe@somedomain.com | 
| Subject          | string | Subject of the message.                                                     | Hello Jane              |
| Message          | string | Body of the message.                                                        | You've got mail!        |
| Is message html  | bool   | Indicates whether the mail message body is in Html.                         | false                   |
| Message encoding | string | Encoding of message body and subject.                                       | utf-8                   |


### Attachments

Settings for included attachments.

| Property                                | Type                                          | Description                                                                                                                                                                                                                                 | Example         |
|-----------------------------------------|-----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------|
| Attachment type                         | enum { FileAttachment, AttachmentFromString } | Chooses if the attachment file is created from a string or copied from disk.                                                                                                                                                                | FileAttachment  |
| File content                            | string                                        | Usable with Attachment type AttachmentFromString. Sets the content of the attachment file.                                                                                                                                                  | Lorem ipsum...  |
| File name                               | string                                        | Usable with Attachment type AttachmentFromString. Sets the name of the attachment file.                                                                                                                                                     | error.txt       |
| File path                               | string                                        | Usable with Attachment type FileAttachment. Attachment file's path. Uses Directory.GetFiles(string, string) as a pattern matching technique. Exception: If the path ends in a directory, all files in that folder are added as attachments. | C:\\temp\\*.csv |
| Throw exception if attachment not found | bool                                          | Usable with Attachment type FileAttachment. If set true and no files match the given path, an exception is thrown.                                                                                                                          | true            |
| Send if no attachments found            | bool                                          | Usable with Attachment type FileAttachment. If set true and no files match the given path, email will be sent nevertheless.                                                                                                                 | false           |

### Exchange Server

Settings for logging in to Exchange using Microsot Graph.

| Property | Type   | Description                                   | Example                            |
|----------|--------|-----------------------------------------------|------------------------------------|
| Username | string | Username of the user who will send the email. | someuser@foobar.com                |
| Password | string | Password of the user who will send the email. | verysecretpassword123              |
| AppId    | string | Application ID of Azure App registration.     | XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXX |
| TenantId | string | Tenant ID of Azure App registration.          | XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXX |

### Result

| Property     | Type   | Description                                   | Example                                                                |
| -------------|--------|-----------------------------------------------|------------------------------------------------------------------------|
| EmailSent    | bool   | Returns true if email message has been sent.  | true                                                                   |
| StatusString | string | Contains information about the task's result. | No attachments found matching path \"C:\\temp\\*.csv\". No email sent. |

## ReadEmailWithIMAP

Read emails using IMAP server.

### Settings for IMAP servers

| Property       | Type   | Description                                                                                                                                                                 | Example         |
|----------------|--------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------|
| Host           | string | Host address.                                                                                                                                                               | imap.frends.com |
| Port           | int    | Host port.                                                                                                                                                                  | 993             |
| UseSSL         | bool   | Use SSL when connecting host.																																				| true            |
| AcceptAllCerts | bool   | Accept all certificates when connecting the host, if true, will accept event invalid certificates. If false, will accept self-signed certificates if the root is untrusted. | false           |
| UserName       | string | Account name to login with.                                                                                                                                                 | emailUser       |
| Password       | string | Account password.                                                                                                                                                           | ***             |

### Options

| Property            | Type | Description                                        | Example |
|---------------------|------|----------------------------------------------------|---------|
| MaxEmails           | int  | Maximum number of emails to retrieve.              | 10      |
| GetOnlyUnreadEmails | bool | If true, will retrieve only unread emails.         | false   |
| MarkEmailsAsRead    | bool | If true, will mark retrieved emails as read.       | false   |
| DeleteReadEmails    | bool | If true, will delete retrieved emails from server. | false   |

### Result
ReadEmail task returns a list of EmailMessageResult objects. Each object contains following properties:

| Property | Type     | Description            | Example                |
|----------|----------|------------------------|------------------------|
| Id       | string   | Email message id.      | ...                    |
| To       | string   | To field from email.   | agent@frends.com       |
| Cc       | string   | Cc field from email.   | doubleagent@frends.com |
| From     | string   | From field from email. | sender@frends.com      |
| Date     | DateTime | Received date.         | ...                    |
| Subject  | string   | Email subject.         | Important email!       |
| BodyText | string   | Plain text email body. | ...                    |
| BodyHtml | string   | Html email body.       | ...                    |

### Usage
You can loop email message by giving task result as input to foreach-shape:
```sh
#result[ReadEmail]
```

You can reference email properties like so:
```sh
#result[ReadEmail][0].BodyText
```
## ReadEmailFromExchangeServer
Read emails from exchange server with or without attachements.

### ExchangeSettings
Settings for logging in to Exchange using Microsot Graph.

| Property   | Type   | Description                                                                      | Example                            |
|------------|--------|----------------------------------------------------------------------------------|------------------------------------|
| Username   | string | Username of the user who will send the email.                                    | someuser@foobar.com                |
| Password   | string | Password of the user who will send the email.                                    | verysecretpassword123              |
| AppId      | string | Application ID of Azure App registration.                                        | XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXX |
| TenantId   | string | Tenant ID of Azure App registration.                                             | XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXX |
| Mailbox    | string | Mailbox from where emails are read. Defaults to users own mailbox.               | someuser@foobar.com                |
| MailFolder | string | Mailfoder from where emails are read. Defaults to Inbox. Name is case sensitive. | Inbox                              |

### Options

| Property                     | Type   | Description                                                                                                       | Example           |
|------------------------------|--------|-------------------------------------------------------------------------------------------------------------------|-------------------|
| MaxEmails                    | int    | Maximum number of emails to retrieve.                                                                             | 10                |
| GetOnlyUnreadEmails          | bool   | If true, only attachments of unread emails will be fetched.                                                       | false             |
| MarkEmailsAsRead             | bool   | If true, will mark processed emails as read (unless execution is cancelled during processing).                    | false             |
| DeleteReadEmails             | bool   | If true, will delete processed emails from server (unless execution is cancelled during processing).              | false             |
| EmailSenderFilter            | string | Optional. If a sender is given, it will be used to filter emails.                                                 | sender@frends.com |
| EmailSubjectFilter           | string | Optional. If a subject is given, it will be used to filter emails (match as substring).                           | Payments          |
| ThrowErrorIfNoMessagesFound  | bool   | If true, error will be thrown if no attachments are found.                                                        | false             |
| IgnoreAttachments			   | bool   | If true, attachement handling will be skipped.                                                                    | true              |
| GetOnlyEmailsWithAttachments | bool   | If true, gets only emails with attachements.                                                                      | true              |
| AttachmentSaveDirectory      | string | Directory where attachments will be saved to.                                                                     | C:\WorkDir\       |
| FileExistsAction             | enum   | What should be done if the attachment file exists in the destination directory. Options: Overwrite, Rename, Error | Error             |

### Result
ReadEmailFromExchangeServer task returns a list of EmailMessageResult objects. Each object contains following properties:

| Property           | Type            | Description                      | Example                                                 |
|--------------------|-----------------|----------------------------------|---------------------------------------------------------|
| Id                 | string          | Email message id.                | ...                                                     |
| To                 | string          | To field from email.             | agent@frends.com                                        |
| Cc                 | string          | Cc field from email.             | doubleagent@frends.com                                  |
| Bcc                | string          | Bcc field from email.            | sender@frends.com                                       |
| From               | string          | From field from email.           | sender@frends.com                                       |
| Date               | DateTime        | Received date.                   | ...                                                     |
| Subject            | string          | Email subject.                   | Important email!                                        |
| BodyHtml           | string          | Plain text email body.           | ...                                                     |
| AttachmentSaveDirs | List of strings | Full paths to saved attachments. | {"C:\WorkDir\attchmnt1.txt","C:\WorkDir\attchmnt2.txt"} |

### Usage
You can loop resulting objects by giving task result as input to foreach-shape:
```sh
#result[ReadEmailFromExchangeServer]
```

You can reference result properties like so:
```sh
#result[ReadEmailFromExchangeServer][0].BodyHtml
```

# Building

Clone a copy of the repo.

`git clone https://github.com/CommunityHiQ/Frends.Community.Email.git`

Build the project.

`dotnet build`

Run Tests.

`dotnet test`

Create a NuGet package.

`dotnet pack --configuration Release`

# License

This project is licensed under the MIT License - see the LICENSE file for details.

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version | Changes                                                                                                                                                   |
|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| 1.0.0   | Initial version of SendEmail.                                                                                                                             |
| 1.1.23  | Added FetchExchangeAttachment.                                                                                                                            |
| 1.2.0   | Tasks no longer use Frends.Task.Attributes.                                                                                                               |
| 1.3.0   | Fixed nuspec and references for build server.                                                                                                             |
| 1.4.0   | ReadEmail can now read attachments. Removed FetchExchangeAttachment.                                                                                      |
| 2.0.0   | Split ReadEmail to ReadEmailWithIMAP and ReadEmailFromExchangeServer. Added IgnoreAttachments and Office356 option.                                       |
| 2.1.0   | Add support for Cc and Bcc recipients to SendEmail task.                                                                                                  |
| 2.2.0   | Ported version for linux agent.                                                                                                                           |
| 2.3.0   | Tighter MAilKit dependency rules.                                                                                                                         |
| 2.4.0   | Now when Windows Authentication is disabled empty credentials are not set and thus frends Agent's credentials are used.                                   |
| 2.5.0   | Changed the EmailTask to use only MailKit and replaced System.Net.Mail SmtpClient to MailKit.Net.Smtp SmtpClient.                                         |
| 2.5.1   | Changed EmailTask to not need credentials and added selector for SecureSocketOptions and removed UseSsl bool attribute                                    |
| 2.5.2   | Fixed bug in SendEmail task. Now email message can contain curly brackets.                                                                                |
| 2.6.0   | Added new task for sending email to Exchange server. Added possibility to use access token as authentication method for ReadEmailFromExchangeServer-task. |
| 3.0.0   | ReadEmailFromExchangeServer modified to use Microsoft Graph. Added task for sending emails to Exchange server. Basic authentication removed from ReadEmailFromExchangeServer-task, which breaks the version 2.6.0 implementation. Migration from Basic authentication to OAuth is necessary when updating to 3.0.0. |
| 3.0.1   | Issue with SendEmailToExchangeServer, where task tried to get attachments even though there were not any.                                                 |
| 3.1.0   | Option added to SendEmail-task to skip authentication to SMTP server.                                                                                     |
| 3.1.1   | ReadEmailFromExchangeServer: Added support to read other users mailboxes.                                                                                 |
| 3.2.0   | ReadEmailFromExchangeServer: Fixed issue where attachments are saved even if the email is not read. Added option to select what should be done if the attachment file exists in destination directory. |
| 3.2.1   | ReadEmailFromExchangeServer: Fixed issue where user was not able to read attachments from other users mailbox.                                            |
| 3.2.2   | SendEmailToExchangeServer: Enabled sending bigger files than 3MB as an attachments.                                                                       |
| 4.0.0   | SendEmailToExchangeServer: Removed encoding-parameter to prevent braking message content.                                                                 |
| 4.1.0   | SendEmailToExchangeServer: Enabled To send emails on behalf of other users.                                                                               |
| 4.1.1   | ReadEmailFromExchangeServer: Fixed casting issue when reading Item Attachment.                                                                            |
| 4.1.2   | ReadEmailFromExchangeServer: Fixed issue where FileAttachment is not recognized correctly when attachment is fetched.                                     |
| 4.1.3   | ReadEmailFromExchangeServer: Ignore ItemAttachments to prevent failing of the Task if attachment is ItemAttachment instead of FileAttachment.             |
| 4.2.0   | SendEmail: Added feature to add custom headers to email sending.                                                                                          |
| 4.2.1   | SendEmailToExchangeServer: Fixed issue which prevented adding multiple attachment to email.                                                               |
| 4.3.0   | ReadEmailFromExchangeServer: Added the option to use client secret for authentication instead of username/password                                                               |