create database RequestAndExceptionLogsCore3_1_WebAPI_LogsDB;

Go
use RequestAndExceptionLogsCore3_1_WebAPI_LogsDB;

GO
CREATE TABLE [dbo].[RequestLogs](
	[RequestLogID] [int] Primary key IDENTITY(1,1) NOT NULL,
	[RequestID] [nvarchar](120) NULL,
	[SessionID] [nvarchar](150) NULL,
	[IpAddress] [nvarchar](30) NULL,
	[RequestURL] [nvarchar](max) NULL,
	[ReferrerURL] [nvarchar](max) NULL,
	[Scheme] [nvarchar](150) NULL,
	[Method] [nvarchar](20) NULL,
	[ContentLength] [bigint] NULL,
	[QueryString] [nvarchar](max) NULL,
	[RequestBody] [nvarchar](max) NULL,
	[RequestFormData] [nvarchar](max) NULL,
	[RequestHeaders] [nvarchar](max) NULL,
	[ResponseCode] [int] NULL,
	[ResponseText] [nvarchar](max) NULL,
	[ResponseHeaders] [nvarchar](max) NULL,
	[CreatedAt] [datetimeoffset] NULL,
)

GO
CREATE TABLE [dbo].[ExceptionLogs](
	[ExceptionLogID] [int] primary key IDENTITY(1,1) NOT NULL,
	[ExceptionType] [nvarchar](150) NULL,
	[ExceptionName] [nvarchar](max) NULL,
	[Message] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
	[ErrorMethodName] [nvarchar](350) NULL,
	[ErrorLineNo] [nvarchar](10) NULL,
	[ExceptionURL] [nvarchar](max) NULL,
	[ErrorLocation] [nvarchar](max) NULL,
	[RequestID] [nvarchar](120),
	[CreatedAt] [datetimeoffset] NULL,
)

GO
select * from RequestLogs;
select * from ExceptionLogs;
GO