using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project44API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Project44API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BolApiController : ControllerBase
    {   
        // GET api/values
        [HttpGet]
        //[Authorize]
        [AllowAnonymous]
        public ActionResult<string> GetByBolId(int bolId)
        {



            var TMWConnectionString =
                "Server=naus03tmw1;Database=TMWCustom;Integrated Security=True;MultipleActiveResultSets=True;";

            var returnErrorMessage = string.Empty;

            LocationResult LR = new LocationResult();
            DataTable dt = new DataTable();


            using (SqlConnection sqlConn = new SqlConnection(TMWConnectionString))
            {

                string sql = "up_Get_LocationByBolid";

                using (SqlCommand command = new SqlCommand(sql, sqlConn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@bolId", bolId);
                    try
                    {
                        sqlConn.Open();
                    }
                    catch (Exception e)
                    {
                        return BadRequest("SQL Connection Failed to Open");
                    }
             
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LR.RequestId = int.Parse(reader[0].ToString());
                            LR.RequestParms = reader[1].ToString();
                            LR.ReturnValue = reader[2].ToString();
                            LR.RequestDate = DateTime.Now;
                        }
                    }
                }
            }

            if (LR.ReturnValue != null)
            {
                try
                {
                    var checkIfValidJson = JObject.Parse(LR.ReturnValue);
                }
                catch (Exception e)
                {
                  
                    return BadRequest("Return value for request id + " + LR.RequestId + " is not in JSON format");
                }

            }

            return Ok(LR.ReturnValue);
        }

        [AllowAnonymous]
        public ActionResult Authenticate([FromBody]LoginRequest loginRequest)
        {
            try
            {
                string username = loginRequest.Username.Trim();
                var userAdInfoJwt = new UserADInfoJwt();
             
               
                    bool isValidUser = (loginRequest.Password == "BiteMe") ? true : false;
                    if (isValidUser)
                    {
                     
                        JwtSecurityTokenHandler tokenHandler = CreateUserJwtToken(out SecurityToken token);
                        userAdInfoJwt = new UserADInfoJwt
                        {
                            Token = tokenHandler.WriteToken(token),
                            IsAuthenticated = true
                        };
                    return Ok(userAdInfoJwt);

                    }

                throw new Exception("Login Error");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        public static JwtSecurityTokenHandler CreateUserJwtToken(out SecurityToken token)
        {

            string tokenSecret = "WhoKnowsCarlThe2nd'sMiddleName";

            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(tokenSecret);
            List<Claim> userClaims = new List<Claim>
            {

                new Claim("IsAuthenticated", true.ToString()),
            };

           

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(30),
                Audience = "ProjectAPI",
                Issuer = "Project44API",
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler;
        }
    }
}
