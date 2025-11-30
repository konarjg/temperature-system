## Open API spec

```Json
{
  "openapi": "3.0.4",
  "info": {
    "title": "TemperatureSystem",
    "version": "1.0"
  },
  "paths": {
    "/api/auth": {
      "post": {
        "tags": [
          "AuthEndpoints"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/AuthRequest"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/auth/refresh": {
      "put": {
        "tags": [
          "AuthEndpoints"
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/auth/logout": {
      "delete": {
        "tags": [
          "AuthEndpoints"
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/auth/verify/{token}": {
      "get": {
        "tags": [
          "AuthEndpoints"
        ],
        "parameters": [
          {
            "name": "token",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/measurements/latest": {
      "get": {
        "tags": [
          "MeasurementEndpoints"
        ],
        "parameters": [
          {
            "name": "SensorId",
            "in": "query",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          },
          {
            "name": "Points",
            "in": "query",
            "required": true,
            "schema": {
              "maximum": 1000,
              "minimum": 1,
              "type": "integer",
              "format": "int32"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/measurements/{id}": {
      "get": {
        "tags": [
          "MeasurementEndpoints"
        ],
        "operationId": "GetMeasurementById",
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      },
      "delete": {
        "tags": [
          "MeasurementEndpoints"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/measurements/history": {
      "get": {
        "tags": [
          "MeasurementEndpoints"
        ],
        "parameters": [
          {
            "name": "StartDate",
            "in": "query",
            "required": true,
            "schema": {
              "type": "string",
              "format": "date-time"
            }
          },
          {
            "name": "EndDate",
            "in": "query",
            "required": true,
            "schema": {
              "type": "string",
              "format": "date-time"
            }
          },
          {
            "name": "PageSize",
            "in": "query",
            "schema": {
              "maximum": 100,
              "minimum": 1,
              "type": "integer",
              "format": "int32",
              "default": 100
            }
          },
          {
            "name": "Cursor",
            "in": "query",
            "schema": {
              "type": "string",
              "format": "date-time"
            }
          },
          {
            "name": "SensorId",
            "in": "query",
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/measurements/aggregated-history": {
      "get": {
        "tags": [
          "MeasurementEndpoints"
        ],
        "parameters": [
          {
            "name": "StartDate",
            "in": "query",
            "required": true,
            "schema": {
              "type": "string",
              "format": "date-time"
            }
          },
          {
            "name": "EndDate",
            "in": "query",
            "required": true,
            "schema": {
              "type": "string",
              "format": "date-time"
            }
          },
          {
            "name": "Granularity",
            "in": "query",
            "required": true,
            "schema": {
              "$ref": "#/components/schemas/MeasurementHistoryGranularity"
            }
          },
          {
            "name": "SensorId",
            "in": "query",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/sensors/{id}": {
      "get": {
        "tags": [
          "SensorEndpoints"
        ],
        "operationId": "GetSensorById",
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      },
      "put": {
        "tags": [
          "SensorEndpoints"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/SensorRequest"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      },
      "delete": {
        "tags": [
          "SensorEndpoints"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/sensors": {
      "get": {
        "tags": [
          "SensorEndpoints"
        ],
        "parameters": [
          {
            "name": "state",
            "in": "query",
            "schema": {
              "$ref": "#/components/schemas/SensorState"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      },
      "post": {
        "tags": [
          "SensorEndpoints"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/SensorRequest"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/users/{id}": {
      "get": {
        "tags": [
          "UserEndpoints"
        ],
        "operationId": "GetUserById",
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      },
      "delete": {
        "tags": [
          "UserEndpoints"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/users": {
      "post": {
        "tags": [
          "UserEndpoints"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/UserRequest"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/users/{id}/credentials": {
      "put": {
        "tags": [
          "UserEndpoints"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/UserRequest"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    },
    "/api/users/{id}/role": {
      "put": {
        "tags": [
          "UserEndpoints"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "integer",
              "format": "int64"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/UserRoleRequest"
              }
            }
          },
          "required": true
        },
        "responses": {
          "200": {
            "description": "OK"
          }
        }
      }
    }
  },
  "components": {
    "schemas": {
      "AuthRequest": {
        "type": "object",
        "properties": {
          "email": {
            "type": "string",
            "nullable": true
          },
          "password": {
            "type": "string",
            "nullable": true
          }
        },
        "additionalProperties": false
      },
      "MeasurementHistoryGranularity": {
        "enum": [
          "Hourly",
          "Daily",
          "Monthly"
        ],
        "type": "string"
      },
      "Role": {
        "enum": [
          "Unverified",
          "Viewer",
          "Admin"
        ],
        "type": "string"
      },
      "SensorRequest": {
        "type": "object",
        "properties": {
          "displayName": {
            "type": "string",
            "nullable": true
          },
          "deviceAddress": {
            "type": "string",
            "nullable": true
          }
        },
        "additionalProperties": false
      },
      "SensorState": {
        "enum": [
          "Operational",
          "Unavailable"
        ],
        "type": "string"
      },
      "UserRequest": {
        "type": "object",
        "properties": {
          "email": {
            "type": "string",
            "nullable": true
          },
          "password": {
            "type": "string",
            "nullable": true
          }
        },
        "additionalProperties": false
      },
      "UserRoleRequest": {
        "type": "object",
        "properties": {
          "role": {
            "$ref": "#/components/schemas/Role"
          }
        },
        "additionalProperties": false
      }
    },
    "securitySchemes": {
      "Bearer": {
        "type": "http",
        "description": "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        "scheme": "Bearer",
        "bearerFormat": "JWT"
      }
    }
  },
  "security": [
    {
      "Bearer": [ ]
    }
  ]
}
```