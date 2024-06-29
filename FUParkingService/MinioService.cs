using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ResponseObject;
using FUParkingModel.ReturnCommon;
using FUParkingService.Interface;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace FUParkingService
{
    public class MinioService : IMinioService
    {
        public async Task<Return<ReturnObjectUrlResDto>> GetObjectUrlByObjectNameAsync(GetObjectReqDto req)
        {
            try
            {
                var minio = GetMinioClient();
                var isExist = await CheckFileIsExistInBucket(req.ObjName, req.BucketName, minio);
                if (isExist.IsSuccess)
                {
                    if (isExist.Message == MinioErrorApplicationDefineEnum.NOT_FOUND)
                    {
                        return new Return<ReturnObjectUrlResDto>
                        {
                            Message = ErrorEnumApplication.SERVER_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = new Exception(MinioErrorApplicationDefineEnum.NOT_FOUND)
                        };
                    }
                    else
                    {
                        return new Return<ReturnObjectUrlResDto>
                        {
                            Data = new ReturnObjectUrlResDto
                            {
                                ObjUrl = "https://miniofile.khangbpa.com/" + req.BucketName + "/" + req.ObjName
                            },
                            IsSuccess = true,
                            Message = SuccessfullyEnumServer.FOUND_OBJECT
                        };
                    }
                }
                else
                {
                    return new Return<ReturnObjectUrlResDto>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = isExist.InternalErrorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new Return<ReturnObjectUrlResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<ReturnObjectUrlResDto>> UploadObjectAsync(UploadObjectReqDto req)
        {
            try
            {
                var minio = GetMinioClient();
                var isExist = await CheckFileIsExistInBucket(req.ObjName, req.BucketName, minio);
                if (isExist.IsSuccess)
                {
                    if (isExist.Message == MinioErrorApplicationDefineEnum.NOT_FOUND)
                    {
                        var uploadObjectArgs = new PutObjectArgs()
                            .WithBucket(req.BucketName)
                            .WithContentType(req.ObjFile.ContentType)
                            .WithObjectSize(req.ObjFile.Length)
                            .WithObject(req.ObjName)
                            .WithStreamData(req.ObjFile.OpenReadStream());

                        await minio.PutObjectAsync(uploadObjectArgs);
                        // Check file is exist after upload
                        var isExistAfterUpload = await CheckFileIsExistInBucket(req.ObjName, req.BucketName, minio);
                        if (isExistAfterUpload.IsSuccess)
                        {
                            return new Return<ReturnObjectUrlResDto>
                            {
                                Data = new ReturnObjectUrlResDto
                                {
                                    ObjUrl = "https://miniofile.khangbpa.com/" + req.BucketName + "/" + req.ObjName
                                },
                                IsSuccess = true,
                                Message = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY
                            };
                        }
                        else
                        {
                            return new Return<ReturnObjectUrlResDto>
                            {
                                Message = ErrorEnumApplication.SERVER_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = new Exception(MinioErrorApplicationDefineEnum.NOT_FOUND)
                            };
                        }
                    }
                    else
                    {
                        return new Return<ReturnObjectUrlResDto>
                        {
                            Message = ErrorEnumApplication.SERVER_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = new Exception(MinioErrorApplicationDefineEnum.FILE_NAME_IS_EXIST)
                        };
                    }
                }
                else
                {
                    return new Return<ReturnObjectUrlResDto>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = isExist.InternalErrorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new Return<ReturnObjectUrlResDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<bool>> DeleteObjectAsync(DeleteObjectReqDto req)
        {
            try
            {
                var minio = GetMinioClient();
                var isExist = await CheckFileIsExistInBucket(req.ObjName, req.BucketName, minio);
                if (isExist.IsSuccess)
                {
                    if (isExist.Message == MinioErrorApplicationDefineEnum.NOT_FOUND)
                    {
                        return new Return<bool>
                        {
                            Message = ErrorEnumApplication.SERVER_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = new Exception(MinioErrorApplicationDefineEnum.NOT_FOUND)
                        };
                    }
                    else
                    {
                        RemoveObjectArgs removeObjectArgs = new RemoveObjectArgs()
                            .WithBucket(req.BucketName)
                            .WithObject(req.ObjName);
                        await minio.RemoveObjectAsync(removeObjectArgs);

                        // Check file is exist after delete
                        var isExistAfterDelete = await CheckFileIsExistInBucket(req.ObjName, req.BucketName, minio);
                        if (isExistAfterDelete.IsSuccess)
                        {
                            return new Return<bool>
                            {
                                IsSuccess = false,
                                Message = ErrorEnumApplication.SERVER_ERROR
                            };
                        }
                        else
                        {
                            return new Return<bool>
                            {
                                IsSuccess = true,
                                Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                            };
                        }
                    }
                }
                else
                {
                    return new Return<bool>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = isExist.InternalErrorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new Return<bool>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex
                };
            }
        }

        #region Private Method
        private static IMinioClient GetMinioClient()
        {
            var endpoint = "miniofile.khangbpa.com";
            var accessKey = "NnWIoeq2t3KCXFyzpjr7";
            var secretKey = "fBpKVFJwjZoze8funCShq6H3LO26IAbkHwLxAJLu";
            return new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build();
        }

        private static async Task<Return<ObjectStat>> CheckFileIsExistInBucket(string objectName, string bucketName, IMinioClient minio)
        {
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);
                var isExist = await minio.StatObjectAsync(statObjectArgs);

                return new Return<ObjectStat>
                {
                    Data = await minio.StatObjectAsync(statObjectArgs),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals(MinioErrorServerDefineEnum.NOT_FOUND_MINIO_SERVER))
                {
                    return new Return<ObjectStat>
                    {
                        Message = MinioErrorApplicationDefineEnum.NOT_FOUND,
                        IsSuccess = true // Why is true, because the action is success but the file is not found
                    };
                }
                else
                {
                    return new Return<ObjectStat>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR,
                        IsSuccess = false, // Why is false, because the action is fail
                        InternalErrorMessage = ex
                    };
                }
            }
        }
        #endregion
    }
}