# Car Park Management API

An ASP.NET Core API for managing entry to and exit from a car park.

SQL Server Express LocalDB is available only on Windows. The connection string can be overridden to use another accessible SQL Server instance.

## Prerequisites

The following software is required:

* .NET 10 SDK
* SQL Server Express LocalDB
* Git

SQL Server Expresss LocalDB is commonly installed with Visual Studio. Its availability can be checked from a command prompt:

```pwsh
sqllocaldb info MSSQLLocalDB
```

The app uses the following local development database by default:

```text
Server=(localdb)\MSSQLLocalDB;Database=CarParkAssessment;Trusted_Connection=True;TrustServerCertificate=True
```

SQL Server Express LocalDB is available only on Windows. If needed, the connection string can be replaced through configuration or overridden using the environment variable:

```text
ConnectionStrings__CarPark
```

## Preparing the application

From the repo root, restore the .NET tools:

```pwsh
dotnet tool restore
```

Restore the project dependencies:

```pwsh
dotnet restore
```

Build the solution:

```powershell
dotnet build
```

The application uses Entity Framework Core migrations to create and update the database schema.

Pending migrations are applied automatically when the application starts. They can also be applied manually:

```pwsh
dotnet ef database update `
  --project src/CarPark.Api `
  --startup-project src/CarPark.Api
```

## Running the application

From the repository root:

```pwsh
dotnet run --project src/CarPark.Api
```

The application will:

1. Connect to the configured SQL Server database.
2. Apply any pending Entity Framework Core migrations.
3. Create ten numbered parking spaces if the initial migration has not already been applied.
4. Start the HTTP API.

When running in the Development environment, Swagger UI is available by opening the URL shown in the console and adding:

```text
/swagger
```

e.g:

```text
https://localhost:<port>/swagger
```

## Running the tests

From the repository root:

```powershell
dotnet test
```

The DBContext integration tests require SQL Server Express LocalDB. Each test creates a unique temporary database, applies the Entity Framework Core migrations and deletes the database when the test completes. The application development database is not used or modified by the tests.

## API

The API preserves the PascalCase property names specified in the assessment brief. All response timestamps are returned as UTC values.

Vehicle types and base rates are:

| Value | Vehicle type | Rate |
| ----: | ------------ | ---: |
| `1` | Small car | £0.10 per minute |
| `2` | Medium car | £0.20 per minute |
| `3` | Large car  | £0.40 per minute |

### Park a vehicle

```http
POST /parking
```

Example request:

```json
{
  "VehicleReg": "ab12 cde",
  "VehicleType": 1
}
```

Example successful response:

```json
{
  "VehicleReg": "AB12CDE",
  "SpaceNumber": 1,
  "TimeIn": "2026-07-13T12:13:04.5747228Z"
}
```

Possible results:

* `200 OK` — the vehicle was parked successfully.
* `400 Bad Request` — the registration or vehicle type was invalid.
* `409 Conflict` — the vehicle is already parked or the car park is full.

### Get parking status

```http
GET /parking
```

Example successful response:

```json
{
  "AvailableSpaces": 9,
  "OccupiedSpaces": 1
}
```

Result:

* `200 OK`

### Exit a vehicle

```http
POST /parking/exit
```

Example request:

```json
{
  "VehicleReg": "AB12 CDE"
}
```

Example successful response:

```json
{
  "VehicleReg": "AB12CDE",
  "VehicleCharge": 0.1,
  "TimeIn": "2026-07-13T12:13:04.5747228Z",
  "TimeOut": "2026-07-13T12:13:47.8331032Z"
}
```

Possible results:

* `200 OK` — the vehicle exited successfully.
* `400 Bad Request` — the registration was invalid.
* `404 Not Found` — the vehicle is not currently parked.

## Charging assumptions

The brief specifies a charge per minute and an additional £1 for every five minutes. Because the treatment of partial minutes is not specified, the following rules are used:

1. Parking duration is calculated from `TimeIn` to `TimeOut`.
2. Any partial minute is rounded up to the next whole minute.
3. A stay shorter than 1 minute is charged as one minute.
4. The base charge is the number of billable minutes multiplied by the vehicle-type rate.
5. An extra £1 is charged for each complete five-minute block.
6. A stay of exactly five billable minutes therefore receives one £1 surcharge.
7. The final charge is rounded to two decimal places.

We can express the calculation as:

```text
Billable minutes = max(1, ceiling(total elapsed minutes))

Base charge = billable minutes × vehicle rate

Five-minute surcharge = floor(billable minutes ÷ 5) × £1

Total charge = base charge + five-minute surcharge
```

Example for a medium car parked for four minutes and thirty seconds:

```text
Billable minutes: 5
Base charge:       5 × £0.20 = £1.00
Surcharge:         1 × £1.00 = £1.00
Total:             £2.00
```

## Error responses

Request-validation failures use the standard ASP.NET Core validation responses.

Expected application failures return an appropriate HTTP status code and a brief message:

```json
{
  "Message": "The vehicle is already parked."
}
```

Handled scenarios include:

* Invalid vehicle registrations — `400 Bad Request`.
* Unsupported vehicle types — `400 Bad Request`.
* Parking a vehicle that already has an active session — `409 Conflict`.
* Parking when no space is available — `409 Conflict`.
* Exiting a vehicle that is not currently parked — `404 Not Found`.

## Data and seeding

The application uses SQL Server through Entity Framework Core.

The database contains:

* `ParkingSpaces`, defining the available numbered spaces.
* `ParkingSessions`, recording current and completed vehicle stays.

The initial migration creates ten parking spaces numbered 1 through 10.

Parking-space rows are the authoritative definition of capacity. There is no separate total-capacity value.

Occupancy is calculated from parking sessions that do not have a `TimeOut`. It is not stored as a separate flag on a parking space.

The initial parking-space data is managed through the Entity Framework Core migration. Applying the migration more than once does not create duplicate spaces.

Vehicle registrations have whitespace removed and are normalised to uppercase before they are stored or compared.

## Assumptions and questions

The brief does not specify every behavioural detail. The following implementation assumptions have therefore been made:

* The car park contains ten spaces.
* The lowest-numbered available space is allocated first.
* A vehicle can have only one active parking session.
* A parking space can have only one active parking session.
* Occupancy is determined from sessions that do not have a `TimeOut`.
* Timestamps are recorded using `DateTimeOffset`.
* Authentication and authorisation are outside the scope of the assessment.
* The charging rules described above are used for partial minutes and five-minute surcharges.

The main questions that would be clarified with the product owner are:

* How many spaces should the car park contain?
* Should partial minutes be rounded up or charged proportionally?
* Does the additional £1 apply when the billable duration reaches exactly five minutes?
* What vehicle-registration formats and validation rules must be supported?

Vehicle registrations have whitespace removed and are normalised to uppercase before they are stored or compared. Full UK registration-format validation has deliberately not been implemented. A reliable regular-expression-based validator should be derived from authoritative official documentation and tested against current and historic registration formats, rather than guessed for this assessment.

## Production considerations

For a production system, the following would be considered:

* Applying database migrations through the deployment pipeline rather than during application startup.
* Using a managed SQL Server instance with securely stored connection details.
* Protecting entry and exit operations against concurrent requests using transactions and appropriate database constraints.
* Adding authentication, authorisation, structured logging, monitoring and health checks.
* Making parking capacity and tariffs configurable where required, while retaining the tariff applied to historical sessions.
* Defining appropriate validation, retention and privacy policies for vehicle-registration data.
