using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.ResponseObject.Gate;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;

namespace FUParkingService
{
    public class GateService : IGateService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHelpperService _helpperService;
        private readonly IGateRepository _gateRepository;
        private readonly IParkingAreaRepository _parkingAreaRepository;

        public GateService(IUserRepository userRepository, IHelpperService helpperService, IGateRepository gateRepository, IParkingAreaRepository parkingAreaRepository)
        {
            _userRepository = userRepository;
            _helpperService = helpperService;
            _gateRepository = gateRepository;
            _parkingAreaRepository = parkingAreaRepository;
        }

        public async Task<Return<IEnumerable<GetGateResDto>>> GetAllGateAsync(GetListObjectWithFiller req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetGateResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _gateRepository.GetAllGateAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetGateResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetGateResDto>>
                {
                    Data = result.Data?.Select(x => new GetGateResDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description ?? "",
                        ParkingAreaName = x.ParkingArea?.Name ?? "",
                        GateTypeName = x.GateType?.Name ?? "",
                        StatusGate = x.StatusGate.ToString(),
                        CreatedBy = x.CreateBy?.Email ?? "",
                        LastModifyBy = x.LastModifyBy?.Email ?? ""
                    }),
                    IsSuccess = true,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                    TotalRecord = result.TotalRecord
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetGateResDto>>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }

        public async Task<Return<dynamic>> CreateGateAsync(CreateGateReqDto req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var isGateNameExisted = await _gateRepository.GetGateByNameAsync(req.Name);
                if (!isGateNameExisted.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.OBJECT_EXISTED,
                        InternalErrorMessage = isGateNameExisted.InternalErrorMessage
                    };
                }

                var isParkingAreaExist = await _parkingAreaRepository.GetParkingAreaByIdAsync(req.ParkingAreaId);
                if (!isParkingAreaExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isParkingAreaExist.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST,
                        InternalErrorMessage = isParkingAreaExist.InternalErrorMessage
                    };
                }

                var gate = new Gate
                {
                    Name = req.Name,
                    StatusGate = StatusParkingEnum.ACTIVE,
                    CreatedById = checkAuth.Data.Id,
                    Description = req.Description,
                    GateTypeId = req.GateTypeId,
                    ParkingAreaId = isParkingAreaExist.Data.Id
                };
                var result = await _gateRepository.CreateGateAsync(gate);

                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = result.InternalErrorMessage };
                }
                return new Return<dynamic> { Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY, IsSuccess = true };
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

        public async Task<Return<dynamic>> UpdateGateAsync(UpdateGateReqDto req, Guid id)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var existingGate = await _gateRepository.GetGateByIdAsync(id);
                if (!existingGate.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || existingGate.Data == null)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.GATE_NOT_EXIST
                    };
                }
                if (req.GateTypeId is not null)
                {
                    var isGateTypeExist = await _gateRepository.GetGateTypeByIdAsync(req.GateTypeId.Value);
                    if (!isGateTypeExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isGateTypeExist.Data == null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.GATE_TYPE_NOT_EXIST
                        };
                    }
                }
                if (req.ParkingAreaId is not null)
                {
                    var isParkingAreaExist = await _parkingAreaRepository.GetParkingAreaByIdAsync(req.ParkingAreaId.Value);
                    if (!isParkingAreaExist.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || isParkingAreaExist.Data == null)
                    {
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST
                        };
                    }
                }
                existingGate.Data.Name = req.Name ?? existingGate.Data.Name;
                existingGate.Data.Description = req.Description ?? existingGate.Data.Description;
                existingGate.Data.GateTypeId = req.GateTypeId ?? existingGate.Data.GateTypeId;
                existingGate.Data.ParkingAreaId = req.ParkingAreaId ?? existingGate.Data.ParkingAreaId;
                existingGate.Data.LastModifyById = checkAuth.Data.Id;
                existingGate.Data.LastModifyDate = DateTime.Now;
                var updateResult = await _gateRepository.UpdateGateAsync(existingGate.Data);
                if (!updateResult.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
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

        public async Task<Return<dynamic>> DeleteGate(Guid id)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.MANAGER);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                // Check if gateId exists
                var existedGate = await _gateRepository.GetGateByIdAsync(id);
                if (existedGate.Data == null || !existedGate.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = existedGate.InternalErrorMessage,
                        Message = ErrorEnumApplication.GATE_NOT_EXIST
                    };
                }
                existedGate.Data.DeletedDate = DateTime.Now;
                existedGate.Data.LastModifyById = checkAuth.Data.Id;
                existedGate.Data.LastModifyDate = DateTime.Now;
                var result = await _gateRepository.UpdateGateAsync(existedGate.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.DELETE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
