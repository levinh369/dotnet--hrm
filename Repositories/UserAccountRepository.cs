using DACN.Data;
using DACN.Models;
using static DACN.Enums.StatusEnums;

namespace DACN.Repositories
{
    public class UserAccountRepository
    {
        private readonly ApplicationDbContext db;

        public UserAccountRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        //public async Task Update(UserAccountModel acc)
        //{
        //    var account = await db.UserAccounts.FindAsync(acc.UserAccountId);
        //    if (account == null)
        //    {
        //        throw new Exception("Employee not found");
        //    }
        //    existingEmployee.Account.FullName = employee.Account.FullName;
        //    existingEmployee.Account.Email = employee.Account.Email;
        //    existingEmployee.Phone = employee.Phone;
        //    existingEmployee.Address = employee.Address;
        //    existingEmployee.DateOfBirth = employee.DateOfBirth;
        //    existingEmployee.CCCD = employee.CCCD;
        //    existingEmployee.Gender = employee.Gender;
        //    existingEmployee.UpdatedAt = DateTime.Now;
        //    db.Employees.Update(employee);
        //    await db.SaveChangesAsync();
        //}
        public async Task UpdateUserRoleAsync(int userAccountId, UserRole newRole,int employeeId)
        {
            var user = await db.UserAccounts.FindAsync(userAccountId);
            if (user == null)
                throw new Exception("UserAccount not found");

            user.Role = newRole;
            user.EmployeeId = employeeId;
            await db.SaveChangesAsync();
        }

    }
}
