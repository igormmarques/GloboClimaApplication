namespace GloboClimaApp.Models
{
    public class UserRegistrationModel
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        // Verifica se a senha e a confirmação coincidem
        public bool IsPasswordMatch => Password == ConfirmPassword;
    }
}
