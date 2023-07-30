using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Project19.AuthContactApp;
using Project19.Entitys;
using Project19.Interfaces;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Expressions;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace Project19.Data
{
    public class ContactDataApi : IContactData
    {
        private HttpClient httpClient { get; set; }
        public static string token;
        public static string roleClaimValue;
        public static string userNameValue;
        public static bool isAuthenticated = false;

        public ContactDataApi()
        {
            httpClient = new HttpClient();
        }

        public IEnumerable<Contact> GetContacts()
        {
            string url = @"https://localhost:7037/api/values";
            if(!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            string json = httpClient.GetStringAsync(url).Result;
            return JsonConvert.DeserializeObject<IEnumerable<Contact>>(json);
        }

        public IList<string> GetCurrentRoles()
        {
            string url = @"https://localhost:7037/api/values/CurrentRoles";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }

            string json = httpClient.GetStringAsync(url).Result;

            IList<string> list = JsonConvert.DeserializeObject<IList<string>>(json);
            return list;
        }

        public IList<string> GetAllUsers()
        {
            string url = @"https://localhost:7037/api/values/GetUsers";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            string json = httpClient.GetStringAsync(url).Result;
            IList<string> list = JsonConvert.DeserializeObject<IList<string>>(json);
            return list;
        }

        public IList<string> GetAllAdmins()
        {
            string url = @"https://localhost:7037/api/values/GetAdmins";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            string json = httpClient.GetStringAsync(url).Result;
            IList<string> list = JsonConvert.DeserializeObject<IList<string>>(json);
            return list;
        }

        public void AddContacts(Contact contact)
        {
            string url = @"https://localhost:7037/api/values";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(contact), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
        }

        public void DeleteContact(int id)
        {
            string url = $"https://localhost:7037/api/values/{id}";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            var r = httpClient.DeleteAsync(
                requestUri: url);
        }

        public async Task<Contact> FindContactById(int id)
        {
            string url = $"https://localhost:7037/api/values/Details/{id}";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            try
            {
                string json = httpClient.GetStringAsync(url).Result;
                return JsonConvert.DeserializeObject<Contact>(json);
            }
            catch
            {
                return null;
            }
        }

        public void ChangeContact(string name, string surname,
            string fatherName, string telephoneNumber, string residenceAdress, string description, int id)
        {
            Contact contact = new Contact()
            {
                Name = name,
                Surname = surname,
                FatherName = fatherName,
                TelephoneNumber = telephoneNumber,
                ResidenceAdress = residenceAdress,
                Description = description
            };

            string url = $"https://localhost:7037/api/values/ChangeContactById/{id}";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(contact), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
        }

        public string RoleCreate(RoleModel model)
        {
            string url = $"https://localhost:7037/api/values/RoleCreate";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            try
            {
                string result = r.Content.ReadAsStringAsync().Result;
                return result;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string AddRoleToUser(RoleModel model)
        {
            string url = $"https://localhost:7037/api/values/AddRoleToUser";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            try
            {
                string result = r.Content.ReadAsStringAsync().Result;
                return result;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string RemoveRoleUser(RoleModel model)
        {
            string url = $"https://localhost:7037/api/values/RemoveUserRole";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            try
            {
                string result = r.Content.ReadAsStringAsync().Result;
                return result;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string UserRemove(RoleModel model)
        {
            string url = $"https://localhost:7037/api/values/RemoveUser";
            if (!string.IsNullOrEmpty(token))
            {
                AddTokenToHeaders();
            }
            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            try
            {
                string result = r.Content.ReadAsStringAsync().Result;
                return result;
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool IsRegister(UserRegistration model)
        {
            string url = $"https://localhost:7037/api/values/Registration/";

            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            if (r.IsSuccessStatusCode)
            {
                string json = r.Content.ReadAsStringAsync().Result;
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                string tokenAuth = tokenResponse.access_token;
                isAuthenticated = true;
                token = $"{tokenAuth}";
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                roleClaimValue = "User";

                foreach (var item in jwtToken.Claims)
                {
                    if (item.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    {
                        userNameValue = item.Value;
                    }
                }
            }
            else
            {
                isAuthenticated = false;
            }
            return isAuthenticated;
        }

        public bool AdministationRegister(UserRegistration model)
        {
            string url = $"https://localhost:7037/api/values/AdminRegistration/";

            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;
            bool isAccountCreate = false;
            if (r.IsSuccessStatusCode)
            {
                isAccountCreate = true;
            }
            else
            {
                isAccountCreate = false;
            }
            return true;
        }

        public bool IsLogin(UserLoginProp model)
        {
            string url = $"https://localhost:7037/api/values/Authenticate/";

            var r = httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                mediaType: "application/json")
                ).Result;

            if (r.IsSuccessStatusCode)
            {
                string json = r.Content.ReadAsStringAsync().Result;
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                string tokenAuth = tokenResponse.access_token;
                isAuthenticated = true;
                token = $"{tokenAuth}";
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                foreach (var item in jwtToken.Claims)
                {
                    if (item.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    {
                        if (item.Value == "Admin")
                        {
                            roleClaimValue = item.Value;
                            break;
                        }
                        else
                        {
                            roleClaimValue = item.Value;
                        }
                    }
                }
                foreach (var item in jwtToken.Claims)
                {
                    if (item.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    {
                        userNameValue = item.Value;
                    }
                }
            }
            else
            {
                isAuthenticated = false;
            }
            return isAuthenticated;
        }

        /// <summary>
        /// Метод для добавления токена в заголовки HTTP-запросов
        /// </summary>
        private void AddTokenToHeaders()
        {
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("Токен: " + httpClient.DefaultRequestHeaders.Authorization);
            }
        }
    }
    public class TokenResponse
    {
        public string access_token { get; set; }
        public string username { get; set; }
    }
}
