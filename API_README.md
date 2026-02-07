# Survey Application REST API Documentation

## Overview

This document provides a comprehensive guide to access the Survey Application REST APIs. The API uses **JWT Bearer Token** authentication and follows RESTful conventions.

**Base URL**: `https://survey.vluccc.com:91/api/v1`
---

## Table of Contents

1. [Authentication](#authentication)
2. [Survey APIs](#survey-apis)
3. [Location APIs](#location-apis)
4. [Survey Details APIs](#survey-details-apis)
5. [Submission & Workflow APIs](#submission--workflow-apis)
6. [Reports APIs](#reports-apis)
7. [Dashboard APIs](#dashboard-apis)
8. [Master Data APIs](#master-data-apis)
9. [Image Upload APIs](#image-upload-apis)
10. [Response Format](#response-format)
11. [Error Handling](#error-handling)

---

## Authentication

All API endpoints (except login) require JWT Bearer Token authentication.

### Login

**POST** `/api/v1/auth/login`

Authenticate user and receive JWT tokens.

**Request Body:**
```json
{
  "loginId": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "userId": 1,
      "loginId": "user@example.com",
      "loginName": "John Doe",
      "roleId": 1,
      "roleName": "Admin",
      "email": "user@example.com"
    }
  }
}
```

### Refresh Token

**POST** `/api/v1/auth/refresh`

Refresh the access token using a valid refresh token.

**Request Body:**
```json
{
  "refreshToken": "string"
}
```

### Logout

**POST** `/api/v1/auth/logout`

Invalidate the current user's tokens.

**Headers:** `Authorization: Bearer {accessToken}`

### Get Current User

**GET** `/api/v1/auth/me`

Get the currently authenticated user's information.

**Headers:** `Authorization: Bearer {accessToken}`

---

## Survey APIs

Base path: `/api/v1/surveys`

### List Surveys

**GET** `/api/v1/surveys`

Get all surveys with optional filtering and pagination.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `status` | string | Filter by survey status |
| `region` | string | Filter by region name |
| `implementationType` | string | Filter by implementation type |
| `clientId` | int | Filter by client ID |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 20) |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "surveyId": 1,
        "surveyName": "Traffic Survey - Delhi",
        "status": "In Progress",
        "clientName": "Client A",
        "regionName": "North",
        "implementationType": "New Project",
        "dueDate": "2024-12-31",
        "surveyDate": "2024-01-15",
        "isRevised": false,
        "locationCount": 5
      }
    ],
    "totalCount": 100,
    "page": 1,
    "pageSize": 20
  }
}
```

### Get Survey by ID

**GET** `/api/v1/surveys/{id}`

Get detailed information about a specific survey.

### Create Survey

**POST** `/api/v1/surveys`

Create a new survey.

**Request Body:**
```json
{
  "surveyName": "string",
  "implementationType": "string",
  "surveyDate": "2024-01-15",
  "surveyTeamName": "string",
  "surveyTeamContact": "string",
  "agencyName": "string",
  "locationSiteName": "string",
  "stateId": 1,
  "cityId": 1,
  "scopeOfWork": "string",
  "latitude": 28.6139,
  "longitude": 77.2090,
  "dueDate": "2024-12-31",
  "regionId": 1,
  "clientId": 1
}
```

### Update Survey

**PUT** `/api/v1/surveys/{id}`

Update an existing survey.

### Delete Survey

**DELETE** `/api/v1/surveys/{id}`

Delete a survey (only if not approved).

### Update Survey Status

**PATCH** `/api/v1/surveys/{id}/status`

Update the status of a survey.

**Request Body:**
```json
{
  "status": "In Progress"
}
```

### Check Completion Status

**GET** `/api/v1/surveys/{id}/completion-status`

Check if a survey is ready for submission.

---

## Location APIs

Base path: `/api/v1/surveys/{surveyId}/locations`

### List Locations

**GET** `/api/v1/surveys/{surveyId}/locations`

Get all locations for a survey.

### Get Location

**GET** `/api/v1/surveys/{surveyId}/locations/{locId}`

Get a specific location.

### Create Location

**POST** `/api/v1/surveys/{surveyId}/locations`

Add a new location to a survey.

**Request Body:**
```json
{
  "locName": "Junction 1",
  "latitude": 28.6139,
  "longitude": 77.2090,
  "locationType": "Traffic",
  "wayType": "Four Way",
  "isGlobal": false,
  "itemTypeIds": [1, 2, 3]
}
```

### Update Location

**PUT** `/api/v1/surveys/{surveyId}/locations/{locId}`

Update an existing location.

### Delete Location

**DELETE** `/api/v1/surveys/{surveyId}/locations/{locId}`

Delete a location.

### Mark Location as Completed

**POST** `/api/v1/surveys/{surveyId}/locations/{locId}/complete`

Mark a location as completed (locks for editing).

### Unlock Location

**POST** `/api/v1/surveys/{surveyId}/locations/{locId}/unlock`

Unlock a location for editing.

### Get Item Types for Location

**GET** `/api/v1/surveys/{surveyId}/locations/{locId}/item-types`

Get available item types for a location.

### Assign Item Types

**POST** `/api/v1/surveys/{surveyId}/locations/{locId}/item-types`

Assign item types to a location.

**Request Body:**
```json
{
  "itemTypeIds": [1, 2, 3]
}
```

---

## Survey Details APIs

Base path: `/api/v1/surveys/{surveyId}/locations/{locId}/details`

### Get Location Details

**GET** `/api/v1/surveys/{surveyId}/locations/{locId}/details`

Get all items/details for a location grouped by item type.

### Get Items by Type

**GET** `/api/v1/surveys/{surveyId}/locations/{locId}/details/types/{itemTypeId}`

Get items for a specific item type at a location.

### Update Item

**PUT** `/api/v1/surveys/{surveyId}/locations/{locId}/details/items/{itemId}`

Update survey item quantities, remarks, and specifications.

**Request Body:**
```json
{
  "itemId": 1,
  "existingQty": 5,
  "requiredQty": 10,
  "remarks": "Additional cameras needed",
  "specifications": [
    {
      "specificationId": 1,
      "value": "6.5m"
    }
  ]
}
```

### Bulk Update Items

**PUT** `/api/v1/surveys/{surveyId}/locations/{locId}/details/items`

Update multiple items at once.

### Get Item Specifications

**GET** `/api/v1/surveys/{surveyId}/locations/{locId}/details/items/{itemId}/specifications`

Get specifications for an item.

### Save Specifications

**PUT** `/api/v1/surveys/{surveyId}/locations/{locId}/details/items/{itemId}/specifications`

Save specification values for an item.

---

## Submission & Workflow APIs

Base path: `/api/v1/submissions`

### Submit Survey

**POST** `/api/v1/submissions/surveys/{surveyId}/submit`

Submit a survey for approval.

**Request Body:**
```json
{
  "remarks": "Ready for review"
}
```

### Get Submission Status

**GET** `/api/v1/submissions/surveys/{surveyId}/status`

Get submission status for a survey.

### Review Survey (Approve)

**POST** `/api/v1/submissions/surveys/{surveyId}/approve`

Approve a submitted survey.

**Request Body:**
```json
{
  "comments": "Approved with no changes"
}
```

### Review Survey (Reject)

**POST** `/api/v1/submissions/surveys/{surveyId}/reject`

Reject a submitted survey.

**Request Body:**
```json
{
  "comments": "Please update location 3 details"
}
```

### Assign Survey

**POST** `/api/v1/submissions/surveys/{surveyId}/assign`

Assign survey to team members.

**Request Body:**
```json
{
  "employeeId": 5,
  "dueDate": "2024-12-31"
}
```

### Get Assignments

**GET** `/api/v1/submissions/surveys/{surveyId}/assignments`

Get all assignments for a survey.

### Remove Assignment

**DELETE** `/api/v1/submissions/surveys/{surveyId}/assignments/{transId}`

Remove an assignment.

### Get Pending Reviews (Admin)

**GET** `/api/v1/submissions/pending-reviews`

Get surveys pending review (Admin only).

---

## Reports APIs

Base path: `/api/v1/reports`

### Summary Report

**GET** `/api/v1/reports/summary`

Get summary report with aggregated statistics.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `fromDate` | string | Start date (YYYY-MM-DD) |
| `toDate` | string | End date (YYYY-MM-DD) |
| `status` | string | Filter by status |
| `region` | string | Filter by region |
| `implementationType` | string | Filter by type |

### Detailed Report

**GET** `/api/v1/reports/{surveyId}/detailed`

Get detailed report for a specific survey.

### Requirement Summary

**GET** `/api/v1/reports/{surveyId}/requirements`

Get requirement summary grouped by item type.

---

## Dashboard APIs

Base path: `/api/v1/dashboard`

### Get Dashboard Statistics

**GET** `/api/v1/dashboard/stats`

Get dashboard statistics overview.

**Response:**
```json
{
  "success": true,
  "data": {
    "totalSurveys": 150,
    "createdSurveys": 20,
    "assignedSurveys": 30,
    "inProgressSurveys": 50,
    "submittedSurveys": 25,
    "completedSurveys": 20,
    "pendingSurveys": 5,
    "onHoldSurveys": 0,
    "missedDeadlineSurveys": 3,
    "completionRate": 13.3,
    "surveysByStatus": {},
    "surveysByRegion": {},
    "surveysByImplementationType": {}
  }
}
```

### Get Recent Surveys

**GET** `/api/v1/dashboard/recent`

Get recently created/updated surveys.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `count` | int | 10 | Number of surveys to return |

### Get Overdue Surveys

**GET** `/api/v1/dashboard/overdue`

Get surveys that have missed their due date.

---

## Master Data APIs

Base path: `/api/v1`

### Clients

**GET** `/api/v1/clients` - Get all clients  
**GET** `/api/v1/clients/{id}` - Get client by ID

### Employees

**GET** `/api/v1/employees` - Get all employees  
**GET** `/api/v1/employees/{id}` - Get employee by ID

### Regions

**GET** `/api/v1/regions` - Get all regions

### Device Modules

**GET** `/api/v1/device-modules` - Get all device modules/categories

### Devices

**GET** `/api/v1/devices` - Get all devices/items  
**GET** `/api/v1/devices/{id}` - Get device by ID

**Query Parameters for devices:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `moduleId` | int | Filter by module ID |
| `activeOnly` | bool | Show only active items |

### Dropdown Options

**GET** `/api/v1/options/implementation-types` - Get implementation type options  
**GET** `/api/v1/options/survey-statuses` - Get survey status options  
**GET** `/api/v1/options/location-types` - Get location type options  
**GET** `/api/v1/options/way-types` - Get way type options

---

## Image Upload APIs

Base path: `/api/v1/images`

### Upload Image

**POST** `/api/v1/images/upload`

Upload an image for a survey item (uses Cloudinary).

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `surveyId` | long | Survey ID |
| `locId` | int | Location ID |
| `itemId` | int | Item ID |

**Request:** `multipart/form-data` with `file` field

**Response:**
```json
{
  "success": true,
  "data": {
    "imageUrl": "https://res.cloudinary.com/...",
    "publicId": "surveys/survey_1/loc_1/item_1_123456789"
  }
}
```

### Delete Image

**DELETE** `/api/v1/images?publicId={publicId}`

Delete an image from Cloudinary.

---

## Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "success": true,
  "message": "Optional success message",
  "data": { ... }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Error 1", "Error 2"]
}
```

---

## Error Handling

### HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Invalid or missing token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 500 | Internal Server Error |

### Authentication Errors

- **401 Unauthorized**: Token is missing, expired, or invalid
- **403 Forbidden**: User doesn't have permission for this action

### Token Expiry

Access tokens expire after 1 hour. Use the refresh token endpoint to get a new access token without re-authenticating.

---

## Usage Examples

### cURL Examples

**Login:**
```bash
curl -X POST "https://survey.vluccc.com:91/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"loginId": "user@example.com", "password": "yourpassword"}'
```

**Get Surveys (with token):**
```bash
curl -X GET "https://survey.vluccc.com:91/api/v1/surveys?page=1&pageSize=10" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Create Survey:**
```bash
curl -X POST "https://survey.vluccc.com:91/api/v1/surveys" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "surveyName": "New Traffic Survey",
    "implementationType": "New Project",
    "regionId": 1,
    "clientId": 1
  }'
```

### JavaScript/Fetch Example

```javascript
// Login
const loginResponse = await fetch('/api/v1/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ loginId: 'user@example.com', password: 'password' })
});
const { data } = await loginResponse.json();
const token = data.accessToken;

// Get Surveys
const surveysResponse = await fetch('/api/v1/surveys', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const surveys = await surveysResponse.json();
```

---

## Swagger Documentation

The API also provides interactive Swagger documentation at:

**URL**: `https://your-domain.com/swagger`

This provides a complete interactive interface to explore and test all API endpoints.

---

## Configuration

### JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "SurveyApp",
    "Audience": "SurveyAppUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Cloudinary Settings (for image uploads)

```json
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

---

## Contact & Support

For API support or issues, please contact the development team.

**Version**: 1.0  
**Last Updated**: February 2026
