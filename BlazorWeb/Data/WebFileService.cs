using BlazorWeb.Models.Web;
using BlazorWeb.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace BlazorWeb.Data
{
    public class WebFileService
    {
        private readonly ILogger logger;
        private readonly WebFileRepository fileRepo;
        public WebFileService(ILogger<WebFileService> logger, WebFileRepository fileRepo)
        {
            this.logger = logger;
            this.fileRepo = fileRepo;
        }

        public SortDefinition<WebFile> GetSortDefinition(string sort)
        {
            var builder = Builders<WebFile>.Sort;
            SortDefinition<WebFile> sortDefinition = sort switch
            {
                "created_time" => builder.Ascending(f => f.CreatedTime),
                "-created_time" => builder.Descending(f => f.CreatedTime),
                "username" => builder.Ascending(f => f.Username),
                "-username" => builder.Descending(f => f.Username),
                "status" => builder.Ascending(f => f.Status),
                "-status" => builder.Descending(f => f.Status),
                "function" => builder.Ascending(f => f.Function),
                "-function" => builder.Descending(f => f.Function),
                _ => new BsonDocument(),
            };
            return sortDefinition;
        }

        public async Task<PagingResponse<WebFile>> PaginationAsync(string? username = null, int page = 1, int pageSize = 10, string sort = "-created_time")
        {
            FilterDefinition<WebFile> filter = new BsonDocument();

            if (username != null)
                filter &= fileRepo.Filter.Eq(f => f.Username, username);

            var sortDefinition = GetSortDefinition(sort);
            var result = await fileRepo.Collection.Find(filter).Limit(pageSize).Skip((page - 1) * pageSize).Sort(sortDefinition).ToListAsync();
            var totalCount = (int)await fileRepo.Collection.CountDocumentsAsync(filter);
            return new PagingResponse<WebFile>(result, totalCount: totalCount, pageSize: pageSize, currentPage: page);
        }

        public async Task<WebFile> AddAsync(string filename, string username, WebFileFunction function, Dictionary<string, object?> parameters,
            DateTime? createdTime = null, DateTime? expiredTime = null)
        {
            var record = new WebFile
            {
                Filename = filename,
                Username = username,
                Function = function,
                Parameters = parameters,
                CreatedTime = createdTime ?? DateTime.UtcNow,
                ExpiredTime = expiredTime ?? DateTime.UtcNow.AddDays(30),
                Progress = 0,
                Status = WebFileStatusEnum.Init
            };

            await fileRepo.InsertAsync(record);
            logger.LogDebug("Add file record: {id}.", record.Id);
            return record;
        }

        public async Task<WebFile> UpdateAsync(WebFile file)
        {
            if (file.Id == default)
            {
                logger.LogError("{username}'s file update without id.", file.Username);
                return file;
            }

            await fileRepo.UpdateAsync(file);
            return file;
        }

        public async Task DeleteAsync(WebFile file)
        {
            if (file.Id == default)
                return;

            try
            {
                if (file.GridfsId != default)
                    await fileRepo.Bucket.DeleteAsync(file.GridfsId);
            }
            catch (GridFSFileNotFoundException)
            {
                logger.LogWarning("GridFS {id} not found for WebFile {file}.", file.GridfsId, file.Id);
            }

            await fileRepo.DeleteAsync(file.Id);
        }

        public async Task<GridFSUploadStream> OpenUploadStreamAsync(WebFile file, GridFSUploadOptions? options = null, CancellationToken cancellation = default)
        {
            try
            {
                if (file.GridfsId != default)
                    await fileRepo.Bucket.DeleteAsync(file.GridfsId, cancellationToken: cancellation);
            }
            catch (GridFSFileNotFoundException)
            {
                logger.LogWarning("GridFS {id} not found for WebFile {file}.", file.GridfsId, file.Id);
            }

            var stream = await fileRepo.Bucket.OpenUploadStreamAsync(file.Filename, options: options, cancellationToken: cancellation);
            file.GridfsId = stream.Id;
            await fileRepo.UpdateAsync(file);
            return stream;
        }

        public async Task<GridFSDownloadStream<ObjectId>> OpenDownloadStreamAsync(WebFile file, GridFSDownloadOptions? options = null, CancellationToken cancellation = default)
        {
            return await fileRepo.Bucket.OpenDownloadStreamAsync(file.GridfsId, options: options, cancellationToken: cancellation);
        }

        public async Task DownloadToStreamAsync(WebFile file, Stream stream, GridFSDownloadOptions? options = null, CancellationToken cancellation = default) => 
            await fileRepo.Bucket.DownloadToStreamAsync(file.GridfsId, stream, options: options, cancellationToken: cancellation);

    }
}
