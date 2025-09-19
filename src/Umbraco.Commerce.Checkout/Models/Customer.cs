namespace Umbraco.Commerce.Checkout.Models;

public record Customer(string FirstName, string LastName, string Email, string Telephone);

public record CustomerAddress(string AddressLine1, string AddressLine2, string City, string ZipCode);
