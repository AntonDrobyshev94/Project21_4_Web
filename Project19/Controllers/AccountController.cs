using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project19.AuthContactApp;
using Project19.ContextFolder;
using Project19.Data;
using Project19.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Project19.Controllers
{
    public class AccountController : Controller
    {
        public static bool isAuth = false;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IContactData _contactData;

        public AccountController(UserManager<User> userManager,
                                SignInManager<User> signInManager,
                                RoleManager<IdentityRole> roleManager,
                                IContactData contactData
                                )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _contactData = contactData;
        }

        /// <summary>
        /// Асинхронный Get запрос на открытие страницы администратора. 
        /// В результате запроса происходит формирование коллекции IList 
        /// string,в которую происходит запись ролей текущего пользователя.
        /// Полученная переменная записывается в ViewBag.Role для
        /// использования в представлении. Также происходит формирование
        /// коллекций IEnumerable имён всех пользователей и имён
        /// пользователей с правами администратора, которые записываются
        /// во ViewBag.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddRole()
        {
            if (ContactDataApi.roleClaimValue != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            
            var userRole = _contactData.GetCurrentRoles();
            if (userRole.Count <= 0) //проверяем количество ролей
            {
                userRole.Add("Роль не определена");
            }

            var users = _contactData.GetAllUsers();
            var adminUsers = _contactData.GetAllAdmins();

            ViewBag.Role = userRole;
            ViewBag.AllUsers = users;
            ViewBag.Admins = adminUsers;

            return View();
        }

        /// <summary>
        /// Асинхронный Post запрос создания новой роли, принимающий 
        /// string переменную roleName (название роли). В блоке
        /// try catch происходит проверка на пустую строку, а также
        /// с помощью метода RoleExistAsync производится проверка
        /// на совпадение указанной роли и имеющихся ролей.
        /// В экземпляры TempData записываются сообщения о результате
        /// выполнения операции.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateNewRole(string roleNameString)
        {
            bool isCreate = false;
            string createRoleResponse = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(roleNameString))
                {
                    RoleModel roleModel = new RoleModel
                    {
                        roleName = roleNameString,
                        userName = ""
                    };

                    createRoleResponse = _contactData.RoleCreate(roleModel);

                    if (createRoleResponse == "Роль успешно добавлена")
                    {
                        TempData["RoleCreateMessage"] = "Роль успешно добавлена!";
                        isCreate = true;
                    }
                    else
                    {
                        TempData["RoleCreateMessage"] = "Роль уже существует";
                        isCreate = false;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["RoleCreateMessage"] = $"{ex}";
                isCreate = false;
            }
            TempData["IsCreate"] = isCreate;
            return RedirectToAction("AddRole", "Account");
        }

        /// <summary>
        /// Асинхронный post запрос добавления роли указанному
        /// пользователю, принимающий переменные строкового типа
        /// roleName и userName (название роли и имя пользователя).
        /// Происходят проверки на наличие указанной роли и имени 
        /// пользователя в существующей базе данных (c помощью 
        /// методов RoleExistAsync и FindByNameAsync).
        /// Добавление роли осуществляется методом AddToRoleAsync.
        /// В экземпляры TempData записываются сообщения о 
        /// результатах выполнения операций.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddUserRole(string roleNameString, string userNameString)
        {
            string addRoleToUserResponse = string.Empty;
            bool isRoleAvailable = false;
            bool isUserAvailable = false;
            if (!string.IsNullOrEmpty(roleNameString) && !string.IsNullOrEmpty(userNameString))
            {
                RoleModel roleModel = new RoleModel
                {
                    roleName = roleNameString,
                    userName = userNameString
                };
                addRoleToUserResponse = _contactData.AddRoleToUser(roleModel);
                if (addRoleToUserResponse.Contains("Роль успешно добавлена"))
                {
                    TempData["UserMessage"] = "Пользователь указан верно";
                    TempData["SuccessMessage"] = "Роль успешно добавлена";
                    TempData["MessageRole"] = "Роль доступна для добавления";
                    isRoleAvailable = true;
                    isUserAvailable = true;
                }
                else
                {
                    if (addRoleToUserResponse.Contains("Роль доступна для добавления"))
                    {
                        TempData["MessageRole"] = "Роль доступна для добавления";
                        isRoleAvailable = true;
                    }
                    else
                    {
                        TempData["MessageRole"] = "Ошибка: указанная роль не существует";
                        isRoleAvailable = false;
                    }
                    if (addRoleToUserResponse.Contains("Пользователь указан верно"))
                    {
                        TempData["UserMessage"] = "Пользователь указан верно";
                        isUserAvailable = true;
                    }
                    else
                    {
                        TempData["UserMessage"] = "Пользователь отсутствует";
                        isUserAvailable = false;
                    }
                }
            }
            else
            {
                TempData["UserMessage"] = "Ошибка при распозновании";
                isUserAvailable = false;
                TempData["MessageRole"] = "указанных данных";
                isRoleAvailable = false;
            }
            TempData["isRoleAvailable"] = isRoleAvailable;
            TempData["isUserAvailable"] = isUserAvailable;
            return RedirectToAction("AddRole", "Account");
        }

        /// <summary>
        /// Асинхронный Post запрос на удаление роли у указанного
        /// пользователя, принимающий переменные строкового типа
        /// roleName и userName (название роли и имя пользователя).
        /// Происходят проверки на наличие указанной роли и имени 
        /// пользователя в существующей базе данных (c помощью 
        /// методов RoleExistAsync и FindByNameAsync).
        /// Удаление производится методом RemoveFromRoleAsync.
        /// В экземпляры TempData записываются сообщения о 
        /// результатах выполнения операций.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RemoveUserRole(string roleNameString, string userNameString)
        {
            string removeUserRoleResponse = string.Empty;
            bool isRoleAvailable = false;
            bool isUserAvailable = false;
            bool isUserHaveRole = false;
            if (!string.IsNullOrEmpty(roleNameString) && !string.IsNullOrEmpty(userNameString))
            {
                RoleModel roleModel = new RoleModel
                {
                    roleName = roleNameString,
                    userName = userNameString
                };
                removeUserRoleResponse = _contactData.RemoveRoleUser(roleModel);
                if (removeUserRoleResponse.Contains("Роль успешно удалена"))
                {
                    TempData["UserDeleteMessage"] = "Пользователь указан верно";
                    TempData["DeleteMessage"] = "Роль успешно удалена";
                    TempData["MessageDeleteRole"] = "Роль доступна для удаления";
                    isRoleAvailable = true;
                    isUserAvailable = true;
                    isUserHaveRole = true;
                }
                else if (removeUserRoleResponse.Contains("Роль отсутствует у указанного пользователя"))
                {
                    TempData["DeleteMessage"] = "Роль отсутствует у указанного пользователя";
                    isUserHaveRole = false;
                }
                else
                {
                    if (removeUserRoleResponse.Contains("Роль доступна для удаления"))
                    {
                        TempData["MessageDeleteRole"] = "Роль доступна для удаления";
                        isRoleAvailable = true;
                    }
                    else
                    {
                        TempData["MessageDeleteRole"] = "Ошибка: указанная роль не существует";
                        isRoleAvailable = false;
                    }
                    if (removeUserRoleResponse.Contains("Пользователь указан верно"))
                    {
                        TempData["UserDeleteMessage"] = "Пользователь указан верно";
                        isUserAvailable = true;
                    }
                    else
                    {
                        TempData["UserDeleteMessage"] = "Пользователь отсутствует";
                        isUserAvailable = false;
                    }
                }
            }
            else
            {
                TempData["MessageDeleteRole"] = "Ошибка при распозновании";
                isRoleAvailable = false;
                TempData["UserDeleteMessage"] = "указанных данных";
                isUserAvailable = false;
            }
            TempData["isRoleAvailable"] = isRoleAvailable;
            TempData["isUserAvailable"] = isUserAvailable;
            TempData["isUserHaveRole"] = isUserHaveRole;
            return RedirectToAction("AddRole", "Account");
        }

        /// <summary>
        /// Асинхронный Post запрос на удаления пользователя, 
        /// принимающий переменную userName строкового типа. 
        /// Происходит проверка на наличие указанного имени 
        /// пользователя в существующей базе данных (c помощью 
        /// метода FindByNameAsync). Удаление происходит
        /// при помощи метода DeleteAsync с указанием 
        /// полученного в результате метода FindByNameAsync
        /// экземпляра. В экземпляры TempData записываются 
        /// сообщения о результате выполнения операции.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RemoveUser(string userNameString)
        {
            string removeUserResponse = string.Empty;
            bool isRemoveUser;
            if (!string.IsNullOrEmpty(userNameString))
            {
                RoleModel roleModel = new RoleModel
                {
                    roleName = "",
                    userName = userNameString
                };
                removeUserResponse = _contactData.UserRemove(roleModel);
                if (removeUserResponse.Contains("Пользователь успешно удален"))
                {
                    TempData["DeleteUserMessage"] = "Пользователь успешно удален";
                    isRemoveUser = true;
                }
                else if (removeUserResponse.Contains("Пользователь отсутствует"))
                {
                    TempData["DeleteUserMessage"] = "Пользователь отсутствует";
                    isRemoveUser = false;
                }
                else
                {
                    TempData["DeleteUserMessage"] = "Ошибка";
                    isRemoveUser = false;
                }  
            }
            else
            {
                TempData["DeleteUserMessage"] = "Ошибка";
                isRemoveUser = false;
            }
            TempData["IsRemoveUser"] = isRemoveUser;
            return RedirectToAction("AddRole", "Account");
        }

        /// <summary>
        /// Get запрос, в результате которого происходит переход
        /// на страницу Login (входа в аккаунт). В результате
        /// запроса происходит запоминание адреса страницы 
        /// при помощи ключевого слова return для дальнейшего
        /// возврата на эту страницу.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new UserLogin()
            {
                ReturnUrl = returnUrl
            });
        }

        public async Task<IActionResult> Login(UserLogin model)
        {
            if (ModelState.IsValid)
            {
                if (model.LoginProp != null && model.Password != null)
                {
                    string loginProp = model.LoginProp;
                    string password = model.Password;
                    UserLoginProp userLogin = new UserLoginProp()
                    {
                        UserName = loginProp,
                        Password = password
                    };
                    isAuth = _contactData.IsLogin(userLogin);
                    if (isAuth)
                    {
                        return RedirectToAction("Index", "MyDefault");
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
            }
            ModelState.AddModelError("", "Пользователь не найден");
            return View(model);
        }

        /// <summary>
        /// Get запрос на открытие формы регистрации, который
        /// отправляет новый экземпляр UserRegistration в
        /// представление Register в качестве модели.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View(new UserRegistration());
        }

        /// <summary>
        /// Асинхронный Post запрос, принимающий модель регистрации,
        /// проверяющий правильность этой модели и на ее основе 
        /// создающий нового пользователя и добавляющий в базу 
        /// данных. Далее, производит вход и переадресацию на
        /// страницу Index. Если модель не корректная, то выдается
        /// ошибка.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegistration model)
        {
            if (ModelState.IsValid)
            {
                if (model.LoginProp != null)
                {
                    isAuth = _contactData.IsRegister(model);

                    if (isAuth)
                    {
                        return RedirectToAction("Index", "MyDefault");
                    }
                    else//иначе
                    {
                        ModelState.AddModelError("", "Ошибка регистрации");
                    }
                }
            }
            return View(model);
        }

        /// <summary>
        /// Get запрос на открытие формы регистрации нового пользователя
        /// от имени Администратора
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AdminRegister()
        {
            return View(new UserRegistration());
        }

        /// <summary>
        /// Асинхронный Post запрос, принимающий модель регистрации,
        /// проверяющий правильность этой модели и на ее основе 
        /// создающий нового пользователя и добавляющий в базу 
        /// данных. Если модель не корректная, то выдается
        /// сообщение об ошибке. Если действие выполнено, то 
        /// выдается сообщение об успешном выполнении.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminRegister(UserRegistration model)
        {
            if (ContactDataApi.roleClaimValue != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            bool isSucceed = false;
            if (ModelState.IsValid)
            {
                if (model.LoginProp != null)
                {
                    isSucceed = _contactData.AdministationRegister(model);

                    if (isSucceed)
                    {

                    }
                    else
                    {
                        ModelState.AddModelError("", "Ошибка регистрации");
                    }
                }
            }
            else
            {
                isSucceed = false;
            }
            ViewBag.IsSuccess = isSucceed;
            TempData["UserCreateMessage"]= isSucceed? "Пользовательский аккаунт создан": "Ошибка при создании";
            return View(model);
        }

        /// <summary>
        /// Асинхронный Post запрос выхода из учетной записи.
        /// </summary>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            ContactDataApi.token = string.Empty;
            ContactDataApi.roleClaimValue = string.Empty;
            ContactDataApi.userNameValue = string.Empty;
            ContactDataApi.isAuthenticated = false;
            isAuth = false;
            return RedirectToAction("Login", "Account");
        }
    }
}
