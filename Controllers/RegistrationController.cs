using LoginRegistrationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace LoginRegistrationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("registration")]
        public IActionResult RegisterUser(Registration registration)
        {
            Response response = new Response();
            try
            {
                using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ToysCon"));
                string query = "INSERT INTO Registration(UserName, Password, Email, IsActive) VALUES (@UserName, @Password, @Email, @IsActive)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserName", registration.UserName);
                cmd.Parameters.AddWithValue("@Password", HashPassword(registration.Password));
                cmd.Parameters.AddWithValue("@Email", registration.Email);
                cmd.Parameters.AddWithValue("@IsActive", registration.IsActive);

                con.Open();
                int i = cmd.ExecuteNonQuery();
                con.Close();

                if (i > 0)
                {
                    response.statusCode = 200;
                    response.statusMessage = "User registered successfully.";
                }
                else
                {
                    response.statusCode = 400;
                    response.statusMessage = "Registration failed.";
                }
            }
            catch (Exception ex)
            {
                response.statusCode = 500;
                response.statusMessage = $"Error: {ex.Message}";
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("login")]
        public IActionResult LoginUser(Registration registration)
        {
            Response response = new Response();
            try
            {
                using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ToysCon"));
                string query = "SELECT * FROM Registration WHERE Email = @Email AND Password = @Password AND IsActive = 1";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@Email", registration.Email);
                da.SelectCommand.Parameters.AddWithValue("@Password", HashPassword(registration.Password));

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes("this_is_a_super_secure_key_1234567890!!");
                    

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, registration.Email)
                        }),
                        Expires = DateTime.UtcNow.AddHours(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    string jwtToken = tokenHandler.WriteToken(token);

                    return Ok(new { Token = jwtToken });
                }
                else
                {
                    response.statusCode = 401;
                    response.statusMessage = "Invalid user";
                    return Unauthorized(response);
                }
            }
            catch (Exception ex)
            {
                response.statusCode = 500;
                response.statusMessage = $"Error: {ex.Message}";
                return StatusCode(500, response);
            }

        }

        [HttpGet]
        [Route("users")]
        public IActionResult GetAllUsers()
        {
            List<Registration> users = new List<Registration>();

            try
            {
                using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ToysCon"));
                string query = "SELECT * FROM Registration";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new Registration
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        UserName = reader["UserName"].ToString(),
                        Email = reader["Email"].ToString(),
                        IsActive = Convert.ToInt32(reader["IsActive"]),
                        Password = "" // hide password in public API
                    });
                }
                con.Close();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }


        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }

    // ✅ Move UserController outside RegistrationController
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        [Route("get-all-users")]
        public IActionResult GetUsers()
        {
            return Ok("✅ Authorized data returned!");
        }
    }
}

