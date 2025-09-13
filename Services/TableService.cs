using Microsoft.Azure.Cosmos.Table;

// Add this class definition if it does not exist elsewhere
public class FileMetadata : TableEntity
{
    // Add properties as needed, for example:
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    // Add other metadata properties here
}

public class TableService
{
    private readonly CloudTable _table;

    public TableService(string connString, string tableName)
    {
        var storageAccount = CloudStorageAccount.Parse(connString);
        var tableClient = storageAccount.CreateCloudTableClient();
        _table = tableClient.GetTableReference(tableName);
        _table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task AddMetadataAsync(FileMetadata meta)
    {
        try
        {
            var insertOp = TableOperation.Insert(meta);
            await _table.ExecuteAsync(insertOp);
        }
        catch (Exception ex)
        {
            // Log or handle error
            throw new ApplicationException("Failed to add metadata.", ex);
        }
    }

    public async Task<List<FileMetadata>> GetFilesAsync(string userId)
    {
        var query = new TableQuery<FileMetadata>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
        var result = await _table.ExecuteQuerySegmentedAsync(query, null);
        return result.Results.ToList();
    }

    public async Task DeleteMetadataAsync(string partitionKey, string rowKey)
    {
        var entity = new FileMetadata { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };
        var deleteOp = TableOperation.Delete(entity);
        await _table.ExecuteAsync(deleteOp);
    }
}