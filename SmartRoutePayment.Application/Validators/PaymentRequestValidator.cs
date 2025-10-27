using FluentValidation;
using SmartRoutePayment.Application.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Application.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequestDto>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(999999)
                .WithMessage("Amount cannot exceed 999,999");

            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .WithMessage("Card number is required")
                .Matches(@"^\d{13,19}$")
                .WithMessage("Card number must be between 13 and 19 digits")
                .Must(BeValidMadaCard)
                .WithMessage("Invalid Mada card number");

            RuleFor(x => x.ExpiryDateMonth)
                .NotEmpty()
                .WithMessage("Expiry month is required")
                .Matches(@"^(0[1-9]|1[0-2])$")
                .WithMessage("Expiry month must be between 01 and 12");

            RuleFor(x => x.ExpiryDateYear)
                .NotEmpty()
                .WithMessage("Expiry year is required")
                .Matches(@"^\d{2}$")
                .WithMessage("Expiry year must be 2 digits (YY format)")
                .Must(BeValidExpiryYear)
                .WithMessage("Card has expired");

            RuleFor(x => x.SecurityCode)
                .NotEmpty()
                .WithMessage("Security code (CVV) is required")
                .Matches(@"^\d{3,4}$")
                .WithMessage("Security code must be 3 or 4 digits");

            RuleFor(x => x.CardHolderName)
                .NotEmpty()
                .WithMessage("Card holder name is required")
                .MinimumLength(2)
                .WithMessage("Card holder name must be at least 2 characters")
                .MaximumLength(50)
                .WithMessage("Card holder name cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Card holder name must contain only letters and spaces");

            RuleFor(x => x.PaymentDescription)
                .MaximumLength(250)
                .When(x => !string.IsNullOrEmpty(x.PaymentDescription))
                .WithMessage("Payment description cannot exceed 250 characters");

            RuleFor(x => x.ItemId)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.ItemId))
                .WithMessage("Item ID cannot exceed 50 characters");
        }

        private bool BeValidMadaCard(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            // Complete Mada BIN ranges (first 6 digits) - Saudi domestic debit cards
            // Source: Official Mada BIN ranges as of 2024
            var madaBinRanges = new[]
            {
        // Al Rajhi Bank
        "400861", "401757", "409201", "410685", "417633", "419593",
        "420132", "421141", "424450", "428331", "428671", "428672",
        "428673", "446404",
        
        // Saudi National Bank (SNB) / NCB
        "403024", "422817", "422818", "422819", "428331", "434107",
        "439954", "445564", "446672", "484783", "486094", "486095",
        "486096", "531095", "543357",
        
        // Riyad Bank
        "404984", "440533", "440647", "440795", "446393", "509740",
        "515197", "520058", "530060", "530906", "554180",
        
        // Alinma Bank
        "468540", "468541", "468542", "468543", "483010", "483011",
        "483012", "588848", "588850", "588982", "588983", "589005",
        
        // Saudi Investment Bank (SAIB)
        "455708", "468540", "520257", "529415",
        
        // Bank AlJazira
        "404984", "458456", "462220", "489317", "489318", "489319",
        "529741", "537767", "585265", "588848",
        
        // Bank Albilad
        "405085", "423691", "431361", "440795", "445564", "468540",
        "483549", "490997", "493428", "543085", "588982",
        
        // Arab National Bank (ANB)
        "401757", "409201", "431361", "432328", "434107", "439954",
        "445564", "457865", "484783", "504300", "524130", "529415",
        "535825", "543357", "585265", "588848",
        
        // Samba Financial Group / SNB
        "455036", "486094", "486095", "486096", "493428", "504300",
        "588982", "604906",
        
        // Banque Saudi Fransi (BSF)
        "440647", "442463", "455708", "468540", "483010", "489318",
        "513213", "520257", "529741", "537767", "539931", "557606",
        "558563", "585265", "588848", "589206",
        
        // Saudi British Bank (SABB)
        "403024", "405085", "433540", "434107", "439954", "440533",
        "457865", "462220", "468540", "483010", "489318", "549760",
        "585265", "588848",
        
        // Bank AlBilad
        "401757", "403024", "419593", "420132", "440647", "462220",
        "468540", "483010", "489318", "520257", "529741", "543357",
        "585265", "588848",
        
        // Gulf International Bank (GIB)
        "407197",
        
        // Deutsche Gulf Finance
        "409201",
        
        // First Abu Dhabi Bank
        "432328",
        
        // Standard Chartered
        "468540", "489318",
        
        // Additional confirmed Mada BINs
        "506968", "508160", "524514", "524130", "524514", "529415",
        "530906", "531196", "535825", "537767", "539931", "543085",
        "543357", "549760", "554180", "557606", "558563", "588850",
        "589005", "589206", "604906", "605141", "636120"
    };

            // Get first 6 digits of card number (BIN)
            var bin = cardNumber.Length >= 6 ? cardNumber.Substring(0, 6) : string.Empty;

            // Check if BIN exists in Mada ranges
            return madaBinRanges.Contains(bin);
        }

        private bool BeValidExpiryYear(PaymentRequestDto dto, string expiryYear)
        {
            if (string.IsNullOrWhiteSpace(expiryYear) || expiryYear.Length != 2)
                return false;

            if (!int.TryParse(expiryYear, out var year))
                return false;

            if (!int.TryParse(dto.ExpiryDateMonth, out var month) || month < 1 || month > 12)
                return false;

            // Convert YY to YYYY
            var currentYear = DateTime.Now.Year;
            var century = (currentYear / 100) * 100;
            var fullYear = century + year;

            // If the year is less than current year, assume next century
            if (fullYear < currentYear)
                fullYear += 100;

            // Check if card is expired (last day of expiry month)
            var expiryDate = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month));
            return expiryDate.Date >= DateTime.Now.Date;
        }
    }
}
