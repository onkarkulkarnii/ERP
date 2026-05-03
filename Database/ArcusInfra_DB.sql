-- ============================================================
-- Arcus Infra Construction - ERP Database Script
-- SQL Server 2019+
-- Created: 2026-05-03
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ArcusInfraERP')
BEGIN
    ALTER DATABASE ArcusInfraERP SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ArcusInfraERP;
END
GO

CREATE DATABASE ArcusInfraERP
    COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

USE ArcusInfraERP;
GO

-- ============================================================
-- PROJECTS
-- ============================================================
CREATE TABLE Projects (
    ProjectId       INT IDENTITY(1,1) PRIMARY KEY,
    ProjectCode     NVARCHAR(20)   NOT NULL UNIQUE,
    ProjectName     NVARCHAR(200)  NOT NULL,
    ClientName      NVARCHAR(200),
    ProjectType     NVARCHAR(50),          -- Residential, Commercial, Infrastructure, Industrial
    StartDate       DATE,
    EndDate         DATE,
    Budget          DECIMAL(18,2)  DEFAULT 0,
    ContractValue   DECIMAL(18,2)  DEFAULT 0,
    Location        NVARCHAR(200),
    Status          NVARCHAR(30)   DEFAULT 'Planning',  -- Planning, Active, OnHold, Completed, Cancelled
    ProgressPercent INT            DEFAULT 0,
    Description     NVARCHAR(MAX),
    ProjectManager  NVARCHAR(100),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- EMPLOYEES
-- ============================================================
CREATE TABLE Employees (
    EmployeeId      INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode    NVARCHAR(20)   NOT NULL UNIQUE,
    FirstName       NVARCHAR(100)  NOT NULL,
    LastName        NVARCHAR(100)  NOT NULL,
    Designation     NVARCHAR(100),
    Department      NVARCHAR(100),
    EmailAddress    NVARCHAR(200),
    PhoneNumber     NVARCHAR(20),
    DateOfJoining   DATE,
    DateOfBirth     DATE,
    BasicSalary     DECIMAL(12,2)  DEFAULT 0,
    EmploymentType  NVARCHAR(30),  -- FullTime, PartTime, Contract, DailyWage
    Status          NVARCHAR(20)   DEFAULT 'Active',
    Address         NVARCHAR(MAX),
    EmergencyContact NVARCHAR(200),
    AadhaarNumber   NVARCHAR(20),
    PANNumber       NVARCHAR(20),
    BankAccountNo   NVARCHAR(50),
    IFSCCode        NVARCHAR(20),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- CONTRACTORS
-- ============================================================
CREATE TABLE Contractors (
    ContractorId    INT IDENTITY(1,1) PRIMARY KEY,
    ContractorCode  NVARCHAR(20)   NOT NULL UNIQUE,
    CompanyName     NVARCHAR(200)  NOT NULL,
    ContactPerson   NVARCHAR(100),
    PhoneNumber     NVARCHAR(20),
    EmailAddress    NVARCHAR(200),
    Address         NVARCHAR(MAX),
    SpecialtyType   NVARCHAR(100), -- Civil, Electrical, Plumbing, Steel, Masonry, Painting, Flooring
    LicenseNumber   NVARCHAR(100),
    LicenseExpiry   DATE,
    GSTNumber       NVARCHAR(20),
    Rating          INT            DEFAULT 3,  -- 1-5 stars
    Status          NVARCHAR(20)   DEFAULT 'Active',
    Remarks         NVARCHAR(MAX),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- MATERIALS (Master Catalogue)
-- ============================================================
CREATE TABLE Materials (
    MaterialId      INT IDENTITY(1,1) PRIMARY KEY,
    MaterialCode    NVARCHAR(30)   NOT NULL UNIQUE,
    MaterialName    NVARCHAR(200)  NOT NULL,
    Category        NVARCHAR(100), -- Cement, Steel, Aggregates, Timber, Bricks, Sand, Plumbing, Electrical, Finishing
    Unit            NVARCHAR(20),  -- MT, CFT, No., Kg, Litre, Sqft, Bag, Rmt
    UnitPrice       DECIMAL(12,2)  DEFAULT 0,
    CurrentStock    DECIMAL(12,2)  DEFAULT 0,
    MinStockLevel   DECIMAL(12,2)  DEFAULT 0,
    Supplier        NVARCHAR(200),
    HSNCode         NVARCHAR(20),
    GSTRate         DECIMAL(5,2)   DEFAULT 0,
    Description     NVARCHAR(MAX),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- MATERIAL TRANSACTIONS (stock in/out per project)
-- ============================================================
CREATE TABLE MaterialTransactions (
    TransactionId   INT IDENTITY(1,1) PRIMARY KEY,
    MaterialId      INT            NOT NULL REFERENCES Materials(MaterialId),
    ProjectId       INT            NULL REFERENCES Projects(ProjectId),
    TransactionType NVARCHAR(20)   NOT NULL,  -- Issue, Receipt, Return
    Quantity        DECIMAL(12,2)  NOT NULL,
    UnitRate        DECIMAL(12,2)  DEFAULT 0,
    TransactionDate DATE           NOT NULL,
    Remarks         NVARCHAR(MAX),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- EQUIPMENT
-- ============================================================
CREATE TABLE Equipment (
    EquipmentId         INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentCode       NVARCHAR(20)   NOT NULL UNIQUE,
    EquipmentName       NVARCHAR(200)  NOT NULL,
    Category            NVARCHAR(100), -- Excavator, Crane, Mixer, Generator, Compactor, Loader, Scaffolding
    Model               NVARCHAR(100),
    Manufacturer        NVARCHAR(100),
    RegistrationNumber  NVARCHAR(50),
    PurchaseDate        DATE,
    PurchaseValue       DECIMAL(14,2)  DEFAULT 0,
    CurrentProjectId    INT            NULL REFERENCES Projects(ProjectId),
    Status              NVARCHAR(30)   DEFAULT 'Available',  -- Available, InUse, Maintenance, Disposed
    LastMaintenanceDate DATE,
    NextMaintenanceDue  DATE,
    RentalCostPerDay    DECIMAL(10,2)  DEFAULT 0,
    IsOwned             BIT            DEFAULT 1,  -- 1 = Owned, 0 = Rented
    Remarks             NVARCHAR(MAX),
    CreatedDate         DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- WORK ORDERS
-- ============================================================
CREATE TABLE WorkOrders (
    WorkOrderId     INT IDENTITY(1,1) PRIMARY KEY,
    WorkOrderCode   NVARCHAR(20)   NOT NULL UNIQUE,
    ProjectId       INT            NOT NULL REFERENCES Projects(ProjectId),
    ContractorId    INT            NULL REFERENCES Contractors(ContractorId),
    Title           NVARCHAR(200)  NOT NULL,
    Description     NVARCHAR(MAX),
    WorkType        NVARCHAR(100), -- Excavation, Foundation, Structure, Finishing, MEP, etc.
    StartDate       DATE,
    EndDate         DATE,
    ContractAmount  DECIMAL(14,2)  DEFAULT 0,
    PaidAmount      DECIMAL(14,2)  DEFAULT 0,
    Status          NVARCHAR(30)   DEFAULT 'Pending', -- Pending, InProgress, Completed, Cancelled
    Priority        NVARCHAR(20)   DEFAULT 'Medium',  -- High, Medium, Low
    AssignedTo      NVARCHAR(100),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- INVOICES
-- ============================================================
CREATE TABLE Invoices (
    InvoiceId       INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber   NVARCHAR(30)   NOT NULL UNIQUE,
    ProjectId       INT            NULL REFERENCES Projects(ProjectId),
    InvoiceType     NVARCHAR(30)   NOT NULL,  -- ClientInvoice, SupplierInvoice, ContractorBill
    PartyName       NVARCHAR(200)  NOT NULL,
    InvoiceDate     DATE           NOT NULL,
    DueDate         DATE,
    Amount          DECIMAL(14,2)  DEFAULT 0,
    TaxAmount       DECIMAL(12,2)  DEFAULT 0,
    TotalAmount     DECIMAL(14,2)  DEFAULT 0,
    PaidAmount      DECIMAL(14,2)  DEFAULT 0,
    Status          NVARCHAR(30)   DEFAULT 'Pending',  -- Pending, PartiallyPaid, Paid, Overdue, Cancelled
    Description     NVARCHAR(MAX),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- EXPENSES
-- ============================================================
CREATE TABLE Expenses (
    ExpenseId       INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId       INT            NULL REFERENCES Projects(ProjectId),
    ExpenseDate     DATE           NOT NULL,
    Category        NVARCHAR(100), -- Labour, Material, Equipment, Transportation, Admin, Overhead
    Description     NVARCHAR(MAX),
    Amount          DECIMAL(12,2)  NOT NULL,
    VoucherNumber   NVARCHAR(50),
    PaymentMode     NVARCHAR(30),  -- Cash, Bank Transfer, Cheque, UPI
    ApprovedBy      NVARCHAR(100),
    CreatedDate     DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- SAFETY INCIDENTS
-- ============================================================
CREATE TABLE SafetyIncidents (
    IncidentId          INT IDENTITY(1,1) PRIMARY KEY,
    IncidentCode        NVARCHAR(20),
    ProjectId           INT            NULL REFERENCES Projects(ProjectId),
    IncidentDate        DATE           NOT NULL,
    IncidentTime        TIME,
    IncidentType        NVARCHAR(100), -- NearMiss, FirstAid, LostTime, Fatality, PropertyDamage, FireAlarm
    Description         NVARCHAR(MAX),
    InjuredPerson       NVARCHAR(200),
    InjuryType          NVARCHAR(100),
    Location            NVARCHAR(200),
    RootCause           NVARCHAR(MAX),
    CorrectiveAction    NVARCHAR(MAX),
    ReportedBy          NVARCHAR(100),
    Status              NVARCHAR(30)   DEFAULT 'Open',  -- Open, UnderInvestigation, Closed
    ClosedDate          DATE,
    CreatedDate         DATETIME       DEFAULT GETDATE()
);

-- ============================================================
-- ATTENDANCE
-- ============================================================
CREATE TABLE Attendance (
    AttendanceId    INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId      INT            NOT NULL REFERENCES Employees(EmployeeId),
    ProjectId       INT            NULL REFERENCES Projects(ProjectId),
    AttendanceDate  DATE           NOT NULL,
    InTime          TIME,
    OutTime         TIME,
    Status          NVARCHAR(30)   NOT NULL,  -- Present, Absent, HalfDay, Leave, Holiday
    OvertimeHours   DECIMAL(4,2)   DEFAULT 0,
    Remarks         NVARCHAR(MAX),
    UNIQUE (EmployeeId, AttendanceDate)
);

-- ============================================================
-- INDEXES
-- ============================================================
CREATE INDEX IX_Projects_Status      ON Projects(Status);
CREATE INDEX IX_Projects_CreatedDate ON Projects(CreatedDate);
CREATE INDEX IX_Employees_Status     ON Employees(Status);
CREATE INDEX IX_WorkOrders_ProjectId ON WorkOrders(ProjectId);
CREATE INDEX IX_WorkOrders_Status    ON WorkOrders(Status);
CREATE INDEX IX_Invoices_Status      ON Invoices(Status);
CREATE INDEX IX_Expenses_ProjectId   ON Expenses(ProjectId);
CREATE INDEX IX_Expenses_ExpenseDate ON Expenses(ExpenseDate);
CREATE INDEX IX_Attendance_Date      ON Attendance(AttendanceDate);
CREATE INDEX IX_SafetyIncidents_Date ON SafetyIncidents(IncidentDate);

-- ============================================================
-- SEED DATA
-- ============================================================

-- Projects
INSERT INTO Projects (ProjectCode, ProjectName, ClientName, ProjectType, StartDate, EndDate, Budget, ContractValue, Location, Status, ProgressPercent, Description, ProjectManager)
VALUES
('PRJ-2025-001', 'Greenview Residential Complex', 'Greenview Developers Pvt Ltd', 'Residential', '2025-01-15', '2026-06-30', 45000000, 52000000, 'Pune, Maharashtra', 'Active', 65, '12-storey residential complex with 144 flats and amenities.', 'Rajesh Kumar'),
('PRJ-2025-002', 'Highway Bridge Reconstruction', 'NHAI - National Highways Authority', 'Infrastructure', '2025-03-01', '2026-12-31', 120000000, 135000000, 'NH-48, Pune-Mumbai', 'Active', 30, 'Reconstruction of 4-lane highway bridge spanning 220 metres.', 'Suresh Patil'),
('PRJ-2025-003', 'Tech Park Office Block', 'Horizon IT Solutions', 'Commercial', '2025-04-10', '2026-08-31', 78000000, 88000000, 'Hinjewadi, Pune', 'Active', 20, 'G+7 commercial office block with basement parking for 500 vehicles.', 'Anita Sharma'),
('PRJ-2024-004', 'City Water Treatment Plant', 'Pune Municipal Corporation', 'Industrial', '2024-06-01', '2025-11-30', 95000000, 108000000, 'Hadapsar, Pune', 'Completed', 100, 'Upgrade of water treatment capacity from 50 MLD to 120 MLD.', 'Vikram Desai'),
('PRJ-2026-005', 'Luxury Villa Project', 'Arcus Premium Homes', 'Residential', '2026-02-01', '2027-05-31', 35000000, 42000000, 'Lavasa, Pune', 'Planning', 0, '24 luxury villas with private gardens and club house.', 'Meena Joshi');

-- Employees
INSERT INTO Employees (EmployeeCode, FirstName, LastName, Designation, Department, EmailAddress, PhoneNumber, DateOfJoining, BasicSalary, EmploymentType, Status)
VALUES
('EMP-001', 'Rajesh', 'Kumar', 'Senior Project Manager', 'Projects', 'rajesh.kumar@arcusinfra.com', '9876543210', '2019-03-01', 125000, 'FullTime', 'Active'),
('EMP-002', 'Suresh', 'Patil', 'Project Manager', 'Projects', 'suresh.patil@arcusinfra.com', '9876543211', '2020-06-15', 95000, 'FullTime', 'Active'),
('EMP-003', 'Anita', 'Sharma', 'Site Engineer', 'Engineering', 'anita.sharma@arcusinfra.com', '9876543212', '2021-09-01', 65000, 'FullTime', 'Active'),
('EMP-004', 'Vikram', 'Desai', 'Civil Engineer', 'Engineering', 'vikram.desai@arcusinfra.com', '9876543213', '2018-01-10', 72000, 'FullTime', 'Active'),
('EMP-005', 'Meena', 'Joshi', 'Project Coordinator', 'Projects', 'meena.joshi@arcusinfra.com', '9876543214', '2022-07-20', 55000, 'FullTime', 'Active'),
('EMP-006', 'Ravi', 'Bhosale', 'Safety Officer', 'EHS', 'ravi.bhosale@arcusinfra.com', '9876543215', '2020-11-05', 58000, 'FullTime', 'Active'),
('EMP-007', 'Priya', 'Wagh', 'Accounts Manager', 'Finance', 'priya.wagh@arcusinfra.com', '9876543216', '2019-08-01', 68000, 'FullTime', 'Active'),
('EMP-008', 'Sandeep', 'More', 'Store Manager', 'Logistics', 'sandeep.more@arcusinfra.com', '9876543217', '2021-04-12', 48000, 'FullTime', 'Active'),
('EMP-009', 'Kavita', 'Kulkarni', 'HR Manager', 'HR', 'kavita.kulkarni@arcusinfra.com', '9876543218', '2020-02-28', 62000, 'FullTime', 'Active'),
('EMP-010', 'Amit', 'Sawant', 'Electrician Foreman', 'MEP', 'amit.sawant@arcusinfra.com', '9876543219', '2022-01-15', 42000, 'FullTime', 'Active'),
('EMP-011', 'Deepak', 'Yadav', 'Mason Foreman', 'Civil', NULL, '9876543220', '2023-03-01', 35000, 'Contract', 'Active'),
('EMP-012', 'Sunita', 'Pawar', 'Surveyor', 'Engineering', 'sunita.pawar@arcusinfra.com', '9876543221', '2023-06-01', 52000, 'FullTime', 'Active');

-- Contractors
INSERT INTO Contractors (ContractorCode, CompanyName, ContactPerson, PhoneNumber, EmailAddress, SpecialtyType, LicenseNumber, Rating, Status)
VALUES
('CON-001', 'Solid Foundation Works', 'Mahesh Kale', '9988776655', 'mahesh@solidfoundation.com', 'Civil', 'MH-CIVIL-2021-001', 5, 'Active'),
('CON-002', 'PowerLine Electricals', 'Arun Vaidya', '9988776656', 'arun@powerline.in', 'Electrical', 'MH-ELEC-2020-045', 4, 'Active'),
('CON-003', 'AquaFlow Plumbing', 'Nilesh Shah', '9988776657', 'nilesh@aquaflow.com', 'Plumbing', 'MH-PLMB-2019-012', 4, 'Active'),
('CON-004', 'Steel Craft Industries', 'Rohit Gupta', '9988776658', 'rohit@steelcraft.com', 'Steel', 'MH-STRL-2022-009', 5, 'Active'),
('CON-005', 'FinishPro Interiors', 'Smita Deshpande', '9988776659', 'smita@finishpro.in', 'Painting', 'MH-PTNG-2021-033', 3, 'Active'),
('CON-006', 'TileMaster Flooring', 'Govind Naik', '9988776660', NULL, 'Flooring', 'MH-FLRG-2020-017', 4, 'Active'),
('CON-007', 'EarthMove Equipment', 'Prakash Jadhav', '9988776661', 'prakash@earthmove.com', 'Excavation', 'MH-EXCV-2023-005', 5, 'Active');

-- Materials
INSERT INTO Materials (MaterialCode, MaterialName, Category, Unit, UnitPrice, CurrentStock, MinStockLevel, Supplier)
VALUES
('MAT-CEM-001', 'OPC 53 Grade Cement',        'Cement',     'Bag',  380,   2500,  500,  'ACC Cement Ltd'),
('MAT-CEM-002', 'PPC Cement',                  'Cement',     'Bag',  360,   1800,  400,  'Ultratech Cement'),
('MAT-STL-001', 'TMT Rebar Fe500D 8mm',        'Steel',      'MT',   68000, 45,    10,   'JSW Steel'),
('MAT-STL-002', 'TMT Rebar Fe500D 12mm',       'Steel',      'MT',   67500, 60,    15,   'TATA Steel'),
('MAT-STL-003', 'TMT Rebar Fe500D 16mm',       'Steel',      'MT',   67000, 40,    10,   'JSW Steel'),
('MAT-AGG-001', 'Coarse Aggregate 20mm',       'Aggregates', 'CFT',  38,    3200,  500,  'Local Quarry'),
('MAT-AGG-002', 'Fine Aggregate (River Sand)', 'Aggregates', 'CFT',  42,    2800,  400,  'Local Supplier'),
('MAT-BRK-001', 'Red Clay Bricks (1st class)', 'Bricks',     'No.',  9,     85000, 10000,'Kumar Brick Factory'),
('MAT-BLK-001', 'AAC Blocks 600x200x200mm',    'Blocks',     'No.',  45,    12000, 2000, 'Siporex India'),
('MAT-TIM-001', 'Plywood 18mm BWR Grade',      'Timber',     'Sqft', 95,    800,   100,  'Greenply Industries'),
('MAT-PLM-001', 'CPVC Pipe 25mm',              'Plumbing',   'Rmt',  185,   450,   50,   'Astral Pipes'),
('MAT-ELC-001', 'PVC Conduit 25mm',            'Electrical', 'Rmt',  45,    600,   100,  'Havells India'),
('MAT-ADM-001', 'Concrete Admixture (SP)',     'Chemicals',  'Litre',95,    200,   50,   'BASF India'),
('MAT-WPF-001', 'Waterproofing Compound',      'Chemicals',  'Kg',   220,   150,   30,   'Dr. Fixit');

-- Equipment
INSERT INTO Equipment (EquipmentCode, EquipmentName, Category, Model, Manufacturer, PurchaseDate, PurchaseValue, Status, IsOwned)
VALUES
('EQP-001', 'JCB Backhoe Loader 3DX',     'Excavator',    '3DX Super',     'JCB India',   '2022-04-01', 2800000, 'InUse',    1),
('EQP-002', 'Tower Crane TC-5510',        'Crane',        'TC-5510',       'Potain',      '2021-08-15', 8500000, 'InUse',    1),
('EQP-003', 'Concrete Mixer 10/7',        'Mixer',        'RM-800',        'Ajax Fiori',  '2023-01-20', 185000,  'Available',1),
('EQP-004', 'Diesel Generator 62.5 KVA',  'Generator',    'EP-63',         'Kirloskar',   '2022-11-10', 350000,  'InUse',    1),
('EQP-005', 'Plate Compactor 80kg',       'Compactor',    'BPR-40/45D',    'Wacker Neuson','2023-06-01',95000,   'Available',1),
('EQP-006', 'Transit Mixer 6m3',          'Transit Mixer','TM-6000',       'Ajax Fiori',  '2021-03-15', 1650000, 'InUse',    1),
('EQP-007', 'Concrete Pump Trailer',      'Pump',         'BSA 1002D',     'Putzmeister', '2020-09-01', 4200000, 'Maintenance',1),
('EQP-008', 'Bar Bending Machine 40mm',   'Misc',         'BBM-40',        'Shri Krishna','2022-05-01', 145000,  'Available',1),
('EQP-009', 'Total Station Leica',        'Survey',       'TS06 Plus',     'Leica',       '2023-02-10', 480000,  'Available',1),
('EQP-010', 'Vibrator Needle 60mm',       'Misc',         'FV-2000',       'Wacker Neuson','2023-07-15',28000,   'Available',1);

-- Work Orders
INSERT INTO WorkOrders (WorkOrderCode, ProjectId, ContractorId, Title, WorkType, StartDate, EndDate, ContractAmount, Status, Priority, AssignedTo)
VALUES
('WO-2025-001', 1, 1, 'Foundation Excavation & PCC Work',   'Excavation', '2025-01-20', '2025-03-15', 3500000, 'Completed', 'High',   'Suresh Patil'),
('WO-2025-002', 1, 4, 'RCC Frame Structure - Ground Floor', 'Structure',  '2025-03-20', '2025-06-30', 8200000, 'Completed', 'High',   'Rajesh Kumar'),
('WO-2025-003', 1, 1, 'RCC Frame Structure - F1 to F5',     'Structure',  '2025-07-01', '2025-12-31', 18500000,'InProgress','High',   'Rajesh Kumar'),
('WO-2025-004', 2, 7, 'Highway Bridge Foundation Piling',   'Foundation', '2025-03-10', '2025-07-31', 22000000,'Completed', 'High',   'Suresh Patil'),
('WO-2025-005', 2, 1, 'Pier and Abutment Construction',     'Structure',  '2025-08-01', '2026-03-31', 35000000,'InProgress','High',   'Suresh Patil'),
('WO-2025-006', 3, 2, 'Electrical LT Panel Installation',   'Electrical', '2025-05-01', '2025-08-31', 4500000, 'InProgress','Medium', 'Anita Sharma'),
('WO-2025-007', 3, 3, 'Plumbing - Water Supply System',     'Plumbing',   '2025-05-15', '2025-09-30', 3200000, 'Pending',   'Medium', 'Anita Sharma'),
('WO-2026-008', 5, 1, 'Site Preparation & Levelling',       'Excavation', '2026-02-10', '2026-04-30', 1800000, 'Pending',   'High',   'Meena Joshi');

-- Invoices
INSERT INTO Invoices (InvoiceNumber, ProjectId, InvoiceType, PartyName, InvoiceDate, DueDate, Amount, TaxAmount, TotalAmount, PaidAmount, Status)
VALUES
('INV-CLI-2025-001', 1, 'ClientInvoice', 'Greenview Developers Pvt Ltd', '2025-03-31', '2025-04-30', 8000000, 1440000, 9440000, 9440000, 'Paid'),
('INV-CLI-2025-002', 1, 'ClientInvoice', 'Greenview Developers Pvt Ltd', '2025-06-30', '2025-07-31', 12000000,2160000, 14160000,14160000,'Paid'),
('INV-CLI-2025-003', 1, 'ClientInvoice', 'Greenview Developers Pvt Ltd', '2025-09-30', '2025-10-31', 10000000,1800000, 11800000,0,        'Pending'),
('INV-CLI-2025-004', 2, 'ClientInvoice', 'NHAI', '2025-07-31', '2025-08-31', 25000000,4500000, 29500000,29500000,'Paid'),
('INV-CLI-2025-005', 2, 'ClientInvoice', 'NHAI', '2025-12-31', '2026-01-31', 30000000,5400000, 35400000,20000000,'PartiallyPaid'),
('INV-SUP-2025-001', 1, 'SupplierInvoice','ACC Cement Ltd', '2025-02-15', '2025-03-15', 950000,  171000,  1121000, 1121000, 'Paid'),
('INV-SUP-2025-002', 1, 'SupplierInvoice','JSW Steel', '2025-03-20', '2025-04-20', 3200000, 576000,  3776000, 3776000, 'Paid'),
('INV-CON-2025-001', 1, 'ContractorBill','Solid Foundation Works', '2025-03-15', '2025-04-15',3500000, 630000,  4130000, 4130000, 'Paid'),
('INV-CON-2025-002', 2, 'ContractorBill','EarthMove Equipment', '2025-07-31', '2025-08-31',22000000,3960000, 25960000,25960000,'Paid'),
('INV-CON-2025-003', 3, 'ContractorBill','PowerLine Electricals', '2025-10-31', '2025-11-30',2000000, 360000,  2360000, 0,        'Pending');

-- Expenses
INSERT INTO Expenses (ProjectId, ExpenseDate, Category, Description, Amount, VoucherNumber, PaymentMode, ApprovedBy)
VALUES
(1, '2025-02-01', 'Labour',        'Daily wage labour - February batch',           285000, 'EXP-2025-001', 'Bank Transfer', 'Rajesh Kumar'),
(1, '2025-02-15', 'Material',      'Cement procurement - 2000 bags',               760000, 'EXP-2025-002', 'Cheque',        'Priya Wagh'),
(1, '2025-03-01', 'Equipment',     'Excavator hiring charges - Feb',               125000, 'EXP-2025-003', 'Bank Transfer', 'Rajesh Kumar'),
(1, '2025-03-15', 'Transportation','Truck charges for steel delivery',             45000,  'EXP-2025-004', 'Cash',          'Suresh Patil'),
(2, '2025-04-01', 'Labour',        'Skilled labour - piling work',                 520000, 'EXP-2025-005', 'Bank Transfer', 'Suresh Patil'),
(2, '2025-04-15', 'Material',      'Steel TMT bars - 50 MT',                       3375000,'EXP-2025-006', 'Bank Transfer', 'Priya Wagh'),
(3, '2025-05-01', 'Admin',         'Site office setup - Hinjewadi',                85000,  'EXP-2025-007', 'Cheque',        'Anita Sharma'),
(NULL, '2025-05-10','Overhead',    'Head office electricity bill - April',         42000,  'EXP-2025-008', 'Bank Transfer', 'Priya Wagh'),
(NULL, '2025-06-01','Admin',       'Vehicle maintenance - company fleet',          68000,  'EXP-2025-009', 'Cheque',        'Rajesh Kumar'),
(1, '2025-06-15', 'Labour',        'Daily wage labour - June batch',               310000, 'EXP-2025-010', 'Bank Transfer', 'Rajesh Kumar');

-- Safety Incidents
INSERT INTO SafetyIncidents (IncidentCode, ProjectId, IncidentDate, IncidentTime, IncidentType, Description, InjuredPerson, InjuryType, Location, RootCause, CorrectiveAction, ReportedBy, Status)
VALUES
('INC-2025-001', 1, '2025-04-12', '10:30', 'FirstAid',      'Worker suffered minor hand cut while cutting steel reinforcement bars.',  'Ramesh Pol',     'Minor Cut',         'Floor 2 - Reinforcement area', 'Improper use of angle grinder without gloves.', 'Mandatory PPE enforcement, toolbox talk conducted.', 'Ravi Bhosale', 'Closed'),
('INC-2025-002', 2, '2025-05-22', '14:15', 'NearMiss',       'Loose scaffolding plank nearly fell from 8m height. No injury.',         NULL,              NULL,                'Bridge Pier P3 scaffolding',   'Scaffolding not inspected before use.',         'Daily scaffolding inspection checklist implemented.', 'Ravi Bhosale', 'Closed'),
('INC-2025-003', 1, '2025-07-08', '09:00', 'LostTime',       'Worker fell from 1.5m height while installing formwork. Fracture.',      'Ganesh Kamble',  'Fractured wrist',   'Floor 4 - Formwork area',      'No fall protection system in place.',           'Fall arrest harness made mandatory above 2m height.', 'Ravi Bhosale', 'Closed'),
('INC-2025-004', 3, '2025-08-15', '11:45', 'NearMiss',       'Electric cable near water source. Short circuit risk noticed.',          NULL,              NULL,                'Basement electrical room',     'Temporary wiring not properly routed.',         'Permanent cabling and ELCB installed immediately.',  'Amit Sawant',  'Closed'),
('INC-2025-005', 2, '2025-10-03', '16:00', 'PropertyDamage', 'Transit mixer scraped against temporary safety barrier, minor damage.',  NULL,              NULL,                'Site entry gate - NH-48',      'Inadequate turning radius marked for vehicles.',  'Traffic management plan revised and signage added.', 'Ravi Bhosale', 'Closed'),
('INC-2026-006', 1, '2026-01-18', '08:30', 'FirstAid',       'Worker eye irritation from concrete dust on windy day.',                'Sanjay Tupe',     'Eye irritation',   'Floor 7 - Slab casting area',  'Safety goggles not worn during windy conditions.', 'Wind speed monitoring and PPE SOP updated.',        'Ravi Bhosale', 'Open');

-- ============================================================
-- USEFUL VIEWS
-- ============================================================

CREATE VIEW vw_ProjectFinancialSummary AS
SELECT
    p.ProjectId,
    p.ProjectCode,
    p.ProjectName,
    p.Budget,
    p.ContractValue,
    ISNULL(SUM(e.Amount), 0)                                          AS TotalExpenses,
    ISNULL(SUM(CASE WHEN i.InvoiceType='ClientInvoice' THEN i.TotalAmount ELSE 0 END),0) AS TotalBilled,
    ISNULL(SUM(CASE WHEN i.InvoiceType='ClientInvoice' THEN i.PaidAmount ELSE 0 END),0)  AS TotalReceived,
    p.Budget - ISNULL(SUM(e.Amount), 0)                               AS RemainingBudget
FROM Projects p
LEFT JOIN Expenses e ON e.ProjectId = p.ProjectId
LEFT JOIN Invoices i ON i.ProjectId = p.ProjectId
GROUP BY p.ProjectId, p.ProjectCode, p.ProjectName, p.Budget, p.ContractValue;
GO

CREATE VIEW vw_WorkOrderSummary AS
SELECT
    wo.WorkOrderId,
    wo.WorkOrderCode,
    wo.Title,
    wo.WorkType,
    wo.Status,
    wo.Priority,
    p.ProjectName,
    p.ProjectCode,
    c.CompanyName AS ContractorName,
    wo.ContractAmount,
    wo.PaidAmount,
    wo.ContractAmount - wo.PaidAmount AS BalanceAmount,
    wo.StartDate,
    wo.EndDate,
    wo.AssignedTo
FROM WorkOrders wo
JOIN Projects p ON p.ProjectId = wo.ProjectId
LEFT JOIN Contractors c ON c.ContractorId = wo.ContractorId;
GO

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

CREATE PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Projects WHERE Status = 'Active')                 AS ActiveProjects,
        (SELECT COUNT(*) FROM Projects)                                          AS TotalProjects,
        (SELECT COUNT(*) FROM Employees WHERE Status = 'Active')                AS ActiveEmployees,
        (SELECT COUNT(*) FROM WorkOrders WHERE Status = 'InProgress')           AS ActiveWorkOrders,
        (SELECT COUNT(*) FROM WorkOrders WHERE Status = 'Pending')              AS PendingWorkOrders,
        (SELECT ISNULL(SUM(TotalAmount - PaidAmount),0) FROM Invoices WHERE Status IN ('Pending','PartiallyPaid') AND InvoiceType = 'ClientInvoice') AS OutstandingReceivables,
        (SELECT ISNULL(SUM(TotalAmount - PaidAmount),0) FROM Invoices WHERE Status IN ('Pending','PartiallyPaid') AND InvoiceType IN ('SupplierInvoice','ContractorBill')) AS OutstandingPayables,
        (SELECT COUNT(*) FROM SafetyIncidents WHERE Status = 'Open')            AS OpenIncidents,
        (SELECT COUNT(*) FROM Equipment WHERE Status = 'Maintenance')           AS EquipmentInMaintenance,
        (SELECT COUNT(*) FROM Materials WHERE CurrentStock <= MinStockLevel)    AS LowStockMaterials;
END
GO

PRINT 'ArcusInfraERP database created and seeded successfully.';
GO
