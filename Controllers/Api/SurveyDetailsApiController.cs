using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Models.Api;
using SurveyApp.Repo;
using System.Security.Claims;

namespace SurveyApp.Controllers.Api
{
    // Survey Details API Controller - Manage items and specifications at locations    
    [Route("api/v1/surveys/{surveyId}/locations/{locId}/details")]
    [ApiController]
    [Authorize]
    public class SurveyDetailsApiController : ControllerBase
    {
        private readonly ISurvey _surveyRepo;
        private readonly ILogger<SurveyDetailsApiController> _logger;

        public SurveyDetailsApiController(
            ISurvey surveyRepo,
            ILogger<SurveyDetailsApiController> logger)
        {
            _surveyRepo = surveyRepo;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        #region Survey Details

        // Get all items/details for a location grouped by item type
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ItemTypeWithItemsDto>>), 200)]
        public IActionResult GetLocationDetails(long surveyId, int locId)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                // Get assigned item types for this location
                var itemTypes = _surveyRepo.GetAssignedTypeList(surveyId, locId);

                var result = new List<ItemTypeWithItemsDto>();

                foreach (var itemType in itemTypes)
                {
                    // Get items for this type
                    var items = _surveyRepo.GetAssignedItemList(surveyId, locId, itemType.ItemTypeID);

                    var itemDtos = items.Select(item => new SurveyDetailsDto
                    {
                        ItemId = item.ItemID,
                        ItemName = item.ItemName,
                        ItemDesc = item.ItemDesc,
                        ExistingQty = item.ItemQtyExist,
                        RequiredQty = item.ItemQtyReq,
                        Remarks = item.Remarks,
                        ImageUrls = !string.IsNullOrEmpty(item.ImgPath) 
                            ? item.ImgPath.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                            : new List<string>(),
                        Specifications = GetItemSpecifications(surveyId, locId, item.ItemID)
                    }).ToList();

                    result.Add(new ItemTypeWithItemsDto
                    {
                        ItemTypeId = itemType.ItemTypeID,
                        TypeName = itemType.TypeName,
                        TypeDesc = itemType.TypeDesc,
                        GroupName = itemType.GroupName,
                        Items = itemDtos
                    });
                }

                return Ok(ApiResponse<List<ItemTypeWithItemsDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting details for survey {SurveyId} location {LocId}", surveyId, locId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        // Get items for a specific item type at a location
        [HttpGet("types/{itemTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<ItemTypeWithItemsDto>), 200)]
        public IActionResult GetItemsByType(long surveyId, int locId, int itemTypeId)
        {
            try
            {
                var items = _surveyRepo.GetAssignedItemList(surveyId, locId, itemTypeId);
                var typeInfo = _surveyRepo.GetAssignedTypeList(surveyId, locId)
                    .FirstOrDefault(t => t.ItemTypeID == itemTypeId);

                if (typeInfo == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Item type not found for this location"));
                }

                var itemDtos = items.Select(item => new SurveyDetailsDto
                {
                    ItemId = item.ItemID,
                    ItemName = item.ItemName,
                    ItemDesc = item.ItemDesc,
                    ExistingQty = item.ItemQtyExist,
                    RequiredQty = item.ItemQtyReq,
                    Remarks = item.Remarks,
                    ImageUrls = !string.IsNullOrEmpty(item.ImgPath)
                        ? item.ImgPath.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>(),
                    Specifications = GetItemSpecifications(surveyId, locId, item.ItemID)
                }).ToList();

                var result = new ItemTypeWithItemsDto
                {
                    ItemTypeId = itemTypeId,
                    TypeName = typeInfo.TypeName,
                    TypeDesc = typeInfo.TypeDesc,
                    GroupName = typeInfo.GroupName,
                    Items = itemDtos
                };

                return Ok(ApiResponse<ItemTypeWithItemsDto>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items for type {ItemTypeId}", itemTypeId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        // Update survey item quantities, remarks, and specifications
        [HttpPut("items/{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult UpdateItem(long surveyId, int locId, int itemId, [FromBody] SurveyItemUpdateRequest request)
        {
            try
            {
                if (itemId != request.ItemId)
                {
                    return BadRequest(ApiResponse<object>.Fail("Item ID mismatch"));
                }

                var userId = GetCurrentUserId();

                // Get item type for this item
                var itemTypes = _surveyRepo.GetAssignedTypeList(surveyId, locId);
                var itemTypeId = 0;
                foreach (var type in itemTypes)
                {
                    var items = _surveyRepo.GetAssignedItemList(surveyId, locId, type.ItemTypeID);
                    if (items.Any(i => i.ItemID == itemId))
                    {
                        itemTypeId = type.ItemTypeID;
                        break;
                    }
                }

                // Create the update model with the item in the list
                var updateModel = new SurveyDetailsUpdate
                {
                    SurveyID = surveyId,
                    LocID = locId,
                    ItemTypeID = itemTypeId,
                    CreateBy = userId,
                    ItemLists = new List<SurveyDetailsUpdatelist>
                    {
                        new SurveyDetailsUpdatelist
                        {
                            ItemID = itemId,
                            ItemQtyExist = request.ExistingQty,
                            ItemQtyReq = request.RequiredQty,
                            Remarks = request.Remarks ?? ""
                        }
                    }
                };

                var result = _surveyRepo.UpdateSurveyDetails(updateModel);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to update item"));
                }

                // Update specifications if provided
                if (request.Specifications != null && request.Specifications.Any())
                {
                    var specModel = new SpecificationDetailsSubmitModel
                    {
                        SurveyID = surveyId,
                        LocID = locId,
                        ItemID = itemId,
                        Specifications = request.Specifications.Select(s => new SpecificationDetailItem
                        {
                            SpecificationID = s.SpecificationId,
                            SpecificationDetails = s.Value ?? ""
                        }).ToList()
                    };
                    _surveyRepo.SaveSpecificationDetails(specModel, userId);
                }

                _logger.LogInformation("Item {ItemId} updated for survey {SurveyId} location {LocId}", itemId, surveyId, locId);
                return Ok(ApiResponse<object>.Ok(null, "Item updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        // Bulk update multiple items at a location
        [HttpPut("items")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult BulkUpdateItems(long surveyId, int locId, [FromBody] List<SurveyItemUpdateRequest> items)
        {
            try
            {
                var userId = GetCurrentUserId();
                var successCount = 0;
                var errors = new List<string>();

                // Group items by type for efficient updates
                var itemTypes = _surveyRepo.GetAssignedTypeList(surveyId, locId);
                
                foreach (var itemType in itemTypes)
                {
                    var typeItems = _surveyRepo.GetAssignedItemList(surveyId, locId, itemType.ItemTypeID);
                    var typeItemIds = typeItems.Select(i => i.ItemID).ToHashSet();
                    
                    var requestItemsForType = items.Where(i => typeItemIds.Contains(i.ItemId)).ToList();
                    if (!requestItemsForType.Any()) continue;

                    var updateModel = new SurveyDetailsUpdate
                    {
                        SurveyID = surveyId,
                        LocID = locId,
                        ItemTypeID = itemType.ItemTypeID,
                        CreateBy = userId,
                        ItemLists = requestItemsForType.Select(item => new SurveyDetailsUpdatelist
                        {
                            ItemID = item.ItemId,
                            ItemQtyExist = item.ExistingQty,
                            ItemQtyReq = item.RequiredQty,
                            Remarks = item.Remarks ?? ""
                        }).ToList()
                    };

                    try
                    {
                        var result = _surveyRepo.UpdateSurveyDetails(updateModel);
                        if (result)
                        {
                            successCount += requestItemsForType.Count;

                            // Update specifications for each item
                            foreach (var item in requestItemsForType.Where(i => i.Specifications != null && i.Specifications.Any()))
                            {
                                var specModel = new SpecificationDetailsSubmitModel
                                {
                                    SurveyID = surveyId,
                                    LocID = locId,
                                    ItemID = item.ItemId,
                                    Specifications = item.Specifications!.Select(s => new SpecificationDetailItem
                                    {
                                        SpecificationID = s.SpecificationId,
                                        SpecificationDetails = s.Value ?? ""
                                    }).ToList()
                                };
                                _surveyRepo.SaveSpecificationDetails(specModel, userId);
                            }
                        }
                        else
                        {
                            foreach (var item in requestItemsForType)
                            {
                                errors.Add($"Failed to update item {item.ItemId}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        foreach (var item in requestItemsForType)
                        {
                            errors.Add($"Error updating item {item.ItemId}: {ex.Message}");
                        }
                    }
                }

                if (errors.Any())
                {
                    return Ok(ApiResponse<object>.Fail(
                        $"Updated {successCount} of {items.Count} items", 
                        errors));
                }

                return Ok(ApiResponse<object>.Ok(null, $"All {successCount} items updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating items");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Specifications

        // Get specifications for an item
        [HttpGet("items/{itemId}/specifications")]
        [ProducesResponseType(typeof(ApiResponse<List<ItemSpecificationDto>>), 200)]
        public IActionResult GetItemSpecificationsEndpoint(long surveyId, int locId, int itemId)
        {
            try
            {
                // Get specification definitions for this item
                var specs = _surveyRepo.GetItemSpecifications(itemId);
                
                // Get saved values
                var savedValues = _surveyRepo.GetSpecificationDetails(surveyId, locId, itemId);

                var result = specs.Select(s => {
                    var savedValue = savedValues.FirstOrDefault(v => v.SpecificationID == s.SpecificationID);
                    var options = s.InputType == "dropdown" 
                        ? _surveyRepo.GetSpecificationOptions(s.SpecificationID)
                        : new List<SpecificationOptionModel>();

                    return new ItemSpecificationDto
                    {
                        SpecificationId = s.SpecificationID,
                        SpecificationName = s.SpecificationName,
                        InputType = s.InputType,
                        IsRequired = false, // Not in original model
                        DisplayOrder = 0, // Not in original model
                        SavedValue = savedValue?.SpecificationDetails,
                        Options = options.Select(o => new SpecificationOptionDto
                        {
                            OptionId = o.OptionID,
                            OptionValue = o.OptionValue,
                            DisplayOrder = o.DisplayOrder
                        }).ToList()
                    };
                }).ToList();

                return Ok(ApiResponse<List<ItemSpecificationDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting specifications for item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        // Save specification values for an item
        [HttpPut("items/{itemId}/specifications")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult SaveSpecifications(long surveyId, int locId, int itemId, [FromBody] List<SpecificationValueDto> specifications)
        {
            try
            {
                var userId = GetCurrentUserId();

                var model = new SpecificationDetailsSubmitModel
                {
                    SurveyID = surveyId,
                    LocID = locId,
                    ItemID = itemId,
                    Specifications = specifications.Select(s => new SpecificationDetailItem
                    {
                        SpecificationID = s.SpecificationId,
                        SpecificationDetails = s.Value ?? ""
                    }).ToList()
                };
                
                _surveyRepo.SaveSpecificationDetails(model, userId);

                return Ok(ApiResponse<object>.Ok(null, "Specifications saved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving specifications for item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Helper Methods

        private List<SpecificationValueDto> GetItemSpecifications(long surveyId, int locId, int itemId)
        {
            try
            {
                var savedValues = _surveyRepo.GetSpecificationDetails(surveyId, locId, itemId);
                return savedValues.Select(s => new SpecificationValueDto
                {
                    SpecificationId = s.SpecificationID,
                    SpecificationName = s.SpecificationName,
                    Value = s.SpecificationDetails
                }).ToList();
            }
            catch
            {
                return new List<SpecificationValueDto>();
            }
        }

        #endregion
    }

    #region Additional DTOs

    public class ItemSpecificationDto
    {
        public int SpecificationId { get; set; }
        public string SpecificationName { get; set; } = string.Empty;
        public string? InputType { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? SavedValue { get; set; }
        public List<SpecificationOptionDto> Options { get; set; } = new();
    }

    public class SpecificationOptionDto
    {
        public int OptionId { get; set; }
        public string OptionValue { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    #endregion
}
