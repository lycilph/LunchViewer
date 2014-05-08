using System;

namespace ModelLibrary
{
    public class ErrorEntity : StorageData
    {
        public Guid ErrorId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }

        public ErrorEntity() { }
        public ErrorEntity(string type, string message)
        {
            ErrorId = Guid.NewGuid();
            Type = type;
            Message = message;

            PartitionKey = Type;
            RowKey = ErrorId.ToString();
        }
    }
}
