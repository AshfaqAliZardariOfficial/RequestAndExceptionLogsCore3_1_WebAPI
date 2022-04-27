using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static RequestAndExceptionLogsCore3_1_WebAPI.DatabaseUtil;

namespace RequestAndExceptionLogsCore3_1_WebAPI.Models
{
    public class ExceptionLogModel
    {
        public int? ExceptionLogID { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionName { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string ErrorMethodName { get; set; }
        public string ErrorLineNo { get; set; }
        public string ExceptionURL { get; set; }
        public string ErrorLocation { get; set; }
        public string RequestID { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public void SaveExceptionLogs(List<ExceptionLogModel> ExceptionLogs)
        {
            string query = @"DECLARE @ExceptionLogsXml xml = @ExceptionLogsXmlString
BEGIN TRANSACTION [ExceptionLogsTRAN]
  BEGIN TRY
    DECLARE @ExceptionLogsTbl TABLE (
      ExceptionLogID int,
      ExceptionType nvarchar(150),
      ExceptionName nvarchar(max),
      [Message] nvarchar(max),
      StackTrace nvarchar(max),
      ErrorMethodName nvarchar(350),
      ErrorLineNo nvarchar(10),
      ExceptionURL nvarchar(max),
      ErrorLocation nvarchar(max),
      RequestID nvarchar(120),
      CreatedAt datetimeoffset
    );
    -- INSERTED XML DATA IN @ExceptionLogsTbl
    INSERT INTO @ExceptionLogsTbl
      SELECT
        T.c.value('(ExceptionLogID/text())[1]', 'int'),
        T.c.value('(ExceptionType/text())[1]', 'nvarchar(150)'),
        T.c.value('(ExceptionName/text())[1]', 'nvarchar(max)'),
        T.c.value('(Message/text())[1]', 'nvarchar(max)'),
        T.c.value('(StackTrace/text())[1]', 'nvarchar(max)'),
        T.c.value('(ErrorMethodName/text())[1]', 'nvarchar(350)'),
        T.c.value('(ErrorLineNo/text())[1]', 'nvarchar(10)'),
        T.c.value('(ExceptionURL/text())[1]', 'nvarchar(max)'),
        T.c.value('(ErrorLocation/text())[1]', 'nvarchar(max)'),
        T.c.value('(RequestID/text())[1]', 'nvarchar(120)'),
        T.c.value('CreatedAt[1]', 'datetimeoffset')
      FROM @ExceptionLogsXml.nodes('/DocumentElement/ExceptionLogs') AS T (c);

    -- Save ExceptionLogs
    MERGE ExceptionLogs AS tgt
    USING (SELECT
      *
    FROM @ExceptionLogsTbl) AS src
    ON (tgt.ExceptionLogID = src.ExceptionLogID)

    WHEN NOT MATCHED BY TARGET THEN
    INSERT (
    ExceptionType,
    ExceptionName,
    [Message],
    StackTrace,
    ErrorMethodName,
    ErrorLineNo,
    ExceptionURL,
    ErrorLocation,
    RequestID,
    CreatedAt
    )
    VALUES (
    src.ExceptionType,
    src.ExceptionName,
    src.[Message],
    src.StackTrace,
    src.ErrorMethodName,
    src.ErrorLineNo,
    src.ExceptionURL,
    src.ErrorLocation,
    src.RequestID,
    src.CreatedAt);

  COMMIT TRANSACTION [ExceptionLogsTRAN]
END TRY
BEGIN CATCH
  ROLLBACK TRANSACTION [ExceptionLogsTRAN]
END CATCH";
            new DatabaseUtil().SaveData(query, ExceptionLogsListToXml(ExceptionLogs));
        }
        private Dictionary<string, object> ExceptionLogsListToXml(List<ExceptionLogModel> ExceptionLogs)
        {
            // DataTable
            DataTable ExceptionLogsDT = new DataTable("ExceptionLogs");

            ExceptionLogsDT.Columns.AddRange(
                new DataColumn[] {
                              new DataColumn("ExceptionLogID")
                            , new DataColumn("ExceptionType")
                            , new DataColumn("ExceptionName")
                            , new DataColumn("Message")
                            , new DataColumn("StackTrace")
                            , new DataColumn("ErrorMethodName")
                            , new DataColumn("ErrorLineNo")
                            , new DataColumn("ExceptionURL")
                            , new DataColumn("ErrorLocation")
                            , new DataColumn("RequestID")
                            , new DataColumn("CreatedAt")
                    }
                );

            // DataTable rows data
            if (ExceptionLogs != null && ExceptionLogs.Count > 0)
            {
                for (int i = 0; i < ExceptionLogs.Count; i++)
                {
                    ExceptionLogModel ExceptionLog = ExceptionLogs[i];
                    ExceptionLogsDT.Rows.Add(ExceptionLog.ExceptionLogID != 0 ? ExceptionLog.ExceptionLogID : 0, string.IsNullOrEmpty(ExceptionLog.ExceptionType) ? null : ExceptionLog.ExceptionType, string.IsNullOrEmpty(ExceptionLog.ExceptionName) ? null : ExceptionLog.ExceptionName, string.IsNullOrEmpty(ExceptionLog.Message) ? null : ExceptionLog.Message, string.IsNullOrEmpty(ExceptionLog.StackTrace) ? null : ExceptionLog.StackTrace, string.IsNullOrEmpty(ExceptionLog.ErrorMethodName) ? null : ExceptionLog.ErrorMethodName, string.IsNullOrEmpty(ExceptionLog.ErrorLineNo) ? null : ExceptionLog.ErrorLineNo, string.IsNullOrEmpty(ExceptionLog.ExceptionURL) ? null : ExceptionLog.ExceptionURL, string.IsNullOrEmpty(ExceptionLog.ErrorLocation) ? null : ExceptionLog.ErrorLocation, string.IsNullOrEmpty(ExceptionLog.RequestID) ? null : ExceptionLog.RequestID, ExceptionLog.CreatedAt);
                }
            }

            string ExceptionLogsXmlString = null;

            using (StringWriter sw = new StringWriter())
            {
                ExceptionLogsDT.WriteXml(sw);
                ExceptionLogsXmlString = sw.ToString();
            }

            return new Dictionary<string, object> { { "ExceptionLogsXmlString", ExceptionLogsXmlString } };
        }

    }
}
