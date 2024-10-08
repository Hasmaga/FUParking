﻿using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject;
using FUParkingModel.RequestObject.Common;
using FUParkingModel.RequestObject.ParkingArea;
using FUParkingModel.ResponseObject.Gate;
using FUParkingModel.ResponseObject.ParkingArea;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using FUParkingService.Interface;
using System.Transactions;

namespace FUParkingService
{
    public class ParkingAreaService : IParkingAreaService
    {
        private readonly IParkingAreaRepository _parkingAreaRepository;
        private readonly IHelpperService _helpperService;
        private readonly ISessionRepository _sessionRepository;
        private readonly IGateRepository _gateRepository;

        public ParkingAreaService(IParkingAreaRepository parkingAreaRepository, IHelpperService helpperService, ISessionRepository sessionRepository, IGateRepository gateRepository)
        {
            _parkingAreaRepository = parkingAreaRepository;
            _helpperService = helpperService;
            _sessionRepository = sessionRepository;
            _gateRepository = gateRepository;
        }

        public async Task<Return<dynamic>> DeleteParkingArea(Guid id)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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
                // Check if ParkingAreaId exists
                var existedParking = await _parkingAreaRepository.GetParkingAreaByIdAsync(id);
                if (existedParking.Data == null || existedParking.IsSuccess is not true)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = existedParking.InternalErrorMessage,
                        Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST
                    };
                }
                // Check any session is using this parking area
                var isUsingParkingArea = await _sessionRepository.GetListSessionActiveByParkingIdAsync(id);
                if (!isUsingParkingArea.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isUsingParkingArea.InternalErrorMessage,
                        Message = ErrorEnumApplication.PARKING_AREA_IS_USING
                    };
                }
                // check if parking area is virtual
                if (existedParking.Data.Name == GateTypeEnum.VIRUTAL)
                {
                    return new Return<dynamic>
                    {
                        Message = ErrorEnumApplication.CANNOT_DELETE_VIRTUAL_PARKING_AREA
                    };
                }
                existedParking.Data.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                existedParking.Data.LastModifyById = checkAuth.Data.Id;
                existedParking.Data.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                var result = await _parkingAreaRepository.UpdateParkingAreaAsync(existedParking.Data);
                if (!result.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                // Delete all gate of this parking area
                var gates = await _gateRepository.GetAllGateByParkingAreaAsync(id);
                if (gates.IsSuccess && gates.Data is not null)
                {
                    foreach (var gate in gates.Data)
                    {
                        gate.DeletedDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        gate.LastModifyById = checkAuth.Data.Id;
                        gate.LastModifyDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        var resultGate = await _gateRepository.UpdateGateAsync(gate);
                        if (!resultGate.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                        {
                            
                            return new Return<dynamic>
                            {
                                InternalErrorMessage = resultGate.InternalErrorMessage,
                                Message = ErrorEnumApplication.SERVER_ERROR
                            };
                        }
                    }
                }
                scope.Complete();
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

        public async Task<Return<dynamic>> CreateParkingAreaAsync(CreateParkingAreaReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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
                // Check for duplicate parking area name
                var isDuplicateName = await _parkingAreaRepository.GetParkingAreaByNameAsync(req.Name);
                if (!isDuplicateName.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT) || isDuplicateName.Data != null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDuplicateName.InternalErrorMessage,
                        Message = ErrorEnumApplication.OBJECT_EXISTED
                    };
                }

                string? mode;
                switch (req.Mode)
                {
                    case 1:
                        mode = ModeEnum.MODE1;
                        break;
                    case 2:
                        mode = ModeEnum.MODE2;
                        break;
                    case 3:
                        mode = ModeEnum.MODE3;
                        break;
                    case 4:
                        mode = ModeEnum.MODE4;
                        break;
                    default:
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.INVALID_INPUT
                        };
                }
                var parkingArea = new ParkingArea
                {
                    Mode = mode ?? ModeEnum.MODE1,
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Name = req.Name,
                    Description = req.Description,
                    MaxCapacity = req.MaxCapacity,
                    Block = req.Block,
                    CreatedById = checkAuth.Data.Id,
                };
                var result = await _parkingAreaRepository.CreateParkingAreaAsync(parkingArea);
                if (!result.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                // Create virtual parkingArea if have
                var isExistVirtual = await _parkingAreaRepository.GetParkingAreaByNameAsync(GateTypeEnum.VIRUTAL);
                if (!isExistVirtual.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isExistVirtual.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isExistVirtual.Data is null)
                {
                    var virtualParkingArea = new ParkingArea
                    {
                        Mode = ModeEnum.MODE1,
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                        Name = GateTypeEnum.VIRUTAL,
                        Description = "Virtual parking area",
                        MaxCapacity = 100000,
                        Block = 0,
                        CreatedById = checkAuth.Data.Id,
                    };
                    var resultVirtual = await _parkingAreaRepository.CreateParkingAreaAsync(virtualParkingArea);
                    if (!resultVirtual.IsSuccess || resultVirtual.Data is null)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = resultVirtual.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    // Create virtual gate
                    var virtualGate = new Gate
                    {
                        Name = GateTypeEnum.VIRUTAL,
                        ParkingAreaId = resultVirtual.Data.Id,
                        StatusGate = StatusGateEnum.ACTIVE,
                        CreatedById = checkAuth.Data.Id
                    };
                    var resultGate = await _gateRepository.CreateGateAsync(virtualGate);
                    if (!resultGate.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = resultGate.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }                
                scope.Complete();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
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

        public async Task<Return<IEnumerable<GetParkingAreaReqDto>>> GetParkingAreasAsync(GetListObjectWithFiller req)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.SUPERVISOR);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetParkingAreaReqDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _parkingAreaRepository.GetAllParkingAreasAsync(req);
                if (!result.IsSuccess)
                {
                    return new Return<IEnumerable<GetParkingAreaReqDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetParkingAreaReqDto>>
                {
                    Data = result.Data?.Select(p => new GetParkingAreaReqDto
                    {
                        Id = p.Id,
                        Block = p.Block,
                        CreateBy = p.CreateBy?.Email ?? "",
                        CreateDate = p.CreatedDate,
                        Description = p.Description ?? "",
                        Mode = p.Mode switch
                        {
                            ModeEnum.MODE1 => 1,
                            ModeEnum.MODE2 => 2,
                            ModeEnum.MODE3 => 3,
                            ModeEnum.MODE4 => 4,
                            _ => 0 
                        },
                        LastModifyBy = p.LastModifyBy?.Email,
                        LastModifyDate = p.LastModifyDate,
                        MaxCapacity = p.MaxCapacity,
                        Name = p.Name,
                        StatusParkingArea = p.StatusParkingArea
                    }),
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT,
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetParkingAreaReqDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> UpdateParkingAreaAsync(UpdateParkingAreaReqDto req)
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
                // Check ParkingArea
                var existingParkingArea = await _parkingAreaRepository.GetParkingAreaByIdAsync(req.ParkingAreaId);
                if (existingParkingArea.Data == null || !existingParkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST, InternalErrorMessage = existingParkingArea.InternalErrorMessage };
                }

                if (!string.IsNullOrWhiteSpace(req.Name) && !req.Name.Trim().Equals(existingParkingArea.Data.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var isNameParkingExist = await _parkingAreaRepository.GetParkingAreaByNameAsync(req.Name);
                    if (!isNameParkingExist.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                    {
                        return new Return<dynamic> { Message = ErrorEnumApplication.OBJECT_EXISTED, InternalErrorMessage = existingParkingArea.InternalErrorMessage };
                    }
                    existingParkingArea.Data.Name = req.Name;
                }

                if (!string.IsNullOrEmpty(req.Description))
                {
                    existingParkingArea.Data.Description = req.Description;
                }
                if (req.MaxCapacity.HasValue)
                {
                    existingParkingArea.Data.MaxCapacity = req.MaxCapacity.Value;
                }
                if (req.Block.HasValue)
                {
                    existingParkingArea.Data.Block = req.Block.Value;
                }
                if (req.Mode is not null)
                {
                    switch (req.Mode)
                    {
                        case 1:
                            existingParkingArea.Data.Mode = ModeEnum.MODE1;
                            break;
                        case 2:
                            existingParkingArea.Data.Mode = ModeEnum.MODE2;
                            break;
                        case 3:
                            existingParkingArea.Data.Mode = ModeEnum.MODE3;
                            break;
                        case 4:
                            existingParkingArea.Data.Mode = ModeEnum.MODE4;
                            break;
                        default:
                            return new Return<dynamic> { Message = ErrorEnumApplication.INVALID_INPUT };
                    }
                }
                var updateResult = await _parkingAreaRepository.UpdateParkingAreaAsync(existingParkingArea.Data);
                if (!updateResult.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = existingParkingArea.InternalErrorMessage };
                }
                return new Return<dynamic> { Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, IsSuccess = true };
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

        public async Task<Return<dynamic>> UpdateStatusParkingAreaAsync(Guid parkingId, bool isActive)
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
                var parkingArea = await _parkingAreaRepository.GetParkingAreaByIdAsync(parkingId);
                if (parkingArea.Data == null || !parkingArea.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.PARKING_AREA_NOT_EXIST, InternalErrorMessage = parkingArea.InternalErrorMessage };
                }
                
                // Check any session is using this parking area
                var isUsingParkingArea = await _sessionRepository.GetListSessionActiveByParkingIdAsync(parkingId);
                if (!isUsingParkingArea.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT))
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isUsingParkingArea.InternalErrorMessage,
                        Message = ErrorEnumApplication.PARKING_AREA_IS_USING
                    };
                }
                if (isActive)
                {
                    if (parkingArea.Data.StatusParkingArea == StatusParkingEnum.ACTIVE)
                    {
                        return new Return<dynamic> { Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY };
                    }
                    parkingArea.Data.StatusParkingArea = StatusParkingEnum.ACTIVE;
                }
                else
                {
                    if (parkingArea.Data.StatusParkingArea == StatusParkingEnum.INACTIVE)
                    {
                        return new Return<dynamic> { Message = ErrorEnumApplication.STATUS_IS_ALREADY_APPLY };
                    }
                    parkingArea.Data.StatusParkingArea = StatusParkingEnum.INACTIVE;
                }
                var updateResult = await _parkingAreaRepository.UpdateParkingAreaAsync(parkingArea.Data);
                if (!updateResult.Message.Equals(SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY))
                {
                    return new Return<dynamic> { Message = ErrorEnumApplication.SERVER_ERROR, InternalErrorMessage = updateResult.InternalErrorMessage };
                }
                return new Return<dynamic> { Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY, IsSuccess = true };
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

        public async Task<Return<IEnumerable<GetParkingAreaOptionResDto>>> GetParkingAreaOptionAsync()
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<IEnumerable<GetParkingAreaOptionResDto>>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }
                var result = await _parkingAreaRepository.GetParkingAreaOptionAsync();
                if (!result.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                {
                    return new Return<IEnumerable<GetParkingAreaOptionResDto>>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                return new Return<IEnumerable<GetParkingAreaOptionResDto>>
                {
                    Data = result.Data?.Select(p => new GetParkingAreaOptionResDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description ?? "",
                    }),
                    IsSuccess = true,
                    TotalRecord = result.TotalRecord,
                    Message = result.TotalRecord > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception ex)
            {
                return new Return<IEnumerable<GetParkingAreaOptionResDto>>
                {
                    InternalErrorMessage = ex,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<dynamic>> CreateParkingAreaAndGateAsync(CreateParkingAreaAndGateReqDto req)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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

                // Check for duplicate parking area name
                var isDuplicateName = await _parkingAreaRepository.GetParkingAreaByNameAsync(req.Name);
                if (!isDuplicateName.Message.Equals(ErrorEnumApplication.NOT_FOUND_OBJECT)) {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isDuplicateName.InternalErrorMessage,
                        Message = ErrorEnumApplication.OBJECT_EXISTED
                    };
                }

                string? mode;
                switch (req.Mode)
                {
                    case 1:
                        mode = ModeEnum.MODE1;
                        break;
                    case 2:
                        mode = ModeEnum.MODE2;
                        break;
                    case 3:
                        mode = ModeEnum.MODE3;
                        break;
                    case 4:
                        mode = ModeEnum.MODE4;
                        break;
                    default:
                        return new Return<dynamic>
                        {
                            Message = ErrorEnumApplication.INVALID_INPUT
                        };
                }
                var parkingArea = new ParkingArea
                {
                    Mode = mode ?? ModeEnum.MODE1,
                    StatusParkingArea = StatusParkingEnum.ACTIVE,
                    Name = req.Name,
                    Description = req.Description,
                    MaxCapacity = req.MaxCapacity,
                    Block = req.Block,
                    CreatedById = checkAuth.Data.Id,
                };
                var result = await _parkingAreaRepository.CreateParkingAreaAsync(parkingArea);
                if (!result.IsSuccess || result.Data is null)
                {
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }

                if (req.Gates is not null && req.Gates.Length > 0)
                {
                    foreach (var gate in req.Gates)
                    {
                        var gateResult = await _gateRepository.GetGateByNameAsync(gate.Name);
                        if (!gateResult.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT))
                        {
                            var newGate = new Gate
                            {
                                Name = gate.Name,
                                ParkingAreaId = result.Data.Id,
                                StatusGate = StatusGateEnum.ACTIVE,
                                CreatedById = checkAuth.Data.Id
                            };
                            var createGateResult = await _gateRepository.CreateGateAsync(newGate);
                            if (!createGateResult.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                            {
                                scope.Dispose();
                                return new Return<dynamic>
                                {
                                    InternalErrorMessage = createGateResult.InternalErrorMessage,
                                    Message = ErrorEnumApplication.SERVER_ERROR
                                };
                            }
                        }
                    }
                }
                // Create virtual parkingArea if have
                var isExistVirtual = await _parkingAreaRepository.GetParkingAreaByNameAsync(GateTypeEnum.VIRUTAL);
                if (!isExistVirtual.IsSuccess)
                {
                    scope.Dispose();
                    return new Return<dynamic>
                    {
                        InternalErrorMessage = isExistVirtual.InternalErrorMessage,
                        Message = ErrorEnumApplication.SERVER_ERROR
                    };
                }
                if (isExistVirtual.Data is null)
                {
                    var virtualParkingArea = new ParkingArea
                    {
                        Mode = ModeEnum.MODE1,
                        StatusParkingArea = StatusParkingEnum.ACTIVE,
                        Name = GateTypeEnum.VIRUTAL,
                        Description = "Virtual parking area",
                        MaxCapacity = 100000,
                        Block = 0,
                        CreatedById = checkAuth.Data.Id,
                    };
                    var resultVirtual = await _parkingAreaRepository.CreateParkingAreaAsync(virtualParkingArea);
                    if (!resultVirtual.IsSuccess || resultVirtual.Data is null)
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = resultVirtual.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                    // Create virtual gate
                    var virtualGate = new Gate
                    {
                        Name = GateTypeEnum.VIRUTAL,
                        ParkingAreaId = resultVirtual.Data.Id,
                        StatusGate = StatusGateEnum.ACTIVE,
                        CreatedById = checkAuth.Data.Id
                    };
                    var resultGate = await _gateRepository.CreateGateAsync(virtualGate);
                    if (!resultGate.Message.Equals(SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY))
                    {
                        scope.Dispose();
                        return new Return<dynamic>
                        {
                            InternalErrorMessage = resultGate.InternalErrorMessage,
                            Message = ErrorEnumApplication.SERVER_ERROR
                        };
                    }
                }
                scope.Complete();
                return new Return<dynamic>
                {
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
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

        public async Task<Return<GetParkingAreaReqDto>> GetParkingAreaByParkingIdAsync(Guid id)
        {
            try
            {
                var checkAuth = await _helpperService.ValidateUserAsync(RoleEnum.STAFF);
                if (!checkAuth.IsSuccess || checkAuth.Data is null)
                {
                    return new Return<GetParkingAreaReqDto>
                    {
                        InternalErrorMessage = checkAuth.InternalErrorMessage,
                        Message = checkAuth.Message
                    };
                }

                var result = await _parkingAreaRepository.GetParkingAreaByParkingIdAsync(id);
                if (!result.Message.Equals(SuccessfullyEnumServer.FOUND_OBJECT) || result.Data == null)
                {
                    return new Return<GetParkingAreaReqDto>
                    {
                        InternalErrorMessage = result.InternalErrorMessage,
                        Message = ErrorEnumApplication.GATE_NOT_EXIST
                    };
                }
                return new Return<GetParkingAreaReqDto>
                {
                    Data = new GetParkingAreaReqDto
                    {
                        Id = result.Data.Id,
                        Block = result.Data.Block,
                        CreateBy = result.Data.CreateBy?.Email ?? "",
                        CreateDate = result.Data.CreatedDate,
                        Description = result.Data.Description ?? "",
                        Mode = result.Data.Mode switch
                        {
                            ModeEnum.MODE1 => 1,
                            ModeEnum.MODE2 => 2,
                            ModeEnum.MODE3 => 3,
                            ModeEnum.MODE4 => 4,
                            _ => 0
                        },
                        LastModifyBy = result.Data.LastModifyBy?.Email,
                        LastModifyDate = result.Data.LastModifyDate,
                        MaxCapacity = result.Data.MaxCapacity,
                        Name = result.Data.Name,
                        StatusParkingArea = result.Data.StatusParkingArea
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                };
            } catch (Exception ex)
            {
                return new Return<GetParkingAreaReqDto>
                {
                    Message = ErrorEnumApplication.SERVER_ERROR,
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
