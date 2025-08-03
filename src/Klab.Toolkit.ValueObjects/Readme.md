# Klab.Toolkit.ValueObjects

## Overview

The `Klab.Toolkit.ValueObjects` package provides a foundation for implementing Domain-Driven Design (DDD) value objects in .NET applications. Value objects are immutable objects that represent concepts defined by their attributes rather than their identity, ensuring data integrity, encapsulating business rules, and promoting a rich domain model.

## Purpose

This package enables developers to:

- **Enforce Business Rules**: Validate data at the object creation level
- **Ensure Immutability**: Prevent accidental data modification after creation
- **Provide Type Safety**: Replace primitive obsession with meaningful types
- **Centralize Validation**: Keep validation logic close to the data it protects
- **Enhance Domain Models**: Create expressive, self-documenting code

## Key Features

### Built-in Value Objects
- **`Email`**: RFC-compliant email address validation
- **`ComPort`**: Serial communication port representation (COM1-COM256)
- **`IpAddress`**: IP address validation and formatting
- **`Voltage`**: Physical voltage measurements with unit conversions
- **`Pressure`**: Physical pressure measurements with unit conversions
- **`Current`**: Electrical current measurements with unit conversions

### Core Principles
- **Validation on Creation**: Invalid objects cannot be instantiated
- **Immutability**: All value objects are implemented as records
- **Self-Contained**: Each value object encapsulates its own validation rules
- **Rich API**: Meaningful methods and properties for domain operations

## Installation

```bash
dotnet add package Klab.Toolkit.ValueObjects
```

## Built-in Value Objects

### Email Address

```csharp
using Klab.Toolkit.ValueObjects;

// Valid email creation
Email validEmail = Email.Create("user@example.com");
Console.WriteLine(validEmail.Value); // "user@example.com"

// Invalid email throws exception
try
{
    Email invalidEmail = Email.Create("not-an-email");
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message); // "E-Mail is invalid"
}

// Empty email handling
try
{
    Email emptyEmail = Email.Create("");
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message); // "Empty E-Mail Address is not possible"
}
```

#### Email Integration Example

```csharp
public class UserService
{
    public Result<User> CreateUser(string emailInput, string name)
    {
        try
        {
            var email = Email.Create(emailInput);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = name,
                CreatedAt = DateTime.UtcNow
            };

            return Result.Success(user);
        }
        catch (ArgumentException ex)
        {
            return Error.Create("INVALID_EMAIL", ex.Message);
        }
    }
}

public class User
{
    public Guid Id { get; set; }
    public Email Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### COM Port

```csharp
// Valid COM port creation
ComPort port1 = ComPort.Create("COM1");
ComPort port255 = ComPort.Create("COM255");

Console.WriteLine(port1.Value); // "COM1"

// Invalid COM port examples
try
{
    ComPort invalidPort = ComPort.Create("COM0"); // Invalid: starts from COM1
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message); // "COM Port is invalid"
}

try
{
    ComPort invalidPort = ComPort.Create("COM257"); // Invalid: max is COM256
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message); // "COM Port is invalid"
}
```

#### COM Port in Industrial Applications

```csharp
public class SerialDeviceManager
{
    private readonly Dictionary<ComPort, ISerialDevice> _devices = new();

    public Result ConnectDevice(string portName, ISerialDevice device)
    {
        try
        {
            var comPort = ComPort.Create(portName);

            if (_devices.ContainsKey(comPort))
            {
                return Error.Create("PORT_IN_USE", $"Port {comPort.Value} is already in use");
            }

            // Attempt connection
            device.Connect(comPort.Value);
            _devices[comPort] = device;

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Error.Create("INVALID_PORT", ex.Message);
        }
        catch (Exception ex)
        {
            return Error.Create("CONNECTION_FAILED", $"Failed to connect to {portName}: {ex.Message}");
        }
    }

    public IEnumerable<ComPort> GetConnectedPorts()
    {
        return _devices.Keys;
    }
}
```

### Physical Measurements

#### Voltage

```csharp
// Create voltage measurements
Voltage voltage1 = Voltage.Create(5.0); // 5 volts
Voltage voltage2 = Voltage.FromMillivolt(3300); // 3.3 volts from millivolts

