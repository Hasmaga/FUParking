using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.Vehicle;
using FUParkingModel.ResponseObject.Vehicle;
using FUParkingModel.ResponseObject.VehicleType;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.Data;
using SkiaSharp;

namespace FUParkingService
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IHelpperService _helpperService;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMinioService _minioService;

        public VehicleService(IVehicleRepository vehicleRepository, IHelpperService helpperService, IUserRepository userRepository, ICustomerRepository customerRepository, IMinioService minioService)
        {
            _vehicleRepository = vehicleRepository;
            _helpperService = helpperService;
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _minioService = minioService;
        }

        public async Task<Return<IEnumerable<VehicleType>>> GetVehicleTypesAsync(GetListObjectWithFiller req)
        {
            try
            {
                // Validate token
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<VehicleType>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<VehicleType>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<VehicleType>>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                return await _vehicleRepository.GetAllVehicleTypeAsync(req);
            }
            catch (Exception)
            {
                return new Return<IEnumerable<VehicleType>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<GetVehicleTypeByCustomerResDto>>> GetListVehicleTypeByCustomer()
        {
            try
            {
                var result = await _vehicleRepository.GetAllVehicleTypeAsync();
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetVehicleTypeByCustomerResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR,
                    };
                }
                return new Return<IEnumerable<GetVehicleTypeByCustomerResDto>>
                {
                    Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY,
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord,
                    Data = result.Data?.Select(x => new GetVehicleTypeByCustomerResDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetVehicleTypeByCustomerResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> CreateVehicleTypeAsync(CreateVehicleTypeReqDto reqDto)
        {
            try
            {
                // check token valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check role = Manager, Supervisor
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                var vehicleTypes = await _vehicleRepository.GetVehicleTypeByName(reqDto.Name);

                if (vehicleTypes.Data != null && vehicleTypes.IsSuccess)
                {
                    if (vehicleTypes.Data.Name != null)
                    {
                        return new Return<bool>
                        {
                            IsSuccess = false,
                            Message = ErrorEnumApplication.OBJECT_EXISTED
                        };
                    }
                }

                var vehicleType = new VehicleType
                {
                    Name = reqDto.Name,
                    Description = reqDto.Description
                };

                var result = await _vehicleRepository.CreateVehicleTypeAsync(vehicleType);
                return new Return<bool>
                {
                    IsSuccess = result.IsSuccess,
                    Data = result.IsSuccess,
                    Message = result.IsSuccess ? SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY : ErrorEnumApplication.SERVER_ERROR
                };
            }
            catch (Exception)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<bool>> UpdateVehicleTypeAsync(Guid Id, UpdateVehicleTypeReqDto reqDto)
        {
            try
            {
                // check token valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                // Check role = Manager, Supervisor
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }

                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<bool> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                // Check the vehicle type id exists
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(Id);
                if (vehicleType.Data == null || vehicleType.IsSuccess == false)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }

                // Check for duplicate vehicle type name with other vehicle types (except the current vehicle type)
                var vehicleTypes = await _vehicleRepository.GetVehicleTypeByName(reqDto.Name ?? "");
                if (vehicleTypes.Data != null)
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.OBJECT_EXISTED
                    };
                }

                // Update from here
                if (reqDto.Name != null && reqDto.Name != "")
                {
                    vehicleType.Data.Name = reqDto.Name;
                }
                if (reqDto.Description != null && reqDto.Description != "")
                {
                    vehicleType.Data.Description = reqDto.Description;
                }

                var result = await _vehicleRepository.UpdateVehicleTypeAsync(vehicleType.Data);
                if (result.IsSuccess)
                {
                    return new Return<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                    };
                }
                else
                {
                    return new Return<bool>
                    {
                        IsSuccess = false,
                        Message = ErrorEnumApplication.UPDATE_OBJECT_ERROR
                    };
                }
            }
            catch (Exception)
            {
                return new Return<bool>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
        public async Task<Return<IEnumerable<Vehicle>>> GetVehiclesAsync()
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<IEnumerable<Vehicle>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                // Check role 
                var userlogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userlogged.Data == null || userlogged.IsSuccess == false)
                {
                    return new Return<IEnumerable<Vehicle>>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                if (!Auth.AuthSupervisor.Contains(userlogged.Data.Role?.Name ?? ""))
                {
                    return new Return<IEnumerable<Vehicle>> { IsSuccess = false, Message = ErrorEnumApplication.NOT_AUTHORITY };
                }

                return await _vehicleRepository.GetVehiclesAsync();
            }
            catch
            {
                return new Return<IEnumerable<Vehicle>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<GetCustomerVehicleByCustomerResDto>>> GetCustomerVehicleByCustomerIdAsync()
        {
            Return<IEnumerable<GetCustomerVehicleByCustomerResDto>> res = new()
            {
                Message = ErrorEnumApplication.SERVER_ERROR
            };
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }
                Return<Customer> customerLogin = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!customerLogin.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || customerLogin.Data == null)
                {
                    res.Message = ErrorEnumApplication.NOT_AUTHORITY;
                    return res;
                }

                if (customerLogin.Data.StatusCustomer.ToLower().Equals(StatusCustomerEnum.INACTIVE.ToLower()))
                {
                    res.Message = ErrorEnumApplication.BANNED;
                    return res;
                }
                var result = await _vehicleRepository.GetAllCustomerVehicleByCustomerIdAsync(customerLogin.Data.Id);
                if (!result.IsSuccess)
                {
                    res.InternalErrorMessage = result.InternalErrorMessage;
                    return res;
                }
                res.Data = result.Data?.Select(x => new GetCustomerVehicleByCustomerResDto
                {
                    Id = x.Id,
                    PlateNumber = x.PlateNumber,
                    VehicleTypeName = x.VehicleType?.Name ?? "",
                    PlateImage = x.PlateImage,
                    StatusVehicle = x.StatusVehicle,
                    CreateDate = x.CreatedDate
                });
                res.TotalRecord = result.TotalRecord;
                res.IsSuccess = true;
                res.Message = SuccessfullyEnumServer.GET_INFORMATION_SUCCESSFULLY;
                return res;
            }
            catch (Exception ex)
            {
                res.InternalErrorMessage = ex;
                return res;
            }
        }

        public async Task<Return<dynamic>> DeleteVehicleTypeAsync(Guid id)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var userLogged = await _userRepository.GetUserByIdAsync(_helpperService.GetAccIdFromLogged());

                if (!userLogged.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = userLogged.InternalErrorMessage };

                if (userLogged.Data == null || !userLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                if (!Auth.AuthSupervisor.Contains(userLogged.Data.Role?.Name ?? ""))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(id);

                if (!vehicleType.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicleType.InternalErrorMessage };

                if (vehicleType.Data == null || !vehicleType.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST };

                vehicleType.Data.DeletedDate = DateTime.Now;

                var result = await _vehicleRepository.UpdateVehicleTypeAsync(vehicleType.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<GetInformationVehicleCreateResDto>> CreateCustomerVehicleAsync(CreateCustomerVehicleReqDto reqDto)
        {
            try
            {
                // Check token is valid
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }
                var userLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (userLogged.Data == null || !userLogged.IsSuccess)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.NOT_AUTHORITY
                    };
                }                
                var vehicleType = await _vehicleRepository.GetVehicleTypeByIdAsync(reqDto.VehicleTypeId);
                if (vehicleType.Data == null || !vehicleType.IsSuccess)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.VEHICLE_TYPE_NOT_EXIST
                    };
                }
                // Check vehicle image using ML                
                MLPlateDetecive.ModelInput checkIsPlateNumber = new()
                {
                    Image = MLImage.CreateFromStream(reqDto.PlateImage.OpenReadStream()),
                };
                var predictionResult = MLPlateDetecive.Predict(checkIsPlateNumber);
                if (predictionResult.PredictedBoundingBoxes == null)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.MUST_HAVE_ONLY_ONE_PLATE_NUMBER
                    };
                }
                var fileExtensionPlateNumber = Path.GetExtension(reqDto.PlateImage.FileName);
                var objNamePlateNumber = userLogged.Data.Id + "_" + "_" + DateTime.Now.Date.ToString("dd-MM-yyyy") + "_plateNumber" + fileExtensionPlateNumber;
                var boxes = predictionResult.PredictedBoundingBoxes.Chunk(4)
                    .Select(x => new { XTop = x[0], YTop = x[1], XBottom = x[2], YBottom = x[3] })
                    .Zip(predictionResult.Score, (a, b) => new { Box = a, Score = b });
                if (boxes.Count() > 1)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.MUST_HAVE_ONLY_ONE_PLATE_NUMBER
                    };
                }
                var boxe = boxes.First();
                // Cut image to plate number
                FormFile imagePlateNumber;
                byte[] croppedImageData;
                string contentType = reqDto.PlateImage.ContentType;
                using (var imageStream = reqDto.PlateImage.OpenReadStream())
                {
                    var bitmap = SKBitmap.Decode(imageStream);
                    var rect = new SKRect(boxe.Box.XTop, boxe.Box.YTop, boxe.Box.XBottom, boxe.Box.YBottom);
                    using (var croppedBitmap = new SKBitmap((int)rect.Width, (int)rect.Height))
                    {
                        SKRect dest = new SKRect(0, 0, croppedBitmap.Width, croppedBitmap.Height);
                        using (var canvas = new SKCanvas(croppedBitmap))
                        {
                            canvas.DrawBitmap(bitmap, rect, dest);
                        }
                        using (var ms = new MemoryStream())
                        {
                            switch (fileExtensionPlateNumber)
                            {
                                case ".jpg":
                                    croppedBitmap.Encode(ms, SKEncodedImageFormat.Jpeg, 100);
                                    break;
                                case ".jpeg":
                                    croppedBitmap.Encode(ms, SKEncodedImageFormat.Jpeg, 100);
                                    break;
                                case ".png":
                                    croppedBitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                                    break;
                                default:
                                    return new Return<GetInformationVehicleCreateResDto>
                                    {
                                        Message = ErrorEnumApplication.FILE_EXTENSION_NOT_SUPPORT
                                    };
                            }
                            croppedImageData = ms.ToArray();
                        }
                    }
                }

                MLTextDetection.ModelInput textDetectionPlateNumber = new()
                {
                    Image = MLImage.CreateFromStream(new MemoryStream(croppedImageData)),
                };
                var textDetectionResult = MLTextDetection.Predict(textDetectionPlateNumber);
                if (textDetectionResult.PredictedBoundingBoxes == null)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.CANNOT_READ_TEXT_FROM_IMAGE
                    };
                }
                // Get all object detected in image and sort from top to bottom and top left to top right and bottom left to bottom right
                const float yTolerance = 10.0f;
                var detectedObjects = textDetectionResult.PredictedBoundingBoxes
                    .Chunk(4)
                    .Select(x => new { XTop = x[0], YTop = x[1], XBottom = x[2], YBottom = x[3] })
                    .Zip(textDetectionResult.Score, (a, b) => new { a.XTop, a.YTop, a.XBottom, a.YBottom, Score = b })
                    .Zip(textDetectionResult.PredictedLabel, (a, b) => new { a.XTop,  a.YTop, a.XBottom, a.YBottom, a.Score, Label = b })
                    .Select(
                        x => new
                        {
                            x.XTop,
                            x.YTop,
                            x.XBottom,
                            x.YBottom,
                            x.Score,
                            x.Label
                        }
                    )
                    .GroupBy(obj => Math.Floor(obj.YTop / yTolerance))
                    .OrderBy(group => group.Min(obj => obj.YTop))
                    .SelectMany(group => group.OrderBy(obj => obj.XTop))
                    .Where(x => x.Score > 0.5)
                    .ToList();

                if (detectedObjects.Count == 0)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        Message = ErrorEnumApplication.CANNOT_READ_TEXT_FROM_IMAGE
                    };
                }
                var plateNumber = detectedObjects.Select(x => x.Label).Aggregate((a, b) => a + b);
                // Check plate number is existed
                //var vehicle = await _vehicleRepository.GetVehicleByPlateNumberAsync(plateNumber);
                //if (!vehicle.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                //{
                //    if (vehicle.Message.Equals(ErrorEnumApplication.SERVER_ERROR))
                //    {
                //        return new Return<dynamic>
                //        {
                //            InternalErrorMessage = vehicle.InternalErrorMessage,
                //            Message = ErrorEnumApplication.SERVER_ERROR
                //        };
                //    }
                //    return new Return<dynamic>
                //    {
                //        Message = ErrorEnumApplication.PLATE_NUMBER_IS_EXIST
                //    };
                //}
                using (var memoryStream = new MemoryStream(croppedImageData))
                {
                    memoryStream.Position = 0; // Reset the position to the beginning after copying

                    // Create a new FormFile using the MemoryStream
                    imagePlateNumber = new FormFile(memoryStream, 0, memoryStream.Length, reqDto.PlateImage.Name, reqDto.PlateImage.FileName)
                    {
                        Headers = reqDto.PlateImage.Headers,
                        ContentType = reqDto.PlateImage.ContentType,
                        ContentDisposition = reqDto.PlateImage.ContentDisposition,
                    };
                    // Prepare the UploadObjectReqDto with the new FormFile
                    UploadObjectReqDto imageUpload = new()
                    {
                        ObjFile = imagePlateNumber,
                        ObjName = objNamePlateNumber,
                        BucketName = BucketMinioEnum.BUCKET_IMAGE_VEHICLE
                    };

                    // Now, memoryStream will remain open for the duration of this upload operation
                    var resultUploadImagePlateNumber = await _minioService.UploadObjectAsync(imageUpload);
                    if (!resultUploadImagePlateNumber.Message.Equals(SuccessfullyEnumServer.UPLOAD_OBJECT_SUCCESSFULLY))
                    {
                        return new Return<GetInformationVehicleCreateResDto>
                        {
                            InternalErrorMessage = resultUploadImagePlateNumber.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR,
                        };
                    }
                }
                var newVehicle = new Vehicle
                {
                    PlateNumber = plateNumber,
                    VehicleTypeId = reqDto.VehicleTypeId,
                    CustomerId = userLogged.Data.Id,
                    PlateImage = "https://miniofile.khangbpa.com/" + BucketMinioEnum.BUCKET_IMAGE_VEHICLE + "/" + objNamePlateNumber,
                    StatusVehicle = StatusVehicleEnum.PENDING
                };
                var result = await _vehicleRepository.CreateVehicleAsync(newVehicle);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY) || result.Data == null)
                {
                    return new Return<GetInformationVehicleCreateResDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<GetInformationVehicleCreateResDto>
                {
                    IsSuccess = true,
                    Data = new GetInformationVehicleCreateResDto
                    {
                        VehicleId = result.Data.Id,
                        PlateNumber = result.Data.PlateNumber,
                        VehicleTypeName = vehicleType.Data.Name,
                        ImagePlateNumber = result.Data.PlateImage                        
                    },
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<GetInformationVehicleCreateResDto>()
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateVehicleInformationAsync(UpdateCustomerVehicleReqDto req)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var userLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!userLogged.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = userLogged.InternalErrorMessage };

                if (userLogged.Data == null || !userLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(req.VehicleId);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };

                if (vehicle.Data == null || !vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };

                if (vehicle.Data.CustomerId != userLogged.Data.Id)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                if (!vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.PENDING))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                if (req.PlateNumber != null)
                    vehicle.Data.PlateNumber = req.PlateNumber;

                if (req.VehicleTypeId != null)
                    vehicle.Data.VehicleTypeId = req.VehicleTypeId.Value;

                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> DeleteVehicleByCustomerAsync(Guid vehicleId)
        {
            try
            {
                var isValidToken = _helpperService.IsTokenValid();
                if (!isValidToken)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var userLogged = await _customerRepository.GetCustomerByIdAsync(_helpperService.GetAccIdFromLogged());
                if (!userLogged.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = userLogged.InternalErrorMessage };

                if (userLogged.Data == null || !userLogged.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
                if (!vehicle.IsSuccess)
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = vehicle.InternalErrorMessage };

                if (vehicle.Data == null || !vehicle.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                    return new Return<dynamic> { Message = ErrorEnumApplication.VEHICLE_NOT_EXIST };

                if (vehicle.Data.CustomerId != userLogged.Data.Id)
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                if (!vehicle.Data.StatusVehicle.Equals(StatusVehicleEnum.PENDING))
                    return new Return<dynamic> { Message = ErrorEnumApplication.NOT_AUTHORITY };

                vehicle.Data.DeletedDate = DateTime.Now;

                var result = await _vehicleRepository.UpdateVehicleAsync(vehicle.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                return new Return<dynamic> { IsSuccess = true, Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
