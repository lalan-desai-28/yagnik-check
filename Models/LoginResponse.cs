﻿namespace Task8Indentity.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
    }


}
