-- Tạo Database (Nếu chưa có)
-- CREATE DATABASE QLNhanVien;
-- GO
-- USE QLNhanVien;
-- GO

-- 1. Bảng Departments (Phòng ban)
CREATE TABLE Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    IsActive BIT DEFAULT 1
);
GO

-- 2. Bảng Positions (Chức vụ/Vị trí)
CREATE TABLE Positions (
    PositionId INT IDENTITY(1,1) PRIMARY KEY,
    PositionName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
);
GO

-- 3. Bảng UserAccounts (Tài khoản người dùng)
-- Lưu ý: EmployeeId sẽ được thêm khóa ngoại sau khi tạo bảng Employees
CREATE TABLE UserAccounts (
    UserAccountId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255),
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    ActivationToken NVARCHAR(MAX),
    TokenExpiry DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    Role NVARCHAR(50), -- Admin, HR, Employee, Candidate
    EmployeeId INT NULL -- Sẽ link tới bảng Employees
);
GO

-- 4. Bảng Employees (Hồ sơ nhân viên)
CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    DateOfBirth DATETIME2,
    Gender NVARCHAR(20),
    Address NVARCHAR(MAX),
    Phone NVARCHAR(20),
    CCCD NVARCHAR(20), -- Căn cước công dân
    StartDate DATETIME2,
    Status NVARCHAR(50), -- Working, Resigned, OnLeave
    DepartmentId INT,
    PositionId INT,
    UserAccountId INT UNIQUE, -- 1 User chỉ ứng với 1 Employee
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    AvatarUrl NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    FOREIGN KEY (PositionId) REFERENCES Positions(PositionId),
    FOREIGN KEY (UserAccountId) REFERENCES UserAccounts(UserAccountId)
);
GO

-- Cập nhật lại UserAccounts để trỏ ngược về Employees (Mối quan hệ 1-1)
ALTER TABLE UserAccounts
ADD CONSTRAINT FK_UserAccounts_Employees
FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId);
GO

-- 5. Bảng JobPosts (Tin tuyển dụng)
CREATE TABLE JobPosts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    SalaryRange NVARCHAR(100),
    JobDescription NVARCHAR(MAX),
    Requirements NVARCHAR(MAX),
    PostedDate DATETIME2 DEFAULT GETDATE(),
    ExpirationDate DATETIME2,
    UpdatedAt DATETIME2,
    Status NVARCHAR(50), -- Open, Closed, Draft
    DepartmentId INT,
    PositionId INT,
    CreatedById INT, -- Người tạo (thường là HR)
    EmploymentType NVARCHAR(50), -- Full-time, Part-time
    RequiredExperience NVARCHAR(100),
    ViewCount INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    IsDeleted BIT DEFAULT 0,

    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    FOREIGN KEY (PositionId) REFERENCES Positions(PositionId),
    FOREIGN KEY (CreatedById) REFERENCES UserAccounts(UserAccountId)
);
GO

-- 6. Bảng JobApplications (Đơn ứng tuyển)
CREATE TABLE JobApplications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NULL, -- Nếu là nhân viên nội bộ ứng tuyển
    UserAccountId INT, -- Tài khoản ứng viên
    JobPostingId INT,
    Skills NVARCHAR(MAX),
    CVFilePath NVARCHAR(MAX),
    SubmittedDate DATETIME2 DEFAULT GETDATE(),
    Status NVARCHAR(50), -- Pending, Interview, Passed, Rejected
    Notes NVARCHAR(MAX),
    ReviewedDate DATETIME2,
    AppliedType NVARCHAR(50), -- Online, Referral
    IsDeleted BIT DEFAULT 0,

    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (UserAccountId) REFERENCES UserAccounts(UserAccountId),
    FOREIGN KEY (JobPostingId) REFERENCES JobPosts(Id)
);
GO

-- 7. Bảng Contracts (Hợp đồng lao động)
CREATE TABLE Contracts (
    ContractId INT IDENTITY(1,1) PRIMARY KEY,
    ContractCode NVARCHAR(50),
    EmployeeId INT NOT NULL,
    SignedDate DATETIME2,
    StartDate DATETIME2,
    EndDate DATETIME2,
    BasicSalary DECIMAL(18, 2),
    FilePath NVARCHAR(MAX), -- Link file scan hợp đồng
    Type NVARCHAR(50), -- Probation, Official
    Status NVARCHAR(50), -- Active, Expired, Terminated
    Note NVARCHAR(MAX),

    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);
GO

-- 8. Bảng Attendances (Chấm công)
CREATE TABLE Attendances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    WorkDate DATE,
    CheckInTime DATETIME2,
    CheckOutTime DATETIME2,
    Status NVARCHAR(50), -- OnTime, Late, Absent
    WorkingHours FLOAT,
    Notes NVARCHAR(MAX),
    EditReason NVARCHAR(MAX),
    EditTime DATETIME2,
    EditedBy INT, -- Người sửa công (HR/Admin)
    RawCheckInTime DATETIME2,
    RawCheckOutTime DATETIME2,

    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (EditedBy) REFERENCES UserAccounts(UserAccountId)
);
GO

-- 9. Bảng LeaveRequests (Yêu cầu nghỉ phép)
CREATE TABLE LeaveRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Type NVARCHAR(50), -- Sick, Annual, Unpaid
    StartDate DATETIME2,
    EndDate DATETIME2,
    Reason NVARCHAR(MAX),
    Status NVARCHAR(50), -- Pending, Approved, Rejected
    ManagerComment NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,

    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);
GO

-- 10. Bảng Salaries (Bảng lương)
CREATE TABLE Salaries (
    SalaryId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Month INT,
    Year INT,
    BaseSalary DECIMAL(18, 2),
    Allowance DECIMAL(18, 2), -- Phụ cấp
    Bonus DECIMAL(18, 2), -- Thưởng
    Deduction DECIMAL(18, 2), -- Khấu trừ (Bảo hiểm, thuế)
    ManualDeduction DECIMAL(18, 2), -- Phạt/Trừ khác
    NetSalary DECIMAL(18, 2), -- Thực lãnh
    StandardWorkDays FLOAT, -- Ngày công chuẩn
    ActualWorkDays FLOAT, -- Ngày công thực tế
    Status NVARCHAR(50), -- Draft, Published, Paid
    Note NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CreatedBy INT,
    UpdatedAt DATETIME2,
    UpdatedBy INT,
    IsDeleted BIT DEFAULT 0,

    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (CreatedBy) REFERENCES UserAccounts(UserAccountId),
    FOREIGN KEY (UpdatedBy) REFERENCES UserAccounts(UserAccountId)
);
GO

-- 11. Bảng Notifications (Thông báo)
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255),
    Content NVARCHAR(MAX),
    EmployeeId INT NULL, -- Người nhận là NV
    UserId INT NULL, -- Người nhận là User/Candidate
    Type NVARCHAR(50), -- System, Salary, Job
    Url NVARCHAR(MAX), -- Link chuyển hướng khi click
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,

    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (UserId) REFERENCES UserAccounts(UserAccountId)
);
GO

-- 12. Bảng EducationExperiences (Học vấn & Kinh nghiệm - Dành cho User Profile/CV)
CREATE TABLE EducationExperiences (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserAccountId INT NOT NULL,
    EducationLevel NVARCHAR(100),
    Major NVARCHAR(255),
    University NVARCHAR(255),
    GraduationYear INT,
    GPA FLOAT,
    ExperienceDescription NVARCHAR(MAX),

    FOREIGN KEY (UserAccountId) REFERENCES UserAccounts(UserAccountId)
);
GO