// Access different units
Console.WriteLine($"Voltage: {voltage1.Volts}V"); // 5V
Console.WriteLine($"Millivolts: {voltage1.Millivolts}mV"); // 5000mV
Console.WriteLine($"Microvolts: {voltage1.Microvolts}μV"); // 5000000μV
Console.WriteLine($"Kilovolts: {voltage1.Kilovolts}kV"); // 0.005kV

// Voltage from different units
Voltage fromMicrovolts = Voltage.FromMicrovolt(1500000); // 1.5V
Voltage fromKilovolts = Voltage.FromKilovolt(0.012); // 12V
Voltage fromMegavolts = Voltage.FromMegavolt(0.000001); // 1V
```

#### Voltage in Electronics Applications

```csharp
public class PowerSupplyController
{
    private readonly Dictionary<string, VoltageRange> _allowedRanges = new()
    {
        ["cpu"] = new VoltageRange(Voltage.Create(0.8), Voltage.Create(1.4)),
        ["memory"] = new VoltageRange(Voltage.Create(1.2), Voltage.Create(1.35)),
        ["io"] = new VoltageRange(Voltage.Create(3.0), Voltage.Create(3.6))
    };

    public Result<string> SetVoltage(string component, double volts)
    {
        var voltage = Voltage.Create(volts);

        if (!_allowedRanges.TryGetValue(component, out var range))
        {
            return Error.Create("UNKNOWN_COMPONENT", $"Component '{component}' is not recognized");
        }

        if (!range.Contains(voltage))
        {
            return Error.Create("VOLTAGE_OUT_OF_RANGE",
                $"Voltage {voltage.Volts}V is outside safe range {range.Min.Volts}V - {range.Max.Volts}V for {component}");
        }

        // Set the voltage (hardware interaction)
        return Result.Success($"Voltage set to {voltage.Volts}V for {component}");
    }
}

public record VoltageRange(Voltage Min, Voltage Max)
{
    public bool Contains(Voltage voltage) =>
        voltage.Volts >= Min.Volts && voltage.Volts <= Max.Volts;
}
```

#### Pressure

```csharp
// Create pressure measurements
Pressure pressure1 = Pressure.Create(101325); // 1 atmosphere in pascals
Pressure pressure2 = Pressure.FromBar(1.5); // 1.5 bar

// Access different units
Console.WriteLine($"Pascals: {pressure1.Pascals}Pa"); // 101325Pa
Console.WriteLine($"Bar: {pressure1.Bar}bar"); // 1.01325bar
Console.WriteLine($"PSI: {pressure1.Psi}psi"); // ~14.7psi
Console.WriteLine($"Atmosphere: {pressure1.Atmosphere}atm"); // 1atm

// Create from different units
Pressure fromPsi = Pressure.FromPsi(30); // Tire pressure
Pressure fromAtmosphere = Pressure.FromAtmosphere(2); // 2 atmospheres
```

#### Current

```csharp
// Create current measurements
Current current1 = Current.Create(2.5); // 2.5 amperes
Current current2 = Current.FromMilliamp(750); // 0.75 amperes

// Access different units
Console.WriteLine($"Amperes: {current1.Amperes}A"); // 2.5A
Console.WriteLine($"Milliamperes: {current1.Milliamperes}mA"); // 2500mA
Console.WriteLine($"Microamperes: {current1.Microamperes}μA"); // 2500000μA
```

## Creating Custom Value Objects

### Basic Value Object Pattern

```csharp
using Klab.Toolkit.ValueObjects;

// Simple value object with validation
public record ProductCode
{
    public string Value { get; }

    public static ProductCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Product code cannot be empty");

        if (code.Length != 8)
            throw new ArgumentException("Product code must be exactly 8 characters");

        if (!code.All(char.IsLetterOrDigit))
            throw new ArgumentException("Product code must contain only letters and digits");

