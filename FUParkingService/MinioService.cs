using FUParkingModel.Enum;
using FUParkingModel.RequestObject;
using FUParkingModel.ReturnCommon;
using FUParkingModel.ReturnObject;
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
                    if (isExist.SuccessfullyMessage == MinioErrorApplicationDefineEnum.NOT_FOUND)
                    {
                        return new Return<ReturnObjectUrlResDto>
                        {
                            ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = MinioErrorApplicationDefineEnum.NOT_FOUND
                        };
                    }
                    else
                    {
                        return new Return<ReturnObjectUrlResDto>
                        {
                            Data = new ReturnObjectUrlResDto
                            {
                                ObjUrl = "https://miniofile.khangbpa.com/parking/" + req.ObjName
                            },
                            IsSuccess = true,
                            SuccessfullyMessage = SuccessfullyEnumServer.FOUND_OBJECT
                        };
                    }
                }
                else
                {
                    return new Return<ReturnObjectUrlResDto>
                    {
                        ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = isExist.InternalErrorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new Return<ReturnObjectUrlResDto>
                {
                    ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
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
                    if (isExist.SuccessfullyMessage == MinioErrorApplicationDefineEnum.NOT_FOUND)
                    {
                        var uploadObjectArgs = new PutObjectArgs()
                            .WithBucket(req.BucketName)
                            .WithContentType(req.ObjFile.ContentType)
                            .WithObjectSize(req.ObjFile.Length)
                            .WithFileName(req.ObjName)
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
                                    ObjUrl = "https://miniofile.khangbpa.com/parking/" + req.ObjName
                                },
                                IsSuccess = true,
                                SuccessfullyMessage = SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY
                            };
                        }
                        else
                        {
                            return new Return<ReturnObjectUrlResDto>
                            {
                                ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                                IsSuccess = false,
                                InternalErrorMessage = MinioErrorApplicationDefineEnum.NOT_FOUND
                            };
                        }                       
                    }
                    else
                    {
                        return new Return<ReturnObjectUrlResDto>
                        {
                            ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                            IsSuccess = false,
                            InternalErrorMessage = MinioErrorApplicationDefineEnum.FILE_NAME_IS_EXIST
                        };
                    }
                }
                else
                {
                    return new Return<ReturnObjectUrlResDto>
                    {
                        ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                        IsSuccess = false,
                        InternalErrorMessage = isExist.InternalErrorMessage
                    };
                }
            } 
            catch (Exception ex)
            {
                return new Return<ReturnObjectUrlResDto>
                {
                    ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                    IsSuccess = false,
                    InternalErrorMessage = ex.Message
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
                    SuccessfullyMessage = SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals(MinioErrorServerDefineEnum.NOT_FOUND_MINIO_SERVER))
                {
                    return new Return<ObjectStat>
                    {
                        SuccessfullyMessage = MinioErrorApplicationDefineEnum.NOT_FOUND,
                        IsSuccess = true // Why is true, because the action is success but the file is not found
                    };
                }
                else
                {
                    return new Return<ObjectStat>
                    {
                        ErrorMessage = ErrorEnumApplication.SERVER_ERROR,
                        IsSuccess = false, // Why is false, because the action is fail
                        InternalErrorMessage = ex.Message
                    };
                }
            }
        }
        #endregion
    }
}
