﻿using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Minio.Exceptions;
using TerminalApi.Models;
using TerminalApi.Utilities;

namespace TerminalApi.Services.minio
{
    public class MinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string bucketName = EnvironmentVariables.MINIO_BUCKETNAME;

        public MinioService()
        {
            string endpoint = "localhost:9000";
            string accessKey = "minioadmin";
            string secretKey = "minioadmin";
            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false)
                .Build();
        }

        public async Task<PutObjectResponse> UploadFileAsync(string objectName, string filePath,
            Dictionary<string, string> metadata)
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath).WithContentType(metadata["Content-Type"]).WithHeaders(metadata);

            return await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        }

        public async Task<string> GetFileUrlAsync(string objectName)
        {
            var presignedGetObjectAsync = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(60 * 60 * 24);
            var presignedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectAsync);
            return presignedUrl;
        }

        public async Task<bool> DoesFileExistsAsync(string objectName)
        {
            try
            {
                var presignedGetObjectAsync = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);
                var presignedUrl = await _minioClient.StatObjectAsync(presignedGetObjectAsync);
                return presignedUrl != null;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking file existence: {ex.Message}", ex);
            }
        }

        public async Task DownloadFileAsync(string objectName, string downloadFilePath)
        {
            using var fileStream = new FileStream(downloadFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(async stream => { await stream.CopyToAsync(fileStream); });

            await _minioClient.GetObjectAsync(getObjectArgs);
        }

        public async Task RemoveFileAsync(string objectName)
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
            await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
        }

        public async Task<PutObjectResponse> UploadFileAsync(string path, string NewFileName, IFormFile file)
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var metadata = new Dictionary<string, string>
        {
            { "Content-Type", file.ContentType },
            { "x-amz-meta-content-disposition", "inline" }
        };

            return await UploadFileAsync($"{path}/{NewFileName}", filePath, metadata);
        }

        public async Task<List<string>> ListAllFiles(string directoryName)
        {
            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithPrefix(directoryName)
                .WithRecursive(true);

            var objects = _minioClient.ListObjectsEnumAsync(listObjectsArgs);
            List<string> allFilenames = new();
            await foreach (var obj in objects)
            {
                // On supprime le nom du dossier et le slash
                var filename = obj.Key.Replace($"{directoryName}/", "");
                allFilenames.Add(filename);
            }

            return allFilenames;
        }

        public async Task<List<FileInfoResponse>> GetAllFiles(Guid id, string modelName, string type)
        {
            var minioFiles = await ListAllFiles($"{modelName}/{type}/{id}");
            List<FileInfoResponse> minioFileInfos = [];
            foreach (var minioFile in minioFiles)
                minioFileInfos.Add(new FileInfoResponse
                {
                    Name = minioFile,
                    Url = await GetFileUrlAsync($"{modelName}/{type}/{id}/{minioFile}")
                });
            return minioFileInfos;
        }

        public async Task<List<FileInfoResponse>> GetFilesWithMetadata(string directoryPath)
        {
            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithPrefix(directoryPath)
                .WithRecursive(true);

            var objects = _minioClient.ListObjectsEnumAsync(listObjectsArgs);
            List<FileInfoResponse> fileInfos = new();

            await foreach (var obj in objects)
            {
                // Extraire le nom du fichier depuis le chemin complet
                var fileName = Path.GetFileName(obj.Key);

                fileInfos.Add(new FileInfoResponse
                {
                    Name = fileName,
                    Url = await GetFileUrlAsync(obj.Key),
                    UploadDate = obj.LastModified != null ? DateTimeOffset.Parse(obj.LastModified) : null
                });
            }

            return fileInfos;
        }

        public async Task<FileInfoResponse?> GetFile(Guid id, string modelName, string type)
        {
            var minioFiles = await ListAllFiles($"{modelName}/{type}/{id}");

            if (minioFiles.Count == 0) return null;

            return new FileInfoResponse
            {
                Name = minioFiles[0],
                Url = await GetFileUrlAsync($"{modelName}/{type}/{id}/{minioFiles[0]}")
            };
        }
    }
}
