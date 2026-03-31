using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CheapDeal.WebApp.Models;

namespace CheapDeal.WebApp.DAL
{
    public class ContractRepository
    {
        private readonly ShopDbContext _db;

        public ContractRepository(ShopDbContext db)
        {
            _db = db;
        }

        // Lấy danh sách hợp đồng có phân trang
        public IQueryable<Contract> GetAll()
        {
            return _db.Contracts
                .Include(c => c.Status)
                .Include(c => c.Customer)
                .OrderByDescending(c => c.CreatedDate);
        }

        // Lấy chi tiết 1 hợp đồng
        public Contract GetById(int id)
        {
            return _db.Contracts
                .Include(c => c.Status)
                .Include(c => c.Customer)
                .Include(c => c.Schedules)
                .FirstOrDefault(c => c.ContractId == id);
        }

        // Tạo hợp đồng mới (gọi Stored Procedure)
        public int Create(Contract contract, int installmentCount)
        {
            // Dùng SP đã tạo ở tuần 9
            var sql = @"DECLARE @newId INT
                        EXEC sp_CreateContract 
                            @CustomerId=@p0, @ContractCode=@p1, @ContractDate=@p2,
                            @StartDate=@p3, @EndDate=@p4, @TotalAmount=@p5,
                            @InstallmentCount=@p6, @CreatedBy=@p7, @NewContractId=@newId OUTPUT
                        SELECT @newId";

            return _db.Database.SqlQuery<int>(sql,
                contract.CustomerId, contract.ContractCode, contract.ContractDate,
                contract.StartDate, contract.EndDate, contract.TotalAmount,
                installmentCount, contract.CreatedBy).FirstOrDefault();
        }
    }
}