        return new ProductCode(code.ToUpperInvariant());
    }

    private ProductCode(string value)
    {
        Value = value;
    }
}
```

### Value Object with Result Pattern

```csharp
using Klab.Toolkit.Results;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Error.Create("NEGATIVE_AMOUNT", "Money amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            return Error.Create("INVALID_CURRENCY", "Currency code is required");

        if (currency.Length != 3)
            return Error.Create("INVALID_CURRENCY", "Currency code must be 3 characters");

        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
```

### Complex Value Object with Business Logic

```csharp
public record Temperature
{
    public double Celsius { get; }
    public double Fahrenheit => (Celsius * 9 / 5) + 32;
    public double Kelvin => Celsius + 273.15;

    public static Result<Temperature> FromCelsius(double celsius)
    {
        if (celsius < -273.15)
            return Error.Create("INVALID_TEMPERATURE", "Temperature cannot be below absolute zero (-273.15°C)");

        return Result.Success(new Temperature(celsius));
    }

    public static Result<Temperature> FromFahrenheit(double fahrenheit)
    {
        var celsius = (fahrenheit - 32) * 5 / 9;
        return FromCelsius(celsius);
    }

    public static Result<Temperature> FromKelvin(double kelvin)
    {
        if (kelvin < 0)
            return Error.Create("INVALID_TEMPERATURE", "Temperature in Kelvin cannot be negative");

        return FromCelsius(kelvin - 273.15);
    }

    private Temperature(double celsius)
    {
        Celsius = celsius;
    }

    public bool IsFreezingPoint => Math.Abs(Celsius) < 0.01;
    public bool IsBoilingPoint => Math.Abs(Celsius - 100) < 0.01;

    public TemperatureRange GetPhase()
    {
        return Celsius switch
        {
            < 0 => TemperatureRange.Solid,
            >= 0 and < 100 => TemperatureRange.Liquid,
            >= 100 => TemperatureRange.Gas,
            _ => TemperatureRange.Unknown
        };
    }
}

public enum TemperatureRange
{
    Unknown,
    Solid,
    Liquid,
    Gas
}
```

## Advanced Patterns

### Value Object Collections

```csharp
public record Coordinates
{
    public double Latitude { get; }
    public double Longitude { get; }

    public static Result<Coordinates> Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            return Error.Create("INVALID_LATITUDE", "Latitude must be between -90 and 90 degrees");

        if (longitude < -180 || longitude > 180)
            return Error.Create("INVALID_LONGITUDE", "Longitude must be between -180 and 180 degrees");

        return Result.Success(new Coordinates(latitude, longitude));
    }

    private Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanceTo(Coordinates other)
    {
        // Haversine formula implementation
        const double R = 6371; // Earth's radius in kilometers

        var lat1Rad = Latitude * Math.PI / 180;
        var lat2Rad = other.Latitude * Math.PI / 180;
        var deltaLatRad = (other.Latitude - Latitude) * Math.PI / 180;
        var deltaLonRad = (other.Longitude - Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}

public record Route
{
    private readonly List<Coordinates> _waypoints;

    public IReadOnlyList<Coordinates> Waypoints => _waypoints.AsReadOnly();
    public double TotalDistance => CalculateTotalDistance();

    public static Result<Route> Create(IEnumerable<Coordinates> waypoints)
    {
        var waypointList = waypoints.ToList();

        if (waypointList.Count < 2)
            return Error.Create("INSUFFICIENT_WAYPOINTS", "Route must have at least 2 waypoints");

        return Result.Success(new Route(waypointList));
    }

    private Route(List<Coordinates> waypoints)
    {
        _waypoints = waypoints;
    }

    private double CalculateTotalDistance()
    {
        double total = 0;
        for (int i = 0; i < _waypoints.Count - 1; i++)
        {
            total += _waypoints[i].DistanceTo(_waypoints[i + 1]);
        }
        return total;
    }
}
```

### Entity Integration

```csharp
public class Product
{
    public Guid Id { get; private set; }
    public ProductCode Code { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public Email ContactEmail { get; private set; }

    public static Result<Product> Create(string code, string name, decimal price, string currency, string contactEmail)
    {
        try
        {
            var productCode = ProductCode.Create(code);
            var email = Email.Create(contactEmail);

            var moneyResult = Money.Create(price, currency);
            if (!moneyResult.IsSuccess)
                return Result.Failure<Product>(moneyResult.Error);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Code = productCode,
                Name = name,
                Price = moneyResult.Value,
                ContactEmail = email
            };

            return Result.Success(product);
        }
        catch (ArgumentException ex)
        {
            return Error.Create("INVALID_PRODUCT_DATA", ex.Message);
        }
    }

    public Result UpdatePrice(decimal newPrice)
    {
        var newMoneyResult = Money.Create(newPrice, Price.Currency);
        if (!newMoneyResult.IsSuccess)
            return newMoneyResult;

        Price = newMoneyResult.Value;
        return Result.Success();
    }
}
```

## Testing Value Objects

```csharp
public class EmailTests
{
    [Test]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        // Arrange
        string validEmail = "test@example.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Test]
    public void Create_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        string invalidEmail = "not-an-email";

        // Act & Assert
        Action act = () => Email.Create(invalidEmail);
        act.Should().Throw<ArgumentException>()
           .WithMessage("E-Mail is invalid");
    }

    [Test]
    [TestCase("", "Empty E-Mail Adress is not possible")]
    [TestCase("   ", "Empty E-Mail Adress is not possible")]
    [TestCase("invalid-email", "E-Mail is invalid")]
    [TestCase("@example.com", "E-Mail is invalid")]
    [TestCase("test@", "E-Mail is invalid")]
    public void Create_WithInvalidInput_ShouldThrowWithCorrectMessage(string input, string expectedMessage)
    {
        // Act & Assert
        Action act = () => Email.Create(input);
        act.Should().Throw<ArgumentException>()
           .WithMessage(expectedMessage);
    }
}

public class VoltageTests
{
    [Test]
    public void Create_ShouldSetVoltsCorrectly()
    {
        // Arrange
        double volts = 5.0;

        // Act
        var voltage = Voltage.Create(volts);

        // Assert
        voltage.Volts.Should().Be(volts);
        voltage.Millivolts.Should().Be(5000);
        voltage.Microvolts.Should().Be(5000000);
    }

    [Test]
    public void FromMillivolt_ShouldConvertCorrectly()
    {
        // Arrange
        double millivolts = 3300;

        // Act
        var voltage = Voltage.FromMillivolt(millivolts);

        // Assert
        voltage.Volts.Should().Be(3.3);
        voltage.Millivolts.Should().Be(millivolts);
    }
}
```

## Best Practices

### 1. Validation at Creation
Always validate input during object creation, never allow invalid objects to exist.

```csharp
// ✅ Good: Validation at creation
public static Email Create(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        throw new ArgumentException("Email cannot be empty");

    if (!IsValidFormat(email))
        throw new ArgumentException("Invalid email format");

    return new Email(email);
}

// ❌ Bad: Validation after creation
public void SetEmail(string email)
{
    if (!IsValidFormat(email))
        throw new ArgumentException("Invalid email");
    _email = email;
}
```

### 2. Immutability
Make all value objects immutable to prevent accidental modification.

```csharp
// ✅ Good: Immutable record
public record Email
{
    public string Value { get; }
    private Email(string value) => Value = value;
}

// ❌ Bad: Mutable class
public class Email
{
    public string Value { get; set; }
}
```

### 3. Rich Domain Models
Provide meaningful operations and properties that reflect the domain.

```csharp
// ✅ Good: Rich domain model
public record Temperature
{
    public double Celsius { get; }
    public double Fahrenheit => (Celsius * 9 / 5) + 32;
    public bool IsFreezing => Celsius <= 0;
    public bool IsBoiling => Celsius >= 100;
}

// ❌ Bad: Anemic model
public record Temperature
{
    public double Value { get; }
}
```

### 4. Equality by Value
Ensure value objects compare by value, not reference (automatic with records).

```csharp
var email1 = Email.Create("test@example.com");
var email2 = Email.Create("test@example.com");
Assert.True(email1 == email2); // Should be equal
```

## Performance Considerations

- **Creation Cost**: Value objects have validation overhead during creation
- **Memory Usage**: Immutable objects may create more garbage, but improve thread safety
- **Comparison**: Record-based equality is efficient for value comparison
- **Caching**: Consider caching frequently used value objects (e.g., common currency codes)

## Thread Safety

All value objects are inherently thread-safe due to their immutable nature:
- No state can be modified after creation
- Multiple threads can safely access the same value object instance
- Validation logic should also be thread-safe (avoid static mutable state)
