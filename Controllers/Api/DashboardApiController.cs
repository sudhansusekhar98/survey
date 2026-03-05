using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Models.Api;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;
using System.Security.Claims;

namespace SurveyApp.Controllers.Api
{
    // Dashboard API Controller - Statistics and analytics

    [Route("api/v1/dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardApiController : ControllerBase
    {
        private readonly ISurvey _surveyRepo;
        private readonly IAdmin _adminRepo;
        private readonly ILogger<DashboardApiController> _logger;

        public DashboardApiController(
            ISurvey surveyRepo,
            IAdmin adminRepo,
            ILogger<DashboardApiController> logger)
        {
            _surveyRepo = surveyRepo;
            _adminRepo = adminRepo;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        
        // Get dashboard statistics
       
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), 200)]
        public IActionResult GetDashboardStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var allSurveys = _surveyRepo.GetAllSurveys(userId);
                var today = DateTime.Now.Date;

                var missedDeadlineSurveys = allSurveys
                    .Where(s => s.DueDate.HasValue && s.DueDate.Value.Date < today && s.SurveyStatus != "Completed")
                    .Count();

                var stats = new DashboardStatsDto
                {
                    TotalSurveys = allSurveys.Count,
                    CreatedSurveys = allSurveys.Count(s => s.SurveyStatus == "Created"),
                    AssignedSurveys = allSurveys.Count(s => s.SurveyStatus == "Assigned"),
                    InProgressSurveys = allSurveys.Count(s => s.SurveyStatus == "In Progress"),
                    SubmittedSurveys = allSurveys.Count(s => s.SurveyStatus == "Submitted"),
                    CompletedSurveys = allSurveys.Count(s => s.SurveyStatus == "Completed"),
                    PendingSurveys = allSurveys.Count(s => s.SurveyStatus == "Pending"),
                    OnHoldSurveys = allSurveys.Count(s => s.SurveyStatus == "On Hold"),
                    MissedDeadlineSurveys = missedDeadlineSurveys,
                    CompletionRate = allSurveys.Count > 0
                        ? Math.Round((decimal)allSurveys.Count(s => s.SurveyStatus == "Completed") / allSurveys.Count * 100, 1)
                        : 0,
                    SurveysByStatus = allSurveys
                        .GroupBy(s => s.SurveyStatus ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),
                    SurveysByRegion = allSurveys
                        .Where(s => !string.IsNullOrEmpty(s.RegionName))
                        .GroupBy(s => s.RegionName!)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    SurveysByImplementationType = allSurveys
                        .Where(s => !string.IsNullOrEmpty(s.ImplementationType))
                        .GroupBy(s => s.ImplementationType!)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get recent surveys
        /// </summary>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(ApiResponse<List<SurveyListDto>>), 200)]
        public IActionResult GetRecentSurveys([FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var surveys = _surveyRepo.GetAllSurveys(userId)
                    .OrderByDescending(s => s.SurveyId)
                    .Take(count)
                    .Select(s => new SurveyListDto
                    {
                        SurveyId = s.SurveyId,
                        SurveyName = s.SurveyName,
                        Status = s.SurveyStatus,
                        ClientName = s.ClientName,
                        RegionName = s.RegionName,
                        ImplementationType = s.ImplementationType,
                        DueDate = s.DueDate,
                        SurveyDate = s.SurveyDate,
                        IsRevised = s.IsRevised
                    })
                    .ToList();

                return Ok(ApiResponse<List<SurveyListDto>>.Ok(surveys));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent surveys");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get overdue surveys
        /// </summary>
        [HttpGet("overdue")]
        [ProducesResponseType(typeof(ApiResponse<List<SurveyListDto>>), 200)]
        public IActionResult GetOverdueSurveys()
        {
            try
            {
                var userId = GetCurrentUserId();
                var today = DateTime.Now.Date;

                var surveys = _surveyRepo.GetAllSurveys(userId)
                    .Where(s => s.DueDate.HasValue && s.DueDate.Value.Date < today && s.SurveyStatus != "Completed")
                    .OrderBy(s => s.DueDate)
                    .Select(s => new SurveyListDto
                    {
                        SurveyId = s.SurveyId,
                        SurveyName = s.SurveyName,
                        Status = s.SurveyStatus,
                        ClientName = s.ClientName,
                        RegionName = s.RegionName,
                        ImplementationType = s.ImplementationType,
                        DueDate = s.DueDate,
                        SurveyDate = s.SurveyDate,
                        IsRevised = s.IsRevised
                    })
                    .ToList();

                return Ok(ApiResponse<List<SurveyListDto>>.Ok(surveys));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue surveys");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }
    }

    /// <summary>
    /// Master Data API Controller - Clients, Employees, Devices, Regions
    /// </summary>
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    public class MasterDataApiController : ControllerBase
    {
        private readonly IAdmin _adminRepo;
        private readonly IClientMaster _clientRepo;
        private readonly IEmpMaster _empRepo;
        private readonly ILogger<MasterDataApiController> _logger;

        public MasterDataApiController(
            IAdmin adminRepo,
            IClientMaster clientRepo,
            IEmpMaster empRepo,
            ILogger<MasterDataApiController> logger)
        {
            _adminRepo = adminRepo;
            _clientRepo = clientRepo;
            _empRepo = empRepo;
            _logger = logger;
        }

        #region Clients

        /// <summary>
        /// Get all clients
        /// </summary>
        [HttpGet("clients")]
        [ProducesResponseType(typeof(ApiResponse<List<ClientDto>>), 200)]
        public IActionResult GetClients([FromQuery] bool activeOnly = true)
        {
            try
            {
                var clients = _clientRepo.GetAllClients();
                
                if (activeOnly)
                {
                    clients = clients.Where(c => c.Isactive).ToList();
                }

                var dtos = clients.Select(c => new ClientDto
                {
                    ClientId = c.ClientID,
                    ClientName = c.ClientName,
                    ClientType = c.ClientType,
                    ContactPerson = c.ContactPerson,
                    ContactNumber = c.ContactNumber,
                    Address = $"{c.Address1} {c.Address3}".Trim(),
                    City = c.City,
                    State = c.State,
                    IsActive = c.Isactive
                }).ToList();

                return Ok(ApiResponse<List<ClientDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get client by ID
        /// </summary>
        [HttpGet("clients/{id}")]
        [ProducesResponseType(typeof(ApiResponse<ClientDto>), 200)]
        public IActionResult GetClient(int id)
        {
            try
            {
                var clients = _clientRepo.GetAllClients();
                var client = clients.FirstOrDefault(c => c.ClientID == id);

                if (client == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Client not found"));
                }

                var dto = new ClientDto
                {
                    ClientId = client.ClientID,
                    ClientName = client.ClientName,
                    ClientType = client.ClientType,
                    ContactPerson = client.ContactPerson,
                    ContactNumber = client.ContactNumber,
                    Address = $"{client.Address1} {client.Address3}".Trim(),
                    City = client.City,
                    State = client.State,
                    IsActive = client.Isactive
                };

                return Ok(ApiResponse<ClientDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Employees

        /// <summary>
        /// Get all employees
        /// </summary>
        [HttpGet("employees")]
        [ProducesResponseType(typeof(ApiResponse<List<EmployeeDto>>), 200)]
        public IActionResult GetEmployees([FromQuery] bool activeOnly = true)
        {
            try
            {
                var employees = _adminRepo.GetEmpMaster();
                
                if (activeOnly)
                {
                    employees = employees.Where(e => e.IsActive).ToList();
                }

                var dtos = employees.Select(e => new EmployeeDto
                {
                    EmpId = e.EmpID,
                    EmpCode = e.EmpCode,
                    EmpName = e.EmpName,
                    Email = e.Email,
                    MobileNo = e.MobileNo,
                    Designation = e.Designation,
                    IsActive = e.IsActive
                }).ToList();

                return Ok(ApiResponse<List<EmployeeDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("employees/{id}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
        public IActionResult GetEmployee(int id)
        {
            try
            {
                var employee = _adminRepo.GetEmployeeById(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Employee not found"));
                }

                var dto = new EmployeeDto
                {
                    EmpId = employee.EmpID,
                    EmpCode = employee.EmpCode,
                    EmpName = employee.EmpName,
                    Email = employee.Email,
                    MobileNo = employee.MobileNo,
                    Designation = employee.Designation,
                    IsActive = employee.IsActive
                };

                return Ok(ApiResponse<EmployeeDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee {EmpId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Regions

        /// <summary>
        /// Get all regions
        /// </summary>
        [HttpGet("regions")]
        [ProducesResponseType(typeof(ApiResponse<List<RegionDto>>), 200)]
        public IActionResult GetRegions()
        {
            try
            {
                var regions = _adminRepo.GetRegionMaster();

                var dtos = regions.Select(r => new RegionDto
                {
                    RegionId = r.RegionID,
                    RegionName = r.RegionName
                }).ToList();

                return Ok(ApiResponse<List<RegionDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regions");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Device Modules & Devices

        /// <summary>
        /// Get all device modules (categories)
        /// </summary>
        [HttpGet("device-modules")]
        [ProducesResponseType(typeof(ApiResponse<List<DeviceModuleDto>>), 200)]
        public IActionResult GetDeviceModules([FromQuery] bool activeOnly = true)
        {
            try
            {
                var modules = _adminRepo.GetAllDeviceModules(activeOnly);

                var dtos = modules.Select(m => new DeviceModuleDto
                {
                    Id = m.Id,
                    TypeName = m.Name,
                    TypeDesc = m.Description,
                    GroupName = m.GroupName,
                    IsActive = m.IsActive
                }).ToList();

                return Ok(ApiResponse<List<DeviceModuleDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device modules");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get all devices/items
        /// </summary>
        [HttpGet("devices")]
        [ProducesResponseType(typeof(ApiResponse<List<DeviceDto>>), 200)]
        public IActionResult GetDevices([FromQuery] int? moduleId = null, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var devices = _adminRepo.GetAllDevices(moduleId, activeOnly);

                var dtos = devices.Select(d => new DeviceDto
                {
                    ItemId = d.ItemId,
                    ItemName = d.Name,
                    ItemCode = d.Code,
                    ItemDesc = d.Description,
                    UOM = d.UOM,
                    TypeId = d.ModuleId,
                    TypeName = d.ModuleName,
                    IsActive = d.IsActive
                }).ToList();

                return Ok(ApiResponse<List<DeviceDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting devices");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get device by ID
        /// </summary>
        [HttpGet("devices/{id}")]
        [ProducesResponseType(typeof(ApiResponse<DeviceDto>), 200)]
        public IActionResult GetDevice(int id)
        {
            try
            {
                var device = _adminRepo.GetDeviceById(id);
                if (device == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Device not found"));
                }

                var dto = new DeviceDto
                {
                    ItemId = device.ItemId,
                    ItemName = device.Name,
                    ItemCode = device.Code,
                    ItemDesc = device.Description,
                    UOM = device.UOM,
                    TypeId = device.ModuleId,
                    TypeName = device.ModuleName,
                    IsActive = device.IsActive
                };

                return Ok(ApiResponse<DeviceDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device {DeviceId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Dropdown Options

        /// <summary>
        /// Get implementation type options
        /// </summary>
        [HttpGet("options/implementation-types")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public IActionResult GetImplementationTypes()
        {
            var types = new List<string>
            {
                "New Project",
                "Upgradation",
                "Extension",
                "Restoration",
                "AMC"
            };
            return Ok(ApiResponse<List<string>>.Ok(types));
        }

        /// <summary>
        /// Get survey status options
        /// </summary>
        [HttpGet("options/survey-statuses")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public IActionResult GetSurveyStatuses()
        {
            var statuses = new List<string>
            {
                "Created",
                "Assigned",
                "In Progress",
                "Submitted",
                "Completed",
                "Pending",
                "On Hold"
            };
            return Ok(ApiResponse<List<string>>.Ok(statuses));
        }

        /// <summary>
        /// Get location type options
        /// </summary>
        [HttpGet("options/location-types")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public IActionResult GetLocationTypes()
        {
            var types = new List<string>
            {
                "Traffic",
                "Surveillance",
                "Control Room"
            };
            return Ok(ApiResponse<List<string>>.Ok(types));
        }

        /// <summary>
        /// Get way type options
        /// </summary>
        [HttpGet("options/way-types")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public IActionResult GetWayTypes()
        {
            var types = new List<string>
            {
                "One Way",
                "Two Way",
                "Three Way",
                "Four Way"
            };
            return Ok(ApiResponse<List<string>>.Ok(types));
        }

        #endregion
    }
}
