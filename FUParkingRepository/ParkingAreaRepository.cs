﻿using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUParkingRepository
{
    public class ParkingAreaRepository : IParkingAreaRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public ParkingAreaRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<ParkingArea>> CreateParkingAreaAsync(ParkingArea parkingArea)
        {
            try
            {
                parkingArea.Mode = ModeEnum.MODE1;
                parkingArea.StatusParkingArea = StatusParkingEnum.ACTIVE;
                await _db.ParkingAreas.AddAsync(parkingArea);
                await _db.SaveChangesAsync();
                return new Return<ParkingArea>
                {
                    Data = parkingArea,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            } catch (Exception e)
            {
                return new Return<ParkingArea>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.ADD_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<ParkingArea>> GetParkingAreaByNameAsync(string name)
        {
            try
            {
                return new Return<ParkingArea>
                {
                    Data = await _db.ParkingAreas.FindAsync(name),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<ParkingArea>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }

        public async Task<Return<IEnumerable<ParkingArea>>> GetParkingAreasAsync()
        {
            try
            {
                return new Return<IEnumerable<ParkingArea>>
                {
                    Data = await _db.ParkingAreas.ToListAsync(),
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.GET_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<ParkingArea>>
                {
                    IsSuccess = false,
                    Message = ErrorEnumApplication.GET_OBJECT_ERROR,
                    InternalErrorMessage = e.Message
                };
            }
        }
    }
}
