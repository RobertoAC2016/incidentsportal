using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace incidents.Models
{
    public class security
    {
        public security() { }
        public String Encrypt(String key = "")
        {
            string source = key.Equals("") ? "Hello RAC!" : key;
            String getHash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                getHash = GetHash(sha256Hash, source);
            }
            return getHash;
        }
        private string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public String CreateToken(String user, String secretkey)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        public LoginInfo verify_login(ISession session)
        {
            LoginInfo ses = null;
            var user = (session.GetString("username") ?? string.Empty).ToString();
            if (!string.IsNullOrEmpty(user))
            {
                ses = new LoginInfo();
                var token = (session.GetString("token") ?? string.Empty).ToString();
                var name = (session.GetString("fullname") ?? string.Empty).ToString();
                ses.username = user;
                ses.fullname = name;
                ses.token = token;
            }
            return ses;
        }
    }
}
