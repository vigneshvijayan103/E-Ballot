using EBallotApi.Dto;
using FluentValidation;
namespace EBallotApi.Validators
{
    public class RegisterCandidateValidator : AbstractValidator<RegisterCandidateDto>
    {
        public RegisterCandidateValidator()
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 100).WithMessage("Age must be between 18 and 100.");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required.")
                .Must(g => g == "Male" || g == "Female" || g == "Other")
                .WithMessage("Gender must be Male, Female, or Other.");

            RuleFor(x => x.PartyName)
                .NotEmpty().WithMessage("Party name is required.")
                .MaximumLength(200);

            RuleFor(x => x.AadharEnc)
                .NotEmpty().WithMessage("Aadhar is required.")
                .Matches(@"^\d{12}$").WithMessage("Aadhar must be 12 digits.");

            RuleFor(x => x.PhoneNumberEnc)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\d{10}$").WithMessage("Phone number must be 10 digits.");


            RuleFor(x => x.ElectionId)
                .GreaterThan(0).WithMessage("ElectionId must be valid.");
        }
    }

}
